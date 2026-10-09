using UnityEngine;
using UnityEngine.UI;
using SpaceJet.Audio;
using SpaceJet.Core;

namespace SpaceJet.UI
{
    public class GarageUI : MonoBehaviour
    {
        [Header("Coin Balance")]
        public Text coinsBalanceText;

        [Header("Engine Upgrade")]
        public Text engineLevelText;
        public Image engineBarFill;
        public Text engineCostText;
        public Button engineUpgradeBtn;

        [Header("Energy Tank Upgrade")]
        public Text energyLevelText;
        public Image energyBarFill;
        public Text energyCostText;
        public Button energyUpgradeBtn;

        [Header("Shield Generator Upgrade")]
        public Text shieldLevelText;
        public Image shieldBarFill;
        public Text shieldCostText;
        public Button shieldUpgradeBtn;

        [Header("Hull Armor Upgrade")]
        public Text hullLevelText;
        public Image hullBarFill;
        public Text hullCostText;
        public Button hullUpgradeBtn;

        [Header("Navigation")]
        public Button backButton;

        private readonly int[] engineCosts = { 500, 750, 1100, 1600 };
        private readonly int[] energyCosts = { 600, 850, 1200, 1700 };
        private readonly int[] shieldCosts = { 750, 1000, 1400, 2000 };
        private readonly int[] hullCosts   = { 500, 800, 1150, 1650 };

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
            if (engineUpgradeBtn)
            {
                engineUpgradeBtn.onClick.RemoveListener(UpgradeEngine);
                engineUpgradeBtn.onClick.AddListener(UpgradeEngine);
            }
            if (energyUpgradeBtn)
            {
                energyUpgradeBtn.onClick.RemoveListener(UpgradeEnergy);
                energyUpgradeBtn.onClick.AddListener(UpgradeEnergy);
            }
            if (shieldUpgradeBtn)
            {
                shieldUpgradeBtn.onClick.RemoveListener(UpgradeShield);
                shieldUpgradeBtn.onClick.AddListener(UpgradeShield);
            }
            if (hullUpgradeBtn)
            {
                hullUpgradeBtn.onClick.RemoveListener(UpgradeHull);
                hullUpgradeBtn.onClick.AddListener(UpgradeHull);
            }
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

        public void RefreshUI()
        {
            int coins = SaveManager.Coins;
            if (coinsBalanceText) coinsBalanceText.text = $"CREDITS: {coins:D4} COINS";

            // Engine
            int engLvl = SaveManager.EngineLevel;
            SetupCard(engLvl, engineLevelText, engineBarFill, engineCostText, engineUpgradeBtn, engineCosts, coins);

            // Energy
            int nrgLvl = SaveManager.EnergyTankLevel;
            SetupCard(nrgLvl, energyLevelText, energyBarFill, energyCostText, energyUpgradeBtn, energyCosts, coins);

            // Shield
            int shdLvl = SaveManager.ShieldLevel;
            SetupCard(shdLvl, shieldLevelText, shieldBarFill, shieldCostText, shieldUpgradeBtn, shieldCosts, coins);

            // Hull
            int hulLvl = SaveManager.HullLevel;
            SetupCard(hulLvl, hullLevelText, hullBarFill, hullCostText, hullUpgradeBtn, hullCosts, coins);
        }

        private void SetupCard(int currentLevel, Text lvlText, Image fill, Text costText, Button btn, int[] costs, int playerCoins)
        {
            if (lvlText) lvlText.text = (currentLevel >= 5) ? "LVL 5 [MAX]" : $"LVL {currentLevel}/5";
            if (fill) fill.fillAmount = currentLevel / 5f;

            if (currentLevel >= 5)
            {
                if (costText) costText.text = "MAXED OUT";
                if (btn) btn.interactable = false;
            }
            else
            {
                int cost = costs[currentLevel - 1];
                if (costText) costText.text = $"{cost} COINS";
                if (btn) btn.interactable = (playerCoins >= cost);
            }
        }

        private void UpgradeEngine()
        {
            int lvl = SaveManager.EngineLevel;
            if (lvl < 5 && SaveManager.SpendCoins(engineCosts[lvl - 1]))
            {
                SaveManager.EngineLevel++;
                OnPurchaseSuccess();
            }
        }

        private void UpgradeEnergy()
        {
            int lvl = SaveManager.EnergyTankLevel;
            if (lvl < 5 && SaveManager.SpendCoins(energyCosts[lvl - 1]))
            {
                SaveManager.EnergyTankLevel++;
                OnPurchaseSuccess();
            }
        }

        private void UpgradeShield()
        {
            int lvl = SaveManager.ShieldLevel;
            if (lvl < 5 && SaveManager.SpendCoins(shieldCosts[lvl - 1]))
            {
                SaveManager.ShieldLevel++;
                OnPurchaseSuccess();
            }
        }

        private void UpgradeHull()
        {
            int lvl = SaveManager.HullLevel;
            if (lvl < 5 && SaveManager.SpendCoins(hullCosts[lvl - 1]))
            {
                SaveManager.HullLevel++;
                OnPurchaseSuccess();
            }
        }

        private void OnPurchaseSuccess()
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySound("coin");
            RefreshUI();
        }

        private void OnBackClicked()
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
            if (UIManager.Instance) UIManager.Instance.ShowPanel(UIManager.Instance.mainMenuPanel);
        }
    }
}
