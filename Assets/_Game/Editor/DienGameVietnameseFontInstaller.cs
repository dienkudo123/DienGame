#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;

namespace DienGame.Editor
{
    /// <summary>
    /// Reuses the LiberationSans font that already exists in this Unity project.
    /// Generates a dynamic TMP asset with Vietnamese glyph coverage automatically,
    /// without importing or shipping any additional font files.
    /// </summary>
    [InitializeOnLoad]
    public static class DienGameVietnameseFontInstaller
    {
        private const string FontSource = "Assets/TextMesh Pro/Fonts/LiberationSans.ttf";
        private const string FontTarget = "Assets/_Game/Resources/UITheme/VietnameseFont.asset";

        static DienGameVietnameseFontInstaller()
        {
            EditorApplication.delayCall += EnsureFont;
        }

        [MenuItem("DienGame/UI/01 - Kiem tra font tieng Viet")]
        public static void EnsureFont()
        {
            if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontTarget) != null) return;
            Font baseFont = AssetDatabase.LoadAssetAtPath<Font>(FontSource);
            if (baseFont == null)
            {
                Debug.LogWarning("DienGame: Could not find existing LiberationSans font. " + FontSource);
                return;
            }
            TMP_FontAsset font = TMP_FontAsset.CreateFontAsset(baseFont);
            if (font == null) return;
            font.name = "DienGameVietnameseDynamic";
            font.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            AssetDatabase.CreateAsset(font, FontTarget);
            if (font.material != null && !AssetDatabase.Contains(font.material))
                AssetDatabase.AddObjectToAsset(font.material, font);
            if (font.atlasTextures != null)
            {
                foreach (Texture2D atlas in font.atlasTextures)
                    if (atlas != null && !AssetDatabase.Contains(atlas))
                        AssetDatabase.AddObjectToAsset(atlas, font);
            }
            EditorUtility.SetDirty(font);
            AssetDatabase.SaveAssets();
            Debug.Log("DienGame UI: created a dynamic Vietnamese TMP font asset.");
        }
    }
}
#endif
