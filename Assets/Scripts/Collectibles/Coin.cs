using UnityEngine;
using SpaceJet.Audio;
using SpaceJet.Core;
using SpaceJet.Player;
using SpaceJet.UI;
using SpaceJet.Visuals;

namespace SpaceJet.Collectibles
{
    public enum CoinType
    {
        Normal,  // +10
        Gold,    // +25
        Cosmic   // +50
    }

    public class Coin : MonoBehaviour
    {
        public CoinType type = CoinType.Normal;
        public float rotationSpeed = 160f;
        public float bobSpeed = 3f;
        public float bobHeight = 0.25f;
        public float magnetDistance = 6.5f;
        public float magnetSpeed = 32f;

        private Vector3 startLocalPos;
        private int coinValue = 10;
        private bool collected = false;

        private void Start()
        {
            startLocalPos = transform.position;
            ConfigureType();
        }

        private void OnEnable()
        {
            collected = false;
            transform.localScale = Vector3.one;
            ConfigureType();
        }

        public void SetCoinType(CoinType newType)
        {
            type = newType;
            ConfigureType();
        }

        private void ConfigureType()
        {
            switch (type)
            {
                case CoinType.Normal:
                    coinValue = 10;
                    break;
                case CoinType.Gold:
                    coinValue = 25;
                    break;
                case CoinType.Cosmic:
                    coinValue = 50;
                    break;
            }

            Renderer r = GetComponent<Renderer>();
            if (r != null && r.material != null)
            {
                Color c = GetCoinColor();
                r.material.color = c;
                if (r.material.HasProperty("_EmissionColor"))
                {
                    r.material.SetColor("_EmissionColor", c * 0.45f);
                    r.material.EnableKeyword("_EMISSION");
                    r.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                }
                if (r.material.HasProperty("_Metallic")) r.material.SetFloat("_Metallic", 0.72f);
                if (r.material.HasProperty("_Glossiness")) r.material.SetFloat("_Glossiness", 0.88f);
            }

            Light lt = GetComponentInChildren<Light>();
            if (lt != null)
            {
                lt.color = GetCoinColor();
                lt.range = 5.0f;
                lt.intensity = 2.2f;
            }
        }

        private void Update()
        {
            if (collected) return;

            // Spin continuously with subtle 3D tilt precession so the minted face catches specular sun glints
            transform.Rotate(new Vector3(0.08f, 1f, 0.04f), rotationSpeed * Time.deltaTime, Space.World);

            // Subtle bob
            float newY = transform.position.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight * Time.deltaTime;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            // Subway Surfers style proximity magnet suction toward player
            if (PlayerController.Instance)
            {
                Vector3 playerPos = PlayerController.Instance.transform.position;
                float dist = Vector3.Distance(transform.position, playerPos);
                float totalMagnetDist = magnetDistance + CharacterManager.GetActiveMagnetBonus();
                if (dist < totalMagnetDist)
                {
                    // Exponential acceleration as coin draws near
                    float progress = 1f - Mathf.Clamp01(dist / totalMagnetDist);
                    float currentSpeed = magnetSpeed * (1f + progress * 2.5f);
                    transform.position = Vector3.MoveTowards(transform.position, playerPos, currentSpeed * Time.deltaTime);

                    // Satisfying scale shrink when being absorbed
                    if (dist < 2.0f)
                    {
                        float shrink = Mathf.Clamp01(dist / 2.0f);
                        transform.localScale = Vector3.one * Mathf.Max(0.25f, shrink);
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (collected) return;

            if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
            {
                Collect();
            }
        }

        private void Collect()
        {
            collected = true;

            if (GameManager.Instance)
            {
                GameManager.Instance.AddCoins(coinValue);
            }

            if (AudioManager.Instance)
            {
                AudioManager.Instance.PlayCoinSound();
            }

            if (ParticleEffectsManager.Instance)
            {
                ParticleEffectsManager.Instance.PlayCoinBurst(transform.position);
            }

            // Deactivate
            gameObject.SetActive(false);
        }

        private Color GetCoinColor()
        {
            switch (type)
            {
                case CoinType.Gold: return new Color(1.0f, 0.72f, 0.05f); // Rich Solar Amber Gold
                case CoinType.Cosmic: return new Color(1.0f, 0.94f, 0.35f); // Radiant Imperial 24K White-Gold
                default: return new Color(1.0f, 0.85f, 0.12f); // Pure Gleaming 24K Gold (#FFD700)
            }
        }
    }
}
