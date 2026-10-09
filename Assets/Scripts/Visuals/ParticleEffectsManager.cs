using System.Collections.Generic;
using UnityEngine;

namespace SpaceJet.Visuals
{
    public class ParticleEffectsManager : MonoBehaviour
    {
        public static ParticleEffectsManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #region Factory Methods

        public static ParticleSystem CreateEngineExhaust(Transform parent, Vector3 localPos, Color color, float size = 0.35f)
        {
            GameObject go = new GameObject("EngineExhaust");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = color;
            main.startSize = size;
            main.startSpeed = 8f;
            main.startLifetime = 0.35f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;

            var emission = ps.emission;
            emission.rateOverTime = 45f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 7f;
            shape.radius = 0.12f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(Color.white, 0.4f), new GradientColorKey(color * 0.75f, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.6f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = grad;

            // Renderer
            var rend = go.GetComponent<ParticleSystemRenderer>();
            Material mat = new Material(Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Mobile/Particles/Additive") ?? Shader.Find("Unlit/Color"));
            mat.color = color;
            rend.material = mat;

            return ps;
        }

        public static ParticleSystem CreateStarfield(Transform parent)
        {
            GameObject go = new GameObject("StarfieldParticles");
            go.transform.SetParent(parent, false);

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = Color.white;
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.25f);
            main.startSpeed = 0f;
            main.startLifetime = 10f;
            main.maxParticles = 1200;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 80f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(80f, 50f, 120f);
            shape.position = new Vector3(0f, 0f, 60f);

            var rend = go.GetComponent<ParticleSystemRenderer>();
            Material mat = new Material(Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Unlit/Color"));
            mat.color = Color.white;
            rend.material = mat;

            return ps;
        }

        public static ParticleSystem CreateWarpDust(Transform parent)
        {
            GameObject go = new GameObject("WarpDustParticles");
            go.transform.SetParent(parent, false);

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new Color(0.7f, 0.9f, 1f, 0.75f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.12f);
            main.startSpeed = -25f;
            main.startLifetime = 1.5f;
            main.maxParticles = 500;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 120f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(36f, 22f, 40f);
            shape.position = new Vector3(0f, 0f, 25f);

            var rend = go.GetComponent<ParticleSystemRenderer>();
            Material mat = new Material(Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Unlit/Color"));
            mat.color = new Color(0.7f, 0.9f, 1f, 0.8f);
            rend.material = mat;
            rend.renderMode = ParticleSystemRenderMode.Stretch;
            rend.velocityScale = 0.22f;
            rend.lengthScale = 3.2f;

            return ps;
        }

        #endregion

        #region Runtime Burst Triggers

        public void PlayCoinBurst(Vector3 position)
        {
            SpawnBurstParticles(position, new Color(1f, 0.86f, 0.15f), 26, 7.0f, 0.6f);
        }

        public void PlayCrystalBurst(Vector3 position)
        {
            SpawnBurstParticles(position, new Color(0.2f, 1f, 0.85f), 24, 7f, 0.6f);
        }

        public void PlayShieldSparks(Vector3 position)
        {
            SpawnBurstParticles(position, new Color(0.2f, 0.7f, 1f), 30, 9f, 0.4f);
        }

        public void PlayExplosion(Vector3 position)
        {
            SpawnBurstParticles(position, new Color(1f, 0.4f, 0.1f), 45, 12f, 0.8f);
        }

        private void SpawnBurstParticles(Vector3 pos, Color color, int count, float speed, float lifetime)
        {
            GameObject go = new GameObject("BurstFX");
            go.transform.position = pos;

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = color;
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.35f);
            main.startSpeed = speed;
            main.startLifetime = lifetime;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = ps.emission;
            emission.enabled = false;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;

            var rend = go.GetComponent<ParticleSystemRenderer>();
            Material mat = new Material(Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Unlit/Color"));
            mat.color = color;
            rend.material = mat;

            ps.Emit(count);
            Destroy(go, lifetime + 0.1f);
        }

        #endregion
    }
}
