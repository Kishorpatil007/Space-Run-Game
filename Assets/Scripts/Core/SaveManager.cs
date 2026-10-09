using System;
using UnityEngine;

namespace SpaceJet.Core
{
    /// <summary>
    /// Manages persistent save data for progress, coin balances, upgrades, and achievements.
    /// </summary>
    public static class SaveManager
    {
        private const string KEY_COINS = "SJ_Coins";
        private const string KEY_UNLOCKED_LEVEL = "SJ_UnlockedLevel";
        private const string KEY_UPGRADE_ENGINE = "SJ_UpgradeEngine";
        private const string KEY_UPGRADE_ENERGY = "SJ_UpgradeEnergy";
        private const string KEY_UPGRADE_SHIELD = "SJ_UpgradeShield";
        private const string KEY_UPGRADE_HULL = "SJ_UpgradeHull";
        private const string KEY_ACHIEVEMENT_PREFIX = "SJ_Ach_";
        private const string KEY_HIGHSCORE_PREFIX = "SJ_Score_";
        private const string KEY_TOTAL_COINS_COLLECTED = "SJ_TotalCoinsCollected";
        private const string KEY_TOTAL_ASTEROIDS_DODGED = "SJ_TotalAsteroidsDodged";
        private const string KEY_SELECTED_SKIN = "SJ_SelectedSkin";
        private const string KEY_SKIN_UNLOCKED_PREFIX = "SJ_Skin_";
        private const string KEY_SELECTED_CHARACTER = "SJ_SelectedCharacter";
        private const string KEY_CHARACTER_UNLOCKED_PREFIX = "SJ_Char_";

        // Default starting values
        public const int INITIAL_UNLOCKED_LEVEL = 1;
        public const int INITIAL_COINS = 5000; // Generous starting balance so player can freely test and equip all skins and upgrades

        public static int SelectedSkinIndex
        {
            get => PlayerPrefs.GetInt(KEY_SELECTED_SKIN, 4);
            set
            {
                PlayerPrefs.SetInt(KEY_SELECTED_SKIN, Mathf.Clamp(value, 0, 4));
                PlayerPrefs.Save();
            }
        }

        public static int SelectedCharacterIndex
        {
            get => PlayerPrefs.GetInt(KEY_SELECTED_CHARACTER, 0);
            set
            {
                PlayerPrefs.SetInt(KEY_SELECTED_CHARACTER, Mathf.Clamp(value, 0, 3));
                PlayerPrefs.Save();
            }
        }

        public static int Coins
        {
            get => PlayerPrefs.GetInt(KEY_COINS, INITIAL_COINS);
            set
            {
                PlayerPrefs.SetInt(KEY_COINS, Mathf.Max(0, value));
                PlayerPrefs.Save();
            }
        }

        public static int UnlockedLevel
        {
            get => PlayerPrefs.GetInt(KEY_UNLOCKED_LEVEL, INITIAL_UNLOCKED_LEVEL);
            set
            {
                if (value > UnlockedLevel)
                {
                    PlayerPrefs.SetInt(KEY_UNLOCKED_LEVEL, Mathf.Clamp(value, 1, 5));
                    PlayerPrefs.Save();
                }
            }
        }

        public static int EngineLevel
        {
            get => PlayerPrefs.GetInt(KEY_UPGRADE_ENGINE, 1);
            set
            {
                PlayerPrefs.SetInt(KEY_UPGRADE_ENGINE, Mathf.Clamp(value, 1, 5));
                PlayerPrefs.Save();
            }
        }

        public static int EnergyTankLevel
        {
            get => PlayerPrefs.GetInt(KEY_UPGRADE_ENERGY, 1);
            set
            {
                PlayerPrefs.SetInt(KEY_UPGRADE_ENERGY, Mathf.Clamp(value, 1, 5));
                PlayerPrefs.Save();
            }
        }

        public static int ShieldLevel
        {
            get => PlayerPrefs.GetInt(KEY_UPGRADE_SHIELD, 1);
            set
            {
                PlayerPrefs.SetInt(KEY_UPGRADE_SHIELD, Mathf.Clamp(value, 1, 5));
                PlayerPrefs.Save();
            }
        }

        public static int HullLevel
        {
            get => PlayerPrefs.GetInt(KEY_UPGRADE_HULL, 1);
            set
            {
                PlayerPrefs.SetInt(KEY_UPGRADE_HULL, Mathf.Clamp(value, 1, 5));
                PlayerPrefs.Save();
            }
        }

        public static int TotalCoinsCollected
        {
            get => PlayerPrefs.GetInt(KEY_TOTAL_COINS_COLLECTED, 0);
            set
            {
                PlayerPrefs.SetInt(KEY_TOTAL_COINS_COLLECTED, value);
                PlayerPrefs.Save();
            }
        }

        public static int TotalAsteroidsDodged
        {
            get => PlayerPrefs.GetInt(KEY_TOTAL_ASTEROIDS_DODGED, 0);
            set
            {
                PlayerPrefs.SetInt(KEY_TOTAL_ASTEROIDS_DODGED, value);
                PlayerPrefs.Save();
            }
        }

        public static void AddCoins(int amount)
        {
            if (amount <= 0) return;
            Coins += amount;
            TotalCoinsCollected += amount;
        }

        public static bool SpendCoins(int amount)
        {
            if (Coins >= amount)
            {
                Coins -= amount;
                return true;
            }
            return false;
        }

        public static bool IsLevelUnlocked(int levelIndex)
        {
            if (levelIndex <= 1) return true;
            return UnlockedLevel >= levelIndex;
        }

        public static void UnlockLevel(int levelIndex)
        {
            if (levelIndex > UnlockedLevel)
            {
                UnlockedLevel = levelIndex;
            }
        }

        public static bool IsSkinUnlocked(int skinIndex)
        {
            return true; // All 5 signature color skins are unlocked so player can equip any color instantly
        }

        public static void UnlockSkin(int skinIndex)
        {
            PlayerPrefs.SetInt(KEY_SKIN_UNLOCKED_PREFIX + skinIndex, 1);
            PlayerPrefs.Save();
        }

        public static bool IsCharacterUnlocked(int characterIndex)
        {
            if (characterIndex <= 0) return true; // Default pilot is free
            return PlayerPrefs.GetInt(KEY_CHARACTER_UNLOCKED_PREFIX + characterIndex, 0) == 1;
        }

        public static void UnlockCharacter(int characterIndex)
        {
            PlayerPrefs.SetInt(KEY_CHARACTER_UNLOCKED_PREFIX + characterIndex, 1);
            PlayerPrefs.Save();
        }

        public static bool IsAchievementUnlocked(string achievementId)
        {
            return PlayerPrefs.GetInt(KEY_ACHIEVEMENT_PREFIX + achievementId, 0) == 1;
        }

        public static void UnlockAchievement(string achievementId)
        {
            PlayerPrefs.SetInt(KEY_ACHIEVEMENT_PREFIX + achievementId, 1);
            PlayerPrefs.Save();
        }

        public static int GetHighScore(int levelIndex)
        {
            return PlayerPrefs.GetInt(KEY_HIGHSCORE_PREFIX + levelIndex, 0);
        }

        public static void SetHighScore(int levelIndex, int score)
        {
            int current = GetHighScore(levelIndex);
            if (score > current)
            {
                PlayerPrefs.SetInt(KEY_HIGHSCORE_PREFIX + levelIndex, score);
                PlayerPrefs.Save();
            }
        }

        public static void ResetAllData()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
}
