#if UNITY_EDITOR
using DienGame.Gameplay.Skills.Mage;
using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    /// <summary>Asset and live preview checks; not compiled into player builds.</summary>
    public static class DienGameApprovedPetArtQA
    {
        private const string Root = "Assets/_Game/Resources/Equipment/BiomechApproved/";
        private static readonly string[] Forms = { "IceFox", "FireHawk", "PoisonLizard", "VoidOrb" };
        private static readonly string[] Types = { "Walk", "Cast", "CastLv5" };

        [MenuItem("DienGame/Pets/Approved V3/01 - Verify 12 sprite sheets")]
        public static void Verify()
        {
            int passed = 0;
            foreach (string form in Forms)
            foreach (string type in Types)
            {
                string suffix = type == "CastLv5" ? "CastLv5_4x4" : type + "4x4";
                string path = Root + type + "/Bio_" + form + "_" + suffix + ".png";
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                bool ok = texture != null && texture.width == 1024 && texture.height == 1024 &&
                          importer != null && importer.isReadable && importer.textureType == TextureImporterType.Sprite;
                if (!ok)
                    Debug.LogError("DienGame Approved V3: missing/invalid sprite: " + path +
                                   " (must be Sprite, 1024x1024, Read/Write enabled)");
                else passed++;
            }
            Debug.Log("DienGame Approved V3 sprite QA: " + passed + "/12 valid sprites, " +
                (passed == 12 ? "PASS" : "FIX REQUIRED"));
        }

        [MenuItem("DienGame/Pets/Approved V3/02 - Preview equipped pet signature Lv1")]
        public static void PreviewLevel1() { Preview(false); }

        [MenuItem("DienGame/Pets/Approved V3/03 - Preview equipped pet signature Lv5")]
        public static void PreviewLevel5() { Preview(true); }

        private static void Preview(bool level5)
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogWarning("Open Mage gameplay in Play Mode first.");
                return;
            }
            ChimeraCompanion pet = Object.FindFirstObjectByType<ChimeraCompanion>();
            if (pet == null || !pet.DebugPreviewApprovedSignature(level5))
            {
                Debug.LogWarning("No equipped biomech pet with a signature active. Select IceFox, FireHawk, PoisonLizard or VoidOrb.");
                return;
            }
            Debug.Log("Approved V3: requested " + (level5 ? "Lv5" : "Lv1") +
                " pet cast visual preview (no damage, no save changes).");
        }
    }
}
#endif
