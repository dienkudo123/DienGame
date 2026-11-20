using DienGame.Client;
using UnityEngine;

namespace DienGame.Gameplay.Equipment
{
    /// <summary>
    /// Quy Tang-inspired EQUIPMENT GAMEPLAY only. This does not establish new story canon.
    /// Core calibration is unlocked at +4, does not spend currency, and has a nearly
    /// identical single-target DPS budget in every mode. It changes NORMAL attacks.
    /// Equipped gear and upgrade values from v7 are preserved without save migration.
    /// </summary>
    public enum EquipmentCalibrationMode { Balanced = 0, Impulse = 1, Rapid = 2 }

    public static class EquipmentResonance
    {
        private const string KeyPrefix = "diengame_equipment_phase08_calibration_";
        public const int UnlockAtLevel = 4;

        public static string ModeName(EquipmentCalibrationMode mode)
        {
            switch (mode)
            {
                case EquipmentCalibrationMode.Impulse: return "XUNG LỰC";
                case EquipmentCalibrationMode.Rapid: return "NHỊP NHANH";
                default: return "CÂN BẰNG";
            }
        }

        public static string ModeDescription(EquipmentCalibrationMode mode)
        {
            switch (mode)
            {
                case EquipmentCalibrationMode.Impulse: return "Đòn thường +10% lực • chu kỳ bắn/chém +12%";
                case EquipmentCalibrationMode.Rapid: return "Đòn thường -10% lực • chu kỳ bắn/chém -10%";
                default: return "Sát thương và nhịp đánh chuẩn • không đánh đổi";
            }
        }

        public static string FieldTag(EquipmentDefinition item)
        {
            if (item == null) return "CHƯA GẮN LÕI";
            if (item.Class == PlayableClass.Mage) return "CƠ-SINH · GIAO THOA TRƯỜNG X";
            switch (item.Effect)
            {
                case EquipmentEffect.Slow: return "TRƯỜNG X · ĐIỀU BIẾN";
                case EquipmentEffect.Arcane: return "TRƯỜNG X · CỘNG HƯỞNG";
                case EquipmentEffect.Poison: return "CÔNG NGHỆ SINH HỌC";
                case EquipmentEffect.Burn: return "CƠ GIỚI · NHIỆT NĂNG";
                default: return "CƠ GIỚI · NĂNG LƯỢNG";
            }
        }

        // Fictional design terminology, not a claim about a named weapon's canonical origin.
        public static string DesignFlavor(EquipmentDefinition item)
        {
            if (item == null) return string.Empty;
            switch (item.Class)
            {
                case PlayableClass.Marksman:
                    return "Buồng đạn cơ giới điều tiết tải năng lượng, giao thoa cùng trường X.";
                case PlayableClass.Assassin:
                    return "Lưỡi hợp kim dẫn xung qua mô-đun tiếp xúc, đổi tầm đánh lấy nhịp ra đòn.";
                case PlayableClass.Mage:
                    return "Khế ước điều khiển sinh thể lai máy; lõi và vật chủ đồng bộ theo chu kỳ.";
                case PlayableClass.Warrior:
                    return "Vũ khí cận chiến có bộ ổn áp, thay lực va chạm bằng tốc độ hồi lực.";
                default: return string.Empty;
            }
        }

        public static int EvolutionStage(int upgradeLevel)
        {
            int level = Mathf.Clamp(upgradeLevel, 0, EquipmentCatalog.MaxLevel);
            if (level >= 10) return 3;
            if (level >= 6) return 2;
            if (level >= 3) return 1;
            return 0;
        }

        public static string StageName(int upgradeLevel)
        {
            switch (EvolutionStage(upgradeLevel))
            {
                case 1: return "ỔN ÁP";
                case 2: return "CỘNG HƯỞNG";
                case 3: return "ĐỒNG BỘ";
                default: return "LÕI NGUYÊN BẢN";
            }
        }

        public static int NextStageLevel(int level)
        {
            if (level < 3) return 3;
            if (level < 6) return 6;
            if (level < 10) return 10;
            return 10;
        }

        public static bool CanCalibrate(EquipmentDefinition item)
        {
            return item != null && EquipmentInventory.IsUnlocked(item) &&
                   EquipmentInventory.Level(item) >= UnlockAtLevel;
        }

        public static EquipmentCalibrationMode CurrentMode(EquipmentDefinition item)
        {
            if (!CanCalibrate(item)) return EquipmentCalibrationMode.Balanced;
            int value = EquipmentQaSandbox.Active
                ? EquipmentQaSandbox.ReadOption("calibration", item.Id)
                : PlayerPrefs.GetInt(KeyPrefix + item.Id, 0);
            return value >= 0 && value <= 2 ? (EquipmentCalibrationMode)value : EquipmentCalibrationMode.Balanced;
        }

        public static bool TrySetMode(EquipmentDefinition item, EquipmentCalibrationMode mode)
        {
            if (!CanCalibrate(item) || !System.Enum.IsDefined(typeof(EquipmentCalibrationMode), mode)) return false;
            if (EquipmentQaSandbox.Active)
                EquipmentQaSandbox.WriteOption("calibration", item.Id, (int)mode);
            else
            {
                PlayerPrefs.SetInt(KeyPrefix + item.Id, (int)mode);
                PlayerPrefs.Save();
            }
            return true;
        }

        // Factors apply to normal attacks only, keeping passive/active skill budgets.
        public static float DamageFactor(EquipmentDefinition item)
        {
            switch (CurrentMode(item))
            {
                case EquipmentCalibrationMode.Impulse: return 1.10f;
                case EquipmentCalibrationMode.Rapid: return .90f;
                default: return 1f;
            }
        }

        public static float IntervalFactor(EquipmentDefinition item)
        {
            switch (CurrentMode(item))
            {
                case EquipmentCalibrationMode.Impulse: return 1.12f;
                case EquipmentCalibrationMode.Rapid: return .90f;
                default: return 1f;
            }
        }

        public static float RelativeDps(EquipmentCalibrationMode mode)
        {
            switch (mode)
            {
                case EquipmentCalibrationMode.Impulse: return 1.10f / 1.12f;
                case EquipmentCalibrationMode.Rapid: return .90f / .90f;
                default: return 1f;
            }
        }

        /// <summary>
        /// A restrained visual indication of item level; no new sprite/animation overwrites.
        /// Alpha is always 1 so the original icon/sprite remains legible.
        /// </summary>
        public static Color VisualTint(EquipmentDefinition item)
        {
            if (item == null) return Color.white;
            int tier = EvolutionStage(EquipmentInventory.Level(item));
            switch (tier)
            {
                case 1: return new Color(.94f, 1f, .97f, 1f);
                case 2: return new Color(.90f, .98f, 1f, 1f);
                case 3: return new Color(1f, .95f, .83f, 1f);
                default: return Color.white;
            }
        }
    }
}
