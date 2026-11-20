#if UNITY_EDITOR
using DienGame.Gameplay.Equipment;
using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    public static class DienGameBiomechValidator
    {
        [MenuItem("DienGame/Equipment/03 - Kiem tra thu co sinh (5 loai)")]
        private static void Check()
        {
            string[] names = { "IceFox", "FireHawk", "PoisonLizard", "VoidOrb", "Wolf" };
            int errors = 0;
            foreach (string name in names)
            {
                string id = name == "IceFox" ? "mage_bio_icefox" :
                    name == "FireHawk" ? "mage_bio_firehawk" :
                    name == "PoisonLizard" ? "mage_bio_lizard" :
                    name == "VoidOrb" ? "mage_bio_void" : "mage_bio_wolf";
                EquipmentDefinition item = EquipmentCatalog.Find(id);
                if (item == null || EquipmentCatalog.LoadIcon(item) == null ||
                    EquipmentCatalog.LoadWorldSprite(item) == null)
                {
                    Debug.LogError("DienGame: thiếu item/icon thú cơ sinh: " + id);
                    errors++;
                }
                foreach (bool attack in new [] { false, true })
                {
                    string suffix = attack ? "Attack/Bio_" + name + "_Attack4x5" :
                        "Walk/Bio_" + name + "_Walk4x4";
                    Texture2D tex = Resources.Load<Texture2D>("Equipment/Biomech/" + suffix);
                    int columns = attack ? 5 : 4;
                    if (tex == null || tex.width % columns != 0 || tex.height % 4 != 0 || !tex.isReadable)
                    {
                        Debug.LogError("DienGame: atlas thiếu/sai lưới/chưa readable: " + suffix);
                        errors++;
                    }
                }
            }
            if (errors == 0) Debug.Log("DienGame Biomech: 5 thú mới, 10 atlas, 5 icons, OK. Chimera gốc không đổi.");
            else Debug.LogError("DienGame Biomech: " + errors + " lỗi. Đợi Unity import xong rồi kiểm tra lại.");
        }
    }
}
#endif
