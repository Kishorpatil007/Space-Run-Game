using System;
using UnityEngine;
using SpaceJet.Audio;
using SpaceJet.Core;

namespace SpaceJet.Player
{
    public class PlayerShield : MonoBehaviour
    {
        public static PlayerShield Instance { get; set; }

        [Header("Shield Configuration")]
        public float baseDuration = 8.0f;
        public float remainingShieldTime = 0f;
        public GameObject shieldVisualObject;

        public bool IsShieldActive => remainingShieldTime > 0f;

        public event Action<bool, float> OnShieldStateChanged; // isActive, remainingSeconds

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (shieldVisualObject) shieldVisualObject.SetActive(false);
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            if (remainingShieldTime > 0f)
            {
                remainingShieldTime -= Time.deltaTime;

                if (shieldVisualObject && !shieldVisualObject.activeSelf)
                {
                    shieldVisualObject.SetActive(true);
                }

                // Rotate visual shield slightly
                if (shieldVisualObject)
                {
                    shieldVisualObject.transform.Rotate(Vector3.up, 60f * Time.deltaTime, Space.Self);
                }

                if (remainingShieldTime <= 0f)
                {
                    remainingShieldTime = 0f;
                    if (shieldVisualObject) shieldVisualObject.SetActive(false);
                    OnShieldStateChanged?.Invoke(false, 0f);
                }
                else
                {
                    OnShieldStateChanged?.Invoke(true, remainingShieldTime);
                }
            }
        }

        public void ActivateShield()
        {
            int shieldLvl = SaveManager.ShieldLevel;
            float totalDuration = baseDuration + (shieldLvl - 1) * 1.5f + CharacterManager.GetActiveShieldBonus();
            remainingShieldTime = totalDuration;

            if (shieldVisualObject)
            {
                shieldVisualObject.SetActive(true);
            }

            if (AudioManager.Instance)
            {
                AudioManager.Instance.PlaySound("shield");
            }

            OnShieldStateChanged?.Invoke(true, remainingShieldTime);
        }

        public void AbsorbImpact()
        {
            if (AudioManager.Instance)
            {
                AudioManager.Instance.PlaySound("shield", 0.7f);
            }
            if (CameraController.Instance)
            {
                CameraController.Instance.TriggerShake(0.2f, 0.2f);
            }
        }

        public void ResetShield()
        {
            remainingShieldTime = 0f;
            if (shieldVisualObject) shieldVisualObject.SetActive(false);
            OnShieldStateChanged?.Invoke(false, 0f);
        }
    }
}
