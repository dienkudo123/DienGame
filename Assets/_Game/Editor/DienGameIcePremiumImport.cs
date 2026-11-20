#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    // Unique class name; does not replace or duplicate DienGameWorldInstaller.
    public sealed class DienGameIcePremiumImport : AssetPostprocessor
    {
        private const string Prefix = "Assets/_Game/Resources/World/IcePremium/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Prefix, System.StringComparison.OrdinalIgnoreCase)) return;
            TextureImporter tex = assetImporter as TextureImporter;
            if (tex == null) return;
            tex.textureType = TextureImporterType.Sprite;
            tex.spriteImportMode = SpriteImportMode.Single;
            tex.alphaIsTransparency = true;
            tex.mipmapEnabled = false;
            tex.npotScale = TextureImporterNPOTScale.None;
            tex.textureCompression = TextureImporterCompression.Uncompressed;
            tex.maxTextureSize = 4096;
            tex.filterMode = FilterMode.Bilinear;
        }

        [MenuItem("DienGame/Ice Arena V2/Reimport Premium Art")]
        public static void ReimportArt()
        {
            string[] ids = AssetDatabase.FindAssets("t:Texture2D", new[] { Prefix.TrimEnd('/') });
            foreach (string id in ids)
            {
                string path = AssetDatabase.GUIDToAssetPath(id);
                if (path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
            AssetDatabase.Refresh();
            Debug.Log("DienGame Ice Arena V2: imported " + ids.Length + " textures.");
        }
    }
}
#endif
