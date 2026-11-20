#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    /// <summary>
    /// Deletes ONLY the four obsolete non-biomech pet sprites and icons.
    /// Installing a ZIP patch adds/replaces files but cannot delete old ones,
    /// so run this once after scripts reload. No Chimera or biomech files touched.
    /// </summary>
    [InitializeOnLoad]
    public static class DienGameLegacyPetCleaner
    {
        private static readonly string[] OldAssetPaths =
        {
            "Assets/_Game/Resources/Equipment/Icons/Mage_IceFox.png",
            "Assets/_Game/Resources/Equipment/Icons/Mage_FireHawk.png",
            "Assets/_Game/Resources/Equipment/Icons/Mage_PoisonLizard.png",
            "Assets/_Game/Resources/Equipment/Icons/Mage_VoidOrb.png",
            "Assets/_Game/Resources/Equipment/Pets/Mage_IceFox.png",
            "Assets/_Game/Resources/Equipment/Pets/Mage_FireHawk.png",
            "Assets/_Game/Resources/Equipment/Pets/Mage_PoisonLizard.png",
            "Assets/_Game/Resources/Equipment/Pets/Mage_VoidOrb.png"
        };

        static DienGameLegacyPetCleaner()
        {
            EditorApplication.delayCall += RemoveLegacySprites;
        }

        [MenuItem("DienGame/Equipment/04 - Xoa 4 thu fantasy trung lap")]
        public static void RemoveLegacySprites()
        {
            // The four old IDs must no longer exist in the catalog before deleting their art.
            if (DienGame.Gameplay.Equipment.EquipmentCatalog.Find("mage_icefox") != null ||
                DienGame.Gameplay.Equipment.EquipmentCatalog.Find("mage_firehawk") != null ||
                DienGame.Gameplay.Equipment.EquipmentCatalog.Find("mage_lizard") != null ||
                DienGame.Gameplay.Equipment.EquipmentCatalog.Find("mage_void") != null)
            {
                Debug.LogWarning("DienGame: catalog cu van con, huy don asset de tranh mat du lieu.");
                return;
            }

            int deleted = 0;
            foreach (string path in OldAssetPaths)
            {
                if (AssetDatabase.LoadMainAssetAtPath(path) == null &&
                    !System.IO.File.Exists(path)) continue;
                if (AssetDatabase.DeleteAsset(path)) deleted++;
                else Debug.LogWarning("DienGame: khong xoa duoc asset cu: " + path);
            }

            const string oldDir = "Assets/_Game/Resources/Equipment/Pets";
            if (AssetDatabase.IsValidFolder(oldDir) && AssetDatabase.FindAssets("", new [] { oldDir }).Length == 0)
                AssetDatabase.DeleteAsset(oldDir);
            if (deleted > 0)
                Debug.Log("DienGame: da xoa " + deleted + "/8 PNG thu fantasy cu (kem .meta); Chimera + 5 thu co sinh giu nguyen.");
        }
    }
}
#endif
