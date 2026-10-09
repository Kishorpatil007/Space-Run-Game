using UnityEngine;
using SpaceJet.Audio;
using SpaceJet.Core;
using SpaceJet.Player;
using SpaceJet.UI;

namespace SpaceJet.Environment
{
    public class EnergyBarrier : MonoBehaviour
    {
        public float damage = 30f;
        public float pulseSpeed = 4f;
        public Renderer barrierRenderer;

        private Material barrierMat;
        private bool hasCollided = false;

        private void Start()
        {
            if (barrierRenderer)
            {
                barrierMat = barrierRenderer.material;
            }
        }

        private void OnEnable()
        {
            hasCollided = false;
        }

        private void Update()
        {
            if (barrierMat)
            {
                float pulse = 0.6f + Mathf.PingPong(Time.time * pulseSpeed, 0.4f);
                barrierMat.color = new Color(1f, 0.2f, 0.1f, pulse);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasCollided) return;

            if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
            {
                hasCollided = true;

                if (PlayerShield.Instance != null && PlayerShield.Instance.IsShieldActive)
                {
                    PlayerShield.Instance.AbsorbImpact();
                    FloatingTextSpawner.Spawn("BARRIER ABSORPTION!", transform.position, Color.cyan);
                }
                else if (PlayerHealth.Instance != null)
                {
                    PlayerHealth.Instance.TakeDamage(damage, "Energy Barrier");
                    int hitsLeft = PlayerHealth.Instance.collisionsRemaining;
                    FloatingTextSpawner.Spawn($"HIT! ({hitsLeft}/3 LEFT) -25 NRG", transform.position, Color.red);
                }

                gameObject.SetActive(false);
            }
        }
    }
}
