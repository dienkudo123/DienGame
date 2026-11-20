#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using DienGame.Gameplay.Equipment;
using DienGame.Gameplay.Skills.Mage;
using UnityEditor;
using UnityEngine;

namespace DienGame.Editor
{
    /// <summary>Read-only asset verification. Never modifies equipment saves or gameplay.</summary>
    public static class DienGamePhase09Validator
    {
        [MenuItem("DienGame/Equipment/09 - Kiem tra thu co-sinh va chip")]
        private static void Verify()
        {
            string[] names = { "IceFox", "FireHawk", "PoisonLizard", "VoidOrb", "Wolf" };
            string[] ids = { "mage_bio_icefox", "mage_bio_firehawk", "mage_bio_lizard",
                "mage_bio_void", "mage_bio_wolf" };
            int errors = 0;
            for (int i = 0; i < ids.Length; i++)
            {
                EquipmentDefinition item = EquipmentCatalog.Find(ids[i]);
                if (item == null || !BiomechPetChips.Supports(item))
                {
                    Debug.LogError("[Phase09] Thiếu/không tương thích: " + ids[i]);
                    errors++;
                    continue;
                }
                if (EquipmentCatalog.LoadIcon(item) == null) { Debug.LogError("[Phase09] Thiếu icon: " + item.Id); errors++; }
                if (EquipmentCatalog.LoadWorldSprite(item) == null) { Debug.LogError("[Phase09] Thiếu sprite: " + item.Id); errors++; }
                string[] actions = { "Walk/Bio_" + names[i] + "_Walk4x4", "Attack/Bio_" + names[i] + "_Attack4x5" };
                for (int j = 0; j < actions.Length; j++)
                {
                    string path = "Equipment/Biomech/" + actions[j];
                    Texture2D tex = Resources.Load<Texture2D>(path);
                    int columns = j == 0 ? 4 : 5;
                    if (tex == null || tex.width % columns != 0 || tex.height % 4 != 0 || !tex.isReadable)
                    {
                        Debug.LogError("[Phase09] Atlas thiếu, sai lưới hoặc chưa bật Read/Write: " + path);
                        errors++;
                    }
                }
            }
            if (EquipmentCatalog.Find("mage_chimera") == null || BiomechPetChips.Supports(EquipmentCatalog.Find("mage_chimera")))
            { Debug.LogError("[Phase09] Chimera gốc không được chuyển sang chip biomech."); errors++; }
            for (int m = 1; m <= 3; m++)
            {
                string icon = BiomechPetChips.Icon((BiomechPetChipMode)m);
                if (Resources.Load<Sprite>(icon) == null) { Debug.LogError("[Phase09] Thiếu icon chip: " + icon); errors++; }
            }
            if (BiomechPetChips.NormalDamageFactor(BiomechPetChipMode.PredatorPulse) >= 1f ||
                BiomechPetChips.NormalDamageFactor(BiomechPetChipMode.ScatterPulse) >= 1f ||
                BiomechPetChips.NormalDamageFactor(BiomechPetChipMode.Symbiosis) >= 1f)
            { Debug.LogError("[Phase09] Chip phải có đánh đổi sát thương đòn thường."); errors++; }
            if (errors == 0) Debug.Log("[Phase09] PASS: 5 thú mới, Chimera gốc, 3 chip, atlas và icon có đủ.");
            else Debug.LogError("[Phase09] FAIL: " + errors + " lỗi. Mở Console để xem chi tiết.");
            EditorUtility.DisplayDialog("DienGame Phase09", errors == 0 ?
                "Kiểm tra asset và thiết kế PASS. Vẫn cần Play Mode/Android để xác nhận." :
                "Có " + errors + " lỗi. Xem Unity Console.", "OK");
        }
    }
}
#endif
