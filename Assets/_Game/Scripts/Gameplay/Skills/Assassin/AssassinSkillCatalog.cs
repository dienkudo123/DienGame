using DienGame.Gameplay.Skills;

namespace DienGame.Gameplay.Skills.Assassin
{
    /// <summary>Values are initial balancing defaults, not a gameplay change to Marksman.</summary>
    public static class AssassinSkillCatalog
    {
        public static readonly SkillId[] PassiveSkills =
        {
            SkillId.AssassinBloodSlash, SkillId.AssassinAfterimage,
            SkillId.AssassinFlyingDagger, SkillId.AssassinShadowClone,
            SkillId.AssassinDeathMark, SkillId.AssassinOrbitBlades,
            SkillId.AssassinExecution,
        };

        private static readonly float[] Slash = { 0f, .35f, .45f, .55f, .65f, .80f };
        private static readonly float[] Dodge = { 0f, .05f, .07f, .09f, .12f, .15f };
        private static readonly float[] KnifeInterval = { 0f, 8f, 7f, 6f, 5f, 4f };
        private static readonly float[] KnifeDamage = { 0f, .50f, .55f, .60f, .65f, .70f };
        private static readonly float[] CloneInterval = { 0f, 18f, 16f, 14f, 12f, 10f };
        private static readonly float[] Mark = { 0f, .60f, .75f, .90f, 1.05f, 1.20f };
        private static readonly float[] Orbit = { 0f, .20f, .275f, .35f, .425f, .50f };
        private static readonly float[] Execution = { 0f, .10f, .15f, .20f, .25f, .30f };

        public static float SlashBonus(int level) => Slash[Clamp(level)];
        public static float DodgeChance(int level) => Dodge[Clamp(level)];
        public static float KnifeCooldown(int level) => KnifeInterval[Clamp(level)];
        public static float KnifeDamageRatio(int level) => KnifeDamage[Clamp(level)];
        public static float CloneCooldown(int level) => CloneInterval[Clamp(level)];
        public static float MarkDamageRatio(int level) => Mark[Clamp(level)];
        public static float OrbitDamageRatio(int level) => Orbit[Clamp(level)];
        public static float ExecutionBonus(int level) => Execution[Clamp(level)];

        public static string Name(SkillId id)
        {
            switch (id)
            {
                case SkillId.AssassinBloodSlash: return "Huyết Ảnh Trảm";
                case SkillId.AssassinAfterimage: return "Tàn Ảnh";
                case SkillId.AssassinFlyingDagger: return "Phi Nhận";
                case SkillId.AssassinShadowClone: return "Ảnh Phân Thân";
                case SkillId.AssassinDeathMark: return "Tử Ấn";
                case SkillId.AssassinOrbitBlades: return "Đao Ảnh Hộ Thân";
                case SkillId.AssassinExecution: return "Hồn Trụ Dẫn Ảnh";
                default: return id.ToString();
            }
        }

        public static string Description(SkillId id, int nextLevel)
        {
            int level = Clamp(nextLevel);
            switch (id)
            {
                case SkillId.AssassinBloodSlash:
                    return $"Mỗi 3 nhát chém trúng, tạo thêm nhát chém diện rộng gây {SlashBonus(level):P0} ATK.";
                case SkillId.AssassinAfterimage:
                    return $"{DodgeChance(level):P0} cơ hội né sát thương (hồi nội bộ 1,5 giây). Lv5 tăng 20% tốc chạy trong 0,8 giây khi né.";
                case SkillId.AssassinFlyingDagger:
                    return $"Tự phóng phi nhận sau {KnifeCooldown(level):0.#} giây, gây {KnifeDamageRatio(level):P0} ATK. Lv5 nảy sang một quái thứ hai.";
                case SkillId.AssassinShadowClone:
                    return $"Một phân thân đứng yên 2 giây, sao chép kỹ năng và chỉ số tấn công; hồi {CloneCooldown(level):0.#} giây.";
                case SkillId.AssassinDeathMark:
                    return $"Đủ {(level >= 5 ? 2 : 3)} đòn đánh thường trên cùng mục tiêu gây thêm {MarkDamageRatio(level):P0} ATK.";
                case SkillId.AssassinOrbitBlades:
                    return $"2 đao ảo xoay 2 giây, gây {OrbitDamageRatio(level):P0} ATK mỗi nhịp trúng.";
                case SkillId.AssassinExecution:
                    return $"Mỗi quái rơi 1 linh hồn; nhặt đủ 20 triệu hồi Hồn Trụ có HP riêng, hút quái khi người chơi ở trong vùng trụ. Tối đa 4 trụ. Lv5 phản kích gây sát thương.";
                default:
                    return string.Empty;
            }
        }

        public static int Clamp(int level) => level < 0 ? 0 : level > 5 ? 5 : level;
    }
}
