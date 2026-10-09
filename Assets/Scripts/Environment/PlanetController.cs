using UnityEngine;
using SpaceJet.Core;

namespace SpaceJet.Environment
{
    public class PlanetController : MonoBehaviour
    {
        public static PlanetController Instance { get; set; }

        [Header("Components")]
        public Transform planetSurface;
        public Transform atmosphereGlow;
        public Transform planetaryRing;

        [Header("Animation")]
        public float rotationSpeed = 2f;
        public float atmospherePulseSpeed = 1f;

        private Renderer surfaceRenderer;
        private Renderer atmosphereRenderer;
        private Renderer ringRenderer;

        private void Awake()
        {
            Instance = this;
            if (planetSurface) surfaceRenderer = planetSurface.GetComponent<Renderer>();
            if (atmosphereGlow) atmosphereRenderer = atmosphereGlow.GetComponent<Renderer>();
            if (planetaryRing) ringRenderer = planetaryRing.GetComponent<Renderer>();
        }

        private void Update()
        {
            if (planetSurface)
            {
                planetSurface.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            }
            if (planetaryRing)
            {
                planetaryRing.Rotate(Vector3.up, rotationSpeed * 0.4f * Time.deltaTime, Space.World);
            }

            if (atmosphereGlow)
            {
                float scale = 1.08f + Mathf.Sin(Time.time * atmospherePulseSpeed) * 0.02f;
                atmosphereGlow.localScale = Vector3.one * scale;
            }
        }

        public void SetupForMission(int levelIndex, float targetZPosition)
        {
            // Position at the destination along Z axis
            transform.position = new Vector3(0f, 0f, targetZPosition + 120f);

            Color surfaceColor;
            Color atmoColor;
            bool hasRings = false;

            switch (levelIndex)
            {
                case 1: // Planet Nova
                    surfaceColor = new Color(0.1f, 0.4f, 0.9f);
                    atmoColor = new Color(0.3f, 0.8f, 1f, 0.35f);
                    hasRings = true;
                    break;

                case 2: // Planet Terra-X
                    surfaceColor = new Color(0.2f, 0.65f, 0.35f);
                    atmoColor = new Color(0.4f, 0.9f, 0.6f, 0.25f);
                    hasRings = false;
                    break;

                case 3: // Planet Zenith
                    surfaceColor = new Color(0.9f, 0.3f, 0.1f);
                    atmoColor = new Color(1f, 0.6f, 0.2f, 0.35f);
                    hasRings = false;
                    break;

                case 4: // Planet Aurora
                    surfaceColor = new Color(0.6f, 0.2f, 0.8f);
                    atmoColor = new Color(0.2f, 0.9f, 0.9f, 0.4f);
                    hasRings = true;
                    break;

                case 5: // Planet Elysium
                default:
                    surfaceColor = new Color(0.1f, 0.75f, 0.85f);
                    atmoColor = new Color(0.4f, 0.95f, 1f, 0.5f);
                    hasRings = true;
                    transform.localScale = Vector3.one * 1.35f; // Extra majestic!
                    break;
            }

            if (surfaceRenderer && surfaceRenderer.material)
            {
                surfaceRenderer.material.color = surfaceColor;
            }

            if (atmosphereRenderer && atmosphereRenderer.material)
            {
                atmosphereRenderer.material.color = atmoColor;
            }

            if (planetaryRing)
            {
                planetaryRing.gameObject.SetActive(hasRings);
                if (ringRenderer && ringRenderer.material)
                {
                    ringRenderer.material.color = new Color(atmoColor.r, atmoColor.g, atmoColor.b, 0.45f);
                }
            }
        }
    }
}
