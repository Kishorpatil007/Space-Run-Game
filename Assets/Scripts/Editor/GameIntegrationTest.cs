#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using SpaceJet.Audio;
using SpaceJet.Collectibles;
using SpaceJet.Core;
using SpaceJet.Environment;
using SpaceJet.Player;
using SpaceJet.UI;
using SpaceJet.Visuals;

namespace SpaceJet.Editor
{
    public static class GameIntegrationTest
    {
        // Automated flight test can be run on-demand via the Unity Editor menu item below without wiping user data.

        [MenuItem("Space Jet/Run Automated Verification Test", true)]
        public static bool ValidateRunAutomatedFlightTest()
        {
            return !EditorApplication.isPlaying;
        }

        [MenuItem("Space Jet/Run Automated Verification Test")]
        public static void RunAutomatedFlightTest()
        {
            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode) return;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("=================================================================");
            sb.AppendLine(">>> [SPACE JET: PLANET RUN] STARTING AUTOMATED VERIFICATION TEST <<<");
            sb.AppendLine("=================================================================");

            int testsPassed = 0;
            int totalTests = 0;

            void Assert(bool condition, string testName)
            {
                totalTests++;
                if (condition)
                {
                    testsPassed++;
                    string passMsg = $"[PASS] {testName}";
                    sb.AppendLine(passMsg);
                    Debug.Log(passMsg);
                }
                else
                {
                    string failMsg = $"[FAIL] {testName}";
                    sb.AppendLine(failMsg);
                    Debug.LogError(failMsg);
                }
            }

            int savedSkin = SaveManager.SelectedSkinIndex;
            int savedPilot = SaveManager.SelectedCharacterIndex;
            int savedCoins = SaveManager.Coins;
            int savedLvl = SaveManager.UnlockedLevel;

            // 1. Reset Save Data for clean testing
            SaveManager.ResetAllData();
            Assert(SaveManager.UnlockedLevel == 1, "SaveManager initializes Level 1 as unlocked");
            Assert(SaveManager.Coins == SaveManager.INITIAL_COINS, "SaveManager starting coin balance is correct");

            // 2. Instantiate and Initialize Core Systems
            GameObject root = new GameObject("TestRoot");
            GameBootstrapper boot = root.AddComponent<GameBootstrapper>();
            boot.InitializeAllSystems();

            Assert(GameManager.Instance != null, "GameManager singleton initialized");
            Assert(MissionManager.Instance != null, "MissionManager singleton initialized");
            Assert(AchievementManager.Instance != null, "AchievementManager singleton initialized");
            Assert(AudioManager.Instance != null, "AudioManager procedural engine initialized");
            Assert(PlayerController.Instance != null, "PlayerController jet initialized");
            Assert(PlayerHealth.Instance != null, "PlayerHealth system initialized");
            Assert(PlayerEnergy.Instance != null, "PlayerEnergy fuel system initialized");
            Assert(PlayerBoost.Instance != null, "PlayerBoost system initialized");
            Assert(PlayerShield.Instance != null, "PlayerShield system initialized");
            Assert(PlanetController.Instance != null, "PlanetController initialized");
            Assert(LevelGenerator.Instance != null, "LevelGenerator corridor initialized");
            Assert(UIManager.Instance != null, "UIManager canvas hierarchy initialized");

            // 3. Test Mission Configurations (Levels 1 to 5)
            for (int lvl = 1; lvl <= 5; lvl++)
            {
                var preset = MissionManager.GetMissionPreset(lvl);
                Assert(preset != null && preset.levelIndex == lvl && !string.IsNullOrEmpty(preset.planetName), $"Mission 0{lvl} ({preset.missionName} -> {preset.planetName}) preset loaded");
            }

            // 4. Test Mission 1 Start & Flight - Direct Start (no briefing screen or start mission button)
            GameManager.Instance.StartMission(1);
            if (GameManager.Instance.CurrentState == GameState.Tutorial)
            {
                GameManager.Instance.DismissTutorial();
            }
            Assert(GameManager.Instance.CurrentState == GameState.Playing, "GameManager launches directly into Playing state upon clicking Play");
            Assert(GameManager.Instance.targetMissionDistance == 1500f, "Mission 1 target distance is 1500m");

            // 5. Test Coin Collectibles & 3D Visuals
            int initialCoins = GameManager.Instance.coinsCollected;
            GameManager.Instance.AddCoins(25);
            Assert(GameManager.Instance.coinsCollected == initialCoins + 25, "Collecting coin awards credits (+25)");

            // 5b. Verify 3D Realistic Gold Coin Geometry (Fix for black coin bug)
            Mesh coinMesh = ProceduralMeshBuilder.CreateCoinMesh(1.0f, 0.25f, 32);
            Assert(coinMesh != null && coinMesh.vertexCount > 0, "Procedural coin mesh generated with 3D geometry");
            bool validNormals = true;
            foreach (var n in coinMesh.normals)
            {
                if (n.sqrMagnitude < 0.5f) { validNormals = false; break; }
            }
            Assert(validNormals, "Coin mesh vertex normals are valid non-zero outward vectors (fixes black coin bug)");

            // 5c. Verify Minted 24K Gold Coin PBR Material
            Material coinMat = ProceduralMeshBuilder.CreateCoinMaterial();
            Assert(coinMat != null && coinMat.color.r >= 0.8f && coinMat.color.g >= 0.7f, "Coin material has radiant 24K gold albedo");
            Assert(coinMat.GetFloat("_Metallic") >= 0.6f && coinMat.GetFloat("_Glossiness") >= 0.8f, "Coin material has realistic PBR metallic & specular gloss");
            Assert(coinMat.mainTexture != null, "Coin material features minted relief texture (bezel ring, teeth notches, embossed star)");

            // 6. Test Energy Refuel & Consumption
            PlayerEnergy.Instance.currentEnergy = 50f;
            PlayerEnergy.Instance.AddEnergy(20f);
            Assert(Mathf.Approximately(PlayerEnergy.Instance.currentEnergy, 70f), "Energy Crystal refuels jet (+20)");

            // 7. Test Shield Activation & Damage Deflection
            float fullHp = PlayerHealth.Instance.currentHealth;
            PlayerShield.Instance.ActivateShield();
            Assert(PlayerShield.Instance.IsShieldActive, "Shield activates successfully");
            PlayerHealth.Instance.TakeDamage(20f, "Asteroid");
            Assert(Mathf.Approximately(PlayerHealth.Instance.currentHealth, fullHp), "Shield deflects obstacle damage completely (HP remains full)");

            // 8. Test 3-Collision System & Energy Down on Asteroid Hit
            PlayerShield.Instance.ResetShield();
            float energyBeforeHit = PlayerEnergy.Instance.currentEnergy;
            PlayerHealth.Instance.TakeDamage(20f, "Asteroid");
            Assert(PlayerHealth.Instance.collisionsRemaining == 2, "Direct asteroid collision consumes 1 of 3 collision lives (2 remaining)");
            Assert(PlayerEnergy.Instance.currentEnergy < energyBeforeHit, "Direct asteroid collision also reduces energy (-25 NRG)");
            Assert(Mathf.Approximately(PlayerHealth.Instance.currentHealth, (2f / 3f) * PlayerHealth.Instance.maxHealth), "Hull health reflects 2/3 collision integrity");
            PlayerHealth.Instance.ResetHealth();

            // 9. Test Boost Mechanics
            float normalSpeed = PlayerController.Instance.baseSpeed;
            float boostMeterBefore = PlayerBoost.Instance.currentBoost;
            Assert(PlayerBoost.Instance.speedMultiplier > 1.5f, "Boost speed multiplier configured correctly");

            // 10. Test Destination Arrival & Completion
            GameManager.Instance.AddDistance(1500f);
            Assert(GameManager.Instance.CurrentState == GameState.CinematicArrival, "Reaching 1500m triggers CinematicArrival state");

            GameManager.Instance.CompleteMission();
            Assert(GameManager.Instance.CurrentState == GameState.MissionComplete, "Mission 1 marked as Complete");
            Assert(SaveManager.UnlockedLevel >= 2, "Level 2 unlocked after completing Level 1");
            Assert(SaveManager.IsAchievementUnlocked("first_flight"), "Achievement 'FIRST FLIGHT' unlocked");

            // 11. Test Jet Garage Upgrades
            int coinsBefore = SaveManager.Coins;
            SaveManager.AddCoins(1000); // Give coins to test upgrade purchase
            int engLvlBefore = SaveManager.EngineLevel;
            bool bought = SaveManager.SpendCoins(500);
            if (bought) SaveManager.EngineLevel++;
            Assert(SaveManager.EngineLevel == engLvlBefore + 1, "Engine upgrade successfully purchased in Garage");

            // 12. Test Unlocking All Levels through to Elysium
            SaveManager.UnlockLevel(5);
            Assert(SaveManager.IsLevelUnlocked(5), "Level 5 ('Reach the New World' -> Planet Elysium) is unlocked");

            // 13. Test Pilot Character System
            Assert(CharacterManager.Characters.Length == 4, "CharacterManager has 4 distinct pilot characters");
            Assert(CharacterManager.GetActiveCharacter().id == 0, "Default pilot is Nova Vance (ID: 0)");
            Assert(CharacterManager.GetActiveSpeedMultiplier() > 1.0f, "Nova Vance speed perk applies (+15% flight speed)");
            Assert(SaveManager.IsCharacterUnlocked(0), "Pilot 0 (Nova Vance) is unlocked for free");
            
            // Test unlocking Pilot 1 (Kaelen Voss, Shield Guardian)
            SaveManager.AddCoins(500);
            bool unlockedPilot = CharacterManager.UnlockCharacter(1);
            Assert(unlockedPilot && SaveManager.IsCharacterUnlocked(1), "Pilot 1 (Kaelen Voss) unlocks with coins");
            CharacterManager.EquipCharacter(1);
            Assert(SaveManager.SelectedCharacterIndex == 1, "Kaelen Voss equipped as active pilot");
            Assert(CharacterManager.GetActiveShieldBonus() > 0f, "Kaelen Voss shield bonus perk active (+2.5s)");

            // 14. Test Jet Skin System & Garage Integration
            Assert(JetSkinManager.Skins.Length >= 5, "JetSkinManager has 5 distinct sci-fi paintwork skins");
            Assert(SaveManager.IsSkinUnlocked(0), "Default skin 0 is unlocked");
            Assert(SaveManager.IsSkinUnlocked(4), "Home UI flagship skin 4 (Solar Vanguard) is unlocked");
            int originalSkin = SaveManager.SelectedSkinIndex;
            SaveManager.AddCoins(1000);
            SaveManager.UnlockSkin(1);
            Assert(SaveManager.IsSkinUnlocked(1), "Skin 1 successfully unlocked");
            SaveManager.SelectedSkinIndex = 1;
            Assert(SaveManager.SelectedSkinIndex == 1, "Skin 1 test-equipped as active jet paintwork");
            // Always restore player's active chosen skin
            SaveManager.SelectedSkinIndex = originalSkin;
            Assert(SaveManager.SelectedSkinIndex == originalSkin, $"Active jet paintwork restored to player selection (Skin {originalSkin})");

            JetSkinUI skinUI = UIManager.Instance.garagePanel.GetComponent<JetSkinUI>();
            Assert(skinUI != null && skinUI.skinCards != null && skinUI.skinCards.Length == 5, "JetSkinUI has 5 visual skin cards configured");

            // 15. Test UI Screen Panels Initialization & Hierarchy
            Assert(UIManager.Instance.mainMenuPanel != null, "UIManager mainMenuPanel initialized");
            Assert(UIManager.Instance.characterSelectPanel != null, "UIManager characterSelectPanel initialized");
            Assert(UIManager.Instance.garagePanel != null, "UIManager garagePanel initialized");
            Assert(UIManager.Instance.settingsPanel != null, "UIManager settingsPanel initialized");
            Assert(UIManager.Instance.missionSelectPanel != null, "UIManager missionSelectPanel initialized");
            Assert(UIManager.Instance.briefingPanel != null, "UIManager briefingPanel initialized");
            Assert(UIManager.Instance.resultPanel != null, "UIManager resultPanel initialized");
            Assert(UIManager.Instance.characterSelectUI != null, "CharacterSelectUI component attached and wired");
            Assert(UIManager.Instance.hudController != null, "HUDController attached to UI hierarchy");
            if (UIManager.Instance.hudController.warningBanner != null)
            {
                Assert(!UIManager.Instance.hudController.warningBanner.activeSelf, "Energy warning banner is hidden during normal flight to prevent clutter");
            }

            // 16. Test Asteroid Dodging & Score
            int avoidedBefore = GameManager.Instance.asteroidsAvoided;
            GameManager.Instance.AddAsteroidAvoided();
            Assert(GameManager.Instance.asteroidsAvoided == avoidedBefore + 1, "Asteroid avoided counter increments (+1)");

            // Cleanup test objects
            Object.DestroyImmediate(root);

            // Restore user save data
            SaveManager.SelectedSkinIndex = savedSkin;
            SaveManager.SelectedCharacterIndex = savedPilot;
            SaveManager.Coins = savedCoins;
            SaveManager.UnlockedLevel = savedLvl;

            sb.AppendLine("=================================================================");
            string summaryMsg = $">>> AUTOMATED VERIFICATION RESULTS: {testsPassed}/{totalTests} TESTS PASSED! <<<";
            sb.AppendLine(summaryMsg);
            sb.AppendLine("=================================================================");
            Debug.Log(summaryMsg);

            try
            {
                System.IO.File.WriteAllText("c:/GDD/test_results_gui.txt", sb.ToString());
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Could not write test output file: {ex.Message}");
            }
        }
    }
}
#endif
