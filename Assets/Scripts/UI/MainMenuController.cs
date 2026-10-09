using UnityEngine;
using UnityEngine.UI;
using SpaceJet.Audio;
using SpaceJet.Core;
using SpaceJet.Player;

namespace SpaceJet.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Menu Buttons")]
        public Button playButton;
        public Button garageButton;
        public Button characterButton;
        public Button settingsButton;
        public Button missionsButton;
        public Button achievementsButton;
        public Button exitButton;

        [Header("Artwork UI Buttons")]
        public Button leaderboardButton;
        public Button tutorialButton;
        public Button topSettingsButton;
        public Button shopButton;

        [Header("Home Info Display")]
        public Text coinsDisplayText;
        public Text energyDisplayText;
        public Text currentLevelDisplayText;
        public Text activePilotText;
        public Text activeSkinText;

        [Header("Showcase Jet")]
        public Transform showcaseJet;
        public float jetOrbitSpeed = 25f;

        private void Awake()
        {
            BindButtons();
        }

        private void Start()
        {
            BindButtons();
            RefreshHomeUI();
        }

        private void OnEnable()
        {
            BindButtons();
            RefreshHomeUI();
            JetSkinManager.OnSkinEquipped -= HandleSkinEquipped;
            JetSkinManager.OnSkinEquipped += HandleSkinEquipped;

            // Hide 3D showcase jet while on Main Menu so uploaded 2D artwork is crisp and unobstructed
            if (PlayerController.Instance)
            {
                PlayerController.Instance.gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            JetSkinManager.OnSkinEquipped -= HandleSkinEquipped;

            // Restore player jet when navigating to gameplay or garage
            if (PlayerController.Instance)
            {
                PlayerController.Instance.gameObject.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            JetSkinManager.OnSkinEquipped -= HandleSkinEquipped;
        }

        private void HandleSkinEquipped(int skinId)
        {
            RefreshHomeUI();
        }

        public void RefreshHomeUI()
        {
            if (coinsDisplayText)
            {
                coinsDisplayText.text = $"{SaveManager.Coins:N0}";
            }

            if (currentLevelDisplayText)
            {
                int lvl = Mathf.Clamp(SaveManager.UnlockedLevel, 1, 5);
                var preset = MissionManager.GetMissionPreset(lvl);
                string pName = (preset != null && !string.IsNullOrEmpty(preset.planetName)) ? preset.planetName.Replace("Planet ", "") : "Earth";
                currentLevelDisplayText.text = $"{lvl}. {pName}";
            }

            if (activePilotText)
            {
                var pilot = CharacterManager.GetActiveCharacter();
                activePilotText.text = $"PILOT: <color=#{ColorUtility.ToHtmlStringRGB(pilot.pilotColor)}>{pilot.pilotName.ToUpper()}</color>";
            }

            if (activeSkinText)
            {
                var skin = JetSkinManager.GetSkin(SaveManager.SelectedSkinIndex);
                activeSkinText.text = $"JET: <color=#{ColorUtility.ToHtmlStringRGB(skin.accentColor)}>{skin.name.ToUpper()}</color>";
            }

            if (PlayerController.Instance)
            {
                PlayerController.Instance.ApplySkin();
            }
        }

        public void BindButtons()
        {
            if (playButton)
            {
                playButton.onClick.RemoveListener(OnPlayClicked);
                playButton.onClick.AddListener(OnPlayClicked);
            }
            if (garageButton)
            {
                garageButton.onClick.RemoveListener(OnGarageClicked);
                garageButton.onClick.AddListener(OnGarageClicked);
            }
            if (shopButton)
            {
                shopButton.onClick.RemoveListener(OnGarageClicked);
                shopButton.onClick.AddListener(OnGarageClicked);
            }
            if (characterButton)
            {
                characterButton.onClick.RemoveListener(OnCharacterClicked);
                characterButton.onClick.AddListener(OnCharacterClicked);
            }
            if (settingsButton)
            {
                settingsButton.onClick.RemoveListener(OnSettingsClicked);
                settingsButton.onClick.AddListener(OnSettingsClicked);
            }
            if (topSettingsButton)
            {
                topSettingsButton.onClick.RemoveListener(OnSettingsClicked);
                topSettingsButton.onClick.AddListener(OnSettingsClicked);
            }
            if (missionsButton)
            {
                missionsButton.onClick.RemoveListener(OnMissionsClicked);
                missionsButton.onClick.AddListener(OnMissionsClicked);
            }
            if (achievementsButton)
            {
                achievementsButton.onClick.RemoveListener(OnAchievementsClicked);
                achievementsButton.onClick.AddListener(OnAchievementsClicked);
            }
            if (leaderboardButton)
            {
                leaderboardButton.onClick.RemoveListener(OnLeaderboardClicked);
                leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
            }
            if (tutorialButton)
            {
                tutorialButton.onClick.RemoveListener(OnTutorialClicked);
                tutorialButton.onClick.AddListener(OnTutorialClicked);
            }
            if (exitButton)
            {
                exitButton.onClick.RemoveListener(OnExitClicked);
                exitButton.onClick.AddListener(OnExitClicked);
            }
        }

        private void Update()
        {
            if (showcaseJet != null && showcaseJet.gameObject.activeInHierarchy)
            {
                showcaseJet.Rotate(Vector3.up, jetOrbitSpeed * Time.deltaTime, Space.World);
            }

            // Keyboard shortcut to start playing directly with Enter or Space
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                OnPlayClicked();
            }
        }

        public void OnPlayClicked()
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
            int lvl = Mathf.Clamp(SaveManager.UnlockedLevel, 1, 5);
            if (GameManager.Instance) GameManager.Instance.StartMission(lvl);
        }

        public void OnMissionsClicked()
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
            if (UIManager.Instance) UIManager.Instance.ShowPanel(UIManager.Instance.missionSelectPanel);
        }

        public void OnGarageClicked()
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
            if (UIManager.Instance) UIManager.Instance.ShowPanel(UIManager.Instance.garagePanel);
        }

        public void OnCharacterClicked()
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
            if (UIManager.Instance) UIManager.Instance.ShowPanel(UIManager.Instance.characterSelectPanel);
        }

        public void OnAchievementsClicked()
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
            if (UIManager.Instance) UIManager.Instance.ShowPanel(UIManager.Instance.achievementsPanel);
        }

        public void OnLeaderboardClicked()
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
            // Show achievements as accomplishment board
            if (UIManager.Instance) UIManager.Instance.ShowPanel(UIManager.Instance.achievementsPanel);
        }

        public void OnTutorialClicked()
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
            if (UIManager.Instance) UIManager.Instance.ShowPanel(UIManager.Instance.tutorialPanel);
        }

        public void OnSettingsClicked()
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
            if (UIManager.Instance) UIManager.Instance.ShowPanel(UIManager.Instance.settingsPanel);
        }

        public void OnExitClicked()
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
