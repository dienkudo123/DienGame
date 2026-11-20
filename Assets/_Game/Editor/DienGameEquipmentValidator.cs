#if UNITY_EDITOR
using DienGame.Client;
using DienGame.Gameplay.Equipment;
using DienGame.Gameplay.World;
using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    /// <summary>One-click asset validation and local playtest resources.</summary>
    public static class DienGameEquipmentValidator
    {
        [MenuItem("DienGame/Equipment/01 - Kiểm tra assets trang bị")]
        private static void Validate()
        {
            int checkedCount = 0, missing = 0;
            foreach (EquipmentDefinition item in EquipmentCatalog.All)
            {
                checkedCount++;
                if (EquipmentCatalog.LoadIcon(item) == null)
                {
                    missing++;
                    Debug.LogError("Thiếu icon trang bị: Resources/" + item.Icon);
                }
                if (!string.IsNullOrEmpty(item.WorldSprite) && EquipmentCatalog.LoadWorldSprite(item) == null)
                {
                    missing++;
                    Debug.LogError("Thiếu sprite chiến đấu: Resources/" + item.WorldSprite);
                }
            }
            foreach (string status in new [] {"Slow", "Poison", "Burn", "Critical", "Pierce", "Lifesteal", "Shield", "Summon"})
            {
                if (Resources.Load<Sprite>("Equipment/Statuses/" + status) == null)
                {
                    missing++;
                    Debug.LogError("Thiếu icon hiệu ứng: " + status);
                }
            }
            if (missing == 0) Debug.Log("DienGame Equipment: OK, " + checkedCount + " definitions, 0 missing sprites.");
            else Debug.LogError("DienGame Equipment: " + missing + " assets missing. Kiểm tra import PNG là Sprite (2D and UI).");
        }

        [MenuItem("DienGame/Equipment/02 - Cộng 2000 vàng và 50 mảnh (PLAYTEST)")]
        private static void GrantTestMoney()
        {
            MetaProgressService.GrantGold(2000);
            WorldInventory.AddFragments(50);
            Debug.Log("DienGame Equipment: +2000 vàng, +50 mảnh cho PlayerPrefs local. Chỉ dùng để test.");
        }
    }
}
#endif
