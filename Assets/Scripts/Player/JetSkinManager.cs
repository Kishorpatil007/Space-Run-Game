using System;
using UnityEngine;

namespace SpaceJet.Player
{
    [System.Serializable]
    public class JetSkinDefinition
    {
        public int id;
        public string name;
        public string classTitle;
        public string description;
        public int unlockCost;
        public Color hullColor;
        public Color accentColor;
        public Color emissionColor;
        public Color canopyColor;
        public Color thrusterColor;
        public Color engineLightColor;
        public float metallic;
        public float smoothness;
    }

    public static class JetSkinManager
    {
        public static event Action<int> OnSkinEquipped;

        public static readonly JetSkinDefinition[] Skins = new JetSkinDefinition[]
        {
            new JetSkinDefinition
            {
                id = 0,
                name = "Neon Striker",
                classTitle = "[ CYAN & COBALT BLUE ]",
                description = "Deep cobalt hull with glowing neon cyan wings and ion thruster trails.",
                unlockCost = 0,
                hullColor = new Color(0.10f, 0.38f, 0.85f), // Rich Cobalt Blue
                accentColor = new Color(0f, 1f, 1f), // Hyper Neon Cyan
                emissionColor = new Color(0f, 1f, 1f) * 2.5f,
                canopyColor = new Color(0.1f, 0.9f, 1f),
                thrusterColor = new Color(0f, 1f, 1f),
                engineLightColor = new Color(0f, 1f, 1f),
                metallic = 0.0f,
                smoothness = 0.40f
            },
            new JetSkinDefinition
            {
                id = 1,
                name = "Solar Phoenix",
                classTitle = "[ BLAZING CRIMSON RED ]",
                description = "High-gloss racing crimson hull with radiant solar fire gold stabilizers.",
                unlockCost = 0,
                hullColor = new Color(1.0f, 0.12f, 0.12f), // Vivid Racing Red, NEVER brown!
                accentColor = new Color(1f, 0.75f, 0.05f), // Solar Fire Gold
                emissionColor = new Color(1f, 0.60f, 0.05f) * 2.5f,
                canopyColor = new Color(1f, 0.85f, 0.15f), // Radiant Amber
                thrusterColor = new Color(1f, 0.50f, 0.05f),
                engineLightColor = new Color(1f, 0.60f, 0.10f),
                metallic = 0.0f,
                smoothness = 0.40f
            },
            new JetSkinDefinition
            {
                id = 2,
                name = "Emerald Viper",
                classTitle = "[ CYBER RACING GREEN ]",
                description = "Aerodynamic racing emerald hull with electric lime energy trails.",
                unlockCost = 0,
                hullColor = new Color(0.0f, 0.80f, 0.28f), // Vivid Emerald Green
                accentColor = new Color(0.20f, 1f, 0.40f), // Hyper Neon Lime
                emissionColor = new Color(0.20f, 1f, 0.40f) * 2.5f,
                canopyColor = new Color(0.2f, 1f, 0.7f),
                thrusterColor = new Color(0.15f, 1f, 0.40f),
                engineLightColor = new Color(0.20f, 1f, 0.40f),
                metallic = 0.0f,
                smoothness = 0.40f
            },
            new JetSkinDefinition
            {
                id = 3,
                name = "Gold Apex",
                classTitle = "[ 24K PURE GOLD ]",
                description = "Gleaming 24-karat aurum composite exterior with high-resonance photon thrusters.",
                unlockCost = 0,
                hullColor = new Color(1.0f, 0.80f, 0.10f), // Gleaming 24K Pure Gold
                accentColor = new Color(1f, 0.96f, 0.40f), // Aurum Flare
                emissionColor = new Color(1f, 0.85f, 0.15f) * 2.5f,
                canopyColor = new Color(1f, 0.90f, 0.30f),
                thrusterColor = new Color(1f, 0.85f, 0.15f),
                engineLightColor = new Color(1f, 0.88f, 0.25f),
                metallic = 0.0f,
                smoothness = 0.40f
            },
            new JetSkinDefinition
            {
                id = 4,
                name = "Solar Vanguard",
                classTitle = "[ TITANIUM POLAR WHITE ]",
                description = "Iconic flagship starfighter. Titanium-white fuselage with 24K gold stabilizers & azure canopy.",
                unlockCost = 0,
                hullColor = new Color(0.98f, 0.98f, 1.0f), // Pure Polar Titanium White
                accentColor = new Color(1f, 0.75f, 0.08f), // 24K Gold Accents
                emissionColor = new Color(1f, 0.80f, 0.15f) * 2.5f,
                canopyColor = new Color(0f, 0.85f, 1f), // Electric Azure Blue
                thrusterColor = new Color(1f, 0.85f, 0.15f),
                engineLightColor = new Color(1f, 0.88f, 0.35f),
                metallic = 0.0f,
                smoothness = 0.40f
            }
        };

        public static JetSkinDefinition GetSkin(int id)
        {
            if (id < 0 || id >= Skins.Length) id = 0;
            return Skins[id];
        }

        public static void NotifySkinEquipped(int skinId)
        {
            OnSkinEquipped?.Invoke(skinId);
        }

        public static void ApplySkinToJet(GameObject jetRoot, int skinId)
        {
            if (jetRoot == null) return;
            JetSkinDefinition skin = GetSkin(skinId);

            void SetMatProps(Renderer rend, Color col, Color? emission = null, float metallic = 0.0f, float gloss = 0.40f)
            {
                if (rend == null) return;
                Material mat = rend.material;
                if (mat == null)
                {
                    mat = new Material(Shader.Find("Standard") ?? Shader.Find("Mobile/Diffuse") ?? Shader.Find("Unlit/Color"));
                    rend.material = mat;
                }
                mat.mainTexture = null;
                mat.color = col;
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", col);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", col);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
                if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", gloss);
                if (mat.HasProperty("_SpecColor")) mat.SetColor("_SpecColor", Color.black);
                Color glow = emission.HasValue ? emission.Value : (col * 0.45f);
                mat.EnableKeyword("_EMISSION");
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", glow);
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                rend.material = mat;

                // Also apply via MaterialPropertyBlock to guarantee instant visual application
                MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                rend.GetPropertyBlock(mpb);
                mpb.SetColor("_Color", col);
                mpb.SetColor("_BaseColor", col);
                mpb.SetColor("_EmissionColor", glow);
                rend.SetPropertyBlock(mpb);
            }

            Transform FindPart(string partName)
            {
                Transform direct = jetRoot.transform.Find(partName);
                if (direct != null) return direct;
                foreach (Transform child in jetRoot.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name == partName) return child;
                }
                return null;
            }

            // Self-illuminating hull glow so jet always radiates its true vivid color in deep space
            Color hullGlow = skin.hullColor * 0.45f;

            // 1. Fuselage & Delta Wings (Hull)
            Transform fuselage = FindPart("Fuselage");
            if (fuselage) SetMatProps(fuselage.GetComponent<Renderer>(), skin.hullColor, hullGlow, skin.metallic, skin.smoothness);

            Transform wings = FindPart("DeltaWings");
            if (wings) SetMatProps(wings.GetComponent<Renderer>(), skin.hullColor, hullGlow, skin.metallic, skin.smoothness);

            // 2. Nose Cone (Accent)
            Transform nose = FindPart("NoseCone");
            if (nose) SetMatProps(nose.GetComponent<Renderer>(), skin.accentColor, skin.emissionColor, skin.metallic, skin.smoothness);

            // 3. Cockpit Canopy
            Transform canopy = FindPart("CockpitCanopy");
            if (canopy) SetMatProps(canopy.GetComponent<Renderer>(), skin.canopyColor, skin.canopyColor * 1.8f, 0.10f, 0.90f);

            // 4. Wingtips and Stabilizer Fins (Accents)
            string[] accentParts = { "Wingtip_L", "Wingtip_R", "Fin_L", "Fin_R" };
            foreach (var partName in accentParts)
            {
                Transform part = FindPart(partName);
                if (part) SetMatProps(part.GetComponent<Renderer>(), skin.accentColor, skin.emissionColor, skin.metallic, skin.smoothness);
            }

            // 5. Engine Nozzles and Thruster Discs
            string[] engines = { "Engine_L", "Engine_R" };
            foreach (var engName in engines)
            {
                Transform eng = FindPart(engName);
                if (eng)
                {
                    SetMatProps(eng.GetComponent<Renderer>(), new Color(0.12f, 0.14f, 0.18f), null, 0.20f, 0.7f);
                    Transform glow = eng.Find("NozzleGlow");
                    if (glow) SetMatProps(glow.GetComponent<Renderer>(), skin.thrusterColor, skin.thrusterColor * 3.5f, 0.05f, 0.1f);
                }
            }

            // 6. Update Particle Systems startColor for thrusters
            ParticleSystem[] pSystems = jetRoot.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in pSystems)
            {
                if (ps.name.Contains("Boost")) continue;
                var main = ps.main;
                main.startColor = skin.thrusterColor;
                var col = ps.colorOverLifetime;
                if (col.enabled)
                {
                    Gradient grad = new Gradient();
                    grad.SetKeys(
                        new GradientColorKey[] {
                            new GradientColorKey(skin.thrusterColor, 0f),
                            new GradientColorKey(Color.white, 0.4f),
                            new GradientColorKey(skin.thrusterColor * 0.7f, 1f)
                        },
                        new GradientAlphaKey[] {
                            new GradientAlphaKey(1f, 0f),
                            new GradientAlphaKey(0.8f, 0.6f),
                            new GradientAlphaKey(0f, 1f)
                        }
                    );
                    col.color = grad;
                }
                var pRend = ps.GetComponent<ParticleSystemRenderer>();
                if (pRend != null && pRend.material != null)
                {
                    pRend.material.color = skin.thrusterColor;
                }
            }

            // 7. Update Engine Point Light
            Light[] lights = jetRoot.GetComponentsInChildren<Light>(true);
            foreach (var l in lights)
            {
                if (l.name == "ShipHullFillLight") continue;
                l.color = skin.engineLightColor;
            }

            // 8. Dynamic Hull Accent Light so jet's painted hull radiates vividly in deep space
            Transform hullLightT = jetRoot.transform.Find("ShipHullFillLight");
            Light hullLight = null;
            if (hullLightT == null)
            {
                GameObject hlObj = new GameObject("ShipHullFillLight");
                hlObj.transform.SetParent(jetRoot.transform, false);
                hlObj.transform.localPosition = new Vector3(0f, 2.2f, 0.4f);
                hullLight = hlObj.AddComponent<Light>();
                hullLight.type = LightType.Point;
                hullLight.range = 8.5f;
            }
            else
            {
                hullLight = hullLightT.GetComponent<Light>();
            }
            if (hullLight != null)
            {
                hullLight.color = Color.Lerp(skin.hullColor, Color.white, 0.45f);
                hullLight.intensity = 2.4f;
            }
        }
    }
}
