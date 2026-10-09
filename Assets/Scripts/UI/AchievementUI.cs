using System.Text;
using UnityEngine;
using UnityEngine.UI;
using SpaceJet.Audio;
using SpaceJet.Core;

namespace SpaceJet.UI
{
    public class AchievementUI : MonoBehaviour
    {
        public Text achievementsListText;
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
            RefreshAchievements();
        }

        public void BindButtons()
        {
            if (backButton)
            {
                backButton.onClick.RemoveListener(OnBackClicked);
                backButton.onClick.AddListener(OnBackClicked);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnBackClicked();
            }
        }

        public void RefreshAchievements()
        {
            if (achievementsListText == null || AchievementManager.Instance == null) return;

            StringBuilder sb = new StringBuilder();

            foreach (var ach in AchievementManager.Instance.achievements)
            {
                bool unlocked = SaveManager.IsAchievementUnlocked(ach.id);
                string tag = unlocked ? "<color=#00FF88>★ UNLOCKED</color>" : "<color=#888888>🔒 LOCKED</color>";
                sb.AppendLine($"<b>{ach.title}</b>  [{tag}]");
                sb.AppendLine($"<size=12><color=#BBBBBB>{ach.description}</color></size>\n");
            }

            achievementsListText.text = sb.ToString();
        }

        private void OnBackClicked()
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
            if (UIManager.Instance) UIManager.Instance.ShowPanel(UIManager.Instance.mainMenuPanel);
        }
    }
}
