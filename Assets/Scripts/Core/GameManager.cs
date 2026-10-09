using System;
using UnityEngine;
using SpaceJet.Audio;
using SpaceJet.Player;
using SpaceJet.Environment;

namespace SpaceJet.Core
{
    public enum GameState
    {
        MainMenu,
        MissionBriefing,
        Tutorial,
        Playing,
        Paused,
        CinematicArrival,
        MissionComplete,
        MissionFailed
    }

    /// <summary>
    /// Master GameManager orchestrating game loop, stats, level progression, and mission states.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; set; }

        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        [Header("Mission & Progress")]
        public int currentLevelIndex = 1;
        public float currentDistanceTraveled = 0f;
        public float targetMissionDistance = 1500f;

        [Header("Session Stats")]
        public int coinsCollected = 0;
        public int energyCrystalsCollected = 0;
        public int asteroidsAvoided = 0;
        public int collisionsOccurred = 0;
        public int energyStationsActivated = 0;
        public int shieldsCollected = 0;
        public float flightTime = 0f;
        public float lowestEnergyPercent = 100f;
        public string failureReason = "";

        // Events for UI and controllers
        public event Action<GameState> OnStateChanged;
        public event Action<int> OnCoinsChanged;
        public event Action<float, float> OnDistanceUpdated; // current, target

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // By default initialize in MainMenu
            SetState(GameState.MainMenu);
            if (AudioManager.Instance)
            {
                AudioManager.Instance.PlayMusic("bgm_orbit");
            }
        }

        private void Update()
        {
            if (CurrentState == GameState.Playing)
            {
                flightTime += Time.deltaTime;

                // Handle pause key
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    TogglePause();
                }
            }
            else if (CurrentState == GameState.Paused)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    ResumeGame();
                }
            }
        }

        public void SetState(GameState newState)
        {
            CurrentState = newState;

            switch (newState)
            {
                case GameState.Playing:
                    Time.timeScale = 1f;
                    if (AudioManager.Instance)
                    {
                        AudioManager.Instance.StartEngineSound();
                    }
                    break;

                case GameState.Paused:
                    Time.timeScale = 0f;
                    if (AudioManager.Instance)
                    {
                        AudioManager.Instance.StopEngineSound();
                        AudioManager.Instance.StopAlarm();
                    }
                    break;

                case GameState.MissionComplete:
                case GameState.MissionFailed:
                    Time.timeScale = 1f;
                    if (AudioManager.Instance)
                    {
                        AudioManager.Instance.StopEngineSound();
                        AudioManager.Instance.StopAlarm();
                    }
                    break;

                default:
                    Time.timeScale = 1f;
                    break;
            }

            OnStateChanged?.Invoke(newState);
        }

        public void StartMission(int levelIndex)
        {
            currentLevelIndex = Mathf.Clamp(levelIndex, 1, 5);
            ResetSessionStats();

            if (MissionManager.Instance)
            {
                MissionManager.Instance.LoadMission(currentLevelIndex);
                targetMissionDistance = MissionManager.Instance.currentConfig.totalDistance;
            }

            // Reset flight state for clean gameplay
            if (PlayerController.Instance)
            {
                PlayerController.Instance.gameObject.SetActive(true);
                PlayerController.Instance.ResetToStart();
            }
            if (PlayerHealth.Instance) PlayerHealth.Instance.ResetHealth();
            if (PlayerEnergy.Instance) PlayerEnergy.Instance.ResetEnergy();
            if (PlayerBoost.Instance) PlayerBoost.Instance.ResetBoost();
            if (PlayerShield.Instance) PlayerShield.Instance.ResetShield();
            if (LevelGenerator.Instance) LevelGenerator.Instance.GenerateLevel(currentLevelIndex, targetMissionDistance);
            if (PlanetController.Instance) PlanetController.Instance.SetupForMission(currentLevelIndex, targetMissionDistance);
            if (CameraController.Instance && PlayerController.Instance) CameraController.Instance.SetTarget(PlayerController.Instance.transform);

            // Directly launch into flight action - no mission briefing screen or start mission button!
            BeginFlight();
        }

        public void BeginFlight()
        {
            SetState(GameState.Playing);

            if (AudioManager.Instance)
            {
                string bgm = (currentLevelIndex >= 4) ? "bgm_intense" : "bgm_orbit";
                AudioManager.Instance.PlayMusic(bgm);
            }
        }

        public void DismissTutorial()
        {
            PlayerPrefs.SetInt("SJ_TutorialSeen", 1);
            PlayerPrefs.Save();
            SetState(GameState.Playing);
        }

        public void TogglePause()
        {
            if (CurrentState == GameState.Playing)
            {
                SetState(GameState.Paused);
            }
            else if (CurrentState == GameState.Paused)
            {
                ResumeGame();
            }
        }

        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                SetState(GameState.Playing);
            }
        }

        public void AddDistance(float distanceDelta)
        {
            if (CurrentState != GameState.Playing && CurrentState != GameState.CinematicArrival) return;

            currentDistanceTraveled += distanceDelta;
            OnDistanceUpdated?.Invoke(currentDistanceTraveled, targetMissionDistance);

            // Reached destination distance?
            if (currentDistanceTraveled >= targetMissionDistance && CurrentState == GameState.Playing)
            {
                TriggerPlanetArrival();
            }
        }

        public void AddCoins(int amount)
        {
            coinsCollected += amount;
            SaveManager.AddCoins(amount);
            OnCoinsChanged?.Invoke(coinsCollected);

            if (MissionManager.Instance)
            {
                MissionManager.Instance.NotifyProgress(MissionObjective.ObjectiveType.CollectCoins, coinsCollected);
            }
        }

        public void AddCrystal()
        {
            energyCrystalsCollected++;
            if (MissionManager.Instance)
            {
                MissionManager.Instance.NotifyProgress(MissionObjective.ObjectiveType.CollectCrystals, energyCrystalsCollected);
            }
        }

        public void AddAsteroidAvoided()
        {
            asteroidsAvoided++;
            SaveManager.TotalAsteroidsDodged++;
            if (MissionManager.Instance)
            {
                MissionManager.Instance.NotifyProgress(MissionObjective.ObjectiveType.AvoidAsteroids, asteroidsAvoided);
            }
        }

        public void AddEnergyStationActivated()
        {
            energyStationsActivated++;
            if (MissionManager.Instance)
            {
                MissionManager.Instance.NotifyProgress(MissionObjective.ObjectiveType.ActivateEnergyStations, energyStationsActivated);
            }
        }

        public void AddShieldCollected()
        {
            shieldsCollected++;
            if (MissionManager.Instance)
            {
                MissionManager.Instance.NotifyProgress(MissionObjective.ObjectiveType.CollectShields, shieldsCollected);
            }
        }

        public void RecordCollision()
        {
            collisionsOccurred++;
        }

        public void TriggerPlanetArrival()
        {
            SetState(GameState.CinematicArrival);
            if (MissionManager.Instance)
            {
                MissionManager.Instance.NotifyProgress(MissionObjective.ObjectiveType.ReachPlanet, 1);
            }

            // Cinematic sequence runs for 3 seconds then completes mission
            CancelInvoke(nameof(CompleteMission));
            Invoke(nameof(CompleteMission), 3.2f);
        }

        public void CompleteMission()
        {
            SetState(GameState.MissionComplete);

            int reward = 500;
            if (MissionManager.Instance && MissionManager.Instance.currentConfig != null)
            {
                reward = MissionManager.Instance.currentConfig.coinReward;
            }

            // Grant completion reward
            SaveManager.AddCoins(reward);

            // Unlock next level
            if (currentLevelIndex < 5)
            {
                SaveManager.UnlockLevel(currentLevelIndex + 1);
            }

            // Save High Score
            int runScore = coinsCollected * 10 + (int)(currentDistanceTraveled);
            SaveManager.SetHighScore(currentLevelIndex, runScore);

            // Check achievements
            if (AchievementManager.Instance)
            {
                float endingHealth = 100f; // checked by PlayerHealth callback or defaults
                AchievementManager.Instance.CheckProgressAfterMission(currentLevelIndex, endingHealth, lowestEnergyPercent, coinsCollected, asteroidsAvoided);
            }

            if (AudioManager.Instance)
            {
                AudioManager.Instance.PlaySound("complete");
            }
        }

        public void FailMission(string reason)
        {
            if (CurrentState == GameState.MissionComplete || CurrentState == GameState.MissionFailed) return;

            failureReason = reason;
            SetState(GameState.MissionFailed);

            if (AudioManager.Instance)
            {
                AudioManager.Instance.PlaySound("failed");
            }
        }

        public void RestartCurrentMission()
        {
            StartMission(currentLevelIndex);
            BeginFlight();
        }

        public void AdvanceToNextMission()
        {
            if (currentLevelIndex < 5)
            {
                StartMission(currentLevelIndex + 1);
            }
            else
            {
                ReturnToMainMenu();
            }
        }

        public void ReturnToMainMenu()
        {
            ResetSessionStats();
            if (PlayerController.Instance) PlayerController.Instance.ResetToStart();
            if (PlayerHealth.Instance) PlayerHealth.Instance.ResetHealth();
            if (PlayerEnergy.Instance) PlayerEnergy.Instance.ResetEnergy();
            if (PlayerBoost.Instance) PlayerBoost.Instance.ResetBoost();
            if (PlayerShield.Instance) PlayerShield.Instance.ResetShield();
            if (LevelGenerator.Instance) LevelGenerator.Instance.ClearLevel();
            SetState(GameState.MainMenu);
            if (AudioManager.Instance)
            {
                AudioManager.Instance.PlayMusic("bgm_orbit");
            }
        }

        private void ResetSessionStats()
        {
            currentDistanceTraveled = 0f;
            coinsCollected = 0;
            energyCrystalsCollected = 0;
            asteroidsAvoided = 0;
            collisionsOccurred = 0;
            energyStationsActivated = 0;
            shieldsCollected = 0;
            flightTime = 0f;
            lowestEnergyPercent = 100f;
            failureReason = "";
            OnCoinsChanged?.Invoke(0);
        }
    }
}
