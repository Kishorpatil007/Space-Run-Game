using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SpaceJet.Audio;
using SpaceJet.Collectibles;
using SpaceJet.Environment;
using SpaceJet.Player;
using SpaceJet.UI;
using SpaceJet.Visuals;

namespace SpaceJet.Core
{
    /// <summary>
    /// Master Bootstrapper that initializes, links, and builds all gameplay, visual, audio, 
    /// and UI systems seamlessly at runtime. Ensures the game is 100% playable out-of-the-box.
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        public static GameBootstrapper Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnAfterSceneLoad()
        {
            // Ensure single root instance if no bootstrapper was present in the loaded scene
            if (Instance == null && FindAnyObjectByType<GameBootstrapper>() == null)
            {
                GameObject boot = new GameObject("=== SPACE_JET_BOOTSTRAPPER ===");
                boot.AddComponent<GameBootstrapper>();
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
            InitializeAllSystems();
        }

        public void InitializeAllSystems()
        {
            EnsureEventSystem();
            EnsureCoreManagers();
            EnsureEnvironmentAndCamera();
            EnsurePlayerJet();
            EnsureDestinationPlanet();
            EnsureLevelGenerator();
            EnsureCompleteCanvasUI();
        }

        #region EventSystem & Core Managers

        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
        }

        private void EnsureCoreManagers()
        {
            if (GameManager.Instance == null)
            {
                GameObject gmObj = new GameObject("GameManager");
                GameManager.Instance = gmObj.AddComponent<GameManager>();
            }

            if (MissionManager.Instance == null)
            {
                GameObject mmObj = new GameObject("MissionManager");
                MissionManager.Instance = mmObj.AddComponent<MissionManager>();
            }

            if (AchievementManager.Instance == null)
            {
                GameObject amObj = new GameObject("AchievementManager");
                AchievementManager am = amObj.AddComponent<AchievementManager>();
                AchievementManager.Instance = am;
                am.InitializeAchievements();
            }

            if (AudioManager.Instance == null)
            {
                GameObject audioObj = new GameObject("AudioManager");
                AudioManager am = audioObj.AddComponent<AudioManager>();
                AudioManager.Instance = am;
                am.InitializeAudio();
            }

            if (ParticleEffectsManager.Instance == null)
            {
                GameObject fxObj = new GameObject("ParticleEffectsManager");
                fxObj.AddComponent<ParticleEffectsManager>();
            }
        }

        #endregion

        #region Environment, Camera, and Lighting

        private void EnsureEnvironmentAndCamera()
        {
            // Main Camera & CameraController
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                cam.tag = "MainCamera";
                camObj.AddComponent<AudioListener>();
            }

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.004f, 0.045f, 0.165f); // Deep space cosmic sapphire navy (#010B2A)
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 1200f;
            cam.allowHDR = true;
            cam.allowMSAA = true;

            CameraController camCtrl = cam.GetComponent<CameraController>();
            if (camCtrl == null)
            {
                camCtrl = cam.gameObject.AddComponent<CameraController>();
            }
            CameraController.Instance = camCtrl;

            // Directional Light & Ambient Lighting
            Light sun = FindAnyObjectByType<Light>();
            if (sun == null || sun.type != LightType.Directional)
            {
                GameObject lightObj = new GameObject("SpaceSunLight");
                sun = lightObj.AddComponent<Light>();
                sun.type = LightType.Directional;
                sun.transform.rotation = Quaternion.Euler(35f, -30f, 0f);
            }
            sun.color = new Color(0.92f, 0.96f, 1f);
            sun.intensity = 1.35f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.85f;
            sun.shadowBias = 0.05f;
            sun.shadowNormalBias = 0.4f;

            // Ambient fill light matching the uploaded cosmic space image
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.10f, 0.22f, 0.52f);
            RenderSettings.ambientEquatorColor = new Color(0.18f, 0.12f, 0.38f);
            RenderSettings.ambientGroundColor = new Color(0.02f, 0.04f, 0.12f);

            // Space Environment & Starfield
            if (SpaceEnvironment.Instance == null)
            {
                GameObject envObj = new GameObject("SpaceEnvironment");
                SpaceEnvironment env = envObj.AddComponent<SpaceEnvironment>();
                SpaceEnvironment.Instance = env;
                env.mainDirectionalLight = sun;
                env.starParticles = ParticleEffectsManager.CreateStarfield(envObj.transform);
                env.dustParticles = ParticleEffectsManager.CreateWarpDust(envObj.transform);
            }
        }

        #endregion

        #region Player Space Jet

        private void EnsurePlayerJet()
        {
            if (PlayerController.Instance != null) return;

            // Build Sleek 3D Space Jet
            GameObject jet = ProceduralMeshBuilder.BuildFuturisticJet("PlayerSpaceJet");
            jet.tag = "Player";
            jet.transform.position = Vector3.zero;

            // Player Controllers
            PlayerController pCtrl = jet.AddComponent<PlayerController>();
            PlayerHealth pHealth = jet.AddComponent<PlayerHealth>();
            PlayerEnergy pEnergy = jet.AddComponent<PlayerEnergy>();
            PlayerBoost pBoost = jet.AddComponent<PlayerBoost>();
            PlayerShield pShield = jet.AddComponent<PlayerShield>();

            PlayerController.Instance = pCtrl;
            PlayerHealth.Instance = pHealth;
            pHealth.ResetHealth();
            PlayerEnergy.Instance = pEnergy;
            pEnergy.ResetEnergy();
            PlayerBoost.Instance = pBoost;
            pBoost.ResetBoost();
            PlayerShield.Instance = pShield;
            pShield.ResetShield();

            // Setup Thruster Particle Effects
            ParticleSystem exhaustL = ParticleEffectsManager.CreateEngineExhaust(jet.transform, new Vector3(-0.45f, 0f, -1.8f), new Color(0.1f, 0.7f, 1f));
            ParticleSystem exhaustR = ParticleEffectsManager.CreateEngineExhaust(jet.transform, new Vector3(0.45f, 0f, -1.8f), new Color(0.1f, 0.7f, 1f));
            pCtrl.normalThrusterParticles = new ParticleSystem[] { exhaustL, exhaustR };

            // Setup Boost Particle Flares
            ParticleSystem boostL = ParticleEffectsManager.CreateEngineExhaust(jet.transform, new Vector3(-0.45f, 0f, -1.85f), Color.white, 0.55f);
            ParticleSystem boostR = ParticleEffectsManager.CreateEngineExhaust(jet.transform, new Vector3(0.45f, 0f, -1.85f), Color.white, 0.55f);
            pCtrl.boostThrusterParticles = new ParticleSystem[] { boostL, boostR };

            // Setup Shield Bubble Visual
            GameObject shieldBubble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shieldBubble.name = "ShieldBubbleVisual";
            shieldBubble.transform.SetParent(jet.transform, false);
            shieldBubble.transform.localScale = new Vector3(4.5f, 2.2f, 4.8f);
            DestroyImmediate(shieldBubble.GetComponent<Collider>());
            Material shieldMat = ProceduralMeshBuilder.CreateMaterial("ShieldMat", new Color(0.2f, 0.8f, 1f, 0.35f), new Color(0f, 0.9f, 1f) * 1.5f, 0.1f, 0.95f, true);
            shieldBubble.GetComponent<Renderer>().material = shieldMat;
            shieldBubble.SetActive(false);
            pShield.shieldVisualObject = shieldBubble;

            // Link Camera to Player
            if (CameraController.Instance != null)
            {
                CameraController.Instance.SetTarget(jet.transform);
            }
        }

        #endregion

        #region Destination Planet

        private void EnsureDestinationPlanet()
        {
            if (PlanetController.Instance != null) return;

            GameObject planetRoot = new GameObject("DestinationPlanet");
            PlanetController pc = planetRoot.AddComponent<PlanetController>();
            PlanetController.Instance = pc;

            // Surface Sphere
            GameObject surface = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            surface.name = "PlanetSurface";
            surface.transform.SetParent(planetRoot.transform, false);
            surface.transform.localScale = Vector3.one * 85f;
            DestroyImmediate(surface.GetComponent<Collider>());
            Material surfMat = ProceduralMeshBuilder.CreateMaterial("PlanetSurfMat", new Color(0.15f, 0.5f, 0.9f), Color.black, 0.2f, 0.6f);
            surface.GetComponent<Renderer>().material = surfMat;
            pc.planetSurface = surface.transform;

            // Atmosphere Glow Shell
            GameObject atmo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            atmo.name = "AtmosphereShell";
            atmo.transform.SetParent(planetRoot.transform, false);
            atmo.transform.localScale = Vector3.one * 89f;
            DestroyImmediate(atmo.GetComponent<Collider>());
            Material atmoMat = ProceduralMeshBuilder.CreateMaterial("PlanetAtmoMat", new Color(0.3f, 0.85f, 1f, 0.3f), new Color(0.1f, 0.6f, 1f) * 1.2f, 0.1f, 0.9f, true);
            atmo.GetComponent<Renderer>().material = atmoMat;
            pc.atmosphereGlow = atmo.transform;

            // Planetary Ring
            GameObject ring = new GameObject("PlanetaryRing");
            ring.transform.SetParent(planetRoot.transform, false);
            ring.transform.localScale = Vector3.one * 35f;
            MeshFilter mf = ring.AddComponent<MeshFilter>();
            mf.sharedMesh = ProceduralMeshBuilder.CreateRingMesh(2.2f, 3.8f, 48);
            MeshRenderer mr = ring.AddComponent<MeshRenderer>();
            Material ringMat = ProceduralMeshBuilder.CreateMaterial("RingMat", new Color(0.6f, 0.85f, 1f, 0.4f), Color.cyan * 0.5f, 0.1f, 0.8f, true);
            mr.material = ringMat;
            ring.transform.localRotation = Quaternion.Euler(25f, 0f, 15f);
            pc.planetaryRing = ring.transform;

            pc.SetupForMission(1, 1500f);
        }

        #endregion

        #region Level Generator & Prefab Templates

        private void EnsureLevelGenerator()
        {
            if (LevelGenerator.Instance != null) return;

            GameObject genObj = new GameObject("LevelGenerator");
            LevelGenerator gen = genObj.AddComponent<LevelGenerator>();
            LevelGenerator.Instance = gen;

            // Generate Prefabs / Templates with Distinctive Sci-Fi Themes
            gen.smallAsteroidPrefab = CreateAsteroidTemplate("SmallAsteroidTemplate", 1.4f, ObstacleType.SmallAsteroid, ProceduralMeshBuilder.AsteroidTheme.Carbon);
            gen.mediumAsteroidPrefab = CreateAsteroidTemplate("MediumAsteroidTemplate", 2.4f, ObstacleType.MediumAsteroid, ProceduralMeshBuilder.AsteroidTheme.Standard);
            gen.largeAsteroidPrefab = CreateAsteroidTemplate("LargeAsteroidTemplate", 3.8f, ObstacleType.LargeAsteroid, ProceduralMeshBuilder.AsteroidTheme.Magma);
            gen.spaceDebrisPrefab = CreateDebrisTemplate("SpaceDebrisTemplate");
            gen.movingObstaclePrefab = CreateMovingObstacleTemplate("MovingAsteroidTemplate");
            gen.energyBarrierPrefab = CreateBarrierTemplate("EnergyBarrierTemplate");
            gen.energyStationPrefab = CreateStationTemplate("EnergyStationTemplate");
            gen.coinPrefab = CreateCoinTemplate("CoinTemplate");
            gen.energyCrystalPrefab = CreateCrystalTemplate("EnergyCrystalTemplate");
            gen.shieldPowerUpPrefab = CreateShieldPowerUpTemplate("ShieldPowerUpTemplate");

            // Initial Generation for Level 1
            gen.GenerateLevel(1, 1500f);
        }

        private GameObject CreateAsteroidTemplate(string name, float radius, ObstacleType type, ProceduralMeshBuilder.AsteroidTheme theme = ProceduralMeshBuilder.AsteroidTheme.Standard)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.SetActive(false);

            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = ProceduralMeshBuilder.CreateAsteroidMesh(radius);
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = ProceduralMeshBuilder.CreateAsteroidMaterial(theme);

            SphereCollider col = go.AddComponent<SphereCollider>();
            col.radius = radius * 0.95f;
            col.isTrigger = true;

            Obstacle obs = go.AddComponent<Obstacle>();
            obs.type = type;

            return go;
        }

        private GameObject CreateDebrisTemplate(string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.SetActive(false);

            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = ProceduralMeshBuilder.CreateSpaceDebrisMesh();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = ProceduralMeshBuilder.CreateMaterial("DebrisMat", new Color(0.6f, 0.65f, 0.72f), new Color(0.1f, 0.3f, 0.5f), 0.85f, 0.7f);

            BoxCollider col = go.AddComponent<BoxCollider>();
            col.size = new Vector3(2.5f, 0.3f, 1.2f);
            col.isTrigger = true;

            Obstacle obs = go.AddComponent<Obstacle>();
            obs.type = ObstacleType.SpaceDebris;

            return go;
        }

        private GameObject CreateMovingObstacleTemplate(string name)
        {
            GameObject go = CreateAsteroidTemplate(name, 1.7f, ObstacleType.MediumAsteroid);
            go.AddComponent<MovingObstacle>();
            return go;
        }

        private GameObject CreateBarrierTemplate(string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.SetActive(false);

            // Left and Right Pylons
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject pylon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pylon.name = (side < 0) ? "Pylon_L" : "Pylon_R";
                pylon.transform.SetParent(go.transform, false);
                pylon.transform.localScale = new Vector3(0.5f, 2.5f, 0.5f);
                pylon.transform.localPosition = new Vector3(side * 5f, 0f, 0f);
                pylon.GetComponent<Renderer>().material = ProceduralMeshBuilder.CreateMaterial("PylonMat", new Color(0.2f, 0.25f, 0.3f), Color.red * 0.8f, 0.8f, 0.7f);
                DestroyImmediate(pylon.GetComponent<Collider>());
            }

            // Laser Beam Fence
            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = "LaserBeam";
            beam.transform.SetParent(go.transform, false);
            beam.transform.localScale = new Vector3(10f, 4.5f, 0.2f);
            beam.transform.localPosition = Vector3.zero;
            Material beamMat = ProceduralMeshBuilder.CreateMaterial("BarrierBeamMat", new Color(1f, 0.2f, 0.1f, 0.65f), new Color(1f, 0.1f, 0.1f) * 2.5f, 0.1f, 0.9f, true);
            beam.GetComponent<Renderer>().material = beamMat;
            DestroyImmediate(beam.GetComponent<Collider>());

            BoxCollider col = go.AddComponent<BoxCollider>();
            col.size = new Vector3(10f, 4.5f, 0.6f);
            col.isTrigger = true;

            EnergyBarrier eb = go.AddComponent<EnergyBarrier>();
            eb.barrierRenderer = beam.GetComponent<Renderer>();

            return go;
        }

        private GameObject CreateStationTemplate(string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.SetActive(false);

            // Outer Ring
            GameObject outer = new GameObject("OuterRing");
            outer.transform.SetParent(go.transform, false);
            MeshFilter mfO = outer.AddComponent<MeshFilter>();
            mfO.sharedMesh = ProceduralMeshBuilder.CreateRingMesh(7.5f, 9.2f, 36);
            MeshRenderer mrO = outer.AddComponent<MeshRenderer>();
            mrO.material = ProceduralMeshBuilder.CreateMaterial("StationRingMat", new Color(0.2f, 0.35f, 0.55f), new Color(0f, 0.85f, 1f) * 1.4f, 0.7f, 0.8f);

            // Inner Ring
            GameObject inner = new GameObject("InnerRing");
            inner.transform.SetParent(go.transform, false);
            MeshFilter mfI = inner.AddComponent<MeshFilter>();
            mfI.sharedMesh = ProceduralMeshBuilder.CreateRingMesh(6.2f, 7.2f, 32);
            MeshRenderer mrI = inner.AddComponent<MeshRenderer>();
            mrI.material = ProceduralMeshBuilder.CreateMaterial("StationInnerRingMat", new Color(0.1f, 0.85f, 0.6f), new Color(0f, 1f, 0.7f) * 2f, 0.4f, 0.9f);

            // Trigger Zone
            BoxCollider col = go.AddComponent<BoxCollider>();
            col.size = new Vector3(13f, 13f, 4.0f);
            col.isTrigger = true;

            EnergyStation es = go.AddComponent<EnergyStation>();
            es.outerRing = outer.transform;
            es.innerRing = inner.transform;

            return go;
        }

        private GameObject CreateCoinTemplate(string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.SetActive(false);

            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = ProceduralMeshBuilder.CreateCoinMesh(1.0f, 0.25f, 32);
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = ProceduralMeshBuilder.CreateCoinMaterial();

            // Radiant golden point light glow in space
            GameObject glowObj = new GameObject("CoinGlow");
            glowObj.transform.SetParent(go.transform, false);
            Light lt = glowObj.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.range = 5.0f;
            lt.intensity = 2.2f;
            lt.color = new Color(1f, 0.84f, 0.0f);

            SphereCollider col = go.AddComponent<SphereCollider>();
            col.radius = 1.4f;
            col.isTrigger = true;

            go.AddComponent<Coin>();
            return go;
        }

        private GameObject CreateCrystalTemplate(string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.SetActive(false);

            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = ProceduralMeshBuilder.CreateOctahedronMesh(1.1f);
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = ProceduralMeshBuilder.CreateMaterial("CrystalMat", new Color(0.1f, 1f, 0.85f), new Color(0f, 1f, 0.85f) * 2.8f, 0.3f, 0.95f);

            // Radiant neon emerald glow light in space
            GameObject glowObj = new GameObject("CrystalGlow");
            glowObj.transform.SetParent(go.transform, false);
            Light lt = glowObj.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.range = 6.5f;
            lt.intensity = 2.8f;
            lt.color = new Color(0.1f, 1f, 0.85f);

            SphereCollider col = go.AddComponent<SphereCollider>();
            col.radius = 1.4f;
            col.isTrigger = true;

            go.AddComponent<EnergyCrystal>();
            return go;
        }

        private GameObject CreateShieldPowerUpTemplate(string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.SetActive(false);

            // Orb
            GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "ShieldOrb";
            orb.transform.SetParent(go.transform, false);
            orb.transform.localScale = Vector3.one * 1.1f;
            DestroyImmediate(orb.GetComponent<Collider>());
            orb.GetComponent<Renderer>().material = ProceduralMeshBuilder.CreateMaterial("PowerUpOrbMat", new Color(0.2f, 0.7f, 1f, 0.8f), Color.cyan * 2.5f, 0.2f, 0.9f, true);

            // Orbit Ring
            GameObject ring = new GameObject("OrbitRing");
            ring.transform.SetParent(go.transform, false);
            MeshFilter mf = ring.AddComponent<MeshFilter>();
            mf.sharedMesh = ProceduralMeshBuilder.CreateRingMesh(1.1f, 1.4f, 24);
            MeshRenderer mr = ring.AddComponent<MeshRenderer>();
            mr.material = ProceduralMeshBuilder.CreateMaterial("OrbitRingMat", Color.cyan, Color.cyan * 2f, 0.5f, 0.9f);

            SphereCollider col = go.AddComponent<SphereCollider>();
            col.radius = 1.3f;
            col.isTrigger = true;

            PowerUp pu = go.AddComponent<PowerUp>();
            pu.innerRings = ring.transform;

            return go;
        }

        #endregion

        #region Complete UI System Canvas Construction

        private void EnsureCompleteCanvasUI()
        {
            if (UIManager.Instance != null) return;

            GameObject canvasObj = new GameObject("=== SPACE_JET_CANVAS ===");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            UIManager uiManager = canvasObj.AddComponent<UIManager>();
            UIManager.Instance = uiManager;

            // 1. Build Main Menu Screen
            BuildMainMenuScreen(canvasObj.transform, uiManager);

            // 2. Build Mission Select Screen
            BuildMissionSelectScreen(canvasObj.transform, uiManager);

            // 3. Build Mission Briefing Screen
            BuildMissionBriefingScreen(canvasObj.transform, uiManager);

            // 4. Build Gameplay HUD Screen
            BuildGameplayHUDScreen(canvasObj.transform, uiManager);

            // 5. Build Pause Menu Screen
            BuildPauseMenuScreen(canvasObj.transform, uiManager);

            // 6. Build Mission Result Screen (Victory / Defeat)
            BuildMissionResultScreen(canvasObj.transform, uiManager);

            // 7. Build Jet Garage & Skins Screen
            BuildJetGarageScreen(canvasObj.transform, uiManager);

            // 8. Build Character Selection Screen
            BuildCharacterSelectScreen(canvasObj.transform, uiManager);

            // 9. Build Achievements Screen
            BuildAchievementsScreen(canvasObj.transform, uiManager);

            // 10. Build Settings Screen
            BuildSettingsScreen(canvasObj.transform, uiManager);

            // 11. Build Exhibition Tutorial Screen
            BuildTutorialScreen(canvasObj.transform, uiManager);

            // Ensure only Main Menu is initially visible and all other panels start hidden
            uiManager.HideAllPanels();
            uiManager.ShowPanel(uiManager.mainMenuPanel);
        }

        private void BuildMainMenuScreen(Transform parent, UIManager mgr)
        {
            GameObject panel = UIBuilderHelper.CreatePanel(parent, "MainMenuPanel", new Color(0.01f, 0.02f, 0.05f, 1f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            mgr.mainMenuPanel = panel;
            MainMenuController mmc = panel.AddComponent<MainMenuController>();

            // 1. Full-screen Home UI Background Image using the uploaded graphic
            Sprite homeSprite = UIBuilderHelper.GetHomeUISprite();
            if (homeSprite != null)
            {
                Image bgImg = panel.GetComponent<Image>();
                bgImg.sprite = homeSprite;
                bgImg.color = Color.white;
                bgImg.type = Image.Type.Simple;
                bgImg.raycastTarget = false;
            }

            // 2. Center PLAY Button (Overlayed directly on the glowing golden Play button)
            // Normalized image coordinates: X: ~0.355 to 0.642, Y: ~0.370 to 0.528
            mmc.playButton = UIBuilderHelper.CreateInteractiveOverlayButton(panel.transform, "BtnPlay_Overlay",
                new Vector2(0.355f, 0.370f), new Vector2(0.642f, 0.528f),
                new Color(1f, 0.92f, 0.35f, 0.28f), new Color(1f, 0.8f, 0.1f, 0.5f));

            // 3. Bottom Navigation Bar Buttons (Level Select, Settings, Shop, Exit)
            // Y: 0.158 to 0.314
            // Button 1: Level Select (opens Mission Select UI)
            mmc.missionsButton = UIBuilderHelper.CreateInteractiveOverlayButton(panel.transform, "BtnLevelSelect_Overlay",
                new Vector2(0.288f, 0.158f), new Vector2(0.390f, 0.314f),
                new Color(0.2f, 0.85f, 1f, 0.28f), new Color(0.1f, 0.6f, 1f, 0.5f));

            // Button 2: Settings
            mmc.settingsButton = UIBuilderHelper.CreateInteractiveOverlayButton(panel.transform, "BtnSettings_Overlay",
                new Vector2(0.395f, 0.158f), new Vector2(0.497f, 0.314f),
                new Color(0.2f, 0.85f, 1f, 0.28f), new Color(0.1f, 0.6f, 1f, 0.5f));

            // Button 3: Shop (opens Jet Garage & Skins)
            mmc.garageButton = UIBuilderHelper.CreateInteractiveOverlayButton(panel.transform, "BtnShop_Overlay",
                new Vector2(0.502f, 0.158f), new Vector2(0.604f, 0.314f),
                new Color(0.2f, 0.85f, 1f, 0.28f), new Color(0.1f, 0.6f, 1f, 0.5f));
            mmc.shopButton = mmc.garageButton;

            // Button 4: Exit
            mmc.exitButton = UIBuilderHelper.CreateInteractiveOverlayButton(panel.transform, "BtnExit_Overlay",
                new Vector2(0.608f, 0.158f), new Vector2(0.710f, 0.314f),
                new Color(1f, 0.35f, 0.35f, 0.28f), new Color(0.9f, 0.2f, 0.2f, 0.5f));

            // 4. Left Sidebar Column (Leaderboard & Achievements removed per user request, How to Play kept)
            mmc.leaderboardButton = null;
            mmc.achievementsButton = null;

            mmc.tutorialButton = UIBuilderHelper.CreateInteractiveOverlayButton(panel.transform, "BtnHowToPlay_Overlay",
                new Vector2(0.017f, 0.086f), new Vector2(0.073f, 0.184f),
                new Color(0.2f, 0.85f, 1f, 0.28f), new Color(0.1f, 0.6f, 1f, 0.5f));

            // Top-left setting icon removed per user request (Settings accessed via bottom navigation bar)
            mmc.topSettingsButton = null;

            // 6. Top Right Live Coin Counter (Crisp, perfectly centered in the coin capsule slot)
            mmc.coinsDisplayText = UIBuilderHelper.CreateTextWithBounds(panel.transform, "LiveCoinsText",
                $"{SaveManager.Coins:N0}", 24, TextAnchor.MiddleCenter, Color.white,
                new Vector2(0.670f, 0.916f), new Vector2(0.725f, 0.958f), Vector2.zero, Vector2.zero, FontStyle.Bold);

            // 7. Right Widget Current Level Display (Crisp, perfectly centered in the planet card slot)
            int currentLvl = Mathf.Clamp(SaveManager.UnlockedLevel, 1, 5);
            var currPreset = MissionManager.GetMissionPreset(currentLvl);
            string currPlanet = (currPreset != null && !string.IsNullOrEmpty(currPreset.planetName)) ? currPreset.planetName.Replace("Planet ", "") : "Earth";
            mmc.currentLevelDisplayText = UIBuilderHelper.CreateTextWithBounds(panel.transform, "LiveCurrentLevelText",
                $"{currentLvl}. {currPlanet}", 22, TextAnchor.MiddleCenter, Color.white,
                new Vector2(0.859f, 0.458f), new Vector2(0.960f, 0.524f), Vector2.zero, Vector2.zero, FontStyle.Bold);

            // 8. Active Jet Skin Indicator (Crisp sci-fi badge showing equipped jet skin)
            mmc.activeSkinText = UIBuilderHelper.CreateTextWithBounds(panel.transform, "ActiveSkinBadge",
                "JET: SOLAR VANGUARD", 18, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.3f),
                new Vector2(0.36f, 0.08f), new Vector2(0.64f, 0.14f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            Shadow skinBadgeShadow = mmc.activeSkinText.gameObject.AddComponent<Shadow>();
            skinBadgeShadow.effectColor = new Color(0f, 0f, 0f, 0.95f);
            skinBadgeShadow.effectDistance = new Vector2(2f, -2f);

            // Also support Pilot selection if clicked on the active pilot area or hotkey
            mmc.characterButton = mmc.garageButton;

            if (PlayerController.Instance) mmc.showcaseJet = PlayerController.Instance.transform;
            mmc.BindButtons();
            mmc.RefreshHomeUI();
        }

        private void BuildMissionSelectScreen(Transform parent, UIManager mgr)
        {
            GameObject panel = UIBuilderHelper.CreatePanel(parent, "MissionSelectPanel", new Color(0.02f, 0.04f, 0.08f, 0.97f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            mgr.missionSelectPanel = panel;
            MissionSelectUI msUI = panel.AddComponent<MissionSelectUI>();

            // Title Container - Large, clean, bold, no extra clutter
            GameObject titleObj = UIBuilderHelper.CreateContainer(panel.transform, "Header", new Vector2(0.1f, 0.88f), new Vector2(0.9f, 0.98f), Vector2.zero, Vector2.zero);
            Text titleTxt = UIBuilderHelper.CreateTextWithBounds(titleObj.transform, "Title", "SELECT MISSION", 52, TextAnchor.MiddleCenter, new Color(0f, 0.95f, 1f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, FontStyle.Bold);
            Shadow titleShadow = titleTxt.gameObject.AddComponent<Shadow>();
            titleShadow.effectColor = new Color(0f, 0f, 0f, 0.95f);
            titleShadow.effectDistance = new Vector2(3f, -3f);

            // Cards Row - 5 spacious cards, clean layout, generous padding
            GameObject row = UIBuilderHelper.CreateContainer(panel.transform, "CardsRow", new Vector2(0.04f, 0.16f), new Vector2(0.96f, 0.86f), Vector2.zero, Vector2.zero);
            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 16f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            msUI.cards = new MissionSelectUI.MissionCard[5];

            for (int i = 0; i < 5; i++)
            {
                int lvlIndex = i + 1;
                GameObject cardObj = UIBuilderHelper.CreatePanel(row.transform, $"Card_Lvl{lvlIndex}", new Color(0.06f, 0.12f, 0.24f, 0.96f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                Image cardImg = cardObj.GetComponent<Image>();
                if (cardImg != null)
                {
                    cardImg.sprite = UIBuilderHelper.GetRoundedCornerSprite(24);
                    cardImg.type = Image.Type.Sliced;
                }
                cardImg.raycastTarget = true;
                Button cardSelectBtn = cardObj.AddComponent<Button>();

                Outline ol = cardObj.AddComponent<Outline>();
                ol.effectColor = new Color(0f, 0.9f, 1f, 0.95f);
                ol.effectDistance = new Vector2(3f, 3f);

                VerticalLayoutGroup cvlg = cardObj.AddComponent<VerticalLayoutGroup>();
                cvlg.padding = new RectOffset(14, 14, 24, 20);
                cvlg.spacing = 10f;
                cvlg.childAlignment = TextAnchor.UpperCenter;
                cvlg.childControlWidth = true;
                cvlg.childControlHeight = true;
                cvlg.childForceExpandWidth = true;
                cvlg.childForceExpandHeight = false;

                // 1. Level Number - Large bold golden font size (38pt)
                Text cardTitle = UIBuilderHelper.CreateText(cardObj.transform, "CardTitle", $"LEVEL {lvlIndex}", 38, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.15f), FontStyle.Bold);
                cardTitle.horizontalOverflow = HorizontalWrapMode.Overflow;
                cardTitle.verticalOverflow = VerticalWrapMode.Truncate;
                cardTitle.resizeTextForBestFit = true;
                cardTitle.resizeTextMinSize = 24;
                cardTitle.resizeTextMaxSize = 38;
                cardTitle.GetComponent<LayoutElement>().preferredHeight = 48;
                Shadow tShadow = cardTitle.gameObject.AddComponent<Shadow>();
                tShadow.effectColor = new Color(0f, 0f, 0f, 0.95f);
                tShadow.effectDistance = new Vector2(2f, -2f);

                // 2. Planet Destination - Prominent crisp white font size (30pt)
                var preset = MissionManager.GetMissionPreset(lvlIndex);
                string planetName = (preset != null && !string.IsNullOrEmpty(preset.planetName)) ? preset.planetName.Replace("Planet ", "").ToUpper() : $"ZONE {lvlIndex}";
                Text dest = UIBuilderHelper.CreateText(cardObj.transform, "CardDest", planetName, 30, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
                dest.horizontalOverflow = HorizontalWrapMode.Overflow;
                dest.verticalOverflow = VerticalWrapMode.Truncate;
                dest.resizeTextForBestFit = true;
                dest.resizeTextMinSize = 18;
                dest.resizeTextMaxSize = 30;
                dest.GetComponent<LayoutElement>().preferredHeight = 40;
                Shadow dShadow = dest.gameObject.AddComponent<Shadow>();
                dShadow.effectColor = new Color(0f, 0f, 0f, 0.95f);
                dShadow.effectDistance = new Vector2(2f, -2f);

                // 3. Clear Coin Reward - Large glowing yellow font size (26pt)
                int coinReward = (preset != null) ? preset.coinReward : 500;
                Text reward = UIBuilderHelper.CreateText(cardObj.transform, "CardReward", $"★ {coinReward} COINS", 26, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.25f), FontStyle.Bold);
                reward.horizontalOverflow = HorizontalWrapMode.Overflow;
                reward.verticalOverflow = VerticalWrapMode.Truncate;
                reward.resizeTextForBestFit = true;
                reward.resizeTextMinSize = 16;
                reward.resizeTextMaxSize = 26;
                reward.GetComponent<LayoutElement>().preferredHeight = 36;
                Shadow rShadow = reward.gameObject.AddComponent<Shadow>();
                rShadow.effectColor = new Color(0f, 0f, 0f, 0.95f);
                rShadow.effectDistance = new Vector2(2f, -2f);

                // 4. Flexible Spacer to push button neatly to bottom
                GameObject spacer = new GameObject("Spacer");
                spacer.transform.SetParent(cardObj.transform, false);
                spacer.AddComponent<LayoutElement>().flexibleHeight = 1f;

                // 5. Action Button with Home Play Button Texture - Large font size (28pt)
                Button launchBtn = UIBuilderHelper.CreateGoldPlayButton(cardObj.transform, "BtnLaunch", "PLAY", new Vector2(185, 58), 28);
                launchBtn.GetComponent<LayoutElement>().preferredHeight = 58;
                Text btnLabel = launchBtn.GetComponentInChildren<Text>();

                msUI.cards[i] = new MissionSelectUI.MissionCard
                {
                    levelIndex = lvlIndex,
                    cardButton = cardSelectBtn,
                    launchButton = launchBtn,
                    cardBg = cardImg,
                    cardOutline = ol,
                    badgeText = null,
                    titleText = cardTitle,
                    destinationText = dest,
                    detailsText = null,
                    rewardText = reward,
                    statusText = null,
                    buttonLabel = btnLabel
                };
            }

            // Back button with Home Play Button Texture - Clearly visible, spacious, no overlap
            msUI.backButton = UIBuilderHelper.CreateGoldPlayButton(panel.transform, "BtnBack", "← BACK TO MENU", new Vector2(300, 58), 24);
            RectTransform bbr = msUI.backButton.GetComponent<RectTransform>();
            bbr.anchorMin = new Vector2(0.5f, 0.07f);
            bbr.anchorMax = new Vector2(0.5f, 0.07f);
            msUI.BindButtons();
            msUI.RefreshCards();
        }

        private void BuildMissionBriefingScreen(Transform parent, UIManager mgr)
        {
            GameObject panel = UIBuilderHelper.CreatePanel(parent, "MissionBriefingPanel", new Color(0.02f, 0.04f, 0.08f, 0.92f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            mgr.briefingPanel = panel;
            MissionBriefingUI bUI = panel.AddComponent<MissionBriefingUI>();
            mgr.briefingUI = bUI;

            // Briefing Card
            GameObject card = UIBuilderHelper.CreatePanel(panel.transform, "BriefingCard", new Color(0.06f, 0.12f, 0.24f, 0.96f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-420, -290), new Vector2(420, 290));
            Image cardBg = card.GetComponent<Image>();
            if (cardBg != null)
            {
                cardBg.sprite = UIBuilderHelper.GetRoundedCornerSprite(24);
                cardBg.type = Image.Type.Sliced;
            }
            Outline ol = card.AddComponent<Outline>();
            ol.effectColor = new Color(0f, 0.9f, 1f, 0.95f);
            ol.effectDistance = new Vector2(3f, 3f);

            VerticalLayoutGroup vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(30, 30, 26, 24);
            vlg.spacing = 16f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // Title & Subtitle - Increased font sizes
            bUI.missionTitleText = UIBuilderHelper.CreateText(card.transform, "Title", "MISSION 01: FIRST FLIGHT", 38, TextAnchor.MiddleCenter, new Color(0f, 0.95f, 1f), FontStyle.Bold);
            bUI.subtitleText = UIBuilderHelper.CreateText(card.transform, "Subtitle", "\"Escape the Orbit\"", 22, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.3f), FontStyle.Italic);

            // Target & Distance Row
            GameObject tRow = new GameObject("TargetRow");
            tRow.transform.SetParent(card.transform, false);
            HorizontalLayoutGroup thlg = tRow.AddComponent<HorizontalLayoutGroup>();
            thlg.childControlWidth = true;
            thlg.childControlHeight = false;
            thlg.spacing = 20f;

            bUI.destinationText = UIBuilderHelper.CreateText(tRow.transform, "Target", "TARGET: PLANET NOVA", 28, TextAnchor.MiddleLeft, Color.white, FontStyle.Bold);
            bUI.distanceText = UIBuilderHelper.CreateText(tRow.transform, "Distance", "DISTANCE: 1.5 KM", 26, TextAnchor.MiddleRight, Color.white, FontStyle.Bold);

            // Simple arcade objectives
            bUI.objectivesListText = UIBuilderHelper.CreateText(card.transform, "Objectives", "• Dodge incoming asteroids\n• Collect gold coins\n• Reach target planet", 24, TextAnchor.MiddleLeft, new Color(0.88f, 0.95f, 1f), FontStyle.Normal);

            // Reward
            bUI.rewardText = UIBuilderHelper.CreateText(card.transform, "Reward", "COMPLETION REWARD: +500 COINS", 28, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.15f), FontStyle.Bold);

            // Spacer
            GameObject spacer = new GameObject("Spacer");
            spacer.transform.SetParent(card.transform, false);
            spacer.AddComponent<LayoutElement>().flexibleHeight = 1f;

            // Buttons Row
            GameObject btnRow = new GameObject("BtnRow");
            btnRow.transform.SetParent(card.transform, false);
            HorizontalLayoutGroup bhlg = btnRow.AddComponent<HorizontalLayoutGroup>();
            bhlg.childAlignment = TextAnchor.MiddleCenter;
            bhlg.childControlWidth = false;
            bhlg.childControlHeight = false;
            bhlg.spacing = 24f;

            bUI.startMissionButton = UIBuilderHelper.CreateGoldPlayButton(btnRow.transform, "BtnStart", "START MISSION", new Vector2(280, 60), 24);
            bUI.backButton = UIBuilderHelper.CreateSciFiButton(btnRow.transform, "BtnCancel", "CANCEL", new Vector2(190, 60), new Color(0.95f, 0.25f, 0.25f), Color.white, 22);

            bUI.BindButtons();
        }

        private void BuildGameplayHUDScreen(Transform parent, UIManager mgr)
        {
            GameObject hud = new GameObject("GameplayHUDPanel");
            hud.transform.SetParent(parent, false);
            RectTransform hr = hud.AddComponent<RectTransform>();
            hr.anchorMin = Vector2.zero;
            hr.anchorMax = Vector2.one;
            hr.offsetMin = Vector2.zero;
            hr.offsetMax = Vector2.zero;

            mgr.hudPanel = hud;
            HUDController hudCtrl = hud.AddComponent<HUDController>();
            mgr.hudController = hudCtrl;

            // Top Bar: Clean, sleek, minimalist status (Height: 52px at top)
            GameObject topBar = UIBuilderHelper.CreateContainer(hud.transform, "TopBar", new Vector2(0.02f, 0.90f), new Vector2(0.98f, 0.985f), Vector2.zero, Vector2.zero);

            // Left Zone: Mission Tag
            hudCtrl.missionNameText = UIBuilderHelper.CreateTextWithBounds(topBar.transform, "MissionLabel", "M-01", 20, TextAnchor.MiddleLeft, Color.cyan, Vector2.zero, new Vector2(0.15f, 1f), Vector2.zero, Vector2.zero, FontStyle.Bold);

            // Center Zone: Slim Distance Progress Bar
            GameObject distContainer = UIBuilderHelper.CreateContainer(topBar.transform, "DistanceTracker", new Vector2(0.18f, 0f), new Vector2(0.70f, 1f), Vector2.zero, Vector2.zero);
            hudCtrl.distanceText = UIBuilderHelper.CreateTextWithBounds(distContainer.transform, "DistText", "0.0 / 1.5 KM", 17, TextAnchor.UpperCenter, new Color(0.85f, 0.95f, 1f), new Vector2(0f, 0.45f), Vector2.one, Vector2.zero, Vector2.zero, FontStyle.Bold);
            var (dFill, _) = UIBuilderHelper.CreateStatBar(distContainer.transform, "DistBar", "", new Color(0.15f, 0.85f, 1f), new Vector2(360, 10));
            RectTransform dfRect = dFill.transform.parent.GetComponent<RectTransform>();
            dfRect.anchorMin = new Vector2(0.5f, 0.22f);
            dfRect.anchorMax = new Vector2(0.5f, 0.22f);
            hudCtrl.distanceFill = dFill;

            // Right Zone: Clean Gold Coin Counter
            hudCtrl.coinsText = UIBuilderHelper.CreateTextWithBounds(topBar.transform, "CoinsLabel", "COINS: 0", 22, TextAnchor.MiddleRight, new Color(1f, 0.85f, 0.12f), new Vector2(0.72f, 0f), Vector2.one, Vector2.zero, Vector2.zero, FontStyle.Bold);

            // Bottom-Left Cockpit Telemetry (Unified, high-visibility cockpit gauges)
            GameObject botBar = UIBuilderHelper.CreatePanel(hud.transform, "BottomStatusBar", new Color(0.015f, 0.035f, 0.08f, 0.94f), new Vector2(0.02f, 0.02f), new Vector2(0.02f, 0.02f), Vector2.zero, new Vector2(370, 148));
            RectTransform botBarRect = botBar.GetComponent<RectTransform>();
            botBarRect.pivot = new Vector2(0f, 0f);

            // Glowing cockpit telemetry frame
            Outline botOl = botBar.AddComponent<Outline>();
            botOl.effectColor = new Color(0f, 0.85f, 1f, 0.90f);
            botOl.effectDistance = new Vector2(2f, 2f);

            Shadow botShadow = botBar.AddComponent<Shadow>();
            botShadow.effectColor = new Color(0f, 0f, 0f, 0.95f);
            botShadow.effectDistance = new Vector2(3f, -3f);

            // Telemetry Header
            GameObject headerObj = new GameObject("TelemetryHeader");
            headerObj.transform.SetParent(botBar.transform, false);
            RectTransform hdrRect = headerObj.AddComponent<RectTransform>();
            hdrRect.anchorMin = new Vector2(0f, 1f);
            hdrRect.anchorMax = new Vector2(1f, 1f);
            hdrRect.pivot = new Vector2(0.5f, 1f);
            hdrRect.sizeDelta = new Vector2(-24f, 20f);
            hdrRect.anchoredPosition = new Vector2(0f, -6f);

            Text hdrText = headerObj.AddComponent<Text>();
            hdrText.font = UIBuilderHelper.GetDefaultFont();
            hdrText.text = "◆  COCKPIT TELEMETRY";
            hdrText.fontSize = 11;
            hdrText.fontStyle = FontStyle.Bold;
            hdrText.alignment = TextAnchor.MiddleLeft;
            hdrText.color = new Color(0.40f, 0.85f, 1f, 0.95f);
            hdrText.raycastTarget = false;

            // 1. Hull Hit Gauge (Size: 346x32, 3-hit segmented design, bright neon emerald green)
            var (hFill, hTxt) = UIBuilderHelper.CreateCockpitGauge(
                botBar.transform,
                "HullBar",
                "HULL",
                "HULL  <b>3/3 HITS</b>",
                new Color(0f, 1f, 0.55f),
                new Vector2(346, 32),
                new Vector2(0, 102),
                fontSize: 15,
                hitSegments: 3);
            hudCtrl.hullFill = hFill;
            hudCtrl.hullText = hTxt;

            // 2. Energy Gauge (Size: 346x32, bright plasma electric cyan)
            var (eFill, eTxt) = UIBuilderHelper.CreateCockpitGauge(
                botBar.transform,
                "EnergyBar",
                "ENERGY",
                "ENERGY  <b>100%</b>",
                new Color(0f, 0.90f, 1f),
                new Vector2(346, 32),
                new Vector2(0, 62),
                fontSize: 15,
                hitSegments: 0);
            hudCtrl.energyFill = eFill;
            hudCtrl.energyText = eTxt;

            // 3. Boost Gauge (Size: 346x22, integrated directly into cockpit console)
            var (bFill, bTxt) = UIBuilderHelper.CreateCockpitGauge(
                botBar.transform,
                "BoostBar",
                "BOOST",
                "BOOST  <b>100%</b>",
                new Color(0.2f, 0.75f, 1f),
                new Vector2(346, 22),
                new Vector2(0, 22),
                fontSize: 13,
                hitSegments: 0);
            hudCtrl.boostFill = bFill;
            hudCtrl.boostText = bTxt;

            // Shield Timer Indicator
            GameObject shieldObj = UIBuilderHelper.CreatePanel(hud.transform, "ShieldIndicator", new Color(0.1f, 0.3f, 0.6f, 0.85f), new Vector2(0.5f, 0.10f), new Vector2(0.5f, 0.10f), new Vector2(-110, -18), new Vector2(110, 18));
            Outline sol = shieldObj.AddComponent<Outline>();
            sol.effectColor = Color.cyan;
            hudCtrl.shieldContainer = shieldObj;
            hudCtrl.shieldTimerText = UIBuilderHelper.CreateText(shieldObj.transform, "ShieldText", "SHIELD 5s", 18, TextAnchor.MiddleCenter, Color.cyan, FontStyle.Bold);
            shieldObj.SetActive(false);

            // Subtle Warning Pill
            GameObject warnObj = UIBuilderHelper.CreatePanel(hud.transform, "WarningBanner", new Color(0.6f, 0.05f, 0.05f, 0.85f), new Vector2(0.5f, 0.86f), new Vector2(0.5f, 0.86f), new Vector2(-180, -20), new Vector2(180, 20));
            Outline wol = warnObj.AddComponent<Outline>();
            wol.effectColor = Color.red;
            hudCtrl.warningBanner = warnObj;
            hudCtrl.warningText = UIBuilderHelper.CreateText(warnObj.transform, "WarnText", "LOW FUEL", 18, TextAnchor.MiddleCenter, Color.yellow, FontStyle.Bold);
            warnObj.SetActive(false);

            // Objective Panel (Kept instantiated for reference but INACTIVE to avoid screen clutter!)
            GameObject objPanel = UIBuilderHelper.CreatePanel(hud.transform, "ObjectivePanel", new Color(0.04f, 0.08f, 0.16f, 0.85f), new Vector2(0.02f, 0.55f), new Vector2(0.02f, 0.55f), new Vector2(0, -90), new Vector2(290, 90));
            hudCtrl.objectivesText = UIBuilderHelper.CreateTextWithBounds(objPanel.transform, "ObjectivesList", "", 16, TextAnchor.MiddleLeft, Color.white, Vector2.zero, Vector2.one, new Vector2(12, 8), new Vector2(-12, -8));
            objPanel.SetActive(false);
        }

        private void BuildPauseMenuScreen(Transform parent, UIManager mgr)
        {
            GameObject panel = UIBuilderHelper.CreatePanel(parent, "PauseMenuPanel", new Color(0.02f, 0.03f, 0.06f, 0.88f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            mgr.pauseMenuPanel = panel;

            // Pause Box
            GameObject box = UIBuilderHelper.CreatePanel(panel.transform, "PauseBox", new Color(0.06f, 0.1f, 0.18f, 0.95f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-240, -240), new Vector2(240, 240));
            Image boxBg = box.GetComponent<Image>();
            if (boxBg != null)
            {
                boxBg.sprite = UIBuilderHelper.GetRoundedCornerSprite(24);
                boxBg.type = Image.Type.Sliced;
            }
            Outline ol = box.AddComponent<Outline>();
            ol.effectColor = Color.cyan;

            UIBuilderHelper.CreateText(box.transform, "PauseTitle", "GAME PAUSED", 32, TextAnchor.UpperCenter, Color.cyan, FontStyle.Bold);

            // Buttons
            GameObject col = new GameObject("PauseButtons");
            col.transform.SetParent(box.transform, false);
            RectTransform cr = col.AddComponent<RectTransform>();
            cr.anchorMin = new Vector2(0.1f, 0.06f);
            cr.anchorMax = new Vector2(0.9f, 0.82f);
            cr.offsetMin = Vector2.zero;
            cr.offsetMax = Vector2.zero;

            VerticalLayoutGroup vlg = col.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14f;
            vlg.childAlignment = TextAnchor.MiddleCenter;

            Vector2 bSize = new Vector2(280, 52);
            Color bBlue = new Color(0f, 0.75f, 1f);

            mgr.resumeButton = UIBuilderHelper.CreateGoldPlayButton(col.transform, "BtnResume", "RESUME", bSize, 22);
            mgr.restartButton = UIBuilderHelper.CreateSciFiButton(col.transform, "BtnRestart", "RESTART MISSION", bSize, bBlue, Color.white, 20);
            mgr.pauseSettingsButton = UIBuilderHelper.CreateSciFiButton(col.transform, "BtnPauseSettings", "SETTINGS", bSize, bBlue, Color.white, 20);
            mgr.pauseMainMenuButton = UIBuilderHelper.CreateSciFiButton(col.transform, "BtnPauseMenu", "MAIN MENU", bSize, new Color(0.95f, 0.25f, 0.25f), Color.white, 20);
            mgr.SetupPauseButtons();

            panel.SetActive(false);
        }

        private void BuildMissionResultScreen(Transform parent, UIManager mgr)
        {
            GameObject panel = UIBuilderHelper.CreatePanel(parent, "MissionResultPanel", new Color(0.02f, 0.03f, 0.08f, 0.92f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            mgr.resultPanel = panel;
            MissionResultUI resUI = panel.AddComponent<MissionResultUI>();
            mgr.resultUI = resUI;

            // Victory Sub-Panel
            GameObject vicPanel = UIBuilderHelper.CreatePanel(panel.transform, "VictoryPanel", new Color(0.05f, 0.12f, 0.22f, 0.96f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-420, -310), new Vector2(420, 310));
            Image vicBg = vicPanel.GetComponent<Image>();
            if (vicBg != null)
            {
                vicBg.sprite = UIBuilderHelper.GetRoundedCornerSprite(24);
                vicBg.type = Image.Type.Sliced;
            }
            Outline vol = vicPanel.AddComponent<Outline>();
            vol.effectColor = new Color(0f, 1f, 0.7f);
            resUI.completePanel = vicPanel;

            resUI.completeTitleText = UIBuilderHelper.CreateTextWithBounds(vicPanel.transform, "VicTitle", "MISSION COMPLETE!", 38, TextAnchor.MiddleCenter, new Color(0f, 1f, 0.7f), new Vector2(0.05f, 0.85f), new Vector2(0.95f, 0.96f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            resUI.planetReachedText = UIBuilderHelper.CreateTextWithBounds(vicPanel.transform, "PlanetReached", "PLANET REACHED: PLANET NOVA", 22, TextAnchor.MiddleCenter, Color.white, new Vector2(0.05f, 0.73f), new Vector2(0.95f, 0.83f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            resUI.victoryStatsText = UIBuilderHelper.CreateTextWithBounds(vicPanel.transform, "Stats", "Coins Collected: 0\nDistance: 1.5 KM", 20, TextAnchor.MiddleCenter, Color.white, new Vector2(0.08f, 0.35f), new Vector2(0.92f, 0.70f), Vector2.zero, Vector2.zero);
            resUI.rewardText = UIBuilderHelper.CreateTextWithBounds(vicPanel.transform, "Reward", "REWARD: +500 COINS", 26, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.1f), new Vector2(0.05f, 0.20f), new Vector2(0.95f, 0.32f), Vector2.zero, Vector2.zero, FontStyle.Bold);

            // Victory Buttons
            GameObject vBtns = new GameObject("VicBtns");
            vBtns.transform.SetParent(vicPanel.transform, false);
            RectTransform vbr = vBtns.AddComponent<RectTransform>();
            vbr.anchorMin = new Vector2(0.05f, 0.05f);
            vbr.anchorMax = new Vector2(0.95f, 0.18f);
            vbr.offsetMin = Vector2.zero;
            vbr.offsetMax = Vector2.zero;
            HorizontalLayoutGroup vhlg = vBtns.AddComponent<HorizontalLayoutGroup>();
            vhlg.spacing = 16f;
            vhlg.childAlignment = TextAnchor.MiddleCenter;

            resUI.nextLevelButton = UIBuilderHelper.CreateGoldPlayButton(vBtns.transform, "BtnNext", "NEXT LEVEL", new Vector2(230, 52), 20);
            resUI.replayButton = UIBuilderHelper.CreateSciFiButton(vBtns.transform, "BtnReplay", "REPLAY", new Vector2(180, 52), new Color(0f, 0.75f, 1f), Color.white, 20);
            resUI.victoryMenuButton = UIBuilderHelper.CreateSciFiButton(vBtns.transform, "BtnMenu", "MAIN MENU", new Vector2(180, 52), new Color(0.95f, 0.25f, 0.25f), Color.white, 20);

            // Defeat Sub-Panel
            GameObject defPanel = UIBuilderHelper.CreatePanel(panel.transform, "DefeatPanel", new Color(0.16f, 0.05f, 0.06f, 0.96f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-400, -290), new Vector2(400, 290));
            Image defBg = defPanel.GetComponent<Image>();
            if (defBg != null)
            {
                defBg.sprite = UIBuilderHelper.GetRoundedCornerSprite(24);
                defBg.type = Image.Type.Sliced;
            }
            Outline dol = defPanel.AddComponent<Outline>();
            dol.effectColor = Color.red;
            resUI.failedPanel = defPanel;

            UIBuilderHelper.CreateTextWithBounds(defPanel.transform, "DefTitle", "MISSION FAILED", 38, TextAnchor.MiddleCenter, Color.red, new Vector2(0.05f, 0.83f), new Vector2(0.95f, 0.96f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            resUI.failureReasonText = UIBuilderHelper.CreateTextWithBounds(defPanel.transform, "FailReason", "REASON: OUT OF ENERGY", 22, TextAnchor.MiddleCenter, Color.yellow, new Vector2(0.05f, 0.70f), new Vector2(0.95f, 0.81f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            resUI.failedStatsText = UIBuilderHelper.CreateTextWithBounds(defPanel.transform, "FailStats", "Distance: 0.8 KM\nCoins: 12", 20, TextAnchor.MiddleCenter, Color.white, new Vector2(0.08f, 0.32f), new Vector2(0.92f, 0.66f), Vector2.zero, Vector2.zero);

            // Defeat Buttons
            GameObject dBtns = new GameObject("DefBtns");
            dBtns.transform.SetParent(defPanel.transform, false);
            RectTransform dbr = dBtns.AddComponent<RectTransform>();
            dbr.anchorMin = new Vector2(0.05f, 0.06f);
            dbr.anchorMax = new Vector2(0.95f, 0.22f);
            dbr.offsetMin = Vector2.zero;
            dbr.offsetMax = Vector2.zero;
            HorizontalLayoutGroup dhlg = dBtns.AddComponent<HorizontalLayoutGroup>();
            dhlg.spacing = 20f;
            dhlg.childAlignment = TextAnchor.MiddleCenter;

            resUI.retryButton = UIBuilderHelper.CreateGoldPlayButton(dBtns.transform, "BtnRetry", "RETRY MISSION", new Vector2(250, 52), 20);
            resUI.failedMenuButton = UIBuilderHelper.CreateSciFiButton(dBtns.transform, "BtnMenu", "MAIN MENU", new Vector2(210, 52), new Color(0.95f, 0.25f, 0.25f), Color.white, 20);
            resUI.BindButtons();

            panel.SetActive(false);
        }

        private void BuildJetGarageScreen(Transform parent, UIManager mgr)
        {
            GameObject panel = UIBuilderHelper.CreatePanel(parent, "GaragePanel", new Color(0.02f, 0.04f, 0.09f, 0.96f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            mgr.garagePanel = panel;
            mgr.jetSkinPanel = panel;
            GarageUI gUI = panel.AddComponent<GarageUI>();
            JetSkinUI skinUI = panel.AddComponent<JetSkinUI>();

            // 1. Title & Balance Header (X: 0.03 to 0.97, Y: 0.90 to 0.99)
            GameObject header = UIBuilderHelper.CreateContainer(panel.transform, "Header", new Vector2(0.03f, 0.90f), new Vector2(0.97f, 0.99f), Vector2.zero, Vector2.zero);
            UIBuilderHelper.CreateTextWithBounds(header.transform, "Title", "JET HANGAR & SKINS SHOP", 40, TextAnchor.MiddleLeft, Color.cyan, Vector2.zero, new Vector2(0.55f, 1f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            gUI.coinsBalanceText = UIBuilderHelper.CreateTextWithBounds(header.transform, "Balance", "COINS: 0000", 28, TextAnchor.MiddleRight, new Color(1f, 0.85f, 0.1f), new Vector2(0.55f, 0f), Vector2.one, Vector2.zero, Vector2.zero, FontStyle.Bold);
            skinUI.creditsText = gUI.coinsBalanceText;

            // 2. Navigation Tab Buttons (X: 0.03 to 0.97, Y: 0.83 to 0.89)
            GameObject tabBar = UIBuilderHelper.CreateContainer(panel.transform, "TabBar", new Vector2(0.03f, 0.83f), new Vector2(0.97f, 0.89f), Vector2.zero, Vector2.zero);
            HorizontalLayoutGroup tabHlg = tabBar.AddComponent<HorizontalLayoutGroup>();
            tabHlg.spacing = 16f;
            tabHlg.childAlignment = TextAnchor.MiddleLeft;
            tabHlg.childControlWidth = false;
            tabHlg.childControlHeight = true;

            skinUI.tabSkinsButton = UIBuilderHelper.CreateSciFiButton(tabBar.transform, "TabSkins", "✈ JET PAINTWORKS (5 SKINS)", new Vector2(340, 44), new Color(0f, 0.75f, 1f, 1f), Color.white, 20);
            skinUI.tabUpgradesButton = UIBuilderHelper.CreateSciFiButton(tabBar.transform, "TabUpgrades", "⚡ HANGAR UPGRADES", new Vector2(280, 44), new Color(0.32f, 0.48f, 0.68f, 0.95f), Color.white, 20);

            // 3. Skins Container (5 Side-by-Side Cards displaying each jet with applied color)
            GameObject skinsBox = UIBuilderHelper.CreateContainer(panel.transform, "SkinsContainer", new Vector2(0.02f, 0.11f), new Vector2(0.98f, 0.82f), Vector2.zero, Vector2.zero);
            skinUI.skinsContainer = skinsBox;

            HorizontalLayoutGroup skinHlg = skinsBox.AddComponent<HorizontalLayoutGroup>();
            skinHlg.spacing = 12f;
            skinHlg.padding = new RectOffset(6, 6, 6, 6);
            skinHlg.childAlignment = TextAnchor.MiddleCenter;
            skinHlg.childControlWidth = true;
            skinHlg.childControlHeight = true;
            skinHlg.childForceExpandWidth = true;
            skinHlg.childForceExpandHeight = true;

            skinUI.skinCards = new JetSkinUI.JetSkinCardUI[JetSkinManager.Skins.Length];
            for (int i = 0; i < JetSkinManager.Skins.Length; i++)
            {
                var skinDef = JetSkinManager.Skins[i];
                skinUI.skinCards[i] = BuildSkinCard(skinsBox.transform, skinDef);
            }

            // 4. Upgrades Container (4 Performance Cards)
            GameObject upBox = UIBuilderHelper.CreateContainer(panel.transform, "UpgradesContainer", new Vector2(0.16f, 0.11f), new Vector2(0.84f, 0.82f), Vector2.zero, Vector2.zero);
            skinUI.upgradesContainer = upBox;
            upBox.SetActive(false); // Initially show skins tab

            VerticalLayoutGroup vlg = upBox.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14f;
            vlg.padding = new RectOffset(6, 6, 6, 6);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            // 1. Engine
            BuildGarageCard(upBox.transform, "Engine Upgrade (Top Speed & Boost)", out gUI.engineLevelText, out gUI.engineBarFill, out gUI.engineCostText, out gUI.engineUpgradeBtn);
            // 2. Energy Tank
            BuildGarageCard(upBox.transform, "Energy Tank Upgrade (Fuel Capacity)", out gUI.energyLevelText, out gUI.energyBarFill, out gUI.energyCostText, out gUI.energyUpgradeBtn);
            // 3. Shield Generator
            BuildGarageCard(upBox.transform, "Shield Generator Upgrade (Duration)", out gUI.shieldLevelText, out gUI.shieldBarFill, out gUI.shieldCostText, out gUI.shieldUpgradeBtn);
            // 4. Hull Armor
            BuildGarageCard(upBox.transform, "Hull Armor Upgrade (Max HP)", out gUI.hullLevelText, out gUI.hullBarFill, out gUI.hullCostText, out gUI.hullUpgradeBtn);

            // Bottom Return Button (Centered, Y: 0.025 to 0.085)
            gUI.backButton = UIBuilderHelper.CreateSciFiButton(panel.transform, "BtnBack", "← BACK TO HOME", new Vector2(320, 54), new Color(0.95f, 0.25f, 0.25f), Color.white, 22);
            RectTransform bbr = gUI.backButton.GetComponent<RectTransform>();
            bbr.anchorMin = new Vector2(0.5f, 0.055f);
            bbr.anchorMax = new Vector2(0.5f, 0.055f);
            bbr.anchoredPosition = Vector2.zero;
            skinUI.backButton = gUI.backButton;

            gUI.BindButtons();
            skinUI.BindButtons();

            panel.SetActive(false);
        }

        private JetSkinUI.JetSkinCardUI BuildSkinCard(Transform parent, JetSkinDefinition skinDef)
        {
            GameObject card = UIBuilderHelper.CreatePanel(parent, $"SkinCard_{skinDef.id}", new Color(0.04f, 0.08f, 0.16f, 0.96f), Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            Image cardBg = card.GetComponent<Image>();
            if (cardBg != null)
            {
                cardBg.sprite = UIBuilderHelper.GetRoundedCornerSprite(24);
                cardBg.type = Image.Type.Sliced;
            }
            Outline cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = new Color(0.1f, 0.55f, 0.85f, 0.7f);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.flexibleWidth = 1f;
            le.flexibleHeight = 1f;

            // 1. Top Preview Image Container (Y: 0.44 to 0.98, X: 0.04 to 0.96)
            GameObject imgBox = UIBuilderHelper.CreatePanel(card.transform, "JetPreviewBox", new Color(0.02f, 0.05f, 0.11f, 0.95f), new Vector2(0.04f, 0.44f), new Vector2(0.96f, 0.98f), Vector2.zero, Vector2.zero);
            Image imgBoxBg = imgBox.GetComponent<Image>();
            if (imgBoxBg != null)
            {
                imgBoxBg.sprite = UIBuilderHelper.GetRoundedCornerSprite(16);
                imgBoxBg.type = Image.Type.Sliced;
            }
            Outline ibol = imgBox.AddComponent<Outline>();
            ibol.effectColor = skinDef.accentColor * 0.75f;

            // RawImage centered inside imgBox with AspectRatioFitter (1:1 square)
            GameObject rawObj = new GameObject("JetPreviewImg");
            rawObj.transform.SetParent(imgBox.transform, false);
            RectTransform rawRt = rawObj.AddComponent<RectTransform>();
            rawRt.anchorMin = Vector2.zero;
            rawRt.anchorMax = Vector2.one;
            rawRt.offsetMin = Vector2.zero;
            rawRt.offsetMax = Vector2.zero;
            RawImage rawImg = rawObj.AddComponent<RawImage>();
            rawImg.texture = ProceduralMeshBuilder.GenerateJetSkinCardTexture(skinDef, 256);
            AspectRatioFitter arf = rawObj.AddComponent<AspectRatioFitter>();
            arf.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            arf.aspectRatio = 1f;

            // 2. Skin Name (Y: 0.36 to 0.43, X: 0.03 to 0.97)
            Text nameTxt = UIBuilderHelper.CreateTextWithBounds(card.transform, "SkinName", skinDef.name.ToUpper(), 21, TextAnchor.MiddleCenter, Color.white, new Vector2(0.03f, 0.36f), new Vector2(0.97f, 0.43f), Vector2.zero, Vector2.zero, FontStyle.Bold);

            // 3. Class / Color Title (Y: 0.30 to 0.36, X: 0.03 to 0.97)
            Text classTxt = UIBuilderHelper.CreateTextWithBounds(card.transform, "SkinClass", skinDef.classTitle, 15, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.2f), new Vector2(0.03f, 0.30f), new Vector2(0.97f, 0.36f), Vector2.zero, Vector2.zero, FontStyle.Bold);

            // 4. Dual Color Swatches Row (Y: 0.23 to 0.29, X: 0.05 to 0.95)
            GameObject swatchRow = UIBuilderHelper.CreateContainer(card.transform, "SwatchRow", new Vector2(0.05f, 0.23f), new Vector2(0.95f, 0.29f), Vector2.zero, Vector2.zero);
            
            // Hull Swatch (Left half: 0% to 48%)
            GameObject hullObj = UIBuilderHelper.CreatePanel(swatchRow.transform, "HullSwatch", skinDef.hullColor, new Vector2(0f, 0f), new Vector2(0.48f, 1f), Vector2.zero, Vector2.zero);
            Outline hol = hullObj.AddComponent<Outline>();
            hol.effectColor = Color.black;
            UIBuilderHelper.CreateTextWithBounds(hullObj.transform, "HullLbl", "HULL", 11, TextAnchor.MiddleCenter, Color.black, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, FontStyle.Bold);

            // Accent Swatch (Right half: 52% to 100%)
            GameObject accObj = UIBuilderHelper.CreatePanel(swatchRow.transform, "AccentSwatch", skinDef.accentColor, new Vector2(0.52f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            Outline aol = accObj.AddComponent<Outline>();
            aol.effectColor = Color.black;
            UIBuilderHelper.CreateTextWithBounds(accObj.transform, "AccLbl", "ACCENT", 11, TextAnchor.MiddleCenter, Color.black, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, FontStyle.Bold);

            // 5. Description Text (Y: 0.12 to 0.22, X: 0.04 to 0.96)
            Text descTxt = UIBuilderHelper.CreateTextWithBounds(card.transform, "SkinDesc", skinDef.description, 12, TextAnchor.MiddleCenter, new Color(0.80f, 0.90f, 0.98f), new Vector2(0.04f, 0.12f), new Vector2(0.96f, 0.22f), Vector2.zero, Vector2.zero, FontStyle.Normal);

            // 6. Equip Button (Y: 0.02 to 0.11, X: 0.05 to 0.95)
            Button eqBtn = UIBuilderHelper.CreateSciFiButton(card.transform, "BtnEquip", "EQUIP JET", new Vector2(160, 46), new Color(0f, 0.75f, 1f), Color.white, 19);
            RectTransform ebr = eqBtn.GetComponent<RectTransform>();
            ebr.anchorMin = new Vector2(0.05f, 0.02f);
            ebr.anchorMax = new Vector2(0.95f, 0.11f);
            ebr.offsetMin = Vector2.zero;
            ebr.offsetMax = Vector2.zero;
            Text eqBtnTxt = eqBtn.GetComponentInChildren<Text>();

            return new JetSkinUI.JetSkinCardUI
            {
                skinId = skinDef.id,
                cardObject = card,
                jetPreviewImage = rawImg,
                nameText = nameTxt,
                classText = classTxt,
                descriptionText = descTxt,
                hullSwatch = hullObj.GetComponent<Image>(),
                accentSwatch = accObj.GetComponent<Image>(),
                equipButton = eqBtn,
                equipButtonText = eqBtnTxt,
                cardOutline = cardOutline
            };
        }

        private void BuildGarageCard(Transform parent, string title, out Text lvlTxt, out Image fill, out Text costTxt, out Button btn)
        {
            GameObject card = UIBuilderHelper.CreatePanel(parent, "UpgradeCard", new Color(0.07f, 0.12f, 0.22f, 0.92f), Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            Image cardBg = card.GetComponent<Image>();
            if (cardBg != null)
            {
                cardBg.sprite = UIBuilderHelper.GetRoundedCornerSprite(20);
                cardBg.type = Image.Type.Sliced;
            }
            RectTransform cr = card.GetComponent<RectTransform>();
            cr.sizeDelta = new Vector2(0, 102);
            LayoutElement le = card.AddComponent<LayoutElement>();
            le.preferredHeight = 102;
            le.minHeight = 92;
            le.flexibleWidth = 1;

            Outline ol = card.AddComponent<Outline>();
            ol.effectColor = new Color(0.1f, 0.55f, 0.85f, 0.65f);

            // Top Row Container (Y: 0.54 to 0.95)
            GameObject topRow = UIBuilderHelper.CreateContainer(card.transform, "TopRow", new Vector2(0.03f, 0.54f), new Vector2(0.97f, 0.95f), Vector2.zero, Vector2.zero);
            UIBuilderHelper.CreateTextWithBounds(topRow.transform, "Title", title, 21, TextAnchor.MiddleLeft, Color.white, Vector2.zero, new Vector2(0.70f, 1f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            lvlTxt = UIBuilderHelper.CreateTextWithBounds(topRow.transform, "Lvl", "LVL 1/5", 21, TextAnchor.MiddleRight, Color.cyan, new Vector2(0.70f, 0f), Vector2.one, Vector2.zero, Vector2.zero, FontStyle.Bold);

            // Bottom Row Container (Y: 0.08 to 0.48)
            GameObject botRow = UIBuilderHelper.CreateContainer(card.transform, "BotRow", new Vector2(0.03f, 0.08f), new Vector2(0.97f, 0.48f), Vector2.zero, Vector2.zero);

            // Stat Bar on Left (X: 0% to 52%)
            var (statFill, _) = UIBuilderHelper.CreateStatBar(botRow.transform, "StatBar", "", Color.cyan, new Vector2(200, 18));
            RectTransform sbr = statFill.transform.parent.GetComponent<RectTransform>();
            sbr.anchorMin = new Vector2(0f, 0.12f);
            sbr.anchorMax = new Vector2(0.52f, 0.88f);
            sbr.offsetMin = Vector2.zero;
            sbr.offsetMax = Vector2.zero;
            fill = statFill;

            // Cost Text (X: 0.54% to 0.74%)
            costTxt = UIBuilderHelper.CreateTextWithBounds(botRow.transform, "Cost", "500 COINS", 20, TextAnchor.MiddleRight, new Color(1f, 0.85f, 0.2f), new Vector2(0.52f, 0f), new Vector2(0.74f, 1f), Vector2.zero, Vector2.zero, FontStyle.Bold);

            // Upgrade Button on Right (X: 0.75% to 1.00%)
            btn = UIBuilderHelper.CreateSciFiButton(botRow.transform, "BtnUpgrade", "UPGRADE", new Vector2(120, 38), new Color(0.1f, 0.88f, 0.45f), Color.white, 20);
            RectTransform br = btn.GetComponent<RectTransform>();
            br.anchorMin = new Vector2(0.75f, 0.05f);
            br.anchorMax = new Vector2(1.0f, 0.95f);
            br.offsetMin = Vector2.zero;
            br.offsetMax = Vector2.zero;
        }

        private void BuildCharacterSelectScreen(Transform parent, UIManager mgr)
        {
            GameObject panel = UIBuilderHelper.CreatePanel(parent, "CharacterSelectPanel", new Color(0.02f, 0.04f, 0.09f, 0.95f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            mgr.characterSelectPanel = panel;
            CharacterSelectUI csUI = panel.AddComponent<CharacterSelectUI>();
            mgr.characterSelectUI = csUI;

            // Title & Balance Header (X: 0.04 to 0.96, Y: 0.88 to 0.98)
            GameObject header = UIBuilderHelper.CreateContainer(panel.transform, "Header", new Vector2(0.04f, 0.88f), new Vector2(0.96f, 0.98f), Vector2.zero, Vector2.zero);
            Text titleText = UIBuilderHelper.CreateTextWithBounds(header.transform, "Title", "PILOT ACADEMY", 46, TextAnchor.MiddleLeft, new Color(0.15f, 0.85f, 1f), Vector2.zero, new Vector2(0.6f, 1f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            Shadow tShadow = titleText.gameObject.AddComponent<Shadow>();
            tShadow.effectColor = new Color(0f, 0f, 0f, 0.95f);
            tShadow.effectDistance = new Vector2(3f, -3f);

            csUI.coinsBalanceText = UIBuilderHelper.CreateTextWithBounds(header.transform, "Coins", "COINS: 0000", 28, TextAnchor.MiddleRight, new Color(1f, 0.85f, 0.1f), new Vector2(0.6f, 0f), Vector2.one, Vector2.zero, Vector2.zero, FontStyle.Bold);
            Shadow cShadow = csUI.coinsBalanceText.gameObject.AddComponent<Shadow>();
            cShadow.effectColor = new Color(0f, 0f, 0f, 0.95f);
            cShadow.effectDistance = new Vector2(2f, -2f);

            // Pilot Cards Row Container (X: 0.04 to 0.96, Y: 0.14 to 0.86)
            GameObject cardsContainer = UIBuilderHelper.CreateContainer(panel.transform, "PilotCardsRow", new Vector2(0.04f, 0.14f), new Vector2(0.96f, 0.86f), Vector2.zero, Vector2.zero);
            HorizontalLayoutGroup hlg = cardsContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 18f;
            hlg.padding = new RectOffset(10, 10, 10, 10);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            int pilotCount = CharacterManager.Characters.Length;
            csUI.pilotCards = new CharacterSelectUI.CharacterCardUI[pilotCount];

            for (int i = 0; i < pilotCount; i++)
            {
                var def = CharacterManager.GetCharacter(i);
                CharacterSelectUI.CharacterCardUI cardUI = new CharacterSelectUI.CharacterCardUI();
                cardUI.characterId = def.id;

                GameObject cardObj = UIBuilderHelper.CreatePanel(cardsContainer.transform, $"PilotCard_{i}", new Color(0.06f, 0.12f, 0.24f, 0.96f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                cardUI.cardObject = cardObj;
                Image cardImg = cardObj.GetComponent<Image>();
                cardImg.sprite = UIBuilderHelper.GetRoundedCornerSprite(24);
                cardImg.type = Image.Type.Sliced;

                Outline col = cardObj.AddComponent<Outline>();
                col.effectColor = def.pilotColor;
                col.effectDistance = new Vector2(3f, 3f);

                LayoutElement le = cardObj.AddComponent<LayoutElement>();
                le.minWidth = 220;
                le.preferredWidth = 320;
                le.flexibleWidth = 1;

                // Card Emblem Swatch (Y: 0.72 to 0.92) - Rounded Pilot Insignia
                GameObject emblem = UIBuilderHelper.CreatePanel(cardObj.transform, "Emblem", def.pilotColor, new Vector2(0.5f, 0.82f), new Vector2(0.5f, 0.82f), new Vector2(-42, -42), new Vector2(42, 42));
                Image eImg = emblem.GetComponent<Image>();
                eImg.sprite = UIBuilderHelper.GetRoundedCornerSprite(18);
                eImg.type = Image.Type.Sliced;
                Outline eol = emblem.AddComponent<Outline>();
                eol.effectColor = Color.white;
                eol.effectDistance = new Vector2(2f, 2f);
                cardUI.emblemImage = eImg;

                // Pilot Name (Y: 0.58 to 0.70) - Large 32pt bold font
                cardUI.nameText = UIBuilderHelper.CreateTextWithBounds(cardObj.transform, "Name", def.pilotName, 32, TextAnchor.MiddleCenter, Color.white, new Vector2(0.04f, 0.58f), new Vector2(0.96f, 0.70f), Vector2.zero, Vector2.zero, FontStyle.Bold);
                Shadow nShadow = cardUI.nameText.gameObject.AddComponent<Shadow>();
                nShadow.effectColor = new Color(0f, 0f, 0f, 0.95f);
                nShadow.effectDistance = new Vector2(2f, -2f);

                // Callsign (Y: 0.47 to 0.57) - Large 22pt bold golden font
                cardUI.callsignText = UIBuilderHelper.CreateTextWithBounds(cardObj.transform, "Callsign", $"[ {def.callsign} ]", 22, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.2f), new Vector2(0.04f, 0.47f), new Vector2(0.96f, 0.57f), Vector2.zero, Vector2.zero, FontStyle.Bold);
                Shadow csShadow = cardUI.callsignText.gameObject.AddComponent<Shadow>();
                csShadow.effectColor = new Color(0f, 0f, 0f, 0.95f);
                csShadow.effectDistance = new Vector2(2f, -2f);

                // Perk Description (Y: 0.24 to 0.46) - Simplified, high-impact 24pt bold neon cyan text
                string cleanPerk = def.perkDescription.Replace("✦ ", "").Trim().ToUpper();
                cardUI.perkText = UIBuilderHelper.CreateTextWithBounds(cardObj.transform, "Perk", $"✦ {cleanPerk}", 24, TextAnchor.MiddleCenter, new Color(0f, 1f, 1f), new Vector2(0.04f, 0.24f), new Vector2(0.96f, 0.46f), Vector2.zero, Vector2.zero, FontStyle.Bold);
                Shadow pShadow = cardUI.perkText.gameObject.AddComponent<Shadow>();
                pShadow.effectColor = new Color(0f, 0f, 0f, 0.95f);
                pShadow.effectDistance = new Vector2(2f, -2f);

                // Action Button (Y: 0.05 to 0.18) - Bright, vibrant rounded button with corner radius
                cardUI.selectButton = UIBuilderHelper.CreateSciFiButton(cardObj.transform, "BtnAction", "SELECT", new Vector2(230, 54), new Color(0.12f, 0.88f, 0.45f), Color.white, 22);
                RectTransform sbr = cardUI.selectButton.GetComponent<RectTransform>();
                sbr.anchorMin = new Vector2(0.5f, 0.11f);
                sbr.anchorMax = new Vector2(0.5f, 0.11f);
                sbr.anchoredPosition = Vector2.zero;
                cardUI.buttonLabel = cardUI.selectButton.GetComponentInChildren<Text>();

                csUI.pilotCards[i] = cardUI;
            }

            // Bottom Return Button (Centered, Y: 0.03 to 0.10) - Bright vibrant coral ruby button
            csUI.backButton = UIBuilderHelper.CreateSciFiButton(panel.transform, "BtnBack", "← BACK TO HOME", new Vector2(320, 56), new Color(0.95f, 0.25f, 0.25f), Color.white, 22);
            RectTransform bbr = csUI.backButton.GetComponent<RectTransform>();
            bbr.anchorMin = new Vector2(0.5f, 0.065f);
            bbr.anchorMax = new Vector2(0.5f, 0.065f);
            bbr.anchoredPosition = Vector2.zero;

            csUI.BindButtons();
            panel.SetActive(false);
        }

        private void BuildAchievementsScreen(Transform parent, UIManager mgr)
        {
            GameObject panel = UIBuilderHelper.CreatePanel(parent, "AchievementsPanel", new Color(0.02f, 0.04f, 0.09f, 0.95f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            mgr.achievementsPanel = panel;
            AchievementUI aUI = panel.AddComponent<AchievementUI>();

            // Title
            GameObject titleObj = UIBuilderHelper.CreateContainer(panel.transform, "Header", new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.98f), new Vector2(-400, -35), new Vector2(400, 35));
            UIBuilderHelper.CreateTextWithBounds(titleObj.transform, "Title", "PILOT ACHIEVEMENTS", 38, TextAnchor.MiddleCenter, Color.cyan, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, FontStyle.Bold);

            // Container Box
            GameObject box = UIBuilderHelper.CreatePanel(panel.transform, "ListContainer", new Color(0.06f, 0.1f, 0.18f, 0.9f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-450, -280), new Vector2(450, 260));
            Image boxBg = box.GetComponent<Image>();
            if (boxBg != null)
            {
                boxBg.sprite = UIBuilderHelper.GetRoundedCornerSprite(24);
                boxBg.type = Image.Type.Sliced;
            }
            Outline ol = box.AddComponent<Outline>();
            ol.effectColor = Color.cyan;

            aUI.achievementsListText = UIBuilderHelper.CreateTextWithBounds(box.transform, "ListText", "Achievements...", 18, TextAnchor.UpperLeft, Color.white, Vector2.zero, Vector2.one, new Vector2(30, 20), new Vector2(-30, -20));

            // Back button
            aUI.backButton = UIBuilderHelper.CreateSciFiButton(panel.transform, "BtnBack", "← BACK TO HOME", new Vector2(300, 54), new Color(0.95f, 0.25f, 0.25f), Color.white, 22);
            RectTransform bbr = aUI.backButton.GetComponent<RectTransform>();
            bbr.anchorMin = new Vector2(0.5f, 0.065f);
            bbr.anchorMax = new Vector2(0.5f, 0.065f);
            bbr.anchoredPosition = Vector2.zero;
            aUI.BindButtons();

            panel.SetActive(false);
        }

        private void BuildSettingsScreen(Transform parent, UIManager mgr)
        {
            GameObject panel = UIBuilderHelper.CreatePanel(parent, "SettingsPanel", new Color(0.02f, 0.04f, 0.09f, 0.95f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            mgr.settingsPanel = panel;
            SettingsUI sUI = panel.AddComponent<SettingsUI>();

            // Title
            GameObject titleObj = UIBuilderHelper.CreateContainer(panel.transform, "Header", new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.98f), new Vector2(-400, -35), new Vector2(400, 35));
            UIBuilderHelper.CreateTextWithBounds(titleObj.transform, "Title", "SETTINGS & AUDIO", 38, TextAnchor.MiddleCenter, Color.cyan, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, FontStyle.Bold);

            // Centered Settings Box
            GameObject box = UIBuilderHelper.CreatePanel(panel.transform, "SettingsBox", new Color(0.06f, 0.1f, 0.18f, 0.95f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-280, -240), new Vector2(280, 240));
            Image boxBg = box.GetComponent<Image>();
            if (boxBg != null)
            {
                boxBg.sprite = UIBuilderHelper.GetRoundedCornerSprite(24);
                boxBg.type = Image.Type.Sliced;
            }
            Outline ol = box.AddComponent<Outline>();
            ol.effectColor = Color.cyan;

            // Sliders Container inside box
            GameObject col = UIBuilderHelper.CreateContainer(box.transform, "Sliders", new Vector2(0.06f, 0.06f), new Vector2(0.94f, 0.94f), Vector2.zero, Vector2.zero);
            VerticalLayoutGroup vlg = col.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14f;
            vlg.padding = new RectOffset(16, 16, 16, 16);
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            UIBuilderHelper.CreateText(col.transform, "MasterLabel", "MASTER VOLUME", 18, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            sUI.masterSlider = UIBuilderHelper.CreateSlider(col.transform, "MasterSlider", new Vector2(420, 24), Color.cyan);

            UIBuilderHelper.CreateText(col.transform, "MusicLabel", "MUSIC VOLUME", 18, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            sUI.musicSlider = UIBuilderHelper.CreateSlider(col.transform, "MusicSlider", new Vector2(420, 24), Color.cyan);

            UIBuilderHelper.CreateText(col.transform, "SfxLabel", "SFX VOLUME", 18, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            sUI.sfxSlider = UIBuilderHelper.CreateSlider(col.transform, "SfxSlider", new Vector2(420, 24), Color.cyan);

            // Reset Data Button inside column
            sUI.resetDataButton = UIBuilderHelper.CreateSciFiButton(col.transform, "BtnReset", "RESET ALL SAVE DATA", new Vector2(280, 48), new Color(0.95f, 0.25f, 0.25f), Color.white, 18);

            // Back button
            sUI.backButton = UIBuilderHelper.CreateSciFiButton(panel.transform, "BtnBack", "← BACK TO HOME", new Vector2(300, 54), new Color(0f, 0.75f, 1f), Color.white, 22);
            RectTransform bbr = sUI.backButton.GetComponent<RectTransform>();
            bbr.anchorMin = new Vector2(0.5f, 0.065f);
            bbr.anchorMax = new Vector2(0.5f, 0.065f);
            bbr.anchoredPosition = Vector2.zero;
            sUI.BindButtons();

            panel.SetActive(false);
        }

        private void BuildTutorialScreen(Transform parent, UIManager mgr)
        {
            GameObject panel = UIBuilderHelper.CreatePanel(parent, "TutorialPanel", new Color(0.02f, 0.03f, 0.08f, 0.94f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            mgr.tutorialPanel = panel;
            TutorialUI tutUI = panel.AddComponent<TutorialUI>();

            // Center Card
            GameObject card = UIBuilderHelper.CreatePanel(panel.transform, "TutorialCard", new Color(0.06f, 0.12f, 0.24f, 0.95f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-420, -280), new Vector2(420, 280));
            Image cardBg = card.GetComponent<Image>();
            if (cardBg != null)
            {
                cardBg.sprite = UIBuilderHelper.GetRoundedCornerSprite(24);
                cardBg.type = Image.Type.Sliced;
            }
            Outline ol = card.AddComponent<Outline>();
            ol.effectColor = Color.cyan;

            UIBuilderHelper.CreateTextWithBounds(card.transform, "TutTitle", "FLIGHT ACADEMY: PILOT CONTROLS", 32, TextAnchor.MiddleCenter, Color.cyan, new Vector2(0.05f, 0.85f), new Vector2(0.95f, 0.96f), Vector2.zero, Vector2.zero, FontStyle.Bold);

            // Guide Lines
            GameObject guide = UIBuilderHelper.CreateContainer(card.transform, "GuideText", new Vector2(0.08f, 0.24f), new Vector2(0.92f, 0.82f), Vector2.zero, Vector2.zero);

            string guideContent =
                "<b>[A / D]</b> Steer Left / Right      <b>[SPACE]</b> Boost Thrusters\n" +
                "Collect <b>Gold Coins</b> & <b>Energy Crystals</b> - Dodge <b>Asteroids</b>!";

            UIBuilderHelper.CreateTextWithBounds(guide.transform, "Content", guideContent, 20, TextAnchor.MiddleCenter, Color.white, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            tutUI.launchButton = UIBuilderHelper.CreateGoldPlayButton(card.transform, "BtnFly", "LAUNCH FLIGHT", new Vector2(220, 50), 20);
            RectTransform lbr = tutUI.launchButton.GetComponent<RectTransform>();
            lbr.anchorMin = new Vector2(0.5f, 0.12f);
            lbr.anchorMax = new Vector2(0.5f, 0.12f);
            lbr.anchoredPosition = Vector2.zero;
            tutUI.BindButtons();

            panel.SetActive(false);
        }

        #endregion
    }
}
