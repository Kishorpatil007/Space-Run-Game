using System;
using UnityEngine;
using SpaceJet.Audio;
using SpaceJet.Core;

namespace SpaceJet.Player
{
    public class PlayerEnergy : MonoBehaviour
    {
        public static PlayerEnergy Instance { get; set; }

        [Header("Energy Stats")]
        public float maxEnergy = 100f;
        public float currentEnergy;
        public float baseDrainRate = 1.8f; // points per second

        [Header("Warning States")]
        public bool isLowEnergy = false;
        public bool isCriticalEnergy = false;

        private float emergencyGraceTimer = 4.0f;
        private bool inEmergency = false;

        public event Action<float, float> OnEnergyChanged; // current, max
        public event Action<bool, bool> OnWarningChanged; // isLow, isCritical

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            ApplyUpgrades();
            ResetEnergy();
        }

        public void ApplyUpgrades()
        {
            // Energy upgrade adds capacity and slightly improves efficiency
            int energyLvl = SaveManager.EnergyTankLevel;
            maxEnergy = 100f + (energyLvl - 1) * 15f;
            baseDrainRate = Mathf.Max(1.0f, 1.8f - (energyLvl - 1) * 0.15f);
        }

        public void ResetEnergy()
        {
            currentEnergy = maxEnergy;
            isLowEnergy = false;
            isCriticalEnergy = false;
            inEmergency = false;
            emergencyGraceTimer = 4.0f;
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
            OnWarningChanged?.Invoke(false, false);

            if (AudioManager.Instance)
            {
                AudioManager.Instance.StopAlarm();
            }
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            // Calculate drain rate
            float missionMultiplier = 1.0f;
            if (MissionManager.Instance != null && MissionManager.Instance.currentConfig != null)
            {
                missionMultiplier = MissionManager.Instance.currentConfig.energyDrainMultiplier;
            }

            float currentDrain = baseDrainRate * missionMultiplier * CharacterManager.GetActiveEnergyDrainMultiplier();

            // Extra drain if boosting
            if (PlayerBoost.Instance != null && PlayerBoost.Instance.IsBoosting)
            {
                currentDrain += 2.5f;
            }

            currentEnergy = Mathf.Max(0f, currentEnergy - currentDrain * Time.deltaTime);
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);

            // Record minimum energy for achievements
            float energyPct = EnergyPercent * 100f;
            if (energyPct < GameManager.Instance.lowestEnergyPercent)
            {
                GameManager.Instance.lowestEnergyPercent = energyPct;
            }

            // Warning checks
            CheckWarnings(energyPct);

            // 0% energy emergency countdown
            if (currentEnergy <= 0f)
            {
                inEmergency = true;
                emergencyGraceTimer -= Time.deltaTime;
                if (emergencyGraceTimer <= 0f)
                {
                    if (AudioManager.Instance)
                    {
                        AudioManager.Instance.StopAlarm();
                    }
                    GameManager.Instance.FailMission("OUT OF ENERGY");
                }
            }
            else
            {
                inEmergency = false;
                emergencyGraceTimer = 4.0f;
            }
        }

        private void CheckWarnings(float energyPct)
        {
            bool low = energyPct <= 25f && energyPct > 10f;
            bool crit = energyPct <= 10f;

            if (low != isLowEnergy || crit != isCriticalEnergy)
            {
                isLowEnergy = low;
                isCriticalEnergy = crit;
                OnWarningChanged?.Invoke(isLowEnergy, isCriticalEnergy);

                if (AudioManager.Instance)
                {
                    if (isCriticalEnergy)
                    {
                        AudioManager.Instance.PlayAlarm(true);
                    }
                    else if (isLowEnergy)
                    {
                        AudioManager.Instance.PlayAlarm(false);
                    }
                    else
                    {
                        AudioManager.Instance.StopAlarm();
                    }
                }
            }
        }

        public void AddEnergy(float amount)
        {
            currentEnergy = Mathf.Min(maxEnergy, currentEnergy + amount);
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);

            float pct = EnergyPercent * 100f;
            CheckWarnings(pct);

            if (currentEnergy > 0f)
            {
                inEmergency = false;
                emergencyGraceTimer = 4.0f;
            }
        }

        public void ConsumeEnergy(float amount)
        {
            currentEnergy = Mathf.Max(0f, currentEnergy - amount);
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);

            float pct = EnergyPercent * 100f;
            CheckWarnings(pct);

            if (currentEnergy <= 0f)
            {
                inEmergency = true;
            }
        }

        public float EnergyPercent => Mathf.Clamp01(currentEnergy / maxEnergy);
        public float EmergencyGraceRemaining => emergencyGraceTimer;
        public bool IsInEmergency => inEmergency;
    }
}
