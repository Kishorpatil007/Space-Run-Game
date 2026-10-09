using System;
using System.Collections;
using UnityEngine;
using SpaceJet.Audio;
using SpaceJet.Core;

namespace SpaceJet.Player
{
    /// <summary>
    /// Handles player hull integrity, obstacle damage, invulnerability frames, and destruction.
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        public static PlayerHealth Instance { get; set; }

        [Header("Health & Collision Settings")]
        public const int MAX_COLLISIONS = 3;
        public int collisionsRemaining = 3;
        public float maxHealth = 100f;
        public float currentHealth;
        public float invulnerabilityDuration = 1.2f;

        private bool isInvulnerable = false;
        private Renderer[] jetRenderers;

        public event Action<float, float> OnHealthChanged; // current, max
        public event Action OnPlayerDestroyed;

        private void Awake()
        {
            Instance = this;
            jetRenderers = GetComponentsInChildren<Renderer>(true);
        }

        private void Start()
        {
            ApplyUpgrades();
            ResetHealth();
        }

        public void ApplyUpgrades()
        {
            // Hull upgrade adds +15 HP per level (Level 1: 100, Level 5: 160)
            int hullLvl = SaveManager.HullLevel;
            maxHealth = 100f + (hullLvl - 1) * 15f;
        }

        public void ResetHealth()
        {
            collisionsRemaining = MAX_COLLISIONS;
            currentHealth = maxHealth;
            isInvulnerable = false;
            StopAllCoroutines();
            if (jetRenderers == null || jetRenderers.Length == 0)
            {
                jetRenderers = GetComponentsInChildren<Renderer>(true);
            }
            SetRenderersVisible(true);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void TakeDamage(float amount, string sourceName = "Obstacle")
        {
            if (isInvulnerable) return;

            // Check if shield is currently active
            if (PlayerShield.Instance && PlayerShield.Instance.IsShieldActive)
            {
                // Shield absorbs 100% of the impact!
                PlayerShield.Instance.AbsorbImpact();
                return;
            }

            // Deduct 1 of the 3 collision lives
            collisionsRemaining = Mathf.Max(0, collisionsRemaining - 1);
            currentHealth = ((float)collisionsRemaining / MAX_COLLISIONS) * maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // Asteroid collision also reduces energy
            if (PlayerEnergy.Instance != null)
            {
                PlayerEnergy.Instance.ConsumeEnergy(25f);
            }

            if (GameManager.Instance)
            {
                GameManager.Instance.RecordCollision();
                if (MissionManager.Instance)
                {
                    int healthPct = Mathf.RoundToInt((currentHealth / maxHealth) * 100f);
                    MissionManager.Instance.NotifyProgress(MissionObjective.ObjectiveType.MaintainMinHealth, healthPct);
                }
            }

            // Play collision audio
            if (AudioManager.Instance)
            {
                AudioManager.Instance.PlaySound("collision");
            }

            // Trigger camera shake
            if (CameraController.Instance)
            {
                CameraController.Instance.TriggerShake(0.55f, 0.4f);
            }

            // Game over only when jet collides with asteroid 3 times
            if (collisionsRemaining <= 0)
            {
                Die("3 ASTEROID COLLISIONS - HULL DESTROYED");
            }
            else
            {
                StartCoroutine(InvulnerabilityRoutine());
            }
        }

        public void Heal(float amount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private IEnumerator InvulnerabilityRoutine()
        {
            isInvulnerable = true;
            float elapsed = 0f;
            float blinkInterval = 0.1f;

            while (elapsed < invulnerabilityDuration)
            {
                SetRenderersVisible(false);
                yield return new WaitForSeconds(blinkInterval);
                SetRenderersVisible(true);
                yield return new WaitForSeconds(blinkInterval);
                elapsed += blinkInterval * 2f;
            }

            SetRenderersVisible(true);
            isInvulnerable = false;
        }

        private void SetRenderersVisible(bool visible)
        {
            if (jetRenderers == null) return;
            foreach (var r in jetRenderers)
            {
                if (r) r.enabled = visible;
            }
        }

        public void Die(string reason = "3 ASTEROID COLLISIONS - JET DESTROYED")
        {
            OnPlayerDestroyed?.Invoke();
            SetRenderersVisible(false);

            if (GameManager.Instance)
            {
                GameManager.Instance.FailMission(reason);
            }
        }

        public float HealthPercent => Mathf.Clamp01(currentHealth / maxHealth);
    }
}
