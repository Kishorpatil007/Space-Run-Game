using System.Collections.Generic;
using UnityEngine;
using SpaceJet.Player;

namespace SpaceJet.Visuals
{
    /// <summary>
    /// Generates high-quality procedural 3D sci-fi meshes and materials for:
    /// Futuristic Space Jet, Asteroids, Energy Crystals, Coins, Planets, Barriers, and Shields.
    /// </summary>
    public static class ProceduralMeshBuilder
    {
        #region Materials Factory

        public static Material CreateMaterial(string name, Color mainColor, Color emissionColor, float metallic = 0.5f, float smoothness = 0.5f, bool transparent = false, Texture2D texture = null)
        {
            Shader shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Mobile/Diffuse");
            if (shader == null) shader = Shader.Find("Unlit/Color");

            Material mat = new Material(shader);
            mat.name = name;
            mat.color = mainColor;
            if (texture != null)
            {
                mat.mainTexture = texture;
            }

            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);

            if (emissionColor != Color.black && mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emissionColor);
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }

            if (transparent)
            {
                mat.SetFloat("_Mode", 3); // Transparent
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }

            return mat;
        }

        private static readonly Dictionary<int, Texture2D> cachedJetCardTextures = new Dictionary<int, Texture2D>();

        public static Texture2D GenerateJetSkinCardTexture(JetSkinDefinition skin, int size = 256)
        {
            if (skin == null) return null;
            if (cachedJetCardTextures.TryGetValue(skin.id, out Texture2D cached) && cached != null)
            {
                return cached;
            }

            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = $"JetSkinTex_{skin.id}";
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            Color[] pixels = new Color[size * size];
            Color bgTop = new Color(0.04f, 0.08f, 0.16f, 0.98f);
            Color bgBot = new Color(0.02f, 0.04f, 0.09f, 0.98f);
            Color gridColor = skin.accentColor * 0.18f;
            Color reticleColor = skin.accentColor * 0.40f;

            for (int y = 0; y < size; y++)
            {
                float v = (float)y / size;
                Color bgColor = Color.Lerp(bgBot, bgTop, v);

                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;
                    float nx = (u - 0.5f) * 2f;
                    float ny = (v - 0.5f) * 2f;

                    bool isGrid = (x % 32 == 0 || y % 32 == 0);
                    Color pixelColor = isGrid ? Color.Lerp(bgColor, gridColor, 0.4f) : bgColor;

                    float distCenter = Mathf.Sqrt(nx * nx + ny * ny);
                    if (Mathf.Abs(distCenter - 0.78f) < 0.015f || Mathf.Abs(distCenter - 0.55f) < 0.01f)
                    {
                        pixelColor = Color.Lerp(pixelColor, reticleColor, 0.55f);
                    }

                    float absX = Mathf.Abs(nx);

                    // 1. Dual Thruster Exhaust Plumes (ny: -0.92 to -0.42)
                    for (int side = -1; side <= 1; side += 2)
                    {
                        float plumeDistX = Mathf.Abs(nx - side * 0.18f);
                        if (ny < -0.42f && ny > -0.92f && plumeDistX < 0.09f)
                        {
                            float plumeT = (-0.42f - ny) / 0.50f;
                            float plumeWidth = Mathf.Lerp(0.065f, 0.015f, plumeT);
                            if (plumeDistX < plumeWidth)
                            {
                                Color plumeCol = Color.Lerp(Color.white, skin.thrusterColor, plumeT * 0.8f);
                                float plumeAlpha = (1f - plumeT) * (1f - (plumeDistX / plumeWidth));
                                pixelColor = Color.Lerp(pixelColor, plumeCol, plumeAlpha * 0.95f);
                            }
                        }
                    }

                    // 2. Main Delta Wings (ny: -0.40 to 0.18)
                    if (ny >= -0.40f && ny <= 0.18f)
                    {
                        float wingT = (ny - (-0.40f)) / (0.18f - (-0.40f));
                        float wingHalfWidth = Mathf.Lerp(0.78f, 0.16f, wingT);
                        if (absX <= wingHalfWidth)
                        {
                            Color wingCol = skin.hullColor;
                            float shade = Mathf.Clamp01(0.75f + (1f - absX / wingHalfWidth) * 0.25f);
                            wingCol = new Color(wingCol.r * shade, wingCol.g * shade, wingCol.b * shade, 1f);

                            if (absX >= wingHalfWidth - 0.05f)
                            {
                                wingCol = Color.Lerp(wingCol, skin.accentColor, 0.85f);
                            }
                            pixelColor = wingCol;
                        }
                    }

                    // 3. Canted Wingtips & Vertical Stabilizers (ny: -0.46 to -0.15)
                    if (ny >= -0.46f && ny <= -0.15f && absX >= 0.70f && absX <= 0.84f)
                    {
                        float finT = (ny - (-0.46f)) / 0.31f;
                        float finWidth = Mathf.Lerp(0.12f, 0.04f, finT);
                        float finDist = Mathf.Abs(absX - 0.77f);
                        if (finDist <= finWidth * 0.5f)
                        {
                            pixelColor = skin.accentColor;
                        }
                    }

                    // 4. Main Fuselage (ny: -0.52 to 0.45)
                    if (ny >= -0.52f && ny <= 0.45f)
                    {
                        float fuseT = (ny - (-0.52f)) / (0.45f - (-0.52f));
                        float fuseHalfWidth = Mathf.Lerp(0.18f, 0.08f, fuseT);
                        if (absX <= fuseHalfWidth)
                        {
                            float crest = 1f - (absX / fuseHalfWidth);
                            Color fuseCol = Color.Lerp(skin.hullColor * 0.8f, skin.hullColor * 1.25f, crest);
                            pixelColor = fuseCol;
                        }
                    }

                    // 5. Twin Engine Nacelles (ny: -0.52 to -0.38)
                    for (int side = -1; side <= 1; side += 2)
                    {
                        float nacelleDist = Mathf.Abs(nx - side * 0.18f);
                        if (ny >= -0.52f && ny <= -0.38f && nacelleDist <= 0.065f)
                        {
                            Color nacelleCol = new Color(0.14f, 0.16f, 0.20f);
                            if (ny <= -0.48f) nacelleCol = skin.thrusterColor;
                            pixelColor = nacelleCol;
                        }
                    }

                    // 6. Needle Nose Cone (ny: 0.42 to 0.85)
                    if (ny >= 0.42f && ny <= 0.85f)
                    {
                        float noseT = (ny - 0.42f) / (0.85f - 0.42f);
                        float noseHalfWidth = Mathf.Lerp(0.085f, 0.005f, noseT);
                        if (absX <= noseHalfWidth)
                        {
                            pixelColor = skin.accentColor;
                        }
                    }

                    // 7. Cockpit Canopy (ny: 0.08 to 0.36)
                    if (ny >= 0.08f && ny <= 0.36f)
                    {
                        float canT = (ny - 0.08f) / (0.36f - 0.08f);
                        float canHalfWidth = Mathf.Sin(canT * Mathf.PI) * 0.075f;
                        if (absX <= canHalfWidth)
                        {
                            Color canCol = skin.canopyColor;
                            if (nx < 0f && absX > 0.02f) canCol = Color.Lerp(canCol, Color.white, 0.6f);
                            pixelColor = canCol;
                        }
                    }

                    // Outer Card Border
                    if (x <= 2 || x >= size - 3 || y <= 2 || y >= size - 3)
                    {
                        pixelColor = skin.accentColor * 0.75f;
                    }

                    pixels[y * size + x] = pixelColor;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            cachedJetCardTextures[skin.id] = tex;
            return tex;
        }

        private static Texture2D cachedCoinTexture;

        public static Texture2D GenerateGoldCoinTexture(int size = 256)
        {
            if (cachedCoinTexture != null) return cachedCoinTexture;

            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            tex.name = "MintedGoldCoinTex";
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            Color[] pixels = new Color[size * size];
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float maxR = size * 0.485f;
            float rimOuter = maxR * 0.98f;
            float rimInner = maxR * 0.82f;
            float bezelOuter = maxR * 0.80f;
            float bezelInner = maxR * 0.72f;

            Color goldGleam = new Color(1.0f, 0.96f, 0.55f);   // Brilliant glint
            Color goldPure = new Color(1.0f, 0.85f, 0.12f);    // 24K pure gold
            Color goldDeep = new Color(0.88f, 0.62f, 0.04f);   // Rich amber gold
            Color goldDark = new Color(0.68f, 0.44f, 0.02f);   // Milled tooth shadow

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 d = new Vector2(x, y) - center;
                    float dist = d.magnitude;
                    float angle = Mathf.Atan2(d.y, d.x);

                    if (dist > maxR)
                    {
                        pixels[y * size + x] = new Color(goldPure.r, goldPure.g, goldPure.b, 1f);
                        continue;
                    }

                    // Base circular brushed metal gradient
                    float radialT = dist / maxR;
                    Color c = Color.Lerp(goldPure, goldDeep, radialT * 0.6f);

                    // Milled edge reeding (radial teeth notches on outer rim)
                    if (dist >= rimInner && dist <= rimOuter)
                    {
                        float teeth = Mathf.Sin(angle * 48f); // 48 sharp teeth
                        c = (teeth > 0.1f) ? Color.Lerp(c, goldGleam, 0.65f) : Color.Lerp(c, goldDark, 0.5f);
                    }
                    // Concentric raised bezel ring
                    else if (dist >= bezelInner && dist <= bezelOuter)
                    {
                        float ringT = (dist - bezelInner) / (bezelOuter - bezelInner);
                        float bump = Mathf.Sin(ringT * Mathf.PI);
                        c = Color.Lerp(goldPure, goldGleam, bump * 0.85f);
                    }
                    // Inner field with embossed 5-point Star
                    else if (dist < bezelInner)
                    {
                        bool inStar = IsPointInStar(d, maxR * 0.42f, maxR * 0.18f, 5);
                        if (inStar)
                        {
                            // 3D directional embossed lighting on the star
                            float sunAngle = Mathf.PI * 0.25f; // light from top-right
                            float emboss = Mathf.Cos(angle - sunAngle);
                            c = emboss > 0f ? Color.Lerp(goldPure, goldGleam, emboss * 0.9f)
                                            : Color.Lerp(goldDeep, goldDark, -emboss * 0.6f);
                        }
                    }

                    pixels[y * size + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true);
            cachedCoinTexture = tex;
            return tex;
        }

        private static bool IsPointInStar(Vector2 p, float outerR, float innerR, int points)
        {
            float angle = Mathf.Atan2(p.y, p.x) + Mathf.PI * 0.5f; // point up
            if (angle < 0) angle += Mathf.PI * 2f;
            float step = Mathf.PI * 2f / points;
            float halfStep = step * 0.5f;
            float relAngle = Mathf.Repeat(angle, step);
            float t = Mathf.Abs(relAngle - halfStep) / halfStep;
            float maxDist = Mathf.Lerp(innerR, outerR, t);
            return p.magnitude <= maxDist;
        }

        private static Material cachedCoinMaterial;

        public static Material CreateCoinMaterial()
        {
            if (cachedCoinMaterial != null) return cachedCoinMaterial;

            Texture2D tex = GenerateGoldCoinTexture(256);
            Material mat = CreateMaterial("MintedGoldCoinMat",
                new Color(1f, 0.88f, 0.16f),               // Gleaming 24K Albedo
                new Color(1f, 0.76f, 0.08f) * 0.45f,       // Warm self-illumination emission
                0.72f,                                     // Rich metallic luster
                0.88f,                                     // High specular glossiness
                false,
                tex);

            cachedCoinMaterial = mat;
            return mat;
        }

        public enum AsteroidTheme
        {
            Standard,
            Magma,
            Carbon,
            Metallic,
            Gold
        }

        public static Texture2D GenerateAsteroidTexture(AsteroidTheme theme, int size = 256)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGB24, true);
            tex.name = theme + "Tex";
            Color[] pixels = new Color[size * size];

            Color baseCol, fissureCol;
            switch (theme)
            {
                case AsteroidTheme.Magma:
                    baseCol = new Color(0.24f, 0.14f, 0.12f);
                    fissureCol = new Color(1.0f, 0.50f, 0.05f);
                    break;
                case AsteroidTheme.Carbon:
                    baseCol = new Color(0.22f, 0.20f, 0.18f); // Dark carbonaceous basalt
                    fissureCol = new Color(0.38f, 0.28f, 0.20f); // Terracotta mineral dust
                    break;
                case AsteroidTheme.Metallic:
                    baseCol = new Color(0.45f, 0.44f, 0.42f);
                    fissureCol = new Color(0.65f, 0.60f, 0.55f);
                    break;
                case AsteroidTheme.Gold:
                    baseCol = new Color(0.65f, 0.52f, 0.15f);
                    fissureCol = new Color(1.0f, 0.88f, 0.22f);
                    break;
                default:
                    baseCol = new Color(0.42f, 0.40f, 0.38f);
                    fissureCol = new Color(0.25f, 0.24f, 0.22f);
                    break;
            }

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)size;
                    float v = y / (float)size;
                    float n1 = Mathf.PerlinNoise(u * 8f, v * 8f);
                    float n2 = Mathf.PerlinNoise(u * 24f, v * 24f) * 0.3f;
                    float val = Mathf.Clamp01(n1 + n2);

                    Color c = Color.Lerp(baseCol * 0.7f, baseCol * 1.3f, val);
                    if (theme == AsteroidTheme.Magma && val > 0.65f)
                    {
                        c = Color.Lerp(c, fissureCol * 1.8f, (val - 0.65f) / 0.35f);
                    }
                    else if (theme == AsteroidTheme.Gold && val > 0.60f)
                    {
                        c = Color.Lerp(c, fissureCol * 1.5f, (val - 0.60f) / 0.40f);
                    }
                    pixels[y * size + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true);
            return tex;
        }

        public static Material CreateAsteroidMaterial(AsteroidTheme theme)
        {
            Texture2D tex = GenerateAsteroidTexture(theme, 256);
            switch (theme)
            {
                case AsteroidTheme.Magma:
                    return CreateMaterial("MagmaAsteroidMat", new Color(0.42f, 0.24f, 0.18f), new Color(1.0f, 0.45f, 0.1f) * 2.0f, 0.25f, 0.45f, false, tex);
                case AsteroidTheme.Carbon:
                    return CreateMaterial("CarbonAsteroidMat", new Color(0.32f, 0.30f, 0.28f), new Color(0.14f, 0.11f, 0.08f), 0.20f, 0.30f, false, tex);
                case AsteroidTheme.Metallic:
                    return CreateMaterial("MetallicAsteroidMat", new Color(0.62f, 0.60f, 0.58f), new Color(0.45f, 0.38f, 0.25f) * 0.4f, 0.88f, 0.65f, false, tex);
                case AsteroidTheme.Gold:
                    return CreateMaterial("GoldAsteroidMat", new Color(1.0f, 0.85f, 0.2f), new Color(1.0f, 0.7f, 0.1f) * 1.6f, 0.90f, 0.82f, false, tex);
                default:
                    return CreateMaterial("StandardAsteroidMat", new Color(0.58f, 0.55f, 0.52f), new Color(0.12f, 0.14f, 0.18f), 0.30f, 0.40f, false, tex);
            }
        }

        #endregion

        #region Mesh Generators

        public static Mesh CreateOctahedronMesh(float radius = 1.1f)
        {
            Mesh mesh = new Mesh();
            mesh.name = "FacetedCrystalMesh";

            Vector3 top = new Vector3(0, radius * 1.6f, 0);
            Vector3 bottom = new Vector3(0, -radius * 1.6f, 0);
            Vector3[] mid = new Vector3[]
            {
                new Vector3(radius, 0, 0),
                new Vector3(0, 0, radius),
                new Vector3(-radius, 0, 0),
                new Vector3(0, 0, -radius)
            };

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
            {
                Vector3 n = Vector3.Cross(b - a, c - a).normalized;
                int start = verts.Count;
                verts.Add(a); verts.Add(b); verts.Add(c);
                norms.Add(n); norms.Add(n); norms.Add(n);
                uvs.Add(new Vector2(0.5f, 1f));
                uvs.Add(new Vector2(0f, 0f));
                uvs.Add(new Vector2(1f, 0f));
                tris.Add(start); tris.Add(start + 1); tris.Add(start + 2);
            }

            // 4 Top facets
            AddTriangle(top, mid[1], mid[0]);
            AddTriangle(top, mid[2], mid[1]);
            AddTriangle(top, mid[3], mid[2]);
            AddTriangle(top, mid[0], mid[3]);

            // 4 Bottom facets
            AddTriangle(bottom, mid[0], mid[1]);
            AddTriangle(bottom, mid[1], mid[2]);
            AddTriangle(bottom, mid[2], mid[3]);
            AddTriangle(bottom, mid[3], mid[0]);

            mesh.vertices = verts.ToArray();
            mesh.normals = norms.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh CreateCoinMesh(float radius = 1.0f, float thickness = 0.25f, int segments = 32)
        {
            Mesh mesh = new Mesh();
            mesh.name = "Realistic3DGoldCoinMesh";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            float halfThick = thickness * 0.5f;
            float bevelR = radius * 0.08f;
            float bevelZ = thickness * 0.15f;
            float innerRadius = radius - bevelR;
            float innerZ = halfThick;
            float rimZ = halfThick - bevelZ;

            // ==========================================
            // 1. FRONT FACE (Z = +halfThick)
            // ==========================================
            int frontCenterIdx = verts.Count;
            verts.Add(new Vector3(0, 0, innerZ));
            norms.Add(new Vector3(0, 0, 1f));
            uvs.Add(new Vector2(0.5f, 0.5f));

            int frontRingStart = verts.Count;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);

                verts.Add(new Vector3(cos * innerRadius, sin * innerRadius, innerZ));
                norms.Add(new Vector3(0, 0, 1f));
                uvs.Add(new Vector2(0.5f + cos * 0.40f, 0.5f + sin * 0.40f));
            }

            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                tris.Add(frontCenterIdx);
                tris.Add(frontRingStart + i);
                tris.Add(frontRingStart + next);
            }

            // ==========================================
            // 2. FRONT BEVEL RIM (innerRadius to radius)
            // ==========================================
            int frontBevelInner = verts.Count;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);

                verts.Add(new Vector3(cos * innerRadius, sin * innerRadius, innerZ));
                Vector3 n = new Vector3(cos * 0.5f, sin * 0.5f, 0.707f).normalized;
                norms.Add(n);
                uvs.Add(new Vector2(0.5f + cos * 0.40f, 0.5f + sin * 0.40f));
            }

            int frontBevelOuter = verts.Count;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);

                verts.Add(new Vector3(cos * radius, sin * radius, rimZ));
                Vector3 n = new Vector3(cos * 0.5f, sin * 0.5f, 0.707f).normalized;
                norms.Add(n);
                uvs.Add(new Vector2(0.5f + cos * 0.48f, 0.5f + sin * 0.48f));
            }

            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                int i0 = frontBevelInner + i;
                int i1 = frontBevelInner + next;
                int o0 = frontBevelOuter + i;
                int o1 = frontBevelOuter + next;

                tris.Add(i0); tris.Add(o0); tris.Add(o1);
                tris.Add(i0); tris.Add(o1); tris.Add(i1);
            }

            // ==========================================
            // 3. CYLINDRICAL MILLED RIM (Connecting front to back)
            // ==========================================
            int rimFrontStart = verts.Count;
            for (int i = 0; i <= segments; i++)
            {
                float angle = (i % segments) * Mathf.PI * 2f / segments;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);

                verts.Add(new Vector3(cos * radius, sin * radius, rimZ));
                norms.Add(new Vector3(cos, sin, 0f));
                uvs.Add(new Vector2(i / (float)segments * 16f, 1f));
            }

            int rimBackStart = verts.Count;
            for (int i = 0; i <= segments; i++)
            {
                float angle = (i % segments) * Mathf.PI * 2f / segments;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);

                verts.Add(new Vector3(cos * radius, sin * radius, -rimZ));
                norms.Add(new Vector3(cos, sin, 0f));
                uvs.Add(new Vector2(i / (float)segments * 16f, 0f));
            }

            for (int i = 0; i < segments; i++)
            {
                int f0 = rimFrontStart + i;
                int f1 = rimFrontStart + i + 1;
                int b0 = rimBackStart + i;
                int b1 = rimBackStart + i + 1;

                tris.Add(f0); tris.Add(b0); tris.Add(b1);
                tris.Add(f0); tris.Add(b1); tris.Add(f1);
            }

            // ==========================================
            // 4. BACK BEVEL RIM
            // ==========================================
            int backBevelOuter = verts.Count;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);

                verts.Add(new Vector3(cos * radius, sin * radius, -rimZ));
                Vector3 n = new Vector3(cos * 0.5f, sin * 0.5f, -0.707f).normalized;
                norms.Add(n);
                uvs.Add(new Vector2(0.5f - cos * 0.48f, 0.5f + sin * 0.48f));
            }

            int backBevelInner = verts.Count;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);

                verts.Add(new Vector3(cos * innerRadius, sin * innerRadius, -innerZ));
                Vector3 n = new Vector3(cos * 0.5f, sin * 0.5f, -0.707f).normalized;
                norms.Add(n);
                uvs.Add(new Vector2(0.5f - cos * 0.40f, 0.5f + sin * 0.40f));
            }

            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                int o0 = backBevelOuter + i;
                int o1 = backBevelOuter + next;
                int i0 = backBevelInner + i;
                int i1 = backBevelInner + next;

                tris.Add(o0); tris.Add(i0); tris.Add(i1);
                tris.Add(o0); tris.Add(i1); tris.Add(o1);
            }

            // ==========================================
            // 5. BACK FACE (Z = -halfThick)
            // ==========================================
            int backCenterIdx = verts.Count;
            verts.Add(new Vector3(0, 0, -innerZ));
            norms.Add(new Vector3(0, 0, -1f));
            uvs.Add(new Vector2(0.5f, 0.5f));

            int backRingStart = verts.Count;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);

                verts.Add(new Vector3(cos * innerRadius, sin * innerRadius, -innerZ));
                norms.Add(new Vector3(0, 0, -1f));
                uvs.Add(new Vector2(0.5f - cos * 0.40f, 0.5f + sin * 0.40f));
            }

            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                tris.Add(backCenterIdx);
                tris.Add(backRingStart + next);
                tris.Add(backRingStart + i);
            }

            mesh.vertices = verts.ToArray();
            mesh.normals = norms.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh CreateAsteroidMesh(float radius = 1.5f, int subdivisions = 14)
        {
            Mesh mesh = new Mesh();
            mesh.name = "RealisticAsteroidMesh";

            List<Vector3> verts = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            int latSegments = subdivisions;
            int lonSegments = subdivisions * 2;

            for (int lat = 0; lat <= latSegments; lat++)
            {
                float theta = lat * Mathf.PI / latSegments;
                float sinTheta = Mathf.Sin(theta);
                float cosTheta = Mathf.Cos(theta);

                for (int lon = 0; lon <= lonSegments; lon++)
                {
                    float phi = lon * 2f * Mathf.PI / lonSegments;
                    float sinPhi = Mathf.Sin(phi);
                    float cosPhi = Mathf.Cos(phi);

                    Vector3 normal = new Vector3(cosPhi * sinTheta, cosTheta, sinPhi * sinTheta);

                    // Multi-frequency Perlin noise for realistic meteor craters and faceted ridges
                    float n1 = Mathf.PerlinNoise(normal.x * 2.2f + 12f, normal.y * 2.2f + 34f) * 0.35f;
                    float n2 = Mathf.PerlinNoise(normal.y * 5.1f + 56f, normal.z * 5.1f + 78f) * 0.15f;
                    float r = radius * (0.85f + n1 + n2);

                    verts.Add(normal * r);
                    uvs.Add(new Vector2(lon / (float)lonSegments, lat / (float)latSegments));
                }
            }

            for (int lat = 0; lat < latSegments; lat++)
            {
                for (int lon = 0; lon < lonSegments; lon++)
                {
                    int first = (lat * (lonSegments + 1)) + lon;
                    int second = first + lonSegments + 1;

                    tris.Add(first);
                    tris.Add(second);
                    tris.Add(first + 1);

                    tris.Add(second);
                    tris.Add(second + 1);
                    tris.Add(first + 1);
                }
            }

            mesh.vertices = verts.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh CreateSpaceDebrisMesh()
        {
            // Creates a broken satellite panel / structural beam
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Mesh mesh = Object.Instantiate(temp.GetComponent<MeshFilter>().sharedMesh);
            Object.DestroyImmediate(temp);

            Vector3[] verts = mesh.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                // Deform into broken satellite wing
                verts[i].x *= 2.5f;
                verts[i].y *= 0.15f;
                verts[i].z *= 1.2f;
                if (verts[i].x > 0) verts[i].y += verts[i].x * 0.2f; // bent edge
            }
            mesh.vertices = verts;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh CreateSphereMesh(float radius = 1f, int segments = 24)
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Mesh mesh = Object.Instantiate(temp.GetComponent<MeshFilter>().sharedMesh);
            Object.DestroyImmediate(temp);
            return mesh;
        }

        public static Mesh CreateRingMesh(float innerRadius = 2.2f, float outerRadius = 3.6f, int segments = 48)
        {
            Mesh mesh = new Mesh();
            mesh.name = "PlanetaryRingMesh";

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            // Top ring (normals pointing UP)
            int topStart = verts.Count;
            for (int i = 0; i <= segments; i++)
            {
                float angle = (i % segments) * Mathf.PI * 2f / segments;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);

                verts.Add(new Vector3(cos * innerRadius, 0.02f, sin * innerRadius));
                norms.Add(Vector3.up);
                uvs.Add(new Vector2(0.5f + cos * 0.3f, 0.5f + sin * 0.3f));

                verts.Add(new Vector3(cos * outerRadius, 0.02f, sin * outerRadius));
                norms.Add(Vector3.up);
                uvs.Add(new Vector2(0.5f + cos * 0.5f, 0.5f + sin * 0.5f));
            }

            for (int i = 0; i < segments; i++)
            {
                int i1 = topStart + i * 2;
                int o1 = i1 + 1;
                int i2 = topStart + (i + 1) * 2;
                int o2 = i2 + 1;

                tris.Add(i1); tris.Add(o1); tris.Add(o2);
                tris.Add(i1); tris.Add(o2); tris.Add(i2);
            }

            // Bottom ring (normals pointing DOWN)
            int botStart = verts.Count;
            for (int i = 0; i <= segments; i++)
            {
                float angle = (i % segments) * Mathf.PI * 2f / segments;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);

                verts.Add(new Vector3(cos * innerRadius, -0.02f, sin * innerRadius));
                norms.Add(Vector3.down);
                uvs.Add(new Vector2(0.5f + cos * 0.3f, 0.5f + sin * 0.3f));

                verts.Add(new Vector3(cos * outerRadius, -0.02f, sin * outerRadius));
                norms.Add(Vector3.down);
                uvs.Add(new Vector2(0.5f + cos * 0.5f, 0.5f + sin * 0.5f));
            }

            for (int i = 0; i < segments; i++)
            {
                int i1 = botStart + i * 2;
                int o1 = i1 + 1;
                int i2 = botStart + (i + 1) * 2;
                int o2 = i2 + 1;

                tris.Add(i1); tris.Add(o2); tris.Add(o1);
                tris.Add(i1); tris.Add(i2); tris.Add(o2);
            }

            mesh.vertices = verts.ToArray();
            mesh.normals = norms.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.RecalculateBounds();
            return mesh;
        }

        #endregion

        #region Complete Composite 3D Space Jet Builder

        public static GameObject BuildFuturisticJet(string name = "SpaceJet")
        {
            GameObject jetRoot = new GameObject(name);

            // Materials (Default to Flagship Solar Vanguard: titanium white + 24K gold + azure canopy)
            Material hullMat = CreateMaterial("JetHullMat", new Color(0.98f, 0.98f, 1f), new Color(0.98f, 0.98f, 1f) * 0.28f, 0.10f, 0.55f);
            Material accentMat = CreateMaterial("JetAccentMat", new Color(1f, 0.75f, 0.08f), new Color(1f, 0.80f, 0.15f) * 2.2f, 0.10f, 0.55f);
            Material canopyMat = CreateMaterial("JetCanopyMat", new Color(0f, 0.85f, 1f), new Color(0f, 0.85f, 1f) * 1.8f, 0.10f, 0.90f);
            Material engineMat = CreateMaterial("JetEngineMat", new Color(0.12f, 0.14f, 0.18f), Color.black, 0.20f, 0.70f);
            Material thrusterGlowMat = CreateMaterial("ThrusterGlowMat", new Color(1f, 0.85f, 0.15f), new Color(1f, 0.85f, 0.15f) * 3.5f, 0.05f, 0.10f);

            // 1. Main Fuselage
            GameObject fuselage = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fuselage.name = "Fuselage";
            fuselage.transform.SetParent(jetRoot.transform, false);
            fuselage.transform.localScale = new Vector3(0.9f, 0.5f, 3.2f);
            fuselage.transform.localPosition = new Vector3(0f, 0f, 0.2f);
            fuselage.GetComponent<Renderer>().material = hullMat;
            Object.DestroyImmediate(fuselage.GetComponent<Collider>());

            // 2. Needle Nose
            GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nose.name = "NoseCone";
            nose.transform.SetParent(jetRoot.transform, false);
            nose.transform.localScale = new Vector3(0.7f, 0.35f, 1.8f);
            nose.transform.localPosition = new Vector3(0f, -0.05f, 2.2f);
            nose.GetComponent<Renderer>().material = accentMat;
            Object.DestroyImmediate(nose.GetComponent<Collider>());

            // 3. Cockpit Canopy
            GameObject cockpit = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cockpit.name = "CockpitCanopy";
            cockpit.transform.SetParent(jetRoot.transform, false);
            cockpit.transform.localScale = new Vector3(0.55f, 0.45f, 1.4f);
            cockpit.transform.localPosition = new Vector3(0f, 0.32f, 0.6f);
            cockpit.GetComponent<Renderer>().material = canopyMat;
            Object.DestroyImmediate(cockpit.GetComponent<Collider>());

            // 4. Main Delta Wings
            GameObject wings = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wings.name = "DeltaWings";
            wings.transform.SetParent(jetRoot.transform, false);
            wings.transform.localScale = new Vector3(4.8f, 0.08f, 1.6f);
            wings.transform.localPosition = new Vector3(0f, -0.05f, -0.2f);
            wings.GetComponent<Renderer>().material = hullMat;
            Object.DestroyImmediate(wings.GetComponent<Collider>());

            // 4b. Neon Wingtips
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject wingtip = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wingtip.name = (side < 0) ? "Wingtip_L" : "Wingtip_R";
                wingtip.transform.SetParent(jetRoot.transform, false);
                wingtip.transform.localScale = new Vector3(0.12f, 0.35f, 1.4f);
                wingtip.transform.localPosition = new Vector3(side * 2.4f, 0.12f, -0.2f);
                wingtip.GetComponent<Renderer>().material = accentMat;
                Object.DestroyImmediate(wingtip.GetComponent<Collider>());
            }

            // 5. Twin Canted Vertical Stabilizers
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject fin = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fin.name = (side < 0) ? "Fin_L" : "Fin_R";
                fin.transform.SetParent(jetRoot.transform, false);
                fin.transform.localScale = new Vector3(0.08f, 0.95f, 1.1f);
                fin.transform.localPosition = new Vector3(side * 0.7f, 0.5f, -0.8f);
                fin.transform.localRotation = Quaternion.Euler(0f, 0f, side * -18f); // canted outward
                fin.GetComponent<Renderer>().material = accentMat;
                Object.DestroyImmediate(fin.GetComponent<Collider>());
            }

            // 6. Dual Plasma Engine Exhausts
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject nacelle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                nacelle.name = (side < 0) ? "Engine_L" : "Engine_R";
                nacelle.transform.SetParent(jetRoot.transform, false);
                nacelle.transform.localScale = new Vector3(0.42f, 0.55f, 0.42f);
                nacelle.transform.localPosition = new Vector3(side * 0.45f, 0f, -1.3f);
                nacelle.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                nacelle.GetComponent<Renderer>().material = engineMat;
                Object.DestroyImmediate(nacelle.GetComponent<Collider>());

                // Inner thruster glow disc
                GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                glow.name = "NozzleGlow";
                glow.transform.SetParent(nacelle.transform, false);
                glow.transform.localScale = new Vector3(0.85f, 0.15f, 0.85f);
                glow.transform.localPosition = new Vector3(0f, -1.0f, 0f);
                glow.GetComponent<Renderer>().material = thrusterGlowMat;
                Object.DestroyImmediate(glow.GetComponent<Collider>());
            }

            // 7. Engine Light
            GameObject lightObj = new GameObject("EngineLight");
            lightObj.transform.SetParent(jetRoot.transform, false);
            lightObj.transform.localPosition = new Vector3(0f, 0f, -1.8f);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.2f, 0.7f, 1f);
            l.range = 7f;
            l.intensity = 2f;

            // 8. Box Collider covering ship body
            BoxCollider col = jetRoot.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.1f, 0.2f);
            col.size = new Vector3(3.6f, 0.9f, 3.8f);
            col.isTrigger = true;

            // Apply active skin immediately from SaveManager
            JetSkinManager.ApplySkinToJet(jetRoot, SpaceJet.Core.SaveManager.SelectedSkinIndex);

            return jetRoot;
        }

        #endregion
    }
}
