using UnityEngine;
using UnityEngine.UI;

namespace SpaceJet.UI
{
    /// <summary>
    /// Utility class for programmatically constructing sleek futuristic sci-fi UI elements
    /// with strictly responsive, zero-overlap layouts.
    /// </summary>
    public static class UIBuilderHelper
    {
        private static Font defaultFont;

        public static Font GetDefaultFont()
        {
            if (defaultFont == null)
            {
                defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            return defaultFont;
        }

        public static GameObject CreatePanel(Transform parent, string name, Color bgColor, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            Image img = go.AddComponent<Image>();
            img.color = bgColor;

            return go;
        }

        public static GameObject CreateContainer(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            return go;
        }

        public static Text CreateText(Transform parent, string name, string content, int fontSize, TextAnchor alignment, Color color, FontStyle style = FontStyle.Normal, bool raycastTarget = false)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Text txt = go.AddComponent<Text>();
            txt.font = GetDefaultFont();
            txt.text = content;
            txt.fontSize = fontSize;
            txt.alignment = alignment;
            txt.color = color;
            txt.fontStyle = style;
            txt.supportRichText = true;
            txt.raycastTarget = raycastTarget;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Truncate;
            txt.resizeTextForBestFit = true;
            txt.resizeTextMinSize = Mathf.Max(12, fontSize - 10);
            txt.resizeTextMaxSize = fontSize;

            // Layout element to guarantee proper spacing in Vertical/HorizontalLayoutGroups
            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minHeight = fontSize + 6;
            le.preferredHeight = fontSize + 8;
            le.flexibleWidth = 1f;

            // Subtle shadow for crisp readability
            Shadow shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);

            return txt;
        }

        public static Text CreateTextWithBounds(Transform parent, string name, string content, int fontSize, TextAnchor alignment, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, FontStyle style = FontStyle.Normal, bool raycastTarget = false)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            Text txt = go.AddComponent<Text>();
            txt.font = GetDefaultFont();
            txt.text = content;
            txt.fontSize = fontSize;
            txt.alignment = alignment;
            txt.color = color;
            txt.fontStyle = style;
            txt.supportRichText = true;
            txt.raycastTarget = raycastTarget;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Truncate;
            txt.resizeTextForBestFit = true;
            txt.resizeTextMinSize = Mathf.Max(12, fontSize - 8);
            txt.resizeTextMaxSize = fontSize;

            Shadow shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);

            return txt;
        }

        private static Sprite roundedCornerSprite;

        public static Sprite GetRoundedCornerSprite(int radius = 24)
        {
            if (roundedCornerSprite != null) return roundedCornerSprite;

            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[size * size];

            float r = radius;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = 0f;
                    float dy = 0f;

                    if (x < r) dx = r - x;
                    else if (x > size - 1 - r) dx = x - (size - 1 - r);

                    if (y < r) dy = r - y;
                    else if (y > size - 1 - r) dy = y - (size - 1 - r);

                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist <= r - 1.0f)
                    {
                        // Slight vertical gradient for metallic/glass shine (brighter at top)
                        float vShine = 0.90f + 0.15f * ((float)y / size);
                        pixels[y * size + x] = new Color(vShine, vShine, vShine, 1f);
                    }
                    else if (dist < r + 0.8f)
                    {
                        float alpha = Mathf.Clamp01(r + 0.8f - dist);
                        pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            Vector4 border = new Vector4(r, r, r, r);
            roundedCornerSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            return roundedCornerSprite;
        }

        public static Button CreateSciFiButton(Transform parent, string name, string label, Vector2 size, Color btnColor, Color textColor, int fontSize = 20, Color? outlineColor = null)
        {
            // Boost color luminance if dark to guarantee vibrant, bright visibility
            float maxC = Mathf.Max(btnColor.r, Mathf.Max(btnColor.g, btnColor.b));
            if (maxC > 0.05f && maxC < 0.78f)
            {
                float boost = 0.88f / maxC;
                btnColor = new Color(Mathf.Clamp01(btnColor.r * boost), Mathf.Clamp01(btnColor.g * boost), Mathf.Clamp01(btnColor.b * boost), btnColor.a);
            }

            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = size;

            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minWidth = size.x;
            le.preferredWidth = size.x;
            le.minHeight = size.y;
            le.preferredHeight = size.y;

            Image img = go.AddComponent<Image>();
            img.sprite = GetRoundedCornerSprite(24);
            img.type = Image.Type.Sliced;
            img.color = btnColor;
            img.raycastTarget = true;

            // Bright glowing outline border for high visibility
            Outline ol = go.AddComponent<Outline>();
            Color defaultGlow = new Color(Mathf.Min(1f, btnColor.r * 1.35f + 0.35f), Mathf.Min(1f, btnColor.g * 1.35f + 0.35f), Mathf.Min(1f, btnColor.b * 1.35f + 0.35f), 0.95f);
            ol.effectColor = outlineColor ?? defaultGlow;
            ol.effectDistance = new Vector2(2.5f, 2.5f);

            // Subtle drop shadow for 3D depth against dark cosmos
            Shadow bShadow = go.AddComponent<Shadow>();
            bShadow.effectColor = new Color(0f, 0f, 0f, 0.75f);
            bShadow.effectDistance = new Vector2(2.5f, -2.5f);

            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.25f, 1.25f, 1.25f, 1f);
            cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            cb.selectedColor = Color.white;
            cb.disabledColor = new Color(0.42f, 0.42f, 0.42f, 0.55f);
            cb.colorMultiplier = 1f;
            btn.colors = cb;

            // Label
            GameObject labelObj = new GameObject("BtnLabel");
            labelObj.transform.SetParent(go.transform, false);
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(10, 2);
            labelRect.offsetMax = new Vector2(-10, -2);

            Text txt = labelObj.AddComponent<Text>();
            txt.font = GetDefaultFont();
            txt.text = label;
            txt.fontSize = fontSize;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = textColor;
            txt.fontStyle = FontStyle.Bold;
            txt.supportRichText = true;
            txt.raycastTarget = false;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Truncate;
            txt.resizeTextForBestFit = true;
            txt.resizeTextMinSize = Mathf.Max(12, fontSize - 8);
            txt.resizeTextMaxSize = fontSize;

            Shadow shadow = labelObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.95f);
            shadow.effectDistance = new Vector2(2f, -2f);

            return btn;
        }

        public static (Image fill, Text label) CreateStatBar(Transform parent, string name, string title, Color fillColor, Vector2 size, int fontSize = 16)
        {
            GameObject container = new GameObject(name);
            container.transform.SetParent(parent, false);
            RectTransform cRect = container.AddComponent<RectTransform>();
            cRect.sizeDelta = size;

            LayoutElement le = container.AddComponent<LayoutElement>();
            le.minWidth = size.x;
            le.preferredWidth = size.x;
            le.minHeight = size.y;
            le.preferredHeight = size.y;

            // Background
            Image bgImg = container.AddComponent<Image>();
            bgImg.color = new Color(0.05f, 0.08f, 0.15f, 0.85f);

            Outline ol = container.AddComponent<Outline>();
            ol.effectColor = fillColor * 0.7f;
            ol.effectDistance = new Vector2(1.5f, 1.5f);

            // Fill Bar
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(container.transform, false);
            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(3, 3);
            fillRect.offsetMax = new Vector2(-3, -3);

            Image fill = fillObj.AddComponent<Image>();
            fill.color = fillColor;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = 1f;

            // Text Label
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(container.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text txt = textObj.AddComponent<Text>();
            txt.font = GetDefaultFont();
            txt.text = title;
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.raycastTarget = false;

            Shadow sh = textObj.AddComponent<Shadow>();
            sh.effectColor = Color.black;
            sh.effectDistance = new Vector2(1.5f, -1.5f);

            return (fill, txt);
        }

        public static (Image fill, Text label) CreateCockpitGauge(
            Transform parent,
            string name,
            string badgeText,
            string title,
            Color themeColor,
            Vector2 size,
            Vector2 anchoredPos,
            int fontSize = 16,
            int hitSegments = 0)
        {
            GameObject container = new GameObject(name);
            container.transform.SetParent(parent, false);
            RectTransform cRect = container.AddComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.5f, 0f);
            cRect.anchorMax = new Vector2(0.5f, 0f);
            cRect.pivot = new Vector2(0.5f, 0.5f);
            cRect.sizeDelta = size;
            cRect.anchoredPosition = anchoredPos;

            // Outer Frame / Slot Background
            Image bgImg = container.AddComponent<Image>();
            bgImg.color = new Color(0.012f, 0.03f, 0.07f, 0.95f);

            Outline ol = container.AddComponent<Outline>();
            ol.effectColor = new Color(themeColor.r, themeColor.g, themeColor.b, 0.85f);
            ol.effectDistance = new Vector2(1.5f, 1.5f);

            Shadow shadow = container.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.90f);
            shadow.effectDistance = new Vector2(2f, -2f);

            // Left Badge Pill (e.g. "HULL" or "ENERGY")
            float badgeWidth = string.IsNullOrEmpty(badgeText) ? 0f : 68f;
            if (!string.IsNullOrEmpty(badgeText))
            {
                GameObject badgeObj = new GameObject("Badge");
                badgeObj.transform.SetParent(container.transform, false);
                RectTransform bRect = badgeObj.AddComponent<RectTransform>();
                bRect.anchorMin = new Vector2(0f, 0f);
                bRect.anchorMax = new Vector2(0f, 1f);
                bRect.pivot = new Vector2(0f, 0.5f);
                bRect.sizeDelta = new Vector2(badgeWidth, 0f);
                bRect.anchoredPosition = Vector2.zero;

                Image bImg = badgeObj.AddComponent<Image>();
                bImg.color = new Color(themeColor.r * 0.20f, themeColor.g * 0.20f, themeColor.b * 0.20f, 0.95f);

                Outline bOl = badgeObj.AddComponent<Outline>();
                bOl.effectColor = new Color(themeColor.r, themeColor.g, themeColor.b, 0.8f);
                bOl.effectDistance = new Vector2(1f, 1f);

                GameObject bTxtObj = new GameObject("BadgeText");
                bTxtObj.transform.SetParent(badgeObj.transform, false);
                RectTransform btRect = bTxtObj.AddComponent<RectTransform>();
                btRect.anchorMin = Vector2.zero;
                btRect.anchorMax = Vector2.one;
                btRect.offsetMin = Vector2.zero;
                btRect.offsetMax = Vector2.zero;

                Text bTxt = bTxtObj.AddComponent<Text>();
                bTxt.font = GetDefaultFont();
                bTxt.text = badgeText;
                bTxt.fontSize = Mathf.Max(11, fontSize - 4);
                bTxt.fontStyle = FontStyle.Bold;
                bTxt.alignment = TextAnchor.MiddleCenter;
                bTxt.color = themeColor;
                bTxt.raycastTarget = false;
            }

            // Bar Track Area
            float barPadding = 2f;
            GameObject trackObj = new GameObject("BarTrack");
            trackObj.transform.SetParent(container.transform, false);
            RectTransform trackRect = trackObj.AddComponent<RectTransform>();
            trackRect.anchorMin = Vector2.zero;
            trackRect.anchorMax = Vector2.one;
            trackRect.offsetMin = new Vector2(badgeWidth + barPadding, barPadding);
            trackRect.offsetMax = new Vector2(-barPadding, -barPadding);

            Image trackImg = trackObj.AddComponent<Image>();
            trackImg.color = new Color(0.015f, 0.025f, 0.05f, 0.95f);

            // Fill Bar
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(trackObj.transform, false);
            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            Image fill = fillObj.AddComponent<Image>();
            fill.color = themeColor;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = 1f;

            // Specular Highlight Line (Glass / Energy Chamber Sheen)
            GameObject glowLineObj = new GameObject("GlassGlow");
            glowLineObj.transform.SetParent(trackObj.transform, false);
            RectTransform glRect = glowLineObj.AddComponent<RectTransform>();
            glRect.anchorMin = new Vector2(0f, 0.65f);
            glRect.anchorMax = new Vector2(1f, 1f);
            glRect.offsetMin = Vector2.zero;
            glRect.offsetMax = Vector2.zero;

            Image glImg = glowLineObj.AddComponent<Image>();
            glImg.color = new Color(1f, 1f, 1f, 0.22f);
            glImg.raycastTarget = false;

            // Optional Hit Segment Dividers
            if (hitSegments > 1)
            {
                for (int i = 1; i < hitSegments; i++)
                {
                    float segX = (float)i / hitSegments;
                    GameObject notch = new GameObject($"Notch_{i}");
                    notch.transform.SetParent(trackObj.transform, false);
                    RectTransform nRect = notch.AddComponent<RectTransform>();
                    nRect.anchorMin = new Vector2(segX, 0f);
                    nRect.anchorMax = new Vector2(segX, 1f);
                    nRect.sizeDelta = new Vector2(2f, 0f);

                    Image nImg = notch.AddComponent<Image>();
                    nImg.color = new Color(0.01f, 0.03f, 0.08f, 0.85f);
                    nImg.raycastTarget = false;
                }
            }

            // High-Visibility Status Text Label
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(trackObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(6, 0);
            textRect.offsetMax = new Vector2(-6, 0);

            Text txt = textObj.AddComponent<Text>();
            txt.font = GetDefaultFont();
            txt.text = title;
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.supportRichText = true;
            txt.raycastTarget = false;

            Shadow sh = textObj.AddComponent<Shadow>();
            sh.effectColor = new Color(0f, 0f, 0f, 0.95f);
            sh.effectDistance = new Vector2(1.5f, -1.5f);

            return (fill, txt);
        }

        public static Slider CreateSlider(Transform parent, string name, Vector2 size, Color handleColor)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = size;

            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minWidth = size.x;
            le.preferredWidth = size.x;
            le.minHeight = size.y;
            le.preferredHeight = size.y;

            Slider slider = go.AddComponent<Slider>();

            // Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(go.transform, false);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 0.25f);
            bgRect.anchorMax = new Vector2(1f, 0.75f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bg = bgObj.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.15f, 0.25f, 0.9f);

            // Fill Area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(go.transform, false);
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;

            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillArea.transform, false);
            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            Image fill = fillObj.AddComponent<Image>();
            fill.color = new Color(0.2f, 0.8f, 1f);

            slider.fillRect = fillRect;

            // Handle Area
            GameObject handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(go.transform, false);
            RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = Vector2.zero;
            handleAreaRect.offsetMax = Vector2.zero;

            GameObject handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(handleArea.transform, false);
            RectTransform handleRect = handleObj.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(22f, 0f);
            Image handle = handleObj.AddComponent<Image>();
            handle.color = handleColor;

            slider.handleRect = handleRect;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            return slider;
        }

        private static Sprite homeUISprite;

        public static Sprite GetHomeUISprite()
        {
            if (homeUISprite != null) return homeUISprite;

            // 1. Try Resources.Load
            homeUISprite = Resources.Load<Sprite>("HomeUI");
            if (homeUISprite != null) return homeUISprite;

            // 2. Try raw bytes from file paths
            string[] possiblePaths = new string[]
            {
                System.IO.Path.Combine(Application.dataPath, "Textures", "HomeUI.png"),
                System.IO.Path.Combine(Application.dataPath, "Resources", "HomeUI.png"),
                System.IO.Path.Combine(Application.dataPath, "Textures", "HomeUI.jpg"),
                System.IO.Path.Combine(Application.streamingAssetsPath, "HomeUI.png")
            };

            foreach (string p in possiblePaths)
            {
                if (System.IO.File.Exists(p))
                {
                    try
                    {
                        byte[] bytes = System.IO.File.ReadAllBytes(p);
                        Texture2D tex = new Texture2D(1024, 576, TextureFormat.RGBA32, false);
                        if (tex.LoadImage(bytes))
                        {
                            tex.filterMode = FilterMode.Bilinear;
                            homeUISprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                            return homeUISprite;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning("[UIBuilderHelper] Failed loading HomeUI sprite from: " + p + " -> " + ex.Message);
                    }
                }
            }

            return null;
        }

        public static Button CreateInteractiveOverlayButton(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color? hoverTint = null, Color? pressedTint = null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image img = go.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.001f);
            img.raycastTarget = true;

            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            ColorBlock cb = btn.colors;
            cb.normalColor = new Color(1f, 1f, 1f, 0f);
            cb.highlightedColor = hoverTint ?? new Color(1f, 1f, 1f, 0.22f);
            cb.pressedColor = pressedTint ?? new Color(0.8f, 0.8f, 0.8f, 0.45f);
            cb.selectedColor = new Color(1f, 1f, 1f, 0f);
            cb.disabledColor = new Color(0f, 0f, 0f, 0f);
            cb.colorMultiplier = 1f;
            btn.colors = cb;

            return btn;
        }

        private static Sprite goldPlayButtonSprite;
        private static Sprite lockedButtonSprite;

        public static Sprite GetGoldPlayButtonSprite()
        {
            if (goldPlayButtonSprite != null) return goldPlayButtonSprite;

            goldPlayButtonSprite = Resources.Load<Sprite>("GoldPlayButton");
            if (goldPlayButtonSprite != null) return goldPlayButtonSprite;

            string[] paths = new string[]
            {
                System.IO.Path.Combine(Application.dataPath, "Textures", "GoldPlayButton.png"),
                System.IO.Path.Combine(Application.dataPath, "Resources", "GoldPlayButton.png")
            };

            foreach (string p in paths)
            {
                if (System.IO.File.Exists(p))
                {
                    try
                    {
                        byte[] bytes = System.IO.File.ReadAllBytes(p);
                        Texture2D tex = new Texture2D(256, 96, TextureFormat.RGBA32, false);
                        if (tex.LoadImage(bytes))
                        {
                            tex.filterMode = FilterMode.Bilinear;
                            Vector4 border = new Vector4(46, 46, 46, 46);
                            goldPlayButtonSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
                            return goldPlayButtonSprite;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning("[UIBuilderHelper] Failed loading GoldPlayButton: " + ex.Message);
                    }
                }
            }

            return GetRoundedCornerSprite(24);
        }

        public static Sprite GetLockedButtonSprite()
        {
            if (lockedButtonSprite != null) return lockedButtonSprite;

            lockedButtonSprite = Resources.Load<Sprite>("LockedButton");
            if (lockedButtonSprite != null) return lockedButtonSprite;

            string[] paths = new string[]
            {
                System.IO.Path.Combine(Application.dataPath, "Textures", "LockedButton.png"),
                System.IO.Path.Combine(Application.dataPath, "Resources", "LockedButton.png")
            };

            foreach (string p in paths)
            {
                if (System.IO.File.Exists(p))
                {
                    try
                    {
                        byte[] bytes = System.IO.File.ReadAllBytes(p);
                        Texture2D tex = new Texture2D(256, 96, TextureFormat.RGBA32, false);
                        if (tex.LoadImage(bytes))
                        {
                            tex.filterMode = FilterMode.Bilinear;
                            Vector4 border = new Vector4(46, 46, 46, 46);
                            lockedButtonSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
                            return lockedButtonSprite;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning("[UIBuilderHelper] Failed loading LockedButton: " + ex.Message);
                    }
                }
            }

            return GetRoundedCornerSprite(24);
        }

        public static Button CreateGoldPlayButton(Transform parent, string name, string label, Vector2 size, int fontSize = 28)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = size;

            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minWidth = size.x;
            le.preferredWidth = size.x;
            le.minHeight = size.y;
            le.preferredHeight = size.y;

            Image img = go.AddComponent<Image>();
            img.sprite = GetGoldPlayButtonSprite();
            img.type = Image.Type.Sliced;
            img.color = Color.white;
            img.raycastTarget = true;

            // Radiant golden glowing outline
            Outline ol = go.AddComponent<Outline>();
            ol.effectColor = new Color(1f, 0.95f, 0.5f, 0.95f);
            ol.effectDistance = new Vector2(2.5f, 2.5f);

            // 3D drop shadow
            Shadow bShadow = go.AddComponent<Shadow>();
            bShadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
            bShadow.effectDistance = new Vector2(3f, -3f);

            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            cb.selectedColor = Color.white;
            cb.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.7f);
            cb.colorMultiplier = 1f;
            btn.colors = cb;

            // Label
            GameObject labelObj = new GameObject("BtnLabel");
            labelObj.transform.SetParent(go.transform, false);
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(10, 2);
            labelRect.offsetMax = new Vector2(-10, -2);

            Text txt = labelObj.AddComponent<Text>();
            txt.font = GetDefaultFont();
            txt.text = label;
            txt.fontSize = fontSize;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.fontStyle = FontStyle.Bold;
            txt.supportRichText = true;
            txt.raycastTarget = false;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Truncate;
            txt.resizeTextForBestFit = true;
            txt.resizeTextMinSize = Mathf.Max(12, fontSize - 8);
            txt.resizeTextMaxSize = fontSize;

            Shadow shadow = labelObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.2f, 0.08f, 0f, 0.95f);
            shadow.effectDistance = new Vector2(2f, -2f);

            return btn;
        }
    }
}
