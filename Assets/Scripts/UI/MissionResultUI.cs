using System;
using UnityEngine;
using UnityEngine.UI;
using SpaceJet.Audio;
using SpaceJet.Core;
using SpaceJet.Player;

namespace SpaceJet.UI
{
    public class MissionResultUI : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject completePanel;
        public GameObject failedPanel;

        [Header("Victory References")]
        public Text completeTitleText;
        public Text planetReachedText;
        public Text victoryStatsText;
        public Text rewardText;
        public Button nextLevelButton;
        public Button replayButton;
        public Button victoryMenuButton;

        [Header("Failure References")]
        public Text failureReasonText;
        public Text failedStatsText;
        public Button retryButton;
        public Button failedMenuButton;

        private void Awake()
        {
            BindButtons();
        }

        private void Start()
        {
            BindButtons();
        }

        private void OnEnable()
        {
            BindButtons();
        }

        public void BindButtons()
        {
            if (nextLevelButton)
            {
                nextLevelButton.onClick.RemoveListener(OnNextLevelClicked);
                nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            }
            if (replayButton)
            {
                replayButton.onClick.RemoveListener(OnReplayClicked);
                replayButton.onClick.AddListener(OnReplayClicked);
            }
            if (victoryMenuButton)
            {
                victoryMenuButton.onClick.RemoveListener(OnMainMenuClicked);
                victoryMenuButton.onClick.AddListener(OnMainMenuClicked);
            }
            if (retryButton)
            {
                retryButton.onClick.RemoveListener(OnRetryClicked);
                retryButton.onClick.AddListener(OnRetryClicked);
            }
            if (failedMenuButton)
            {
                failedMenuButton.onClick.RemoveListener(OnMainMenuClicked);
                failedMenuButton.onClick.AddListener(OnMainMenuClicked);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                if (completePanel != null && completePanel.activeSelf)
                {
                    OnNextLevelClicked();
                }
                else if (failedPanel != null && failedPanel.activeSelf)
                {
                    OnRetryClicked();
                }
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnMainMenuClicked();
            }
        }

        public void ShowVictory()
        {
            if (failedPanel) failedPanel.SetActive(false);
            if (completePanel) completePanel.SetActive(true);

            if (GameManager.Instance == null) return;

            int lvl = GameManager.Instance.currentLevelIndex;
            string planetName = (MissionManager.Instance != null && MissionManager.Instance.currentConfig != null)
                ? MissionManager.Instance.currentConfig.planetName
                : "Unknown Planet";

            if (completeTitleText)
            {
                completeTitleText.text = (lvl == 5) ? "JOURNEY COMPLETE!" : "MISSION COMPLETE!";
            }

            if (planetReachedText)
            {
                if (lvl == 5)
                {
                    planetReachedText.text = "<b><color=#00FFFF>YOU REACHED ELYSIUM!</color></b>\n<size=16>A New Era for Humanity Begins.</size>";
                }
                else
                {
                    planetReachedText.text = $"PLANET REACHED: <b><color=#00FF88>{planetName.ToUpper()}</color></b>";
                }
            }

            int reward = (MissionManager.Instance != null && MissionManager.Instance.currentConfig != null)
                ? MissionManager.Instance.currentConfig.coinReward
                : 500;

            if (rewardText) rewardText.text = $"REWARD: +{reward} COINS";

            // Format statistics
            float hpPct = (PlayerHealth.Instance != null) ? (PlayerHealth.Instance.HealthPercent * 100f) : 100f;
            int minutes = (int)(GameManager.Instance.flightTime / 60f);
            int seconds = (int)(GameManager.Instance.flightTime % 60f);

            if (victoryStatsText)
            {
                victoryStatsText.text =
                    $"Coins Collected: {GameManager.Instance.coinsCollected}\n" +
                    $"Energy Crystals: {GameManager.Instance.energyCrystalsCollected}\n" +
                    $"Distance Flown: {GameManager.Instance.currentDistanceTraveled / 1000f:F1} KM\n" +
                    $"Hull Integrity: {hpPct:F0}%\n" +
                    $"Flight Time: {minutes:D2}:{seconds:D2}\n" +
                    $"Asteroids Avoided: {GameManager.Instance.asteroidsAvoided}";
            }

            if (nextLevelButton)
            {
                // If on final level, change text or disable next level
                nextLevelButton.gameObject.SetActive(lvl < 5);
            }
        }

        public void ShowDefeat()
        {
            if (completePanel) completePanel.SetActive(false);
            if (failedPanel) failedPanel.SetActive(true);

            if (GameManager.Instance == null) return;

            string reason = string.IsNullOrEmpty(GameManager.Instance.failureReason) ? "MISSION ABORTED" : GameManager.Instance.failureReason;
            if (failureReasonText) failureReasonText.text = $"REASON: <color=#FF3333>{reason}</color>";

            int minutes = (int)(GameManager.Instance.flightTime / 60f);
            int seconds = (int)(GameManager.Instance.flightTime % 60f);

            if (failedStatsText)
            {
                failedStatsText.text =
                    $"Distance Covered: {GameManager.Instance.currentDistanceTraveled / 1000f:F1} KM\n" +
                    $"Coins Collected: {GameManager.Instance.coinsCollected}\n" +
                    $"Flight Time: {minutes:D2}:{seconds:D2}\n" +
                    $"Asteroids Avoided: {GameManager.Instance.asteroidsAvoided}";
            }
        }

        private void OnNextLevelClicked()
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
            if (GameManager.Instance) GameManager.Instance.AdvanceToNextMission();
        }

        private void OnReplayClicked()
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
            if (GameManager.Instance) GameManager.Instance.RestartCurrentMission();
        }

        private void OnRetryClicked()
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
            if (GameManager.Instance) GameManager.Instance.RestartCurrentMission();
        }

        private void OnMainMenuClicked()
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
            if (GameManager.Instance) GameManager.Instance.ReturnToMainMenu();
        }
    }
}
