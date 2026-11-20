using DienGame.Client;
using DienGame.Gameplay.World;
using UnityEngine;

namespace DienGame.Gameplay.Equipment
{
    /// <summary>
    /// Persist each core item level/unlock and one equipped item per class.
    /// Does not replace the user's saved armour/charm, gold, gems or fragments.
    /// PlayerPrefs is for the current local/offline prototype only; authoritative
    /// inventory and purchases must be server verified in online play.
    /// </summary>
    public static class EquipmentInventory
    {
        private const string Prefix = "diengame_equipment_v2_";
        private const string LegacyMageMigrationKey = Prefix + "biomech_legacy_pet_migrated_v1";
        private static bool _migrationCheckedThisSession;

        /// <summary>
        /// Four old fantasy pet equipment choices were superseded by biomechanical
        /// versions. Transfer unlocks, reinforcement and selection with no cost.
        /// The old save keys are intentionally left intact for safe rollback.
        /// </summary>
        private static void MigrateLegacyMagePets()
        {
            if (_migrationCheckedThisSession) return;
            _migrationCheckedThisSession = true;
            if (PlayerPrefs.GetInt(LegacyMageMigrationKey, 0) == 1) return;

            string[] oldIds = { "mage_icefox", "mage_firehawk", "mage_lizard", "mage_void" };
            string[] newIds = { "mage_bio_icefox", "mage_bio_firehawk", "mage_bio_lizard", "mage_bio_void" };
            string selectedKey = Prefix + "selected_" + (int)PlayableClass.Mage;
            string selectedId = PlayerPrefs.GetString(selectedKey, "mage_chimera");

            for (int i = 0; i < oldIds.Length; i++)
            {
                string oldUnlockKey = Prefix + "unlocked_" + oldIds[i];
                string oldLevelKey = Prefix + "level_" + oldIds[i];
                string newUnlockKey = Prefix + "unlocked_" + newIds[i];
                string newLevelKey = Prefix + "level_" + newIds[i];
                bool wasEquipped = selectedId == oldIds[i];
                int oldLevel = Mathf.Clamp(PlayerPrefs.GetInt(oldLevelKey, 0), 0, EquipmentCatalog.MaxLevel);
                if (wasEquipped || PlayerPrefs.GetInt(oldUnlockKey, 0) == 1 || oldLevel > 0)
                    PlayerPrefs.SetInt(newUnlockKey, 1);
                if (oldLevel > PlayerPrefs.GetInt(newLevelKey, 0))
                    PlayerPrefs.SetInt(newLevelKey, oldLevel);
                if (wasEquipped) PlayerPrefs.SetString(selectedKey, newIds[i]);
            }

            PlayerPrefs.SetInt(LegacyMageMigrationKey, 1);
            PlayerPrefs.Save();
        }

        public static EquipmentDefinition Selected(PlayableClass cls)
        {
            MigrateLegacyMagePets();
            EquipmentDefinition fallback = EquipmentCatalog.Starter(cls);
            if (fallback == null) return null;
            string id = PlayerPrefs.GetString(Prefix + "selected_" + (int)cls, fallback.Id);
            if (EquipmentQaSandbox.Active)
                id = EquipmentQaSandbox.Selected(cls, id);
            EquipmentDefinition found = EquipmentCatalog.Find(id);
            return found != null && found.Class == cls && IsUnlocked(found) ? found : fallback;
        }
        public static bool IsUnlocked(EquipmentDefinition item)
        {
            MigrateLegacyMagePets();
            return item != null && (EquipmentQaSandbox.Active || item.Starter ||
                PlayerPrefs.GetInt(Prefix + "unlocked_" + item.Id, 0) == 1);
        }

        public static int Level(EquipmentDefinition item)
        {
            MigrateLegacyMagePets();
            if (item == null) return 0;
            if (EquipmentQaSandbox.Active) return EquipmentQaSandbox.ItemLevel(item);
            string key = Prefix + "level_" + item.Id;
            // Migrate the legacy global weapon level to each starting core type,
            // but only until that type receives its first v2 save.
            if (item.Starter && !PlayerPrefs.HasKey(key))
                return Mathf.Clamp(WorldInventory.WeaponLevel, 0, EquipmentCatalog.MaxLevel);
            return Mathf.Clamp(PlayerPrefs.GetInt(key, 0), 0, EquipmentCatalog.MaxLevel);
        }
        public static int UpgradeGoldCost(EquipmentDefinition item) => item == null ? int.MaxValue : 180 * (Level(item) + 1);
        public static int UpgradeFragmentsCost(EquipmentDefinition item) => item == null ? int.MaxValue : 2 + Level(item);

        public static bool TryUnlock(EquipmentDefinition item)
        {
            if (item == null || item.Starter || IsUnlocked(item)) return false;
            if (WorldInventory.Fragments < item.UnlockFragments || MetaProgressService.Gold < item.UnlockGold) return false;
            if (!MetaProgressService.TrySpendGold(item.UnlockGold)) return false;
            WorldInventory.SpendFragments(item.UnlockFragments);
            PlayerPrefs.SetInt(Prefix + "unlocked_" + item.Id, 1);
            PlayerPrefs.Save();
            return true;
        }
        public static bool TryEquip(EquipmentDefinition item)
        {
            if (item == null || !IsUnlocked(item)) return false;
            if (EquipmentCatalog.Starter(item.Class) == null) return false;
            if (EquipmentQaSandbox.Active)
            {
                EquipmentQaSandbox.Select(item);
                return true;
            }
            PlayerPrefs.SetString(Prefix + "selected_" + (int)item.Class, item.Id);
            PlayerPrefs.Save();
            return true;
        }
        public static bool TryUpgrade(EquipmentDefinition item)
        {
            if (item == null || !IsUnlocked(item) || Level(item) >= EquipmentCatalog.MaxLevel) return false;
            if (EquipmentQaSandbox.Active)
            {
                EquipmentQaSandbox.SetItemLevel(item, Level(item) + 1);
                return true; // No currency spent; no real save changes.
            }
            int fragments = UpgradeFragmentsCost(item), gold = UpgradeGoldCost(item);
            if (WorldInventory.Fragments < fragments || MetaProgressService.Gold < gold) return false;
            if (!MetaProgressService.TrySpendGold(gold)) return false;
            WorldInventory.SpendFragments(fragments);
            PlayerPrefs.SetInt(Prefix + "level_" + item.Id, Level(item) + 1);
            PlayerPrefs.Save();
            return true;
        }
    }
}
