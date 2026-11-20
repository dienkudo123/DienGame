#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    /// <summary>
    /// Automatic import configuration. Only NEW biomech asset paths are modified;
    /// old player / enemy / Chimera / previous equipment textures are untouched.
    /// </summary>
    public sealed class DienGameBiomechImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            bool isBiomech = assetPath.StartsWith("Assets/_Game/Resources/Equipment/Biomech/");
            bool isIcon = assetPath.StartsWith("Assets/_Game/Resources/Equipment/Icons/Mage_Bio");
            if (!isBiomech && !isIcon) return;
            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.isReadable = isBiomech && (assetPath.Contains("/Walk/") || assetPath.Contains("/Attack/"));
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
        }
    }
}
#endif
