using UnityEngine;
using UnityEngine.UI;

namespace SpaceJet.UI
{
    public class FloatingText : MonoBehaviour
    {
        public Text textComponent;
        public float duration = 1.0f;
        public float floatSpeed = 3.5f;

        private float elapsed = 0f;
        private Color initialColor;

        public void Initialize(string message, Color color)
        {
            if (textComponent == null)
            {
                textComponent = GetComponentInChildren<Text>();
            }

            if (textComponent != null)
            {
                textComponent.text = message;
                textComponent.color = color;
                initialColor = color;
            }

            elapsed = 0f;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;

            // Float upward and face camera
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;
            if (Camera.main)
            {
                transform.rotation = Camera.main.transform.rotation;
            }

            // Fade out
            if (textComponent != null)
            {
                float alpha = Mathf.Clamp01(1f - (elapsed / duration));
                textComponent.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            }

            if (elapsed >= duration)
            {
                Destroy(gameObject);
            }
        }
    }

    public static class FloatingTextSpawner
    {
        private static GameObject textPrefab;

        public static void SetPrefab(GameObject prefab)
        {
            textPrefab = prefab;
        }

        public static void Spawn(string message, Vector3 worldPosition, Color color)
        {
            if (textPrefab != null)
            {
                GameObject obj = Object.Instantiate(textPrefab, worldPosition + Vector3.up * 1f, Quaternion.identity);
                obj.SetActive(true);
                FloatingText ft = obj.GetComponent<FloatingText>();
                if (ft) ft.Initialize(message, color);
            }
            else
            {
                // Fallback procedural creation
                GameObject go = new GameObject("FloatingTextPopup");
                go.transform.position = worldPosition + Vector3.up * 1f;
                Canvas canvas = go.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                go.AddComponent<CanvasScaler>();

                RectTransform rect = go.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(340, 75);
                rect.localScale = Vector3.one * 0.026f;

                GameObject textGo = new GameObject("Label");
                textGo.transform.SetParent(go.transform, false);
                Text txt = textGo.AddComponent<Text>();
                txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                txt.fontSize = 36;
                txt.fontStyle = FontStyle.Bold;
                txt.alignment = TextAnchor.MiddleCenter;

                Outline outl = textGo.AddComponent<Outline>();
                outl.effectColor = new Color(0f, 0f, 0f, 0.95f);
                outl.effectDistance = new Vector2(2f, -2f);

                FloatingText ft = go.AddComponent<FloatingText>();
                ft.textComponent = txt;
                ft.Initialize(message, color);
            }
        }
    }
}
