using UnityEngine;
using UnityEngine.UI;
using SpaceJet.Audio;
using SpaceJet.Core;
using SpaceJet.Player;

namespace SpaceJet.UI
{
    public class JetSkinUI : MonoBehaviour
    {
        [System.Serializable]
        public class JetSkinCardUI
        {
            public int skinId;
            public GameObject cardObject;
            public RawImage jetPreviewImage;
            public Text nameText;
            public Text classText;
            public Text descriptionText;
            public Image hullSwatch;
            public Image accentSwatch;
            public Button equipButton;
            public Text equipButtonText;
            public Outline cardOutline;
        }

        [Header("Header Elements")]
        public Text creditsText;

        [Header("5 Jet Skin Cards")]
        public JetSkinCardUI[] skinCards;

        [Header("Tabs & Screens")]
        public Button tabSkinsButton;
        public Button tabUpgradesButton;
        public GameObject skinsContainer;
        public GameObject upgradesContainer;
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
            SwitchToSkinsTab(false);
            RefreshUI();
            if (PlayerController.Instance)
            {
                PlayerController.Instance.ApplySkin();
            }
        }

        public void BindButtons()
        {
            if (backButton)
            {
                backButton.onClick.RemoveListener(OnBackClicked);
                backButton.onClick.AddListener(OnBackClicked);
            }

            if (tabSkinsButton)
            {
                tabSkinsButton.onClick.RemoveListener(OnTabSkinsClicked);
                tabSkinsButton.onClick.AddListener(OnTabSkinsClicked);
            }

            if (tabUpgradesButton)
            {
                tabUpgradesButton.onClick.RemoveListener(OnTabUpgradesClicked);
                tabUpgradesButton.onClick.AddListener(OnTabUpgradesClicked);
            }

            if (skinCards != null)
            {
                for (int i = 0; i < skinCards.Length; i++)
                {
                    int index = i;
                    var card = skinCards[i];
                    if (card == null) continue;

                    if (card.equipButton != null)
                    {
                        card.equipButton.onClick.RemoveAllListeners();
                        card.equipButton.onClick.AddListener(() => OnCardEquipClicked(index));
                    }

                    if (card.cardObject != null)
                    {
                        Button cardBtn = card.cardObject.GetComponent<Button>();
                        if (cardBtn == null)
                        {
                            cardBtn = card.cardObject.AddComponent<Button>();
                            cardBtn.transition = Selectable.Transition.ColorTint;
                        }
                        cardBtn.onClick.RemoveAllListeners();
                        cardBtn.onClick.AddListener(() => OnCardEquipClicked(index));
                    }

                    if (card.jetPreviewImage != null)
                    {
                        Button imgBtn = card.jetPreviewImage.GetComponent<Button>();
                        if (imgBtn == null)
                        {
                            imgBtn = card.jetPreviewImage.gameObject.AddComponent<Button>();
                            imgBtn.transition = Selectable.Transition.ColorTint;
                        }
                        imgBtn.onClick.RemoveAllListeners();
                        imgBtn.onClick.AddListener(() => OnCardEquipClicked(index));
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

        public void RefreshUI()
        {
            if (creditsText)
            {
                creditsText.text = $"COINS: <color=#FFD700>{SaveManager.Coins:N0}</color>";
            }

            int selectedSkin = SaveManager.SelectedSkinIndex;

            if (skinCards != null)
            {
                for (int i = 0; i < skinCards.Length; i++)
                {
                    var card = skinCards[i];
                    if (card == null) continue;

                    var skin = JetSkinManager.GetSkin(card.skinId);
                    if (skin == null) continue;

                    if (card.nameText) card.nameText.text = skin.name.ToUpper();
                    if (card.classText) card.classText.text = skin.classTitle;
                    if (card.descriptionText) card.descriptionText.text = skin.description;
                    if (card.hullSwatch) card.hullSwatch.color = skin.hullColor;
                    if (card.accentSwatch) card.accentSwatch.color = skin.accentColor;

                    bool isEquipped = (card.skinId == selectedSkin);

                    if (card.equipButton != null)
                    {
                        if (isEquipped)
                        {
                            if (card.equipButtonText) card.equipButtonText.text = "✓ EQUIPPED";
                            card.equipButton.interactable = false;
                            card.equipButton.image.color = new Color(0.1f, 0.9f, 0.45f, 1f);
                            if (card.cardOutline) card.cardOutline.effectColor = new Color(0.1f, 1f, 0.6f, 0.95f);
                        }
                        else
                        {
                            if (card.equipButtonText) card.equipButtonText.text = "EQUIP JET";
                            card.equipButton.interactable = true;
                            card.equipButton.image.color = new Color(0f, 0.75f, 1f, 1f);
                            if (card.cardOutline) card.cardOutline.effectColor = new Color(0.2f, 0.75f, 1f, 0.85f);
                        }
                    }
                }
            }
        }

        public void OnCardEquipClicked(int cardIndex)
        {
            if (skinCards == null || cardIndex < 0 || cardIndex >= skinCards.Length) return;

            int targetSkinId = skinCards[cardIndex].skinId;
            SaveManager.SelectedSkinIndex = targetSkinId;
            JetSkinManager.NotifySkinEquipped(targetSkinId);

            if (PlayerController.Instance)
            {
                PlayerController.Instance.ApplySkin();
            }

            if (AudioManager.Instance)
            {
                AudioManager.Instance.PlaySound("click");
            }

            RefreshUI();
        }

        public void SwitchToSkinsTab(bool playSound = true)
        {
            if (skinsContainer) skinsContainer.SetActive(true);
            if (upgradesContainer) upgradesContainer.SetActive(false);
            if (tabSkinsButton) tabSkinsButton.image.color = new Color(0f, 0.75f, 1f, 1f);
            if (tabUpgradesButton) tabUpgradesButton.image.color = new Color(0.32f, 0.48f, 0.68f, 0.95f);
            if (playSound && AudioManager.Instance) AudioManager.Instance.PlaySound("click");
        }

        public void SwitchToUpgradesTab(bool playSound = true)
        {
            if (skinsContainer) skinsContainer.SetActive(false);
            if (upgradesContainer) upgradesContainer.SetActive(true);
            if (tabSkinsButton) tabSkinsButton.image.color = new Color(0.32f, 0.48f, 0.68f, 0.95f);
            if (tabUpgradesButton) tabUpgradesButton.image.color = new Color(0f, 0.75f, 1f, 1f);
            if (playSound && AudioManager.Instance) AudioManager.Instance.PlaySound("click");
        }

        public void OnTabSkinsClicked() => SwitchToSkinsTab(true);
        public void OnTabUpgradesClicked() => SwitchToUpgradesTab(true);

        private void OnBackClicked()
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");

            if (PlayerController.Instance)
            {
                PlayerController.Instance.ApplySkin();
            }

            if (UIManager.Instance)
            {
                UIManager.Instance.ShowPanel(UIManager.Instance.mainMenuPanel);
            }
        }
    }
}
