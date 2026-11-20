using DienGame.Client;
using DienGame.Gameplay.World;
using UnityEngine;

namespace DienGame.Gameplay.Equipment
{
    /// <summary>
    /// Isolated Phase 09 play-test state. All gear is available in the Unity Editor and
    /// DEVELOPMENT_BUILD only; a release build always reads the real inventory.
    /// This class never modifies live progression keys or currency balances.
    /// </summary>
    public static class EquipmentQaSandbox
    {
        private const string Prefix = "diengame_phase09_qa_";
        private const string ActiveKey = Prefix + "enabled";
        private const int StarterTestLevel = 3;

        public static bool Active
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                return PlayerPrefs.GetInt(ActiveKey, 1) == 1;
#else
                return false;
#endif
            }
        }

        public static int ItemLevel(EquipmentDefinition item)
        {
            if (item == null) return 0;
            return Mathf.Clamp(PlayerPrefs.GetInt(Prefix + "level_" + item.Id, StarterTestLevel),
                0, EquipmentCatalog.MaxLevel);
        }

        public static void SetItemLevel(EquipmentDefinition item, int level)
        {
            if (!Active || item == null) return;
            PlayerPrefs.SetInt(Prefix + "level_" + item.Id,
                Mathf.Clamp(level, 0, EquipmentCatalog.MaxLevel));
            PlayerPrefs.Save();
        }

        public static int SharedLevel(int slot)
        {
            return Mathf.Clamp(PlayerPrefs.GetInt(Prefix + "shared_" + slot, 0), 0, 5);
        }

        public static void SetSharedLevel(int slot, int level)
        {
            if (!Active || slot < 0 || slot > 2) return;
            PlayerPrefs.SetInt(Prefix + "shared_" + slot, Mathf.Clamp(level, 0, 5));
            PlayerPrefs.Save();
        }

        public static string Selected(PlayableClass cls, string defaultId)
        {
            return PlayerPrefs.GetString(Prefix + "selected_" + (int)cls, defaultId);
        }

        public static void Select(EquipmentDefinition item)
        {
            if (!Active || item == null) return;
            PlayerPrefs.SetString(Prefix + "selected_" + (int)item.Class, item.Id);
            PlayerPrefs.Save();
        }

        // Test chip/calibration values are separate from real saved equipment presets.
        public static int ReadOption(string category, string id)
        {
            return PlayerPrefs.GetInt(Prefix + category + "_" + id, 0);
        }

        public static void WriteOption(string category, string id, int value)
        {
            if (!Active || string.IsNullOrEmpty(id)) return;
            PlayerPrefs.SetInt(Prefix + category + "_" + id, value);
            PlayerPrefs.Save();
        }

        public static void SetEnabled(bool enabled)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            PlayerPrefs.SetInt(ActiveKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
#else
            Debug.LogWarning("[DienGame] Test trang bị chỉ bật ở Editor/Development Build.");
#endif
        }

        /// <summary>
        /// Only QA keys are reset. No PlayerPrefs.DeleteAll and no actual equipment
        /// unlock, item upgrades, class selection, gold, or fragments are touched.
        /// </summary>
        public static void ResetTestOverrides()
        {
            foreach (EquipmentDefinition item in EquipmentCatalog.All)
            {
                PlayerPrefs.DeleteKey(Prefix + "level_" + item.Id);
                PlayerPrefs.DeleteKey(Prefix + "chip_" + item.Id);
                PlayerPrefs.DeleteKey(Prefix + "calibration_" + item.Id);
            }
            foreach (PlayableClass cls in System.Enum.GetValues(typeof(PlayableClass)))
                PlayerPrefs.DeleteKey(Prefix + "selected_" + (int)cls);
            for (int i = 0; i < 3; i++) PlayerPrefs.DeleteKey(Prefix + "shared_" + i);
            PlayerPrefs.Save();
        }

        /// <summary>Set every core item to a chosen test tier, including Warrior data.</summary>
        public static void SetAllLevels(int level)
        {
            if (!Active) return;
            int safeLevel = Mathf.Clamp(level, 0, EquipmentCatalog.MaxLevel);
            foreach (EquipmentDefinition item in EquipmentCatalog.All)
                PlayerPrefs.SetInt(Prefix + "level_" + item.Id, safeLevel);
            PlayerPrefs.Save();
        }
    }
}
