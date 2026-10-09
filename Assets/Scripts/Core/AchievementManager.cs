using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceJet.Core
{
    [Serializable]
    public class Achievement
    {
        public string id;
        public string title;
        public string description;
        public bool isUnlocked;

        public Achievement(string id, string title, string description)
        {
            this.id = id;
            this.title = title;
            this.description = description;
            this.isUnlocked = false;
        }
    }

    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance { get; set; }

        public event Action<Achievement> OnAchievementUnlocked;

        public List<Achievement> achievements = new List<Achievement>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAchievements();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void InitializeAchievements()
        {
            achievements.Clear();
            achievements.Add(new Achievement("first_flight", "FIRST FLIGHT", "Complete Level 1."));
            achievements.Add(new Achievement("coin_hunter", "COIN HUNTER", "Collect 100 coins."));
            achievements.Add(new Achievement("asteroid_dodger", "ASTEROID DODGER", "Avoid 50 asteroids."));
            achievements.Add(new Achievement("energy_master", "ENERGY MASTER", "Complete a mission without energy dropping below 20%."));
            achievements.Add(new Achievement("survivor", "SURVIVOR", "Complete a mission with less than 20 HP remaining."));
            achievements.Add(new Achievement("space_explorer", "SPACE EXPLORER", "Complete all 5 levels."));

            foreach (var ach in achievements)
            {
                ach.isUnlocked = SaveManager.IsAchievementUnlocked(ach.id);
            }
        }

        public void TryUnlock(string achievementId)
        {
            var ach = achievements.Find(a => a.id == achievementId);
            if (ach != null && !ach.isUnlocked)
            {
                ach.isUnlocked = true;
                SaveManager.UnlockAchievement(achievementId);
                OnAchievementUnlocked?.Invoke(ach);
                Debug.Log($"[ACHIEVEMENT UNLOCKED] {ach.title} - {ach.description}");
            }
        }

        public void CheckProgressAfterMission(int levelIndex, float endingHealth, float lowestEnergyPercent, int coinsCollectedRun, int asteroidsAvoidedRun)
        {
            if (levelIndex == 1)
            {
                TryUnlock("first_flight");
            }

            if (SaveManager.TotalCoinsCollected >= 100)
            {
                TryUnlock("coin_hunter");
            }

            if (SaveManager.TotalAsteroidsDodged >= 50)
            {
                TryUnlock("asteroid_dodger");
            }

            if (lowestEnergyPercent >= 20f)
            {
                TryUnlock("energy_master");
            }

            if (endingHealth <= 20f && endingHealth > 0f)
            {
                TryUnlock("survivor");
            }

            if (SaveManager.UnlockedLevel >= 5 || levelIndex >= 5)
            {
                TryUnlock("space_explorer");
            }
        }
    }
}
