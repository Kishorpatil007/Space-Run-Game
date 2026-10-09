using System.Text;
using UnityEngine;
using UnityEngine.UI;
using SpaceJet.Core;
using SpaceJet.Player;

namespace SpaceJet.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Top Bar")]
        public Text missionNameText;
        public Text coinsText;
        public Text distanceText;
        public Image distanceFill;

        [Header("Player Status Bars")]
        public Image hullFill;
        public Text hullText;

        public Image energyFill;
        public Text energyText;

        public Image boostFill;
        public Text boostText;

        [Header("Shield Indicator")]
        public GameObject shieldContainer;
        public Text shieldTimerText;

        [Header("Energy Warnings")]
        public GameObject warningBanner;
        public Text warningText;

        [Header("Objectives")]
        public Text objectivesText;

        private void OnEnable()
        {
            SubscribeEvents();
            UpdateAllElements();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            if (PlayerHealth.Instance)
            {
                PlayerHealth.Instance.OnHealthChanged += HandleHealthChanged;
            }
            if (PlayerEnergy.Instance)
            {
                PlayerEnergy.Instance.OnEnergyChanged += HandleEnergyChanged;
                PlayerEnergy.Instance.OnWarningChanged += HandleWarningChanged;
            }
            if (PlayerBoost.Instance)
            {
                PlayerBoost.Instance.OnBoostChanged += HandleBoostChanged;
            }
            if (PlayerShield.Instance)
            {
                PlayerShield.Instance.OnShieldStateChanged += HandleShieldChanged;
            }
            if (GameManager.Instance)
            {
                GameManager.Instance.OnCoinsChanged += HandleCoinsChanged;
                GameManager.Instance.OnDistanceUpdated += HandleDistanceUpdated;
            }
        }

        private void UnsubscribeEvents()
        {
            if (PlayerHealth.Instance)
            {
                PlayerHealth.Instance.OnHealthChanged -= HandleHealthChanged;
            }
            if (PlayerEnergy.Instance)
            {
                PlayerEnergy.Instance.OnEnergyChanged -= HandleEnergyChanged;
                PlayerEnergy.Instance.OnWarningChanged -= HandleWarningChanged;
            }
            if (PlayerBoost.Instance)
            {
                PlayerBoost.Instance.OnBoostChanged -= HandleBoostChanged;
            }
            if (PlayerShield.Instance)
            {
                PlayerShield.Instance.OnShieldStateChanged -= HandleShieldChanged;
            }
            if (GameManager.Instance)
            {
                GameManager.Instance.OnCoinsChanged -= HandleCoinsChanged;
                GameManager.Instance.OnDistanceUpdated -= HandleDistanceUpdated;
            }
        }

        private void Update()
        {
            UpdateObjectivesDisplay();

            // Animate warning banner pulse
            if (warningBanner != null && warningBanner.activeSelf)
            {
                float alpha = 0.5f + Mathf.PingPong(Time.time * 4f, 0.5f);
                if (warningText != null)
                {
                    Color c = warningText.color;
                    warningText.color = new Color(c.r, c.g, c.b, alpha);
                }
            }

            // Dynamic low/critical energy pulse on energy bar for instant player visibility
            if (PlayerEnergy.Instance != null && PlayerEnergy.Instance.isLowEnergy && energyFill != null)
            {
                float pulse = 0.65f + Mathf.PingPong(Time.time * 5f, 0.35f);
                Color baseCol = PlayerEnergy.Instance.isCriticalEnergy ? new Color(1f, 0.2f, 0.2f) : new Color(1f, 0.60f, 0f);
                energyFill.color = new Color(baseCol.r * pulse, baseCol.g * pulse, baseCol.b * pulse);
            }

            // Dynamic 1-hit critical warning pulse on hull hit bar
            if (PlayerHealth.Instance != null && PlayerHealth.Instance.collisionsRemaining <= 1 && hullFill != null)
            {
                float pulse = 0.65f + Mathf.PingPong(Time.time * 5f, 0.35f);
                hullFill.color = new Color(1f, 0.2f * pulse, 0.25f * pulse);
            }
        }

        public void UpdateAllElements()
        {
            if (GameManager.Instance && MissionManager.Instance && MissionManager.Instance.currentConfig != null)
            {
                var cfg = MissionManager.Instance.currentConfig;
                if (missionNameText) missionNameText.text = $"M-0{cfg.levelIndex}";
                HandleCoinsChanged(GameManager.Instance.coinsCollected);
                HandleDistanceUpdated(GameManager.Instance.currentDistanceTraveled, GameManager.Instance.targetMissionDistance);
            }

            if (PlayerHealth.Instance)
            {
                HandleHealthChanged(PlayerHealth.Instance.currentHealth, PlayerHealth.Instance.maxHealth);
            }

            if (PlayerEnergy.Instance)
            {
                HandleEnergyChanged(PlayerEnergy.Instance.currentEnergy, PlayerEnergy.Instance.maxEnergy);
                HandleWarningChanged(PlayerEnergy.Instance.isLowEnergy, PlayerEnergy.Instance.isCriticalEnergy);
            }

            if (PlayerBoost.Instance)
            {
                HandleBoostChanged(PlayerBoost.Instance.currentBoost, PlayerBoost.Instance.maxBoost);
            }

            if (PlayerShield.Instance)
            {
                HandleShieldChanged(PlayerShield.Instance.IsShieldActive, PlayerShield.Instance.remainingShieldTime);
            }
        }

        private void HandleHealthChanged(float current, float max)
        {
            float pct = Mathf.Clamp01(current / max);
            int hitsLeft = PlayerHealth.Instance != null ? PlayerHealth.Instance.collisionsRemaining : Mathf.RoundToInt(pct * 3f);
            if (hullFill)
            {
                hullFill.fillAmount = pct;
                // High-visibility glowing colors: Neon emerald -> Electric gold -> Warning crimson
                hullFill.color = (pct > 0.5f) ? Color.Lerp(new Color(1f, 0.82f, 0.1f), new Color(0f, 1f, 0.55f), (pct - 0.5f) * 2f)
                                              : Color.Lerp(new Color(1f, 0.2f, 0.25f), new Color(1f, 0.82f, 0.1f), pct * 2f);
            }
            if (hullText)
            {
                string hitCol = hitsLeft >= 3 ? "#00FF88" : (hitsLeft == 2 ? "#FFD000" : "#FF3344");
                hullText.text = $"HULL  <color={hitCol}><b>{hitsLeft}/3 HITS</b></color>";
            }
        }

        private void HandleEnergyChanged(float current, float max)
        {
            float pct = Mathf.Clamp01(current / max);
            if (energyFill)
            {
                energyFill.fillAmount = pct;
                // High-visibility electric cyan -> Warm amber -> Warning red
                energyFill.color = (pct > 0.3f) ? Color.Lerp(new Color(1f, 0.60f, 0f), new Color(0f, 0.92f, 1f), (pct - 0.3f) / 0.7f)
                                                : Color.Lerp(new Color(1f, 0.2f, 0.2f), new Color(1f, 0.60f, 0f), pct / 0.3f);
            }
            if (energyText)
            {
                string energyCol = pct > 0.3f ? "#00E5FF" : (pct > 0.15f ? "#FFAA00" : "#FF3344");
                energyText.text = $"ENERGY  <color={energyCol}><b>{pct * 100f:F0}%</b></color>";
            }
        }

        private void HandleWarningChanged(bool isLow, bool isCrit)
        {
            if (warningBanner == null) return;

            if (isCrit)
            {
                warningBanner.SetActive(true);
                if (warningText)
                {
                    warningText.text = "CRITICAL FUEL";
                    warningText.color = Color.red;
                }
            }
            else if (isLow)
            {
                warningBanner.SetActive(true);
                if (warningText)
                {
                    warningText.text = "LOW FUEL";
                    warningText.color = new Color(1f, 0.65f, 0f);
                }
            }
            else
            {
                warningBanner.SetActive(false);
            }
        }

        private void HandleBoostChanged(float current, float max)
        {
            float pct = Mathf.Clamp01(current / max);
            if (boostFill)
            {
                boostFill.fillAmount = pct;
                boostFill.color = new Color(0.2f, 0.75f, 1f);
            }
            if (boostText)
            {
                bool unlocked = PlayerBoost.Instance != null && PlayerBoost.Instance.IsBoostUnlocked;
                boostText.text = unlocked ? $"BOOST  <b>{pct * 100f:F0}%</b>" : "";
            }
        }

        private void HandleShieldChanged(bool active, float remainingTime)
        {
            if (shieldContainer)
            {
                shieldContainer.SetActive(active);
            }
            if (shieldTimerText && active)
            {
                shieldTimerText.text = $"SHIELD {remainingTime:F0}s";
            }
        }

        private void HandleCoinsChanged(int coins)
        {
            if (coinsText) coinsText.text = $"COINS: {coins:N0}";
        }

        private void HandleDistanceUpdated(float current, float target)
        {
            float pct = Mathf.Clamp01(current / target);
            if (distanceFill) distanceFill.fillAmount = pct;

            float kmCurrent = current / 1000f;
            float kmTarget = target / 1000f;
            if (distanceText) distanceText.text = $"{kmCurrent:F1} / {kmTarget:F1} KM";
        }

        private void UpdateObjectivesDisplay()
        {
            if (objectivesText == null || !objectivesText.gameObject.activeInHierarchy || MissionManager.Instance == null) return;

            StringBuilder sb = new StringBuilder();
            foreach (var obj in MissionManager.Instance.activeObjectives)
            {
                string check = obj.isCompleted ? "<color=#00FF88>[OK]</color>" : "[  ]";
                sb.AppendLine($"{check} {obj.description}");
            }
            objectivesText.text = sb.ToString();
        }
    }
}
