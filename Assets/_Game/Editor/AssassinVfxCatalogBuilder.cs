#if UNITY_EDITOR
using System.IO;
using DienGame.Gameplay.Skills.Assassin;
using UnityEditor;
using UnityEngine;

namespace DienGame.Editor
{
    /// <summary>No Inspector work required; links the existing Art PNGs into a lightweight Resources asset.</summary>
    [InitializeOnLoad]
    public static class AssassinVfxCatalogBuilder
    {
        private const string ArtRoot = "Assets/_Game/Art/Skills/Assassin/";
        private const string TargetPath = "Assets/_Game/Resources/Assassin/VfxCatalog.asset";
        static AssassinVfxCatalogBuilder() { EditorApplication.delayCall += AutoSync; }
        private static void AutoSync()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!File.Exists(ArtRoot + "MeleeNormal.png")) return;
            Sync();
        }

        [MenuItem("DienGame/Assassin/Sync VFX from Art")]
        public static void Sync()
        {
            string dir = Path.GetDirectoryName(TargetPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            AssassinVfxCatalog catalog = AssetDatabase.LoadAssetAtPath<AssassinVfxCatalog>(TargetPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<AssassinVfxCatalog>();
                AssetDatabase.CreateAsset(catalog, TargetPath);
            }
            catalog.meleeNormal = Get("MeleeNormal");
            catalog.meleeLv5 = Get("MeleeLv5");
            catalog.bloodNormal = Get("BloodNormal");
            catalog.bloodLv5 = Get("BloodLv5");
            catalog.knifeNormal = Get("KnifeNormal");
            catalog.knifeLv5 = Get("KnifeLv5");
            catalog.deathMark = Get("DeathMark");
            catalog.orbitNormal = Get("OrbitNormal");
            catalog.orbitLv5 = Get("OrbitLv5");
            catalog.pillarNormal = Get("PillarNormal");
            catalog.pillarLv5 = Get("PillarLv5");
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log("DienGame: linked 11 Assassin VFX PNGs; game art remains under Art/Skills/Assassin.");
        }
        private static Texture2D Get(string name)
        {
            string path = ArtRoot + name + ".png";
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) { Debug.LogWarning("Assassin VFX missing " + path); return null; }
            bool needsImport = importer.textureType != TextureImporterType.Default ||
                               importer.mipmapEnabled || !importer.alphaIsTransparency;
            if (needsImport)
            {
                importer.textureType = TextureImporterType.Default;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}
#endif
