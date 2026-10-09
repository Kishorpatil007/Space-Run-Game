using System;
using UnityEngine;
using SpaceJet.Audio;
using SpaceJet.Core;

namespace SpaceJet.Player
{
    public class PlayerBoost : MonoBehaviour
    {
        public static PlayerBoost Instance { get; set; }

        [Header("Boost Settings")]
        public float maxBoost = 100f;
        public float currentBoost;
        public float boostDrainRate = 30f;
        public float boostRechargeRate = 18f;
        public float speedMultiplier = 1.75f;

        public bool IsBoosting { get; private set; } = false;
        public bool IsBoostUnlocked => GameManager.Instance != null && (GameManager.Instance.currentLevelIndex >= 2 || SaveManager.UnlockedLevel >= 2);

        public event Action<float, float> OnBoostChanged; // current, max
        public event Action<bool> OnBoostStateToggled;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            ApplyUpgrades();
            ResetBoost();
        }

        public void ApplyUpgrades()
        {
            int engineLvl = SaveManager.EngineLevel;
            speedMultiplier = 1.6f + (engineLvl - 1) * 0.1f;
            boostRechargeRate = 16f + (engineLvl - 1) * 2.5f;
        }

        public void ResetBoost()
        {
            currentBoost = maxBoost;
            IsBoosting = false;
            OnBoostChanged?.Invoke(currentBoost, maxBoost);
            OnBoostStateToggled?.Invoke(false);
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            {
                if (IsBoosting) SetBoosting(false);
                return;
            }

            // Check if boost is unlocked and player is holding Space
            bool wantsBoost = IsBoostUnlocked && (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(1));

            if (wantsBoost && currentBoost > 5f)
            {
                if (!IsBoosting) SetBoosting(true);
                currentBoost = Mathf.Max(0f, currentBoost - boostDrainRate * Time.deltaTime);

                if (currentBoost <= 0f)
                {
                    SetBoosting(false);
                }
            }
            else
            {
                if (IsBoosting) SetBoosting(false);
                // Passive recharge
                if (currentBoost < maxBoost)
                {
                    currentBoost = Mathf.Min(maxBoost, currentBoost + boostRechargeRate * Time.deltaTime);
                }
            }

            OnBoostChanged?.Invoke(currentBoost, maxBoost);
        }

        private void SetBoosting(bool active)
        {
            IsBoosting = active;
            OnBoostStateToggled?.Invoke(active);

            if (active)
            {
                if (AudioManager.Instance)
                {
                    AudioManager.Instance.PlaySound("boost");
                }
                if (CameraController.Instance)
                {
                    CameraController.Instance.TriggerShake(0.18f, 0.2f);
                }
            }
        }

        public void AddBoost(float amount)
        {
            currentBoost = Mathf.Min(maxBoost, currentBoost + amount);
            OnBoostChanged?.Invoke(currentBoost, maxBoost);
        }

        public float BoostPercent => Mathf.Clamp01(currentBoost / maxBoost);
    }
}
