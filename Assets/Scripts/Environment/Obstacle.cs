using UnityEngine;
using SpaceJet.Audio;
using SpaceJet.Core;
using SpaceJet.Player;
using SpaceJet.UI;

namespace SpaceJet.Environment
{
    public enum ObstacleType
    {
        SmallAsteroid,   // 20 damage
        MediumAsteroid,  // 20 damage
        LargeAsteroid,   // 30 damage
        SpaceDebris,     // 25 damage
        EnergyBarrier    // 30 damage
    }

    public class Obstacle : MonoBehaviour
    {
        public ObstacleType type = ObstacleType.MediumAsteroid;
        public float damageAmount = 20f;
        public Vector3 tumbleAxis;
        public float tumbleSpeed = 45f;

        private bool hasCollided = false;
        private bool hasPassedPlayer = false;

        private void Start()
        {
            SetupDamageAndTumble();
        }

        private void OnEnable()
        {
            hasCollided = false;
            hasPassedPlayer = false;
            SetupDamageAndTumble();
        }

        private void SetupDamageAndTumble()
        {
            tumbleAxis = Random.onUnitSphere;
            tumbleSpeed = Random.Range(20f, 65f);

            switch (type)
            {
                case ObstacleType.SmallAsteroid:
                    damageAmount = 20f;
                    break;
                case ObstacleType.MediumAsteroid:
                    damageAmount = 20f;
                    break;
                case ObstacleType.LargeAsteroid:
                    damageAmount = 30f;
                    break;
                case ObstacleType.SpaceDebris:
                    damageAmount = 25f;
                    break;
                case ObstacleType.EnergyBarrier:
                    damageAmount = 30f;
                    break;
            }
        }

        private void Update()
        {
            // Tumble rotation
            transform.Rotate(tumbleAxis, tumbleSpeed * Time.deltaTime, Space.Self);

            // Check if passed player safely without collision
            if (!hasPassedPlayer && !hasCollided && PlayerController.Instance != null)
            {
                if (transform.position.z < PlayerController.Instance.transform.position.z - 2f)
                {
                    hasPassedPlayer = true;
                    if (GameManager.Instance)
                    {
                        GameManager.Instance.AddAsteroidAvoided();

                        // Rewarding near-miss dodge feedback
                        float xyDist = Vector2.Distance(
                            new Vector2(transform.position.x, transform.position.y),
                            new Vector2(PlayerController.Instance.transform.position.x, PlayerController.Instance.transform.position.y)
                        );
                        if (xyDist < 5.0f)
                        {
                            FloatingTextSpawner.Spawn("★ DODGED! +5", transform.position, new Color(0.1f, 1f, 0.6f));
                            GameManager.Instance.AddCoins(5);
                        }
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasCollided) return;

            if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
            {
                hasCollided = true;

                // Check if player has active shield
                if (PlayerShield.Instance != null && PlayerShield.Instance.IsShieldActive)
                {
                    PlayerShield.Instance.AbsorbImpact();
                    FloatingTextSpawner.Spawn("DEFLECTED!", transform.position, Color.cyan);
                }
                else if (PlayerHealth.Instance != null)
                {
                    PlayerHealth.Instance.TakeDamage(damageAmount, type.ToString());
                    int hitsLeft = PlayerHealth.Instance.collisionsRemaining;
                    FloatingTextSpawner.Spawn($"HIT! ({hitsLeft}/3 LEFT) -25 NRG", transform.position, Color.red);
                }

                // Deactivate obstacle upon impact
                gameObject.SetActive(false);
            }
        }
    }
}
