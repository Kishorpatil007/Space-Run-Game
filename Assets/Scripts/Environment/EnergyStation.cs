using UnityEngine;
using SpaceJet.Audio;
using SpaceJet.Core;
using SpaceJet.Player;
using SpaceJet.UI;

namespace SpaceJet.Environment
{
    public class EnergyStation : MonoBehaviour
    {
        public float energyRefuelAmount = 80f;
        public float ringRotateSpeed = 45f;
        public Transform outerRing;
        public Transform innerRing;

        private bool hasActivated = false;

        private void OnEnable()
        {
            hasActivated = false;
        }

        private void Update()
        {
            if (outerRing)
            {
                outerRing.Rotate(Vector3.forward, ringRotateSpeed * Time.deltaTime, Space.Self);
            }
            if (innerRing)
            {
                innerRing.Rotate(Vector3.forward, -ringRotateSpeed * 1.5f * Time.deltaTime, Space.Self);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasActivated) return;

            if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
            {
                hasActivated = true;

                if (PlayerEnergy.Instance)
                {
                    PlayerEnergy.Instance.AddEnergy(energyRefuelAmount);
                }

                if (GameManager.Instance)
                {
                    GameManager.Instance.AddEnergyStationActivated();
                }

                if (AudioManager.Instance)
                {
                    AudioManager.Instance.PlaySound("station");
                }

                FloatingTextSpawner.Spawn("STATION OVERCHARGE +80", transform.position, new Color(0f, 1f, 0.7f));
            }
        }
    }
}
