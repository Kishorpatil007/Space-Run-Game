using UnityEngine;
using SpaceJet.Audio;
using SpaceJet.Core;
using SpaceJet.Player;
using SpaceJet.UI;

namespace SpaceJet.Collectibles
{
    public enum PowerUpType
    {
        Shield,
        EnergySurge,
        BoostRefill
    }

    public class PowerUp : MonoBehaviour
    {
        public PowerUpType powerUpType = PowerUpType.Shield;
        public float rotationSpeed = 100f;
        public float ringRotationSpeed = 200f;
        public Transform innerRings;
        public Renderer orbRenderer;

        private bool collected = false;

        private void OnEnable()
        {
            collected = false;
        }

        public void ConfigureType(PowerUpType type)
        {
            powerUpType = type;
            if (orbRenderer == null)
            {
                Transform orb = transform.Find("ShieldOrb");
                if (orb) orbRenderer = orb.GetComponent<Renderer>();
            }

            if (orbRenderer && orbRenderer.material)
            {
                switch (type)
                {
                    case PowerUpType.Shield:
                        orbRenderer.material.color = new Color(0.2f, 0.7f, 1f, 0.85f);
                        if (orbRenderer.material.HasProperty("_EmissionColor"))
                        {
                            orbRenderer.material.EnableKeyword("_EMISSION");
                            orbRenderer.material.SetColor("_EmissionColor", Color.cyan * 2.5f);
                        }
                        break;

                    case PowerUpType.EnergySurge:
                        orbRenderer.material.color = new Color(0.1f, 0.95f, 0.45f, 0.85f);
                        if (orbRenderer.material.HasProperty("_EmissionColor"))
                        {
                            orbRenderer.material.EnableKeyword("_EMISSION");
                            orbRenderer.material.SetColor("_EmissionColor", new Color(0.1f, 1f, 0.4f) * 2.8f);
                        }
                        break;

                    case PowerUpType.BoostRefill:
                        orbRenderer.material.color = new Color(1f, 0.65f, 0.1f, 0.85f);
                        if (orbRenderer.material.HasProperty("_EmissionColor"))
                        {
                            orbRenderer.material.EnableKeyword("_EMISSION");
                            orbRenderer.material.SetColor("_EmissionColor", new Color(1f, 0.65f, 0.1f) * 2.5f);
                        }
                        break;
                }
            }
        }

        private void Update()
        {
            if (collected) return;

            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

            if (innerRings)
            {
                innerRings.Rotate(Vector3.forward, ringRotationSpeed * Time.deltaTime, Space.Self);
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

            switch (powerUpType)
            {
                case PowerUpType.Shield:
                    if (PlayerShield.Instance) PlayerShield.Instance.ActivateShield();
                    if (PlayerEnergy.Instance) PlayerEnergy.Instance.AddEnergy(20f); // Shield collection also gives bonus fuel
                    if (GameManager.Instance) GameManager.Instance.AddShieldCollected();
                    if (AudioManager.Instance) AudioManager.Instance.PlaySound("shield");
                    FloatingTextSpawner.Spawn("SHIELD MATRIX ENGAGED!", transform.position, new Color(0.3f, 0.8f, 1f));
                    break;

                case PowerUpType.EnergySurge:
                    if (PlayerEnergy.Instance) PlayerEnergy.Instance.AddEnergy(60f); // Massive fuel renewal!
                    if (GameManager.Instance) GameManager.Instance.AddCrystal();
                    if (AudioManager.Instance) AudioManager.Instance.PlaySound("energy");
                    FloatingTextSpawner.Spawn("+60 ENERGY SURGE!", transform.position, new Color(0.15f, 1f, 0.45f));
                    break;

                case PowerUpType.BoostRefill:
                    if (PlayerBoost.Instance) PlayerBoost.Instance.currentBoost = PlayerBoost.Instance.maxBoost;
                    if (PlayerEnergy.Instance) PlayerEnergy.Instance.AddEnergy(25f);
                    if (AudioManager.Instance) AudioManager.Instance.PlaySound("boost");
                    FloatingTextSpawner.Spawn("BOOST TANK 100%!", transform.position, new Color(1f, 0.75f, 0.2f));
                    break;
            }

            gameObject.SetActive(false);
        }
    }
}
