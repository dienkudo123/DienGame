#if UNITY_EDITOR
using System.Collections.Generic;
using DienGame.Gameplay.Equipment;
using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    /// <summary>Lightweight no-Play-Mode smoke test; reports model and missing sprites.</summary>
    public static class DienGameEquipmentPhase08Validator
    {
        [MenuItem("DienGame/Equipment/08 - Kiểm tra lõi Quy Tàng")]
        private static void Validate()
        {
            HashSet<string> ids = new HashSet<string>();
            int count = 0, issues = 0;
            foreach (EquipmentDefinition item in EquipmentCatalog.All)
            {
                count++;
                if (!ids.Add(item.Id) || item.DamageMultiplier <= 0f || item.DamageMultiplier > 1.15f)
                {
                    Debug.LogError("Phase08: sai ID hoặc hệ số: " + item.Id);
                    issues++;
                }
                if (EquipmentCatalog.LoadIcon(item) == null)
                {
                    Debug.LogError("Phase08: thiếu icon " + item.Icon);
                    issues++;
                }
                if (string.IsNullOrEmpty(EquipmentResonance.FieldTag(item))) issues++;
            }
            for (int level = 0; level <= EquipmentCatalog.MaxLevel; level++)
            {
                if (level > 0 && EquipmentResonance.EvolutionStage(level) <
                    EquipmentResonance.EvolutionStage(level - 1)) issues++;
            }
            foreach (EquipmentCalibrationMode mode in new[] {
                EquipmentCalibrationMode.Balanced,
                EquipmentCalibrationMode.Impulse,
                EquipmentCalibrationMode.Rapid })
            {
                float relative = EquipmentResonance.RelativeDps(mode);
                if (Mathf.Abs(relative - 1f) > .035f)
                {
                    Debug.LogError("Phase08: mode lệch DPS: " + mode + " = " + relative);
                    issues++;
                }
            }
            if (issues == 0) Debug.Log("Phase08: OK, " + count + " định nghĩa; không trùng ID; DPS lệch <3.5%; icon có sẵn.");
            else Debug.LogError("Phase08: " + issues + " vấn đề; xem Console.");
        }
    }
}
#endif
