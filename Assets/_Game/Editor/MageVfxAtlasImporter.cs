using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    /// <summary>
    /// Configures Mage VFX textures without overwriting Sprite Mode.
    /// MageArt slices 4x2 directly from Texture2D at runtime, regardless
    /// of whether the artist selects Single or Multiple in Sprite Editor.
    /// </summary>
    public sealed class MageVfxAtlasImporter : AssetPostprocessor
    {
        private const string Prefix = "Assets/_Game/Resources/Mage/Vfx/";
        private static readonly string[] Atlases =
        {
            "TinhChamPhanLiet", "TrongTruongDiGioi", "TraoLietHopKim",
            "PhaoSinhThe", "SongTrungOanhKich", "CongHuongSanMoi", "ManChanSongThe",
            "TinhChamPhanLiet_Lv5", "TrongTruongDiGioi_Lv5", "TraoLietHopKim_Lv5",
            "PhaoSinhThe_Lv5", "SongTrungOanhKich_Lv5", "CongHuongSanMoi_Lv5", "ManChanSongThe_Lv5"
        };

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Prefix, System.StringComparison.OrdinalIgnoreCase) ||
                !assetPath.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
                return;

            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            // Keep the Sprite Mode chosen in Inspector (Single or Multiple).
            // Forcing Single here prevented artists from using Slice.
            // MageArt loads Texture2D and creates runtime sprites, so no
            // spriteImportMode override is needed for gameplay.
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.isReadable = true;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
        }

        [MenuItem("DienGame/Mage/2 - Reimport 14 Normal + Lv5 VFX Atlases")]
        private static void ReimportVfx()
        {
            foreach (string name in Atlases)
            {
                string path = Prefix + name + ".png";
                if (AssetDatabase.LoadAssetAtPath<Texture2D>(path) != null)
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                else
                    Debug.LogWarning("Mage VFX atlas missing: " + path);
            }
            AssetDatabase.Refresh();
            Debug.Log("DienGame Mage: 14 normal/Lv5 VFX atlases imported; Sprite Mode preserved. MageArt slices 4x2 at runtime.");
        }
    }
}
