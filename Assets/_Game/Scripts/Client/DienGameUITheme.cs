using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DienGame.Client
{
    /// <summary>
    /// Single source of truth for runtime UI art, layout and Vietnamese typography.
    /// All visual assets are part of Resources/UITheme. No Inspector setup is needed.
    /// </summary>
    public static class DienGameUITheme
    {
        public static readonly Color Gold = new Color(.99f, .81f, .48f);
        public static readonly Color Ivory = new Color(.96f, .96f, .91f);
        public static readonly Color Muted = new Color(.72f, .83f, .84f);
        public static readonly Color Teal = new Color(.47f, .94f, .81f);
        public static readonly Color Red = new Color(.99f, .53f, .52f);
        public static readonly Color Veil = new Color(.008f, .019f, .038f, .86f);

        private static TMP_FontAsset _font;
        private static bool _fontChecked;

        public static void Skin(Image image, string spriteName)
        {
            if (image == null) return;
            Sprite s = Resources.Load<Sprite>("UITheme/" + spriteName);
            if (s == null) return;
            image.sprite = s;
            image.type = s.border.sqrMagnitude > 0f ? Image.Type.Sliced : Image.Type.Simple;
            image.color = Color.white;
        }

        public static void StyleText(TMP_Text label, float fontSize, Color color, bool bold = false,
            bool wrap = true, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            if (label == null) return;
            EnsureFont();
            if (_font != null) label.font = _font;
            label.fontSize = fontSize;
            label.enableAutoSizing = true;
            label.fontSizeMax = fontSize;
            // Keep a legible floor; fitting is handled by per-panel layout first.
            label.fontSizeMin = Mathf.Max(17f, fontSize * .66f);
            label.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            label.alignment = alignment;
            label.enableWordWrapping = wrap;
            // Ellipsis is explicit: an incomplete word must never be rendered across the frame.
            // UI QA reports labels that still require ellipsis after autosizing.
            label.overflowMode = TextOverflowModes.Ellipsis;
            label.color = color;
            label.raycastTarget = false;
            label.margin = new Vector4(4f, 2f, 4f, 2f);
            label.isRightToLeftText = false;
        }

        private static void EnsureFont()
        {
            if (_fontChecked) return;
            _fontChecked = true;
            _font = Resources.Load<TMP_FontAsset>("UITheme/VietnameseFont");
            if (_font == null)
            {
                // Existing project asset: dynamic TMP fallback with Unicode source font.
                // Unlike platform OS fonts, this is available in Android and WebGL builds.
                _font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF - Fallback");
            }
            if (_font != null) return;
            try
            {
                // Dynamic OS fonts provide Vietnamese glyphs even when the project's
                // default static TMP atlas omits them. Fall back safely on unsupported builds.
                Font osFont = Font.CreateDynamicFontFromOSFont(
                    new[] { "Arial", "Roboto", "Noto Sans", "Liberation Sans" }, 40);
                if (osFont != null) _font = TMP_FontAsset.CreateFontAsset(osFont);
            }
            catch (Exception e)
            {
                Debug.LogWarning("DienGame UI: unable to create dynamic font; using TMP default. " + e.Message);
            }
        }

        public static GameObject Panel(Transform parent, string name, string skin = "Panel")
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Image img = go.GetComponent<Image>();
            img.raycastTarget = false;
            img.color = new Color(.02f, .055f, .09f, .98f);
            Skin(img, skin);
            return go;
        }

        public static void Rect(RectTransform rt, float x0, float y0, float x1, float y1,
            float left = 0, float bottom = 0, float right = 0, float top = 0)
        {
            rt.anchorMin = new Vector2(x0, y0);
            rt.anchorMax = new Vector2(x1, y1);
            rt.pivot = new Vector2(.5f, .5f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        public static TextMeshProUGUI Text(Transform parent, string name, string content,
            float size, Color color, float x0, float y0, float x1, float y1,
            TextAlignmentOptions align = TextAlignmentOptions.Center, bool bold = false)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            Rect((RectTransform)go.transform, x0, y0, x1, y1);
            var text = go.GetComponent<TextMeshProUGUI>();
            text.text = content;
            StyleText(text, size, color, bold, true, align);
            return text;
        }

        public static Button Button(Transform parent, string name, string caption, Action onClick,
            string skin, float x0, float y0, float x1, float y1, float textSize = 32f)
        {
            GameObject go = Panel(parent, name, skin);
            Rect((RectTransform)go.transform, x0, y0, x1, y1);
            Image bg = go.GetComponent<Image>();
            bg.raycastTarget = true;
            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = bg;
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.05f, 1.05f, 1.04f);
            colors.pressedColor = new Color(.78f, .86f, .87f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(.48f, .49f, .49f, .76f);
            colors.colorMultiplier = 1f;
            btn.colors = colors;
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            Color fg = skin == "ButtonGold" ? new Color(.19f, .13f, .07f) : Ivory;
            Text(go.transform, "Caption", caption, textSize, fg, .07f, .10f, .93f, .90f,
                TextAlignmentOptions.Center, true);
            return btn;
        }

        public static Image Icon(Transform parent, string name, Sprite sprite,
            float x0, float y0, float x1, float y1)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Rect((RectTransform)go.transform, x0, y0, x1, y1);
            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }
    }
}
