using UnityEngine;
using UnityEngine.UI;
using SpaceJet.Audio;
using SpaceJet.Core;

namespace SpaceJet.UI
{
    public class MissionSelectUI : MonoBehaviour
    {
        [System.Serializable]
        public class MissionCard
        {
            public int levelIndex;
            public Button cardButton;
            public Button launchButton;
            public Image cardBg;
            public Outline cardOutline;
            public Text badgeText;
            public Text titleText;
            public Text destinationText;
            public Text detailsText;
            public Text rewardText;
            public Text statusText;
            public Text buttonLabel;
            public Image lockIcon;
        }

        public MissionCard[] cards;
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
            RefreshCards();
        }

        public void BindButtons()
        {
            if (backButton)
            {
                backButton.onClick.RemoveListener(OnBackClicked);
                backButton.onClick.AddListener(OnBackClicked);
            }

            if (cards != null)
            {
                foreach (var card in cards)
                {
                    if (card == null) continue;
                    int lvl = card.levelIndex;

                    if (card.cardButton != null)
                    {
                        card.cardButton.onClick.RemoveAllListeners();
                        card.cardButton.onClick.AddListener(() => OnCardSelected(lvl));
                    }

                    if (card.launchButton != null)
                    {
                        card.launchButton.onClick.RemoveAllListeners();
                        card.launchButton.onClick.AddListener(() => OnCardSelected(lvl));
                    }
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnBackClicked();
            }
        }

        public void RefreshCards()
        {
            if (cards == null) return;

            int unlockedMax = SaveManager.UnlockedLevel;

            foreach (var card in cards)
            {
                if (card == null) continue;
                bool isUnlocked = (card.levelIndex <= unlockedMax);
                var preset = MissionManager.GetMissionPreset(card.levelIndex);

                // Hide cluttered/redundant secondary texts
                if (card.badgeText) card.badgeText.gameObject.SetActive(false);
                if (card.detailsText) card.detailsText.gameObject.SetActive(false);
                if (card.statusText) card.statusText.gameObject.SetActive(false);

                // 1. Prominent Level Title - Large bold golden font
                if (card.titleText)
                {
                    card.titleText.text = $"LEVEL {card.levelIndex}";
                    card.titleText.fontSize = 38;
                    card.titleText.color = isUnlocked ? new Color(1f, 0.85f, 0.15f) : new Color(0.55f, 0.62f, 0.72f);
                    card.titleText.horizontalOverflow = HorizontalWrapMode.Overflow;
                    card.titleText.verticalOverflow = VerticalWrapMode.Truncate;
                    card.titleText.resizeTextForBestFit = true;
                    card.titleText.resizeTextMinSize = 24;
                    card.titleText.resizeTextMaxSize = 38;
                }

                // 2. Clear Planet Destination Name - Prominent white font
                if (card.destinationText)
                {
                    string planet = (preset != null && !string.IsNullOrEmpty(preset.planetName))
                        ? preset.planetName.Replace("Planet ", "").ToUpper()
                        : $"ZONE {card.levelIndex}";
                    card.destinationText.text = planet;
                    card.destinationText.fontSize = 30;
                    card.destinationText.color = isUnlocked ? Color.white : new Color(0.6f, 0.65f, 0.75f);
                    card.destinationText.horizontalOverflow = HorizontalWrapMode.Overflow;
                    card.destinationText.verticalOverflow = VerticalWrapMode.Truncate;
                    card.destinationText.resizeTextForBestFit = true;
                    card.destinationText.resizeTextMinSize = 18;
                    card.destinationText.resizeTextMaxSize = 30;
                }

                // 3. Clear High-Contrast Coin Reward - Glowing yellow font
                if (card.rewardText)
                {
                    int reward = (preset != null) ? preset.coinReward : 500;
                    card.rewardText.text = $"★ {reward} COINS";
                    card.rewardText.fontSize = 26;
                    card.rewardText.color = isUnlocked ? new Color(1f, 0.9f, 0.25f) : new Color(0.55f, 0.5f, 0.35f);
                    card.rewardText.horizontalOverflow = HorizontalWrapMode.Overflow;
                    card.rewardText.verticalOverflow = VerticalWrapMode.Truncate;
                    card.rewardText.resizeTextForBestFit = true;
                    card.rewardText.resizeTextMinSize = 16;
                    card.rewardText.resizeTextMaxSize = 26;
                }

                // 4. Large Action Button with Home Play Button Texture
                if (card.buttonLabel)
                {
                    card.buttonLabel.text = isUnlocked ? "PLAY" : "LOCKED";
                    card.buttonLabel.fontSize = 28;
                    card.buttonLabel.color = Color.white;
                    card.buttonLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
                    card.buttonLabel.verticalOverflow = VerticalWrapMode.Truncate;
                    card.buttonLabel.resizeTextForBestFit = true;
                    card.buttonLabel.resizeTextMinSize = 20;
                    card.buttonLabel.resizeTextMaxSize = 28;
                }

                if (card.cardBg)
                {
                    card.cardBg.color = isUnlocked ? new Color(0.06f, 0.12f, 0.24f, 0.96f) : new Color(0.03f, 0.05f, 0.10f, 0.88f);
                }

                if (card.cardOutline)
                {
                    card.cardOutline.effectColor = isUnlocked ? new Color(0f, 0.9f, 1f, 0.95f) : new Color(0.2f, 0.25f, 0.35f, 0.45f);
                    card.cardOutline.effectDistance = new Vector2(3f, 3f);
                }

                if (card.lockIcon)
                {
                    card.lockIcon.gameObject.SetActive(!isUnlocked);
                }

                if (card.cardButton)
                {
                    card.cardButton.interactable = isUnlocked;
                }

                if (card.launchButton)
                {
                    card.launchButton.interactable = isUnlocked;
                    Image btnImg = card.launchButton.GetComponent<Image>();
                    if (btnImg)
                    {
                        btnImg.sprite = isUnlocked ? UIBuilderHelper.GetGoldPlayButtonSprite() : UIBuilderHelper.GetLockedButtonSprite();
                        btnImg.type = Image.Type.Sliced;
                        btnImg.color = Color.white;
                    }
                }
            }
        }

        private void OnCardSelected(int levelIndex)
        {
            if (SaveManager.IsLevelUnlocked(levelIndex))
            {
                if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
                if (GameManager.Instance) GameManager.Instance.StartMission(levelIndex);
            }
        }

        private void OnBackClicked()
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
            if (UIManager.Instance) UIManager.Instance.ShowPanel(UIManager.Instance.mainMenuPanel);
        }
    }
}
