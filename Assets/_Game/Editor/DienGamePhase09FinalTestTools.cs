#if UNITY_EDITOR
using System.Collections.Generic;
using DienGame.Client;
using DienGame.Gameplay.Equipment;
using DienGame.Gameplay.Skills.Mage;
using UnityEditor;
using UnityEngine;

namespace DienGame.Editor
{
    /// <summary>
    /// Safe QA controls: unlock all definitions in Editor/Development, separate test
    /// data from the user's progression save, and verify pet assets after import.
    /// </summary>
    public static class DienGamePhase09FinalTestTools
    {
        private const string Menu = "DienGame/Equipment/09 FINAL TEST/";

        [MenuItem(Menu + "01 - Mo khoa tat ca 19 trang bi")]
        private static void Enable()
        {
            EquipmentQaSandbox.SetEnabled(true);
            Debug.Log("[Phase09 QA] Mo khoa tat ca 19 trang bi. Mac dinh +3, " +
                "co the chinh cap test trong menu; tien vang/manh that khong bi tru.");
            EditorUtility.DisplayDialog("DienGame - QA", "Đã bật TEST: mở khóa 19/19 trang bị. " +
                "Chimera và 5 thú cơ-sinh có thể chọn ngay; thú biomech mặc định +3 để lắp CHIP. " +
                "Đấu sĩ đã có scene riêng ở Phase10, có thể thử ngay.", "Đóng");
        }

        [MenuItem(Menu + "02 - Cap thu +0")]
        private static void Level0() { SetLevels(0); }
        [MenuItem(Menu + "03 - Cap thu +3 (CHIP)")]
        private static void Level3() { SetLevels(3); }
        [MenuItem(Menu + "04 - Cap thu +4 (LOI)")]
        private static void Level4() { SetLevels(4); }
        [MenuItem(Menu + "05 - Cap thu +6")]
        private static void Level6() { SetLevels(6); }
        [MenuItem(Menu + "06 - Cap thu +10")]
        private static void Level10() { SetLevels(10); }

        private static void SetLevels(int level)
        {
            EquipmentQaSandbox.SetEnabled(true);
            EquipmentQaSandbox.SetAllLevels(level);
            for (int i = 0; i < 3; i++)
                EquipmentQaSandbox.SetSharedLevel(i, Mathf.Min(level, 5));
            Debug.Log("[Phase09 QA] Set tat ca trang bi test +" + level +
                "; giap/phu an toi da +5. Neu dang Play Mode, thoat vao lai Lobby de nap chi so.");
        }

        [MenuItem(Menu + "07 - Tat TEST, dung save that")]
        private static void Disable()
        {
            EquipmentQaSandbox.SetEnabled(false);
            Debug.Log("[Phase09 QA] TEST OFF. Game su dung lai trang bi, cap va tien trinh that.");
        }

        [MenuItem(Menu + "08 - Xoa du lieu TEST (giu save that)")]
        private static void Reset()
        {
            EquipmentQaSandbox.ResetTestOverrides();
            Debug.Log("[Phase09 QA] Da xoa rieng du lieu TEST; mo khoa mac dinh +3 khi QA bat.");
        }

        [MenuItem(Menu + "09 - Kiem tra asset, ID va unlock")]
        private static void Verify()
        {
            int fails = 0;
            int itemCount = 0;
            var ids = new HashSet<string>();
            var counts = new Dictionary<PlayableClass, int>();
            foreach (EquipmentDefinition item in EquipmentCatalog.All)
            {
                itemCount++;
                if (item == null || string.IsNullOrEmpty(item.Id) || !ids.Add(item.Id))
                {
                    Debug.LogError("[Phase09 QA] ID trung hoac null: " + (item == null ? "null" : item.Id));
                    fails++;
                    continue;
                }
                if (!counts.ContainsKey(item.Class)) counts[item.Class] = 0;
                counts[item.Class]++;
                if (EquipmentCatalog.LoadIcon(item) == null)
                { Debug.LogError("[Phase09 QA] Thieu icon " + item.Id + " (" + item.Icon + ")"); fails++; }
                if (EquipmentQaSandbox.Active && !EquipmentInventory.IsUnlocked(item))
                { Debug.LogError("[Phase09 QA] Chua mo khoa trong TEST: " + item.Id); fails++; }
            }
            CheckCount(counts, PlayableClass.Marksman, 5, ref fails);
            CheckCount(counts, PlayableClass.Assassin, 4, ref fails);
            CheckCount(counts, PlayableClass.Mage, 6, ref fails);
            CheckCount(counts, PlayableClass.Warrior, 4, ref fails);

            foreach (string form in new[] { "IceFox", "FireHawk", "PoisonLizard", "VoidOrb", "Wolf" })
            {
                for (int facing = 0; facing < 4; facing++)
                {
                    for (int frame = 0; frame < 4; frame++)
                        if (BiomechSummonArt.GetFrame(form, facing, false, frame) == null) fails++;
                    for (int frame = 0; frame < 5; frame++)
                        if (BiomechSummonArt.GetFrame(form, facing, true, frame) == null) fails++;
                }
            }
            EquipmentDefinition original = EquipmentCatalog.Find("mage_chimera");
            if (original == null || BiomechPetChips.Supports(original)) { fails++; Debug.LogError("[Phase09 QA] Chimera goc phai giu nguyen."); }
            if (itemCount != 19) { fails++; Debug.LogError("[Phase09 QA] Phai co 19 trang bi, hien co " + itemCount); }
            if (fails == 0) Debug.Log("[Phase09 QA] PASS: " + itemCount + " trang bi, 5 x 36 sprite frames, " +
                "Chimera doc lap. Van can test Play Mode/Android.");
            else Debug.LogError("[Phase09 QA] FAIL " + fails + " loi. Xem Console.");
            EditorUtility.DisplayDialog("Phase 09 FINAL - QA", fails == 0 ?
                "PASS: 19 trang bị và 180 frame thú biomech. Vẫn cần kiểm thử Play Mode." :
                "Phát hiện " + fails + " lỗi asset/dữ liệu. Xem Console.", "OK");
        }

        private static void CheckCount(Dictionary<PlayableClass, int> counts,
            PlayableClass cls, int expected, ref int fails)
        {
            int actual = counts.ContainsKey(cls) ? counts[cls] : 0;
            if (actual == expected) return;
            fails++;
            Debug.LogError("[Phase09 QA] " + cls + " co " + actual + "/" + expected + " trang bi.");
        }
    }
}
#endif
