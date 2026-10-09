using UnityEngine;
using UnityEngine.UI;
using SpaceJet.Audio;
using SpaceJet.Core;
using SpaceJet.Player;

namespace SpaceJet.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; set; }

        [Header("Screens & Panels")]
        public GameObject mainMenuPanel;
        public GameObject missionSelectPanel;
        public GameObject briefingPanel;
        public GameObject hudPanel;
        public GameObject tutorialPanel;
        public GameObject pauseMenuPanel;
        public GameObject resultPanel;
        public GameObject garagePanel;
        public GameObject achievementsPanel;
        public GameObject settingsPanel;
        public GameObject jetSkinPanel;
        public GameObject characterSelectPanel;

        [Header("Controllers")]
        public HUDController hudController;
        public MissionBriefingUI briefingUI;
        public MissionResultUI resultUI;
        public CharacterSelectUI characterSelectUI;

        [Header("Pause Menu Buttons")]
        public Button resumeButton;
        public Button restartButton;
        public Button pauseSettingsButton;
        public Button pauseMainMenuButton;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            SetupPauseButtons();
        }

        private void Start()
        {
            SetupPauseButtons();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += HandleStateChanged;
                HandleStateChanged(GameManager.Instance.CurrentState);
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        public void SetupPauseButtons()
        {
            if (resumeButton)
            {
                resumeButton.onClick.RemoveListener(OnResumeClicked);
                resumeButton.onClick.AddListener(OnResumeClicked);
            }
            if (restartButton)
            {
                restartButton.onClick.RemoveListener(OnRestartClicked);
                restartButton.onClick.AddListener(OnRestartClicked);
            }
            if (pauseSettingsButton)
            {
                pauseSettingsButton.onClick.RemoveListener(OnPauseSettingsClicked);
                pauseSettingsButton.onClick.AddListener(OnPauseSettingsClicked);
            }
            if (pauseMainMenuButton)
            {
                pauseMainMenuButton.onClick.RemoveListener(OnPauseMainMenuClicked);
                pauseMainMenuButton.onClick.AddListener(OnPauseMainMenuClicked);
            }
        }

        public void ShowPanel(GameObject panelToShow)
        {
            HideAllPanels();
            if (panelToShow != null)
            {
                panelToShow.SetActive(true);
            }

            // Ensure 3D player jet is hidden on MainMenu so the 2D Home UI artwork remains pristine
            if (panelToShow == mainMenuPanel)
            {
                if (PlayerController.Instance) PlayerController.Instance.gameObject.SetActive(false);
            }
            else if (panelToShow == hudPanel || panelToShow == garagePanel)
            {
                if (PlayerController.Instance) PlayerController.Instance.gameObject.SetActive(true);
            }
        }

        public void HideAllPanels()
        {
            if (mainMenuPanel) mainMenuPanel.SetActive(false);
            if (missionSelectPanel) missionSelectPanel.SetActive(false);
            if (briefingPanel) briefingPanel.SetActive(false);
            if (hudPanel) hudPanel.SetActive(false);
            if (tutorialPanel) tutorialPanel.SetActive(false);
            if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
            if (resultPanel) resultPanel.SetActive(false);
            if (garagePanel) garagePanel.SetActive(false);
            if (achievementsPanel) achievementsPanel.SetActive(false);
            if (settingsPanel) settingsPanel.SetActive(false);
            if (jetSkinPanel) jetSkinPanel.SetActive(false);
            if (characterSelectPanel) characterSelectPanel.SetActive(false);
        }

        private void HandleStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.MainMenu:
                    ShowPanel(mainMenuPanel);
                    if (PlayerController.Instance) PlayerController.Instance.gameObject.SetActive(false);
                    break;

                case GameState.MissionBriefing:
                    ShowPanel(briefingPanel);
                    if (briefingUI) briefingUI.PopulateBriefing();
                    break;

                case GameState.Tutorial:
                    ShowPanel(tutorialPanel);
                    break;

                case GameState.Playing:
                case GameState.CinematicArrival:
                    ShowPanel(hudPanel);
                    if (PlayerController.Instance)
                    {
                        PlayerController.Instance.gameObject.SetActive(true);
                        PlayerController.Instance.ApplySkin();
                    }
                    if (hudController) hudController.UpdateAllElements();
                    break;

                case GameState.Paused:
                    if (pauseMenuPanel) pauseMenuPanel.SetActive(true);
                    break;

                case GameState.MissionComplete:
                    ShowPanel(resultPanel);
                    if (resultUI) resultUI.ShowVictory();
                    break;

                case GameState.MissionFailed:
                    ShowPanel(resultPanel);
                    if (resultUI) resultUI.ShowDefeat();
                    break;
            }
        }

        private void OnResumeClicked()
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
            if (GameManager.Instance) GameManager.Instance.ResumeGame();
        }

        private void OnRestartClicked()
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
            if (GameManager.Instance) GameManager.Instance.RestartCurrentMission();
        }

        private void OnPauseSettingsClicked()
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
            ShowPanel(settingsPanel);
        }

        private void OnPauseMainMenuClicked()
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
            if (GameManager.Instance) GameManager.Instance.ReturnToMainMenu();
        }
    }
}
