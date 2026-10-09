using UnityEngine;
using UnityEngine.UI;
using SpaceJet.Audio;
using SpaceJet.Core;
using SpaceJet.Player;

namespace SpaceJet.UI
{
    public class CharacterSelectUI : MonoBehaviour
    {
        [System.Serializable]
        public class CharacterCardUI
        {
            public int characterId;
            public GameObject cardObject;
            public Text nameText;
            public Text callsignText;
            public Text perkText;
            public Image emblemImage;
            public Button selectButton;
            public Text buttonLabel;
        }

        [Header("Header Elements")]
        public Text coinsBalanceText;
        public CharacterCardUI[] pilotCards;

        [Header("Navigation")]
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
            RefreshUI();
        }

        public void BindButtons()
        {
            if (backButton)
            {
                backButton.onClick.RemoveListener(OnBackClicked);
                backButton.onClick.AddListener(OnBackClicked);
            }

            if (pilotCards != null)
            {
                for (int i = 0; i < pilotCards.Length; i++)
                {
                    int index = i;
                    var card = pilotCards[i];
                    if (card != null && card.selectButton != null)
                    {
                        card.selectButton.onClick.RemoveAllListeners();
                        card.selectButton.onClick.AddListener(() => OnCardButtonClicked(index));
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
            if (coinsBalanceText)
            {
                coinsBalanceText.text = $"COINS: {SaveManager.Coins:N0}";
            }

            int selectedId = SaveManager.SelectedCharacterIndex;

            if (pilotCards != null)
            {
                for (int i = 0; i < pilotCards.Length; i++)
                {
                    var card = pilotCards[i];
                    if (card == null) continue;

                    var def = CharacterManager.GetCharacter(card.characterId);
                    if (def == null) continue;

                    if (card.nameText) card.nameText.text = def.pilotName;
                    if (card.callsignText) card.callsignText.text = $"[ {def.callsign} ]";
                    if (card.perkText)
                    {
                        // Clean, punchy perk description without multi-line clutter
                        string perkClean = def.perkDescription.Replace("✦ ", "").Trim().ToUpper();
                        card.perkText.text = $"<color=#00FFFF>✦ {perkClean}</color>";
                    }
                    if (card.emblemImage) card.emblemImage.color = def.pilotColor;

                    bool isUnlocked = CharacterManager.IsCharacterUnlocked(def.id);
                    bool isEquipped = (def.id == selectedId);

                    Image btnImg = card.selectButton != null ? card.selectButton.GetComponent<Image>() : null;
                    Outline btnOl = card.selectButton != null ? card.selectButton.GetComponent<Outline>() : null;

                    if (card.buttonLabel)
                    {
                        if (isEquipped)
                        {
                            card.buttonLabel.text = "✓ EQUIPPED";
                            if (card.selectButton) card.selectButton.interactable = false;
                            if (btnImg) btnImg.color = new Color(0.12f, 0.72f, 0.45f); // Vibrant emerald
                            if (btnOl) btnOl.effectColor = new Color(0.25f, 1f, 0.65f, 0.95f);
                        }
                        else if (isUnlocked)
                        {
                            card.buttonLabel.text = "SELECT";
                            if (card.selectButton) card.selectButton.interactable = true;
                            if (btnImg) btnImg.color = new Color(0f, 0.78f, 1f); // Vibrant bright electric cyan
                            if (btnOl) btnOl.effectColor = new Color(0.45f, 0.95f, 1f, 0.95f);
                        }
                        else
                        {
                            card.buttonLabel.text = $"UNLOCK ({def.unlockCost})";
                            bool canAfford = (SaveManager.Coins >= def.unlockCost);
                            if (card.selectButton) card.selectButton.interactable = canAfford;
                            if (btnImg) btnImg.color = canAfford ? new Color(1f, 0.75f, 0.12f) : new Color(0.45f, 0.4f, 0.35f); // Radiant 24K gold
                            if (btnOl) btnOl.effectColor = canAfford ? new Color(1f, 0.95f, 0.45f, 0.95f) : new Color(0.35f, 0.35f, 0.35f, 0.6f);
                        }
                    }
                }
            }
        }

        private void OnCardButtonClicked(int index)
        {
            if (pilotCards == null || index < 0 || index >= pilotCards.Length) return;
            var card = pilotCards[index];
            var def = CharacterManager.GetCharacter(card.characterId);

            if (CharacterManager.IsCharacterUnlocked(def.id))
            {
                CharacterManager.EquipCharacter(def.id);
                if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
            }
            else
            {
                if (CharacterManager.UnlockCharacter(def.id))
                {
                    if (AudioManager.Instance) AudioManager.Instance.PlaySound("shield");
                    FloatingTextSpawner.Spawn("PILOT UNLOCKED!", transform.position, Color.yellow);
                }
                else
                {
                    if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
                }
            }

            RefreshUI();
        }

        private void OnBackClicked()
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
            if (UIManager.Instance)
            {
                UIManager.Instance.ShowPanel(UIManager.Instance.mainMenuPanel);
            }
        }
    }
}
