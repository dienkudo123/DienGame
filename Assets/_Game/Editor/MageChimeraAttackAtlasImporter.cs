using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    /// <summary>
    /// The runtime MageArt slicer handles 5x4. Keep Unity import as Single.
    /// </summary>
    public sealed class MageChimeraAttackAtlasImporter : AssetPostprocessor
    {
        private const string AtlasPath = "Assets/_Game/Resources/Mage/ChimeraAttack4x5.png";

        private void OnPreprocessTexture()
        {
            if (!assetPath.Equals(AtlasPath, System.StringComparison.OrdinalIgnoreCase)) return;

            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.isReadable = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
        }

        [MenuItem("DienGame/Mage/3 - Reimport Chimera Claw Attack")]
        private static void Reimport()
        {
            AssetDatabase.ImportAsset(AtlasPath, ImportAssetOptions.ForceUpdate);
            Debug.Log("Chimera Attack (4 rows x 5 frames) reimported.");
        }
    }
}
