namespace DienGame.Gameplay.Skills.Marksman
{
    public static class ThunderBulletSkill
    {
        public static int GetShotsPerTrigger(int level)
        {
            return level switch
            {
                <= 0 => int.MaxValue,
                1 => 5,
                2 => 4,
                3 => 3,
                4 => 2,
                _ => 1,
            };
        }

        public static int GetChainTargets(int level)
        {
            return level switch
            {
                <= 0 => 0,
                1 => 2,
                2 => 3,
                3 => 4,
                4 => 5,
                _ => 6,
            };
        }

        public static float GetDamageMultiplier(int level)
        {
            return level switch
            {
                <= 0 => 0f,
                1 => 0.45f,
                2 => 0.50f,
                3 => 0.55f,
                4 => 0.60f,
                _ => 0.70f,
            };
        }

        public static float GetChainRange(int level)
        {
            return level >= 5
                ? 4.5f
                : 3.5f;
        }

        public static bool IsMaxForm(int level)
        {
            return level >= 5;
        }

        public const float DragonCooldown = 5f;
        public const float DragonDamageMultiplier = 2f;
        public const float DragonFlightRange = 7f;
        public const float DragonMoveSpeed = 7f;

        public static string GetDescription(int level)
        {
            return level switch
            {
                1 =>
                    "Mỗi 5 phát bắn thường, viên đạn khi trúng mục tiêu kích hoạt Lôi Đạn qua tối đa 2 mục tiêu.",
                2 =>
                    "Mỗi 4 phát bắn thường kích hoạt Lôi Đạn qua tối đa 3 mục tiêu.",
                3 =>
                    "Mỗi 3 phát bắn thường kích hoạt Lôi Đạn qua tối đa 4 mục tiêu.",
                4 =>
                    "Mỗi 2 phát bắn thường kích hoạt Lôi Đạn qua tối đa 5 mục tiêu.",
                5 =>
                    "MAX: Mỗi phát bắn thường đều kích hoạt Lôi Đạn tím qua tối đa 6 mục tiêu. Mỗi 5 giây triệu hồi Lôi Long quét chiến trường.",
                _ =>
                    "Đạn thường có thể kích hoạt chuỗi sét giữa các kẻ địch.",
            };
        }
    }
}
