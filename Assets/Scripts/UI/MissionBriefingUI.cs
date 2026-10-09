using System.Text;
using UnityEngine;
using UnityEngine.UI;
using SpaceJet.Audio;
using SpaceJet.Core;

namespace SpaceJet.UI
{
    public class MissionBriefingUI : MonoBehaviour
    {
        [Header("UI References")]
        public Text missionTitleText;
        public Text subtitleText;
        public Text destinationText;
        public Text distanceText;
        public Text difficultyText;
        public Text objectivesListText;
        public Text rewardText;
        public Button startMissionButton;
        public Button backButton;

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
            PopulateBriefing();
        }

        public void BindButtons()
        {
            if (startMissionButton)
            {
                startMissionButton.onClick.RemoveListener(OnStartClicked);
                startMissionButton.onClick.AddListener(OnStartClicked);
            }
            if (backButton)
            {
                backButton.onClick.RemoveListener(OnBackClicked);
                backButton.onClick.AddListener(OnBackClicked);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                OnStartClicked();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnBackClicked();
            }
        }

        public void PopulateBriefing()
        {
            if (MissionManager.Instance == null || MissionManager.Instance.currentConfig == null) return;

            var cfg = MissionManager.Instance.currentConfig;

            if (missionTitleText) missionTitleText.text = $"MISSION 0{cfg.levelIndex}: {cfg.missionName}";
            if (subtitleText) subtitleText.text = $"\"{cfg.subtitle}\"";
            if (destinationText) destinationText.text = $"TARGET: {cfg.planetName.ToUpper()}";
            if (distanceText) distanceText.text = $"DISTANCE: {cfg.totalDistance / 1000f:F1} KM";
            if (difficultyText) difficultyText.text = $"DIFFICULTY: {cfg.difficulty.ToUpper()}";
            if (rewardText) rewardText.text = $"COMPLETION REWARD: +{cfg.coinReward} COINS";

            if (objectivesListText)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var obj in cfg.objectives)
                {
                    sb.AppendLine($"• {obj.description}");
                }
                objectivesListText.text = sb.ToString();
            }
        }

        private void OnStartClicked()
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
            if (GameManager.Instance) GameManager.Instance.BeginFlight();
        }

        private void OnBackClicked()
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
            if (GameManager.Instance) GameManager.Instance.ReturnToMainMenu();
        }
    }
}
