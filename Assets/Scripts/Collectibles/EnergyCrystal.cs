using UnityEngine;
using SpaceJet.Audio;
using SpaceJet.Core;
using SpaceJet.Player;
using SpaceJet.UI;

namespace SpaceJet.Collectibles
{
    public enum CrystalSize
    {
        Standard, // +20
        Large     // +40
    }

    public class EnergyCrystal : MonoBehaviour
    {
        public CrystalSize size = CrystalSize.Standard;
        public float rotationSpeed = 120f;
        public float bobSpeed = 2.5f;
        public float bobHeight = 0.35f;

        private float energyValue = 20f;
        private bool collected = false;

        private void Start()
        {
            ConfigureSize();
        }

        private void OnEnable()
        {
            collected = false;
        }

        public void SetSize(CrystalSize newSize)
        {
            size = newSize;
            ConfigureSize();
        }

        private void ConfigureSize()
        {
            energyValue = (size == CrystalSize.Large) ? 40f : 20f;
            transform.localScale = (size == CrystalSize.Large) ? Vector3.one * 1.5f : Vector3.one * 1.0f;
            Light lt = GetComponentInChildren<Light>();
            if (lt != null)
            {
                lt.range = (size == CrystalSize.Large) ? 8.5f : 6.5f;
                lt.intensity = (size == CrystalSize.Large) ? 3.5f : 2.8f;
            }
        }

        private void Update()
        {
            if (collected) return;

            // Multi-axis crystal rotation
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            transform.Rotate(Vector3.right, rotationSpeed * 0.4f * Time.deltaTime, Space.Self);

            float newY = transform.position.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight * Time.deltaTime;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            // Proximity magnetism toward player
            if (PlayerController.Instance)
            {
                float dist = Vector3.Distance(transform.position, PlayerController.Instance.transform.position);
                if (dist < 6.5f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, PlayerController.Instance.transform.position, 28f * Time.deltaTime);
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

            if (PlayerEnergy.Instance)
            {
                PlayerEnergy.Instance.AddEnergy(energyValue);
            }

            if (GameManager.Instance)
            {
                GameManager.Instance.AddCrystal();
            }

            if (AudioManager.Instance)
            {
                AudioManager.Instance.PlaySound("energy");
            }

            FloatingTextSpawner.Spawn($"+{energyValue:F0} ENERGY", transform.position, new Color(0.1f, 1f, 0.8f));

            gameObject.SetActive(false);
        }
    }
}
