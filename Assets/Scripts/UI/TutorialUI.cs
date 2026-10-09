using UnityEngine;
using UnityEngine.UI;
using SpaceJet.Audio;
using SpaceJet.Core;

namespace SpaceJet.UI
{
    public class TutorialUI : MonoBehaviour
    {
        public Button launchButton;
        public float autoDismissSeconds = 15f;

        private float timer = 0f;

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
            timer = autoDismissSeconds;
        }

        public void BindButtons()
        {
            if (launchButton)
            {
                launchButton.onClick.RemoveListener(OnLaunchClicked);
                launchButton.onClick.AddListener(OnLaunchClicked);
            }
        }

        private void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0f || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                OnLaunchClicked();
            }
        }

        private void OnLaunchClicked()
        {
            if (AudioManager.Instance) AudioManager.Instance.PlaySound("click");
            if (GameManager.Instance) GameManager.Instance.DismissTutorial();
        }
    }
}
