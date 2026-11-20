#if UNITY_EDITOR
using DienGame.Gameplay.Equipment;
using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    public static class DienGameEquipmentEvolutionValidator
    {
        [MenuItem("DienGame/Equipment/08 FINAL - Kiem tra hinh tien hoa")]
        public static void Verify()
        {
            int checks = 0, failures = 0;
            foreach (EquipmentDefinition item in EquipmentCatalog.All)
            {
                Sprite original = EquipmentEvolutionArt.IconFor(item, 0);
                if (original == null) { failures++; Debug.LogError("Missing starter icon: " + item.Id); }
                for (int stage = 1; stage <= 3; stage++)
                {
                    int level = stage == 1 ? 3 : stage == 2 ? 6 : 10;
                    string path = "Equipment/Evolution/Icons/" + item.Id + "_T" + stage;
                    Sprite tier = Resources.Load<Sprite>(path);
                    checks++;
                    if (tier == null) { failures++; Debug.LogError("Missing tier art: " + path); }
                    if (EquipmentEvolutionArt.IconFor(item, level) == null) failures++;
                }
                if (item.WorldSprite.StartsWith("Equipment/Projectiles/"))
                {
                    for (int stage = 1; stage <= 3; stage++)
                    {
                        string path = "Equipment/Evolution/Projectiles/" + item.Id + "_T" + stage;
                        checks++;
                        if (Resources.Load<Sprite>(path) == null) { failures++; Debug.LogError("Missing bullet art: " + path); }
                    }
                }
            }
            for (int i = 1; i <= 3; i++)
            {
                checks++;
                if (EquipmentEvolutionArt.RingForStage(i) == null) { failures++; Debug.LogError("Missing core ring " + i); }
            }
            foreach (EquipmentCalibrationMode mode in new[] { EquipmentCalibrationMode.Balanced,
                EquipmentCalibrationMode.Impulse, EquipmentCalibrationMode.Rapid })
            {
                checks++;
                float relativeDps = EquipmentResonance.RelativeDps(mode);
                if (Mathf.Abs(relativeDps - 1f) > .035f) { failures++; Debug.LogError("Calibration DPS out of budget: " + mode); }
            }
            if (failures == 0) Debug.Log("DienGame Phase08 Final: OK - " + checks + " asset/DPS checks. Check Play Mode separately.");
            else Debug.LogError("DienGame Phase08 Final: " + failures + " errors in " + checks + " checks.");
        }
    }
}
#endif
