using UnityEngine;
using UnityEngine.UI;
using SpaceJet.Audio;
using SpaceJet.Core;

namespace SpaceJet.UI
{
    public class SettingsUI : MonoBehaviour
    {
        public Slider masterSlider;
        public Slider musicSlider;
        public Slider sfxSlider;
        public Button resetDataButton;
        public Button backButton;

        private void Awake()
        {
            BindButtons();
        }

        private void Start()
        {
            BindButtons();
        }

        public void BindButtons()
        {
            if (masterSlider)
            {
                masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
                masterSlider.onValueChanged.AddListener(OnMasterChanged);
            }
            if (musicSlider)
            {
                musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
                musicSlider.onValueChanged.AddListener(OnMusicChanged);
            }
            if (sfxSlider)
            {
                sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
                sfxSlider.onValueChanged.AddListener(OnSfxChanged);
            }
            if (resetDataButton)
            {
                resetDataButton.onClick.RemoveListener(OnResetData);
                resetDataButton.onClick.AddListener(OnResetData);
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

        private void OnEnable()
        {
            if (AudioManager.Instance)
            {
                if (masterSlider) masterSlider.value = AudioManager.Instance.masterVolume;
                if (musicSlider) musicSlider.value = AudioManager.Instance.musicVolume;
                if (sfxSlider) sfxSlider.value = AudioManager.Instance.sfxVolume;
            }
        }

        private void OnMasterChanged(float val)
        {
            if (AudioManager.Instance)
            {
                AudioManager.Instance.masterVolume = val;
                AudioManager.Instance.UpdateVolumes();
            }
        }

        private void OnMusicChanged(float val)
        {
            if (AudioManager.Instance)
            {
                AudioManager.Instance.musicVolume = val;
                AudioManager.Instance.UpdateVolumes();
            }
        }

        private void OnSfxChanged(float val)
        {
            if (AudioManager.Instance)
            {
                AudioManager.Instance.sfxVolume = val;
                AudioManager.Instance.UpdateVolumes();
            }
        }

        private void OnResetData()
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
            SaveManager.ResetAllData();
        }

        private void OnBackClicked()
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
            if (UIManager.Instance)
            {
                // Return to pause menu if paused, otherwise main menu
                if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Paused)
                {
                    UIManager.Instance.ShowPanel(UIManager.Instance.pauseMenuPanel);
                }
                else
                {
                    UIManager.Instance.ShowPanel(UIManager.Instance.mainMenuPanel);
                }
            }
        }
    }
}
