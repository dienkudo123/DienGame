namespace DienGame.Gameplay.Skills.Marksman
{
    public static class PiercingBulletSkill
    {
        public const float Cooldown = 1f;
        public const float TargetRange = 12f;
        public const float ProjectileSpeed = 12f;

        public static bool IsMaxForm(int level)
        {
            return level >= 5;
        }

        public static float GetDamageMultiplier(int level)
        {
            return level switch
            {
                <= 0 => 0f,
                1 => 1.00f,
                2 => 1.40f,
                3 => 1.80f,
                4 => 2.30f,
                _ => 3.00f,
            };
        }

        public static float GetVisualScale(int level)
        {
            return IsMaxForm(level)
                ? 0.30f
                : 0.25f;
        }

        public static float GetHitboxScale(int level)
        {
            return IsMaxForm(level)
                ? 0.30f
                : 0.25f;
        }

        public static string GetDescription(int level)
        {
            return level switch
            {
                1 =>
                    "Mỗi 1 giây tự bắn một Đạn Xuyên Phá gây 100% sát thương và xuyên qua tất cả kẻ địch.",
                2 =>
                    "Sát thương Đạn Xuyên Phá tăng lên 140%.",
                3 =>
                    "Sát thương Đạn Xuyên Phá tăng lên 180%.",
                4 =>
                    "Sát thương Đạn Xuyên Phá tăng lên 230%.",
                5 =>
                    "MAX: Đạn Xuyên Phá biến thành đạn khổng lồ, gây 300% sát thương và xuyên qua tất cả kẻ địch.",
                _ =>
                    "Tự động bắn một viên đạn xuyên toàn bộ kẻ địch trên đường bay.",
            };
        }
    }
}
