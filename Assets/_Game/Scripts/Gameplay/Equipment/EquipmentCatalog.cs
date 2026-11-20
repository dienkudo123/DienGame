using System;
using System.Collections.Generic;
using DienGame.Client;
using UnityEngine;

namespace DienGame.Gameplay.Equipment
{
    public enum EquipmentEffect
    {
        None, Slow, Poison, Burn, Piercing, Splash, Arcane
    }

    /// <summary>
    /// Immutable gameplay definition. Effects are deliberately mutually exclusive;
    /// upgrading increases raw power, never crowd-control potency.
    /// </summary>
    public sealed class EquipmentDefinition
    {
        public readonly string Id;
        public readonly PlayableClass Class;
        public readonly string Name;
        public readonly string Description;
        public readonly string Icon;
        public readonly string WorldSprite;
        public readonly EquipmentEffect Effect;
        public readonly float DamageMultiplier;
        public readonly int UnlockGold;
        public readonly int UnlockFragments;
        public readonly bool Starter;

        public EquipmentDefinition(string id, PlayableClass characterClass, string name,
            string description, string icon, string worldSprite, EquipmentEffect effect,
            float damageMultiplier, int gold, int fragments, bool starter = false)
        {
            Id = id;
            Class = characterClass;
            Name = name;
            Description = description;
            Icon = "Equipment/Icons/" + icon;
            WorldSprite = string.IsNullOrEmpty(worldSprite) ? string.Empty : worldSprite;
            Effect = effect;
            DamageMultiplier = damageMultiplier;
            UnlockGold = gold;
            UnlockFragments = fragments;
            Starter = starter;
        }
    }

    public static class EquipmentCatalog
    {
        public const int MaxLevel = 10;
        private static readonly EquipmentDefinition[] Definitions =
        {
            // Marksman normal bullets are intentionally competitive against elemental rounds.
            new EquipmentDefinition("marksman_basic", PlayableClass.Marksman, "Đạn Động Năng", "Đạn tiêu chuẩn công nghệ. Sát thương ổn định, không khống chế.", "Marksman_Basic", string.Empty, EquipmentEffect.None, 1.00f, 0, 0, true),
            new EquipmentDefinition("marksman_frost", PlayableClass.Marksman, "Đạn Hàn X", "Lõi hàn giao thoa trường X: làm chậm 22% / 1,2 giây, đổi sát thương.", "Marksman_Frost", "Equipment/Projectiles/Marksman_Frost", EquipmentEffect.Slow, .92f, 300, 3),
            new EquipmentDefinition("marksman_poison", PlayableClass.Marksman, "Đạn Sinh Độc", "Dược tố sinh học tác động theo thời gian; độc không cộng dồn.", "Marksman_Poison", "Equipment/Projectiles/Marksman_Poison", EquipmentEffect.Poison, .86f, 350, 4),
            new EquipmentDefinition("marksman_explosive", PlayableClass.Marksman, "Đạn Chấn Nổ", "Nén xung chấn thành vùng nổ nhỏ; đánh nhóm tốt hơn đánh đơn.", "Marksman_Explosive", "Equipment/Projectiles/Marksman_Explosive", EquipmentEffect.Splash, .87f, 450, 5),
            new EquipmentDefinition("marksman_pierce", PlayableClass.Marksman, "Đạn Xuyên Pha", "Đầu xuyên pha chạm thêm 1 mục tiêu; mục tiêu sau nhận 60% lực.", "Marksman_Pierce", "Equipment/Projectiles/Marksman_Pierce", EquipmentEffect.Piercing, .92f, 500, 5),
            // Assassin changes normal attack's shape: speed/power and area tradeoffs.
            new EquipmentDefinition("assassin_daggers", PlayableClass.Assassin, "Song Đao Hợp Kim", "Cặp đao hợp kim cân bằng lực, nhịp chém và phạm vi.", "Assassin_TwinDaggers", string.Empty, EquipmentEffect.None, 1.00f, 0, 0, true),
            new EquipmentDefinition("assassin_sickles", PlayableClass.Assassin, "Song Liềm Dẫn Xung", "Lưỡi dẫn xung tăng tầm nhẹ, đổi lại sát thương thấp hơn.", "Assassin_Sickles", string.Empty, EquipmentEffect.None, .94f, 300, 3),
            new EquipmentDefinition("assassin_scythe", PlayableClass.Assassin, "Liềm Xích Cơ Giới", "Liềm nối xích mở rộng góc và tầm quét, giảm lực đòn đơn.", "Assassin_ChainScythe", string.Empty, EquipmentEffect.None, .83f, 450, 5),
            new EquipmentDefinition("assassin_venom", PlayableClass.Assassin, "Trảo Sinh Độc", "Trảo cấy sinh độc, gây độc không cộng dồn; tầm đánh ngắn.", "Assassin_VenomClaws", string.Empty, EquipmentEffect.Poison, .86f, 400, 4),
            // The existing Chimera remains the default animated companion.
            new EquipmentDefinition("mage_chimera", PlayableClass.Mage, "Chimera Cơ-Sinh", "Sinh thể lai máy nguyên bản; giữ kỹ năng và hoạt ảnh gốc.", "Mage_Chimera", string.Empty, EquipmentEffect.None, 1.00f, 0, 0, true),
            // Biomechanical Mage summons. Four obsolete non-mechanical variants were removed.
            // Keep Chimera and all five biomechanical forms; old saves migrate by element.
            new EquipmentDefinition("mage_bio_icefox", PlayableClass.Mage, "Hồ Ly Băng Cơ Sinh", "Lõi băng cộng sinh: làm chậm; mỗi 4 đòn lan băng sang 1 quái gần.", "Mage_BioIceFox", "Equipment/Biomech/Pets/Bio_IceFox", EquipmentEffect.Slow, .85f, 550, 6),
            new EquipmentDefinition("mage_bio_firehawk", PlayableClass.Mage, "Hỏa Điểu Cơ Sinh", "Lò phản ứng cánh máy: gây bỏng; mỗi 4 đòn cháy lan 1 quái.", "Mage_BioFireHawk", "Equipment/Biomech/Pets/Bio_FireHawk", EquipmentEffect.Burn, .81f, 600, 7),
            new EquipmentDefinition("mage_bio_lizard", PlayableClass.Mage, "Độc Tích Cơ Sinh", "Bình độc tích hợp: gây độc; mỗi 4 đòn truyền độc sang 1 quái.", "Mage_BioPoisonLizard", "Equipment/Biomech/Pets/Bio_PoisonLizard", EquipmentEffect.Poison, .84f, 650, 7),
            new EquipmentDefinition("mage_bio_void", PlayableClass.Mage, "Hạch Hư Không Cơ Sinh", "Mắt máy sinh thể: sóng lan nhỏ; mỗi 5 đòn cộng hưởng 1 mục tiêu.", "Mage_BioVoidOrb", "Equipment/Biomech/Pets/Bio_VoidOrb", EquipmentEffect.Arcane, .77f, 750, 8),
            new EquipmentDefinition("mage_bio_wolf", PlayableClass.Mage, "Lang Cơ Sinh Tử Quang", "Sói máy sinh học: đòn đơn ổn định; mỗi 5 đòn chớp vuốt mạnh.", "Mage_BioWolf", "Equipment/Biomech/Pets/Bio_Wolf", EquipmentEffect.None, .94f, 700, 8),
            // Warrior swords now play in their dedicated melee scene; equipment IDs are unchanged.
            new EquipmentDefinition("warrior_balanced", PlayableClass.Warrior, "Kiếm Hợp Kim", "Kiếm tiêu chuẩn cho Đấu sĩ.", "Warrior_Balanced", string.Empty, EquipmentEffect.None, 1.00f, 0, 0, true),
            new EquipmentDefinition("warrior_great", PlayableClass.Warrior, "Đại Kiếm Động Năng", "Đánh chậm, quét 225° và tầm xa hơn, đổi lại DPS đơn thấp.", "Warrior_Greatsword", string.Empty, EquipmentEffect.None, .86f, 300, 3),
            new EquipmentDefinition("warrior_flame", PlayableClass.Warrior, "Kiếm Buồng Nhiệt", "Đòn kiếm gia nhiệt gây bỏng ngắn không cộng dồn.", "Warrior_Flame", string.Empty, EquipmentEffect.Burn, .84f, 400, 4),
            new EquipmentDefinition("warrior_frost", PlayableClass.Warrior, "Kiếm Kết Tinh X", "Lõi kết tinh trường X làm chậm, đổi lại sát thương gốc.", "Warrior_Frost", string.Empty, EquipmentEffect.Slow, .90f, 400, 4),
        };

        public static IEnumerable<EquipmentDefinition> All => Definitions;
        public static EquipmentDefinition Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < Definitions.Length; i++)
                if (string.Equals(Definitions[i].Id, id, StringComparison.Ordinal)) return Definitions[i];
            return null;
        }
        public static List<EquipmentDefinition> ForClass(PlayableClass cls)
        {
            List<EquipmentDefinition> result = new List<EquipmentDefinition>();
            foreach (EquipmentDefinition item in Definitions) if (item.Class == cls) result.Add(item);
            return result;
        }
        public static EquipmentDefinition Starter(PlayableClass cls)
        {
            foreach (EquipmentDefinition item in Definitions)
                if (item.Class == cls && item.Starter) return item;
            return null;
        }
        private static readonly Dictionary<string, Sprite> CachedSprites = new Dictionary<string, Sprite>();
        private static Sprite LoadSprite(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (!CachedSprites.TryGetValue(path, out Sprite sprite))
            {
                sprite = Resources.Load<Sprite>(path);
                CachedSprites[path] = sprite;
            }
            return sprite;
        }
        public static Sprite LoadIcon(EquipmentDefinition item) => item != null ? LoadSprite(item.Icon) : null;
        public static Sprite LoadWorldSprite(EquipmentDefinition item) => item != null ? LoadSprite(item.WorldSprite) : null;
        public static float DamageAtLevel(EquipmentDefinition item, int level) =>
            (item != null ? item.DamageMultiplier : 1f) * (1f + .035f * Mathf.Clamp(level, 0, MaxLevel));
    }
}
