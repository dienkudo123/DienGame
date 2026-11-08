namespace DienGame.Gameplay.Skills.Marksman
{
    public static class PiercingBulletSkill
    {
        // Skill tự kích hoạt mỗi 1 giây.
        public const float Cooldown = 2.5f;

        // Tầm tìm mục tiêu.
        public const float TargetRange = 12f;

        // Tốc độ projectile.
        public const float ProjectileSpeed = 8f;

        public static bool IsMaxForm(int level)
        {
            return level >= 5;
        }

        // Damage dựa trên Damage hiện tại của Player.
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
                ? 0.3f
                : 0.25f;
        }

        public static float GetHitboxScale(int level)
        {
            return IsMaxForm(level)
                ? 0.3f
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
                    "Tự động bắn một viên đạn có khả năng xuyên qua toàn bộ kẻ địch.",
            };
        }
    }
}