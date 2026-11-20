using DienGame.Gameplay.Skills;
using DienGame.Client;
using DienGame.Gameplay.Equipment;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Mage
{
    public static class MageSkillCatalog
    {
        public static readonly SkillId[] Passives =
        {
            SkillId.MageCrystalNeedles, SkillId.MageGravityWell, SkillId.MageMetalClaws,
            SkillId.MageBiocannon, SkillId.MageCrystalStrike, SkillId.MagePredatorResonance,
            SkillId.MageDualShield
        };

        public static bool PhoenixSelected
        {
            get
            {
                EquipmentDefinition pet = EquipmentInventory.Selected(PlayableClass.Mage);
                return pet != null && pet.Id == "mage_bio_firehawk";
            }
        }

        // The selected summon owns exactly one pet-specific skill; the two shared
        // upgrades remain selectable. This also prevents other pet signatures
        // from appearing on the Mage level-up screen.
        public static bool AvailableForSelectedPet(SkillId id)
        {
            if (id == SkillId.MagePredatorResonance || id == SkillId.MageDualShield)
                return true;
            EquipmentDefinition selected = EquipmentInventory.Selected(PlayableClass.Mage);
            return id == SummonSignatureSkill.SkillFor(selected);
        }

        public static int Limit(int level) => Mathf.Clamp(level, 0, 5);
        public static float NeedleInterval(int level) => new[] { 100f, 5f, 4.5f, 4f, 3.5f, 3f }[Limit(level)];
        public static float NeedleRatio(int level) => new[] { 0f, .30f, .35f, .40f, .45f, .55f }[Limit(level)];
        public static float MetalClawRatio(int level) => new[] { 0f, .50f, .65f, .80f, .95f, 1.15f }[Limit(level)];
        public static float ResonanceRatio(int level) => new[] { 0f, .60f, .75f, .90f, 1.05f, 1.20f }[Limit(level)];
        public static string Name(SkillId id)
        {
            switch (id)
            {
                case SkillId.MageCrystalNeedles: return "Tinh Châm Phân Liệt";
                case SkillId.MageGravityWell: return "Trọng Trường Dị Giới";
                case SkillId.MageMetalClaws: return "Trảo Liệt Hợp Kim";
                case SkillId.MageBiocannon: return "Pháo Sinh Thể";
                case SkillId.MageCrystalStrike: return "Song Trùng Oanh Kích";
                case SkillId.MagePredatorResonance: return "Cộng Hưởng Săn Mồi";
                case SkillId.MageDualShield: return "Màn Chắn Song Thể";
                default: return id.ToString();
            }
        }
        public static string Description(SkillId id, int lvl)
        {
            bool evo = lvl == 5;
            switch (id)
            {
                case SkillId.MageCrystalNeedles: return "Hồ Ly Băng: 4 đòn trúng kích hoạt Tinh Châm Phân Liệt, làm chậm. Hồi 6s." + (evo ? " Lv5: lan 2 mục tiêu, VFX băng mạnh hơn." : "");
                case SkillId.MageGravityWell: return "Hạch Hư Không: 5 đòn kích hoạt xung hư không, đánh lan. Hồi 7,5s." + (evo ? " Lv5: thêm 1 mục tiêu, VFX tím mạnh hơn." : "");
                case SkillId.MageMetalClaws: return "Chimera: 3 đòn kích hoạt Trảo Liệt Hợp Kim, chém thêm mục tiêu gần. Hồi 5s." + (evo ? " Lv5: tăng sát thương có giới hạn." : "");
                case SkillId.MageBiocannon: return "Độc Tích: 4 đòn kích hoạt Pháo Sinh Thể, truyền độc. Hồi 6,5s." + (evo ? " Lv5: lan 2 mục tiêu, VFX độc mạnh hơn." : "");
                case SkillId.MageCrystalStrike:
                    return PhoenixSelected
                        ? "Phượng Hoàng đủ 4 đòn trúng phóng 2 hỏa xung, tối đa 2 mục tiêu. Hồi 7 giây."
                            + (evo ? " Lv5: hỏa xung mạnh hơn, hoạt ảnh bùng cháy." : "")
                        : "Phượng Hoàng: 4 đòn trúng kích hoạt Song Trùng Oanh Kích.";
                case SkillId.MagePredatorResonance: return $"Player và thú cùng đánh một mục tiêu: đủ {(evo ? 3 : 4)} hit gây cắn cường hóa." + (evo ? " Nổ diện rộng." : "");
                case SkillId.MageDualShield: return "Khi một bên còn <35% HP tạo khiên cho cả hai 2s, hồi 20s." + (evo ? " Phản xung khi kết thúc." : "");
                default: return "";
            }
        }
    }
}
