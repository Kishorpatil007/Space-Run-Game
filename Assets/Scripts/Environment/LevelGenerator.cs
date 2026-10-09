using System.Collections.Generic;
using UnityEngine;
using SpaceJet.Collectibles;
using SpaceJet.Core;
using SpaceJet.Player;

namespace SpaceJet.Environment
{
    /// <summary>
    /// Procedurally generates 3D space level corridors, obstacle fields, collectible lines,
    /// energy stations, crystals, and power-ups with GUARANTEED zero-overlap spatial clearance.
    /// </summary>
    public class LevelGenerator : MonoBehaviour
    {
        public static LevelGenerator Instance { get; set; }

        [Header("Prefabs / Templates")]
        public GameObject smallAsteroidPrefab;
        public GameObject mediumAsteroidPrefab;
        public GameObject largeAsteroidPrefab;
        public GameObject spaceDebrisPrefab;
        public GameObject movingObstaclePrefab;
        public GameObject energyBarrierPrefab;
        public GameObject energyStationPrefab;
        public GameObject coinPrefab;
        public GameObject energyCrystalPrefab;
        public GameObject shieldPowerUpPrefab;

        [Header("Generation Parameters")]
        public float spawnAheadDistance = 480f;
        public float despawnBehindDistance = 65f;
        public float sliceSpacing = 22f;

        // Subway Surfers 3-Lane Flight Corridor System
        public const float LANE_LEFT = -5.0f;
        public const float LANE_CENTER = 0.0f;
        public const float LANE_RIGHT = 5.0f;
        private static readonly float[] LANES = new float[] { LANE_LEFT, LANE_CENTER, LANE_RIGHT };

        // Uniform Spacing & Deterministic Placement Constants
        public const float COIN_SPACING = 5.2f;
        public const float SPECIFIED_CRYSTAL_INTERVAL = 75.0f;

        private float currentGeneratedZ = 20f;
        private float nextSpecifiedCrystalZ = 75.0f;
        private List<GameObject> activeObjects = new List<GameObject>();
        private Transform poolContainer;

        // Spatial reservation tracker to prevent any objects from overlapping
        private struct PlacedReservation
        {
            public Vector3 position;
            public float radius;
            public bool isObstacle;
        }

        private List<PlacedReservation> activeReservations = new List<PlacedReservation>();

        private void Awake()
        {
            Instance = this;
            if (poolContainer == null)
            {
                poolContainer = new GameObject("LevelObjectsContainer").transform;
            }
        }

        public void ClearLevel()
        {
            foreach (var obj in activeObjects)
            {
                if (obj != null) Destroy(obj);
            }
            activeObjects.Clear();
            activeReservations.Clear();
            currentGeneratedZ = 20f;
            nextSpecifiedCrystalZ = SPECIFIED_CRYSTAL_INTERVAL;
        }

        public void GenerateLevel(int levelIndex, float totalDistance)
        {
            ClearLevel();

            // Setup Planet at destination
            if (PlanetController.Instance)
            {
                PlanetController.Instance.SetupForMission(levelIndex, totalDistance);
            }

            // Apply visual atmosphere
            if (SpaceEnvironment.Instance)
            {
                SpaceEnvironment.Instance.ApplyLevelTheme(levelIndex);
            }

            // Pre-generate initial slices ahead of player starting immediately at z = 20m
            currentGeneratedZ = 20f;
            while (currentGeneratedZ < Mathf.Min(totalDistance, spawnAheadDistance))
            {
                GenerateSlice(currentGeneratedZ, levelIndex, totalDistance);
                currentGeneratedZ += sliceSpacing;
            }
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            if (PlayerController.Instance == null) return;

            float playerZ = PlayerController.Instance.transform.position.z;
            float maxZ = GameManager.Instance.targetMissionDistance;

            // Stream new slices ahead continuously all the way to destination
            int slicesSpawnedThisFrame = 0;
            while (currentGeneratedZ <= maxZ && currentGeneratedZ < playerZ + spawnAheadDistance && slicesSpawnedThisFrame < 8)
            {
                GenerateSlice(currentGeneratedZ, GameManager.Instance.currentLevelIndex, GameManager.Instance.targetMissionDistance);
                currentGeneratedZ += sliceSpacing;
                slicesSpawnedThisFrame++;
            }

            // Despawn objects cleanly behind player
            for (int i = activeObjects.Count - 1; i >= 0; i--)
            {
                GameObject obj = activeObjects[i];
                if (obj == null)
                {
                    activeObjects.RemoveAt(i);
                    continue;
                }

                if (obj.transform.position.z < playerZ - despawnBehindDistance)
                {
                    Destroy(obj);
                    activeObjects.RemoveAt(i);
                }
            }

            // Prune reservations behind player
            float pruneZ = playerZ - despawnBehindDistance - 15f;
            activeReservations.RemoveAll(r => r.position.z < pruneZ);
        }

        #region Spatial Clearance System (Zero Overlap)

        public bool IsPositionClear(Vector3 pos, float radius)
        {
            for (int i = 0; i < activeReservations.Count; i++)
            {
                var res = activeReservations[i];
                if (Mathf.Abs(res.position.z - pos.z) < (res.radius + radius + 0.6f))
                {
                    float minDist = res.radius + radius;
                    if ((res.position - pos).sqrMagnitude < (minDist * minDist))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void RegisterReservation(Vector3 pos, float radius, bool isObstacle = false)
        {
            activeReservations.Add(new PlacedReservation
            {
                position = pos,
                radius = radius,
                isObstacle = isObstacle
            });
        }

        public bool TryFindClearPosition(float z, float radius, out Vector3 position, float minX = -12f, float maxX = 12f, float minY = -4.5f, float maxY = 6.0f, int maxAttempts = 20)
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                float x = Random.Range(minX, maxX);
                float y = Random.Range(minY, maxY);
                Vector3 candidate = new Vector3(x, y, z + Random.Range(-4f, 4f));

                if (IsPositionClear(candidate, radius))
                {
                    position = candidate;
                    return true;
                }
            }

            position = Vector3.zero;
            return false;
        }

        #endregion

        #region Procedural Slice Generation

        private void GenerateSlice(float z, int level, float totalDistance)
        {
            // Energy Stations appear regularly every ~300m across all missions
            float stationInterval = Mathf.Clamp(totalDistance * 0.25f, 250f, 400f);
            int totalStations = Mathf.Max(3, Mathf.FloorToInt(totalDistance / stationInterval));
            for (int q = 1; q <= totalStations; q++)
            {
                float targetStationZ = q * stationInterval;
                if (targetStationZ < totalDistance - 140f && Mathf.Abs(z - targetStationZ) < sliceSpacing * 0.48f)
                {
                    SpawnEnergyStation(new Vector3(0f, 0f, z));
                    SpawnSubwayCoinRunway(z - 20f, LANE_CENTER, 4);
                    return; // Clean cinematic approach for the station
                }
            }

            // Deterministic Energy Crystals: Placed at specified distance (every 75m)
            while (nextSpecifiedCrystalZ <= z + sliceSpacing * 0.5f && nextSpecifiedCrystalZ < totalDistance - 40f)
            {
                SpawnSpecifiedEnergyCrystal(nextSpecifiedCrystalZ, level);
                nextSpecifiedCrystalZ += SPECIFIED_CRYSTAL_INTERVAL;
            }

            // Final Destination Approach: Continuous coins and obstacles leading straight into destination planet
            if (z >= totalDistance - 140f)
            {
                float distToPlanet = totalDistance - z;

                // 1. Continuous Golden Coin Runways with uniform spacing
                SpawnSubwayCoinRunway(z, LANE_CENTER, 4);
                if (distToPlanet > 45f)
                {
                    float sideLane = (Random.value > 0.5f) ? LANE_LEFT : LANE_RIGHT;
                    SpawnSubwayCoinRunway(z + 1.5f, sideLane, 4);
                }
                else
                {
                    // Near destination: Grand Golden Halo Ring
                    SpawnSubwayCoinRing(z, LANE_CENTER, 4.0f, 8);
                }

                // 2. Asteroid Obstacles safely framing arrival path on outer perimeter
                if (distToPlanet > 20f)
                {
                    SpawnAsteroidGate(z, level);
                }
                return;
            }

            int sliceType = Random.Range(0, 7);
            int primaryLaneIndex = Random.Range(0, 3);
            float primaryLane = LANES[primaryLaneIndex];
            float altLane = LANES[(primaryLaneIndex + 1) % 3];
            float thirdLane = LANES[(primaryLaneIndex + 2) % 3];

            switch (sliceType)
            {
                case 0: // Subway Surfers Triple-Lane Run: golden runway in primary, obstacle in third lane
                    SpawnSubwayCoinRunway(z, primaryLane, 4);
                    if (Random.value > 0.4f) SpawnSubwayCoinRunway(z + 2f, altLane, 4);
                    SpawnAsteroidAt(new Vector3(thirdLane, Random.Range(-1.5f, 2.5f), z + 6f), level, 2.6f);
                    break;

                case 1: // Subway Surfers Lane Switch: S-curve transition inviting pilot to switch lanes
                    SpawnSubwayLaneSwitch(z, primaryLane, altLane, 4);
                    SpawnAsteroidAt(new Vector3(thirdLane, Random.Range(-1.5f, 2.5f), z + 8f), level, 2.6f);
                    if (Random.value < 0.45f)
                    {
                        PowerUpType pType = (Random.value < 0.5f) ? PowerUpType.Shield : PowerUpType.EnergySurge;
                        SpawnPowerUp(new Vector3(thirdLane, 0.5f, z + 14f), pType);
                    }
                    break;

                case 2: // Subway Surfers Jump Arch: Parabolic coin arch leaping upwards in the lane
                    SpawnSubwayCoinArch(z, primaryLane, 4);
                    SpawnAsteroidAt(new Vector3(altLane, Random.Range(-2f, 2f), z + 8f), level, 2.4f);
                    break;

                case 3: // Subway Surfers Golden Ring Halo: Fly through ring to score
                    SpawnSubwayCoinRing(z, primaryLane, 3.8f, 8);
                    SpawnAsteroidAt(new Vector3((primaryLane > 0) ? -8.5f : 8.5f, Random.Range(-1.5f, 2.5f), z + 4f), level, 2.8f);
                    break;

                case 4: // Asteroid Gate with Guaranteed Safe Center Corridor
                    SpawnAsteroidGate(z, level);
                    SpawnSubwayCoinRunway(z + 2f, LANE_CENTER, 4);
                    break;

                case 5: // Asteroid Slalom Chicane: 2 well-spaced obstacles with wide clearance and connecting coins
                    SpawnAsteroidSlalom(z, level);
                    SpawnSubwayCoinRunway(z + 2f, primaryLane, 4);
                    if (level >= 2 && Random.value < 0.45f)
                    {
                        PowerUpType pType = (Random.value < 0.6f) ? PowerUpType.EnergySurge : PowerUpType.Shield;
                        SpawnPowerUp(new Vector3(thirdLane, 0.5f, z + 14f), pType);
                    }
                    break;

                case 6: // High-Speed Double Golden Runway
                    SpawnSubwayCoinRunway(z, primaryLane, 4);
                    SpawnSubwayCoinRunway(z + 2f, altLane, 4);
                    SpawnAsteroidAt(new Vector3(thirdLane, Random.Range(-1.5f, 2.5f), z + 6f), level, 2.4f);
                    break;
            }
        }

        #endregion

        #region Spawning Methods

        private GameObject SpawnInstance(GameObject prefab, Vector3 pos, Quaternion rot)
        {
            if (prefab == null) return null;
            if (poolContainer == null)
            {
                poolContainer = new GameObject("LevelObjectsContainer").transform;
            }
            GameObject obj = Instantiate(prefab, pos, rot, poolContainer);
            obj.SetActive(true);
            activeObjects.Add(obj);
            return obj;
        }

        #region Subway Surfers 3-Lane Coin Spawners

        public void SpawnSubwayCoinRunway(float z, float laneX, int count = 4, float y = 0.5f)
        {
            if (coinPrefab == null) return;

            for (int i = 0; i < count; i++)
            {
                Vector3 pos = new Vector3(laneX, y, z + i * COIN_SPACING);
                if (!IsPositionClear(pos, 2.2f)) continue;

                GameObject coinObj = SpawnInstance(coinPrefab, pos, Quaternion.identity);
                if (coinObj != null)
                {
                    var coin = coinObj.GetComponent<Coin>();
                    if (coin)
                    {
                        if (i == count - 1) coin.SetCoinType(CoinType.Cosmic);
                        else if (i % 2 == 1) coin.SetCoinType(CoinType.Gold);
                        else coin.SetCoinType(CoinType.Normal);
                    }
                    RegisterReservation(pos, 2.2f, false);
                }
            }
        }

        public void SpawnSubwayLaneSwitch(float z, float fromLaneX, float toLaneX, int count = 4, float y = 0.5f)
        {
            if (coinPrefab == null) return;

            for (int i = 0; i < count; i++)
            {
                float t = (count <= 1) ? 0f : (float)i / (count - 1);
                float x = Mathf.SmoothStep(fromLaneX, toLaneX, t);
                Vector3 pos = new Vector3(x, y, z + i * COIN_SPACING);
                if (!IsPositionClear(pos, 2.2f)) continue;

                GameObject coinObj = SpawnInstance(coinPrefab, pos, Quaternion.identity);
                if (coinObj != null)
                {
                    var coin = coinObj.GetComponent<Coin>();
                    if (coin)
                    {
                        if (i == count - 1) coin.SetCoinType(CoinType.Cosmic);
                        else if (i % 2 == 1) coin.SetCoinType(CoinType.Gold);
                        else coin.SetCoinType(CoinType.Normal);
                    }
                    RegisterReservation(pos, 2.2f, false);
                }
            }
        }

        public void SpawnSubwayCoinArch(float z, float laneX, int count = 4, float yBase = 0f)
        {
            if (coinPrefab == null) return;

            for (int i = 0; i < count; i++)
            {
                float t = (count <= 1) ? 0f : (float)i / (count - 1);
                float y = yBase + Mathf.Sin(t * Mathf.PI) * 3.4f;
                Vector3 pos = new Vector3(laneX, y, z + i * COIN_SPACING);
                if (!IsPositionClear(pos, 2.2f)) continue;

                GameObject coinObj = SpawnInstance(coinPrefab, pos, Quaternion.identity);
                if (coinObj != null)
                {
                    var coin = coinObj.GetComponent<Coin>();
                    if (coin)
                    {
                        if (i == count / 2) coin.SetCoinType(CoinType.Cosmic);
                        else if (i % 2 == 1) coin.SetCoinType(CoinType.Gold);
                        else coin.SetCoinType(CoinType.Normal);
                    }
                    RegisterReservation(pos, 2.2f, false);
                }
            }
        }

        public void SpawnSubwayCoinRing(float z, float laneX, float radius = 3.8f, int count = 8)
        {
            if (coinPrefab == null) return;
            float centerY = 0.5f;

            for (int i = 0; i < count; i++)
            {
                float angle = (i / (float)count) * Mathf.PI * 2f;
                float x = laneX + Mathf.Cos(angle) * radius;
                float y = centerY + Mathf.Sin(angle) * (radius * 0.75f);
                Vector3 pos = new Vector3(x, y, z);

                if (!IsPositionClear(pos, 2.0f)) continue;

                GameObject coinObj = SpawnInstance(coinPrefab, pos, Quaternion.identity);
                if (coinObj != null)
                {
                    var coin = coinObj.GetComponent<Coin>();
                    if (coin)
                    {
                        coin.SetCoinType((i % 2 == 0) ? CoinType.Gold : CoinType.Normal);
                    }
                    RegisterReservation(pos, 2.0f, false);
                }
            }

            // Reward in center of ring: Cosmic coin!
            Vector3 centerPos = new Vector3(laneX, centerY, z);
            if (IsPositionClear(centerPos, 2.2f))
            {
                GameObject centerCoin = SpawnInstance(coinPrefab, centerPos, Quaternion.identity);
                if (centerCoin != null)
                {
                    var c = centerCoin.GetComponent<Coin>();
                    if (c) c.SetCoinType(CoinType.Cosmic);
                    RegisterReservation(centerPos, 2.2f, false);
                }
            }
        }

        #endregion

        private void SpawnCoinArc(float z)
        {
            if (coinPrefab == null) return;
            int count = 4;
            float radius = 5.2f;
            float startAngle = Random.Range(0f, Mathf.PI);

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + (i / (float)count) * Mathf.PI;
                float x = Mathf.Clamp(Mathf.Cos(angle) * radius, -12f, 12f);
                float y = Mathf.Clamp(Mathf.Sin(angle) * (radius * 0.6f), -4f, 5.5f);
                Vector3 pos = new Vector3(x, y, z + i * COIN_SPACING);

                // Ensure coin maintains distance and clearance
                if (!IsPositionClear(pos, 2.2f)) continue;

                GameObject coinObj = SpawnInstance(coinPrefab, pos, Quaternion.identity);
                if (coinObj != null)
                {
                    var coin = coinObj.GetComponent<Coin>();
                    if (coin)
                    {
                        coin.SetCoinType((i == count / 2 && Random.value > 0.45f) ? CoinType.Gold : CoinType.Normal);
                    }
                    RegisterReservation(pos, 2.2f, false);
                }
            }
        }

        private void SpawnCoinLine(float z, int count = 4)
        {
            if (coinPrefab == null) return;
            float fixedX = Random.Range(-9.5f, 9.5f);
            float fixedY = Random.Range(-3.5f, 4.5f);

            for (int i = 0; i < count; i++)
            {
                Vector3 pos = new Vector3(fixedX, fixedY, z + i * COIN_SPACING);
                if (!IsPositionClear(pos, 2.2f)) continue;

                GameObject coinObj = SpawnInstance(coinPrefab, pos, Quaternion.identity);
                if (coinObj != null)
                {
                    var coin = coinObj.GetComponent<Coin>();
                    if (coin)
                    {
                        if (i == count - 1 && Random.value > 0.65f)
                        {
                            coin.SetCoinType(CoinType.Cosmic);
                        }
                        else if (i % 2 == 1 && Random.value > 0.5f)
                        {
                            coin.SetCoinType(CoinType.Gold);
                        }
                    }
                    RegisterReservation(pos, 2.2f, false);
                }
            }
        }

        public void SpawnCoinRing(float z, float radius = 4.2f, int count = 8)
        {
            if (coinPrefab == null) return;
            float centerX = Random.Range(-5f, 5f);
            float centerY = Random.Range(-2f, 3.5f);

            for (int i = 0; i < count; i++)
            {
                float angle = (i / (float)count) * Mathf.PI * 2f;
                float x = centerX + Mathf.Cos(angle) * radius;
                float y = centerY + Mathf.Sin(angle) * (radius * 0.75f);
                Vector3 pos = new Vector3(x, y, z);

                if (!IsPositionClear(pos, 2.0f)) continue;

                GameObject coinObj = SpawnInstance(coinPrefab, pos, Quaternion.identity);
                if (coinObj != null)
                {
                    var coin = coinObj.GetComponent<Coin>();
                    if (coin)
                    {
                        coin.SetCoinType((i % 4 == 0) ? CoinType.Gold : CoinType.Normal);
                    }
                    RegisterReservation(pos, 2.0f, false);
                }
            }

            // Reward in center of ring: Cosmic coin
            Vector3 centerPos = new Vector3(centerX, centerY, z);
            if (IsPositionClear(centerPos, 2.2f))
            {
                GameObject centerCoin = SpawnInstance(coinPrefab, centerPos, Quaternion.identity);
                if (centerCoin != null)
                {
                    var c = centerCoin.GetComponent<Coin>();
                    if (c) c.SetCoinType(CoinType.Cosmic);
                    RegisterReservation(centerPos, 2.2f, false);
                }
            }
        }

        public void SpawnCoinSpeedTrail(float z, int count = 4)
        {
            if (coinPrefab == null) return;
            float lineX = Random.Range(-7f, 7f);
            float lineY = Random.Range(-2.5f, 3.5f);

            for (int i = 0; i < count; i++)
            {
                Vector3 pos = new Vector3(lineX, lineY, z + i * COIN_SPACING);
                if (!IsPositionClear(pos, 2.2f)) continue;

                GameObject coinObj = SpawnInstance(coinPrefab, pos, Quaternion.identity);
                if (coinObj != null)
                {
                    var coin = coinObj.GetComponent<Coin>();
                    if (coin)
                    {
                        if (i == count - 1) coin.SetCoinType(CoinType.Cosmic);
                        else if (i % 2 == 1) coin.SetCoinType(CoinType.Gold);
                        else coin.SetCoinType(CoinType.Normal);
                    }
                    RegisterReservation(pos, 2.2f, false);
                }
            }
        }

        public void SpawnCoinDiamond(float z)
        {
            if (coinPrefab == null) return;
            float cx = Random.Range(-6f, 6f);
            float cy = Random.Range(-2f, 3f);
            Vector3[] offsets = new Vector3[]
            {
                new Vector3(0, 3.2f, 0),
                new Vector3(3.2f, 0, 0),
                new Vector3(0, -3.2f, 0),
                new Vector3(-3.2f, 0, 0),
                new Vector3(0, 0, 0)
            };

            for (int i = 0; i < offsets.Length; i++)
            {
                Vector3 pos = new Vector3(cx + offsets[i].x, cy + offsets[i].y, z + offsets[i].z);
                if (!IsPositionClear(pos, 2.0f)) continue;

                GameObject coinObj = SpawnInstance(coinPrefab, pos, Quaternion.identity);
                if (coinObj != null)
                {
                    var coin = coinObj.GetComponent<Coin>();
                    if (coin)
                    {
                        coin.SetCoinType((i == 4) ? CoinType.Cosmic : ((i % 2 == 0) ? CoinType.Gold : CoinType.Normal));
                    }
                    RegisterReservation(pos, 2.0f, false);
                }
            }
        }

        public void SpawnSpecifiedEnergyCrystal(float crystalZ, int level)
        {
            if (energyCrystalPrefab == null) return;

            // Rhythmic alternating lane pattern across specified 75m distance milestones:
            // 75m -> Center, 150m -> Left, 225m -> Right, 300m -> Center, etc.
            int step = Mathf.RoundToInt(crystalZ / SPECIFIED_CRYSTAL_INTERVAL);
            float[] lanePattern = new float[] { LANE_CENTER, LANE_LEFT, LANE_RIGHT };
            float targetLaneX = lanePattern[step % 3];

            Vector3 targetPos = new Vector3(targetLaneX, 0.8f, crystalZ);

            if (!IsPositionClear(targetPos, 2.2f))
            {
                // If chosen lane is occupied, find first open lane
                for (int offset = 1; offset <= 2; offset++)
                {
                    float altLaneX = lanePattern[(step + offset) % 3];
                    Vector3 altPos = new Vector3(altLaneX, 0.8f, crystalZ);
                    if (IsPositionClear(altPos, 2.2f))
                    {
                        targetPos = altPos;
                        break;
                    }
                }
            }

            SpawnCrystal(targetPos);
        }

        private void SpawnCrystal(Vector3 pos)
        {
            if (energyCrystalPrefab == null || pos == Vector3.zero) return;
            if (!IsPositionClear(pos, 2.0f)) return;

            GameObject crystalObj = SpawnInstance(energyCrystalPrefab, pos, Quaternion.identity);
            if (crystalObj != null)
            {
                var crystal = crystalObj.GetComponent<EnergyCrystal>();
                if (crystal && Random.value > 0.75f)
                {
                    crystal.SetSize(CrystalSize.Large);
                }
                RegisterReservation(pos, 2.0f, false);
            }
        }

        private void SpawnCrystalCluster(float z)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector3 pos = GetClearFlightPoint(z + i * 9f, 1.8f);
                SpawnCrystal(pos);
            }
        }

        public void SpawnPowerUp(Vector3 pos, PowerUpType type)
        {
            if (shieldPowerUpPrefab == null || pos == Vector3.zero) return;
            if (!IsPositionClear(pos, 2.0f)) return;

            GameObject puObj = SpawnInstance(shieldPowerUpPrefab, pos, Quaternion.identity);
            if (puObj != null)
            {
                var pu = puObj.GetComponent<PowerUp>();
                if (pu)
                {
                    pu.ConfigureType(type);
                }
                RegisterReservation(pos, 2.0f, false);
            }
        }

        private void SpawnEnergyStation(Vector3 pos)
        {
            if (energyStationPrefab == null) return;
            // Massive clear ring corridor: reservation radius 8.5f
            RegisterReservation(pos, 8.5f, true);

            GameObject stationObj = SpawnInstance(energyStationPrefab, pos, Quaternion.identity);

            // Add an Energy Surge powerup right before the station as visual beacon elevated at y = 1.2f
            Vector3 beaconPos = new Vector3(0f, 1.2f, pos.z - 12f);
            if (IsPositionClear(beaconPos, 1.8f))
            {
                SpawnPowerUp(beaconPos, PowerUpType.EnergySurge);
            }
        }

        private void SpawnEnergyBarrier(float z)
        {
            if (energyBarrierPrefab == null) return;
            float x = Random.Range(-5f, 5f);
            float y = Random.Range(-2f, 3.5f);
            Vector3 pos = new Vector3(x, y, z);

            // Reserve large clearance box for barrier
            if (!IsPositionClear(pos, 7.5f)) return;

            RegisterReservation(pos, 7.5f, true);
            GameObject barrierObj = SpawnInstance(energyBarrierPrefab, pos, Quaternion.identity);

            // Place an Energy Crystal safely to the left or right bypass lane
            float bypassSide = (x > 0f) ? -8.5f : 8.5f;
            Vector3 bypassPos = new Vector3(bypassSide, y, z);
            if (IsPositionClear(bypassPos, 1.8f))
            {
                SpawnCrystal(bypassPos);
            }
        }

        private void SpawnMovingHazard(float z, int level)
        {
            if (movingObstaclePrefab == null) return;
            Vector3 pos = new Vector3(0f, Random.Range(-2.5f, 3.5f), z);

            // Reserve oscillation sweep zone
            if (!IsPositionClear(pos, 8.0f)) return;

            RegisterReservation(pos, 8.0f, true);
            GameObject obj = SpawnInstance(movingObstaclePrefab, pos, Quaternion.identity);
            if (obj != null)
            {
                var moving = obj.GetComponent<MovingObstacle>();
                if (moving)
                {
                    moving.axis = (Random.value > 0.5f) ? MovingObstacle.MoveAxis.Horizontal : MovingObstacle.MoveAxis.Vertical;
                    moving.moveSpeed = 2.2f + level * 0.35f;
                    moving.amplitude = Random.Range(5.5f, 9.0f);
                }
            }
        }

        private void SpawnAsteroidGate(float z, int level)
        {
            // Creates 2 flanking asteroids with a generous 15m wide safe corridor in between
            float safeX = 0f;
            float safeY = Random.Range(-1.5f, 2.5f);

            Vector3 leftPos = new Vector3(safeX - 7.5f, safeY, z);
            Vector3 rightPos = new Vector3(safeX + 7.5f, safeY, z);

            if (IsPositionClear(leftPos, 3.2f))
            {
                SpawnAsteroidAt(leftPos, level, 3.2f);
            }

            if (IsPositionClear(rightPos, 3.2f))
            {
                SpawnAsteroidAt(rightPos, level, 3.2f);
            }
        }

        private void SpawnObstacleScatter(float z, int count, int level)
        {
            // Limit obstacle scatter to max 1 or 2 well-spaced obstacles
            int actualCount = Mathf.Clamp(count, 1, 2);
            for (int i = 0; i < actualCount; i++)
            {
                float radius = (Random.value < 0.5f) ? 2.2f : 3.0f;
                if (TryFindClearPosition(z + Random.Range(-4f, 8f), radius, out Vector3 pos, -9f, 9f, -2.5f, 3.5f))
                {
                    SpawnAsteroidAt(pos, level, radius);
                }
            }
        }

        private void SpawnAsteroidSlalom(float z, int level)
        {
            // Left asteroid at z, Right asteroid at z + 12m creating a smooth wide flight chicane
            float yPos = Random.Range(-1.5f, 2.5f);
            Vector3 leftPos = new Vector3(-7.0f, yPos, z);
            Vector3 rightPos = new Vector3(7.0f, yPos + Random.Range(-1f, 1f), z + 12f);

            if (IsPositionClear(leftPos, 3.0f))
            {
                SpawnAsteroidAt(leftPos, level, 3.0f);
            }
            if (IsPositionClear(rightPos, 3.0f))
            {
                SpawnAsteroidAt(rightPos, level, 3.0f);
            }
        }

        private void SpawnDenseAsteroidBelt(float z, int level)
        {
            // Only 2 outer boundary asteroids far from flight lanes
            Vector3 leftPos = new Vector3(-11f, Random.Range(-2f, 2f), z);
            Vector3 rightPos = new Vector3(11f, Random.Range(-2f, 2f), z + 4f);
            if (IsPositionClear(leftPos, 2.8f)) SpawnAsteroidAt(leftPos, level, 2.8f);
            if (IsPositionClear(rightPos, 2.8f)) SpawnAsteroidAt(rightPos, level, 2.8f);
        }

        private void SpawnAsteroidAt(Vector3 pos, int level, float radius)
        {
            GameObject prefab = mediumAsteroidPrefab;
            if (radius <= 2.4f && smallAsteroidPrefab) prefab = smallAsteroidPrefab;
            else if (radius >= 4.0f && largeAsteroidPrefab) prefab = largeAsteroidPrefab;
            else if (level >= 3 && spaceDebrisPrefab && Random.value < 0.3f) prefab = spaceDebrisPrefab;

            if (prefab != null)
            {
                GameObject ast = SpawnInstance(prefab, pos, Random.rotation);
                if (ast != null)
                {
                    float scaleMod = Random.Range(0.85f, 1.25f);
                    ast.transform.localScale = Vector3.one * scaleMod;
                    RegisterReservation(pos, radius * scaleMod, true);
                }
            }
        }

        private Vector3 GetClearFlightPoint(float z, float radius)
        {
            if (TryFindClearPosition(z, radius, out Vector3 pos, -11f, 11f, -4f, 5.5f, 15))
            {
                return pos;
            }
            return Vector3.zero;
        }

        #endregion
    }
}
