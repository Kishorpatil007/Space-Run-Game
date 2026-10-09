using UnityEngine;
using SpaceJet.Core;
using SpaceJet.Player;

namespace SpaceJet.Environment
{
    public class SpaceEnvironment : MonoBehaviour
    {
        public static SpaceEnvironment Instance { get; set; }

        [Header("Starfield")]
        public ParticleSystem starParticles;
        public ParticleSystem dustParticles;

        [Header("Lighting")]
        public Light mainDirectionalLight;

        private GameObject celestialDome;
        private GameObject celestialBackdrop;
        private ReflectionProbe spaceReflectionProbe;

        private void Awake()
        {
            Instance = this;
            SetupRealisticGraphics();
        }

        private void Start()
        {
            EnsureCosmicCelestialDome();
            EnsureReflectionProbe();
        }

        private void SetupRealisticGraphics()
        {
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
            QualitySettings.shadowDistance = 250f;
            QualitySettings.shadowCascades = 4;
            QualitySettings.antiAliasing = 4;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;

            if (mainDirectionalLight)
            {
                mainDirectionalLight.shadows = LightShadows.Soft;
                mainDirectionalLight.shadowStrength = 0.85f;
                mainDirectionalLight.shadowBias = 0.05f;
                mainDirectionalLight.shadowNormalBias = 0.4f;
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        }

        private void EnsureReflectionProbe()
        {
            if (spaceReflectionProbe == null)
            {
                GameObject probeObj = new GameObject("SpaceReflectionProbe");
                probeObj.transform.SetParent(transform, false);
                spaceReflectionProbe = probeObj.AddComponent<ReflectionProbe>();
                spaceReflectionProbe.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
                spaceReflectionProbe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.EveryFrame;
                spaceReflectionProbe.clearFlags = UnityEngine.Rendering.ReflectionProbeClearFlags.SolidColor;
                spaceReflectionProbe.backgroundColor = new Color(0.004f, 0.045f, 0.165f);
                spaceReflectionProbe.size = new Vector3(80f, 60f, 160f);
                spaceReflectionProbe.intensity = 1.25f;
            }
        }

        private void EnsureCosmicCelestialDome()
        {
            // 360-degree celestial sky dome with exact deep space sapphire navy background color
            if (celestialDome == null)
            {
                celestialDome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                celestialDome.name = "CelestialCosmicDome";
                celestialDome.transform.SetParent(transform, false);
                celestialDome.transform.localScale = new Vector3(-700f, 700f, 700f); // Inverted sphere for sky dome
                DestroyImmediate(celestialDome.GetComponent<Collider>());

                Renderer r = celestialDome.GetComponent<Renderer>();
                if (r)
                {
                    Material skyMat = new Material(Shader.Find("Unlit/Color") ?? Shader.Find("Mobile/Diffuse"));
                    skyMat.color = new Color(0.004f, 0.045f, 0.165f); // Deep cosmic sapphire navy (#010B2A)
                    r.material = skyMat;
                }
            }

            // High-resolution panoramic cosmic vista backdrop matching the uploaded gameplay art
            if (celestialBackdrop == null)
            {
                celestialBackdrop = GameObject.CreatePrimitive(PrimitiveType.Quad);
                celestialBackdrop.name = "CelestialVistaBackdrop";
                celestialBackdrop.transform.SetParent(transform, false);
                celestialBackdrop.transform.localPosition = new Vector3(0f, -15f, 520f);
                celestialBackdrop.transform.localScale = new Vector3(1450f, 820f, 1f);
                celestialBackdrop.transform.localRotation = Quaternion.identity;
                DestroyImmediate(celestialBackdrop.GetComponent<Collider>());

                Renderer br = celestialBackdrop.GetComponent<Renderer>();
                if (br)
                {
                    Texture2D bgTex = Resources.Load<Texture2D>("GameplaySpaceBackground");
                    Shader shader = Shader.Find("Unlit/Texture") ?? Shader.Find("Mobile/Unlit (Supports Lightmap)") ?? Shader.Find("Unlit/Color");
                    Material mat = new Material(shader);
                    if (bgTex != null)
                    {
                        mat.mainTexture = bgTex;
                    }
                    mat.color = Color.white;
                    br.material = mat;
                }
            }
        }

        private void LateUpdate()
        {
            // Follow player along Z axis so the starfield, celestial dome, and lighting are continuous
            if (PlayerController.Instance)
            {
                Vector3 pPos = PlayerController.Instance.transform.position;
                transform.position = new Vector3(0f, 0f, pPos.z);
            }
        }

        public void ApplyLevelTheme(int levelIndex)
        {
            Color lightCol = Color.white;
            Color skyAmb = new Color(0.10f, 0.22f, 0.52f);
            Color eqAmb = new Color(0.18f, 0.12f, 0.38f);
            Color gndAmb = new Color(0.02f, 0.04f, 0.12f);
            Color fogCol = new Color(0.004f, 0.045f, 0.165f); // Deep cosmic sapphire navy (#010B2A)

            switch (levelIndex)
            {
                case 1: // Blue/purple stellar space matching uploaded gameplay image
                    lightCol = new Color(0.88f, 0.94f, 1f);
                    skyAmb = new Color(0.10f, 0.22f, 0.52f);
                    eqAmb = new Color(0.18f, 0.12f, 0.38f);
                    fogCol = new Color(0.004f, 0.045f, 0.165f);
                    break;
                case 2: // Emerald asteroid belt
                    lightCol = new Color(0.80f, 1f, 0.85f);
                    skyAmb = new Color(0.10f, 0.28f, 0.35f);
                    eqAmb = new Color(0.08f, 0.20f, 0.24f);
                    fogCol = new Color(0.004f, 0.045f, 0.165f);
                    break;
                case 3: // Red/orange solar nebula
                    lightCol = new Color(1f, 0.85f, 0.70f);
                    skyAmb = new Color(0.25f, 0.16f, 0.32f);
                    eqAmb = new Color(0.20f, 0.10f, 0.22f);
                    fogCol = new Color(0.004f, 0.045f, 0.165f);
                    break;
                case 4: // Cosmic storm violet
                    lightCol = new Color(0.95f, 0.75f, 1f);
                    skyAmb = new Color(0.22f, 0.14f, 0.42f);
                    eqAmb = new Color(0.15f, 0.08f, 0.30f);
                    fogCol = new Color(0.004f, 0.045f, 0.165f);
                    break;
                case 5: // Deep radiant celestial space
                    lightCol = new Color(0.92f, 0.98f, 1f);
                    skyAmb = new Color(0.12f, 0.24f, 0.55f);
                    eqAmb = new Color(0.16f, 0.14f, 0.36f);
                    fogCol = new Color(0.004f, 0.045f, 0.165f);
                    break;
            }

            if (mainDirectionalLight)
            {
                mainDirectionalLight.color = lightCol;
                mainDirectionalLight.shadows = LightShadows.Soft;
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = skyAmb;
            RenderSettings.ambientEquatorColor = eqAmb;
            RenderSettings.ambientGroundColor = gndAmb;

            RenderSettings.fog = true;
            RenderSettings.fogColor = fogCol;
            RenderSettings.fogDensity = 0.0018f;

            if (Camera.main != null)
            {
                Camera.main.backgroundColor = fogCol;
            }
        }
    }
}
