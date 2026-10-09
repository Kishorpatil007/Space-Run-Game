using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceJet.Core
{
    [Serializable]
    public class MissionObjective
    {
        public enum ObjectiveType
        {
            CollectCoins,
            CollectCrystals,
            AvoidAsteroids,
            MaintainMinHealth,
            ActivateEnergyStations,
            CollectShields,
            ReachPlanet
        }

        public ObjectiveType type;
        public string description;
        public int targetValue;
        public int currentValue;
        public bool isCompleted;

        public MissionObjective(ObjectiveType type, string description, int targetValue)
        {
            this.type = type;
            this.description = description;
            this.targetValue = targetValue;
            this.currentValue = 0;
            this.isCompleted = false;
        }

        public void UpdateProgress(int value)
        {
            currentValue = value;
            if (type == ObjectiveType.MaintainMinHealth)
            {
                isCompleted = currentValue >= targetValue;
            }
            else
            {
                if (currentValue >= targetValue)
                {
                    isCompleted = true;
                }
            }
        }
    }

    [Serializable]
    public class MissionConfig
    {
        public int levelIndex;
        public string missionName;
        public string subtitle;
        public string planetName;
        public float totalDistance = 1500f; // meters
        public string difficulty;
        public int coinReward;
        public float energyDrainMultiplier = 1.0f;
        public float baseObstacleDensity = 1.0f;
        public float baseSpeed = 30f;
        public Color environmentColor = Color.cyan;
        public Color nebulaColor = new Color(0.2f, 0.4f, 0.9f);
        public string unlockText;
        public List<MissionObjective> objectives = new List<MissionObjective>();
    }

    /// <summary>
    /// Holds the predefined data for all 5 game missions and provides active runtime tracking.
    /// </summary>
    public class MissionManager : MonoBehaviour
    {
        public static MissionManager Instance { get; set; }

        public MissionConfig currentConfig;
        public List<MissionObjective> activeObjectives = new List<MissionObjective>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public static MissionConfig GetMissionPreset(int levelIndex)
        {
            MissionConfig config = new MissionConfig();
            config.levelIndex = levelIndex;

            switch (levelIndex)
            {
                case 1:
                    config.missionName = "FIRST FLIGHT";
                    config.subtitle = "Escape the Orbit";
                    config.planetName = "Planet Nova";
                    config.totalDistance = 1500f;
                    config.difficulty = "Easy";
                    config.coinReward = 500;
                    config.energyDrainMultiplier = 1.0f;
                    config.baseObstacleDensity = 0.8f;
                    config.environmentColor = new Color(0.2f, 0.5f, 1f);
                    config.nebulaColor = new Color(0.3f, 0.1f, 0.8f);
                    config.unlockText = "Unlocks: Level 2";
                    config.objectives.Add(new MissionObjective(MissionObjective.ObjectiveType.CollectCoins, "Collect 20 Coins", 20));
                    config.objectives.Add(new MissionObjective(MissionObjective.ObjectiveType.CollectCrystals, "Collect 3 Energy Crystals", 3));
                    config.objectives.Add(new MissionObjective(MissionObjective.ObjectiveType.AvoidAsteroids, "Avoid 8 Asteroids", 8));
                    config.objectives.Add(new MissionObjective(MissionObjective.ObjectiveType.ReachPlanet, "Reach Planet Nova", 1));
                    break;

                case 2:
                    config.missionName = "ASTEROID RUN";
                    config.subtitle = "Danger Zone";
                    config.planetName = "Planet Terra-X";
                    config.totalDistance = 2500f;
                    config.difficulty = "Medium";
                    config.coinReward = 750;
                    config.energyDrainMultiplier = 1.15f;
                    config.baseObstacleDensity = 1.25f;
                    config.environmentColor = new Color(0.1f, 0.8f, 0.5f);
                    config.nebulaColor = new Color(0.1f, 0.4f, 0.3f);
                    config.unlockText = "Unlocks: Boost Ability & Level 3";
                    config.objectives.Add(new MissionObjective(MissionObjective.ObjectiveType.CollectCoins, "Collect 40 Coins", 40));
                    config.objectives.Add(new MissionObjective(MissionObjective.ObjectiveType.CollectCrystals, "Collect 5 Energy Crystals", 5));
                    config.objectives.Add(new MissionObjective(MissionObjective.ObjectiveType.AvoidAsteroids, "Avoid 15 Asteroids", 15));
                    config.objectives.Add(new MissionObjective(MissionObjective.ObjectiveType.MaintainMinHealth, "Survive with ≥50% Hull", 50));
                    config.objectives.Add(new MissionObjective(MissionObjective.ObjectiveType.ReachPlanet, "Reach Planet Terra-X", 1));
                    break;

                case 3:
                    config.missionName = "ENERGY CRISIS";
                    config.subtitle = "Fuel for Survival";
                    config.planetName = "Planet Zenith";
                    config.totalDistance = 3500f;
                    config.difficulty = "Medium-Hard";
                    config.coinReward = 1000;
                    config.energyDrainMultiplier = 1.6f; // Rapid fuel depletion
                    config.baseObstacleDensity = 1.4f;
                    config.environmentColor = new Color(1f, 0.35f, 0.1f);
                    config.nebulaColor = new Color(0.7f, 0.1f, 0.05f);
                    config.unlockText = "Unlocks: Shield Power-up & Level 4";
                    config.objectives.Add(new MissionObjective(MissionObjective.ObjectiveType.CollectCoins, "Collect 60 Coins", 60));
                    config.objectives.Add(new MissionObjective(MissionObjective.ObjectiveType.CollectCrystals, "Collect 8 Energy Crystals", 8));
                    config.objectives.Add(new MissionObjective(MissionObjective.ObjectiveType.ActivateEnergyStations, "Activate 2 Space Energy Stations", 2));
                    config.objectives.Add(new MissionObjective(MissionObjective.ObjectiveType.ReachPlanet, "Reach Planet Zenith", 1));
                    break;

                case 4:
                    config.missionName = "COSMIC STORM";
                    config.subtitle = "Storm Breaker";
                    config.planetName = "Planet Aurora";
                    config.totalDistance = 4500f;
                    config.difficulty = "Hard";
                    config.coinReward = 1500;
                    config.energyDrainMultiplier = 1.35f;
                    config.baseObstacleDensity = 1.7f;
                    config.environmentColor = new Color(0.85f, 0.2f, 1f);
                    config.nebulaColor = new Color(0.2f, 0.8f, 0.9f);
                    config.unlockText = "Unlocks: Final Mission (Level 5)";
                    config.objectives.Add(new MissionObjective(MissionObjective.ObjectiveType.CollectCoins, "Collect 80 Coins", 80));
                    config.objectives.Add(new MissionObjective(MissionObjective.ObjectiveType.CollectCrystals, "Collect 10 Energy Crystals", 10));
                    config.objectives.Add(new MissionObjective(MissionObjective.ObjectiveType.CollectShields, "Collect 2 Shield Power-ups", 2));
                    config.objectives.Add(new MissionObjective(MissionObjective.ObjectiveType.MaintainMinHealth, "Maintain at least 30% Hull", 30));
                    config.objectives.Add(new MissionObjective(MissionObjective.ObjectiveType.ReachPlanet, "Reach Planet Aurora", 1));
                    break;

                case 5:
                default:
                    config.missionName = "FINAL JOURNEY";
                    config.subtitle = "Reach the New World";
                    config.planetName = "Planet Elysium";
                    config.totalDistance = 6000f;
                    config.difficulty = "Very Hard";
                    config.coinReward = 2500;
                    config.energyDrainMultiplier = 1.5f;
                    config.baseObstacleDensity = 2.0f;
                    config.environmentColor = new Color(0.1f, 0.9f, 0.95f);
                    config.nebulaColor = new Color(0.05f, 0.3f, 0.8f);
                    config.unlockText = "Victory: The Journey is Complete!";
                    config.objectives.Add(new MissionObjective(MissionObjective.ObjectiveType.CollectCoins, "Collect 100 Coins", 100));
                    config.objectives.Add(new MissionObjective(MissionObjective.ObjectiveType.CollectCrystals, "Collect 12 Energy Crystals", 12));
                    config.objectives.Add(new MissionObjective(MissionObjective.ObjectiveType.AvoidAsteroids, "Survive Asteroid Swarm (Avoid 30)", 30));
                    config.objectives.Add(new MissionObjective(MissionObjective.ObjectiveType.ReachPlanet, "Reach Planet Elysium", 1));
                    break;
            }

            return config;
        }

        public void LoadMission(int levelIndex)
        {
            currentConfig = GetMissionPreset(levelIndex);
            activeObjectives.Clear();
            foreach (var obj in currentConfig.objectives)
            {
                activeObjectives.Add(new MissionObjective(obj.type, obj.description, obj.targetValue));
            }
        }

        public void NotifyProgress(MissionObjective.ObjectiveType type, int value)
        {
            foreach (var obj in activeObjectives)
            {
                if (obj.type == type)
                {
                    obj.UpdateProgress(value);
                }
            }
        }

        public bool AreCoreObjectivesComplete()
        {
            // Reach Planet is always required, but other objectives add bonuses / clear conditions
            foreach (var obj in activeObjectives)
            {
                if (obj.type == MissionObjective.ObjectiveType.MaintainMinHealth && !obj.isCompleted)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
