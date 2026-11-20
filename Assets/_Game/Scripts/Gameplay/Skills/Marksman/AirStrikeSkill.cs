namespace DienGame.Gameplay.Skills.Marksman
{
    public static class AirStrikeSkill
    {
        public const float Cooldown = 5f;
        public const float TargetRange = 10f;
        public const float WarningDuration = 0.65f;
        public const float BombFallDuration = 0.45f;
        public const float BombStartHeight = 5f;
        public const float BombStagger = 0.18f;

        public static int GetBombCount(int level)
        {
            return level switch
            {
                <= 0 => 0,
                1 => 1,
                2 => 1,
                3 => 2,
                4 => 3,
                _ => 3,
            };
        }

        public static float GetDamageMultiplier(int level)
        {
            return level switch
            {
                <= 0 => 0f,
                1 => 1.80f,
                2 => 2.40f,
                3 => 2.00f,
                4 => 2.00f,
                _ => 2.20f,
            };
        }

        public static float GetExplosionRadius(int level)
        {
            return level switch
            {
                <= 0 => 0f,
                1 => 1.40f,
                2 => 1.70f,
                3 => 1.70f,
                4 => 1.90f,
                _ => 2.10f,
            };
        }

        public static bool IsMaxForm(int level)
        {
            return level >= 5;
        }

        public const float FireDuration = 4f;
        public const float FireTickInterval = 0.5f;
        public const float FireDamageMultiplier = 0.35f;

        public static float GetFireRadius(int level)
        {
            return IsMaxForm(level)
                ? 1.85f
                : 0f;
        }

        public static string GetDescription(int level)
        {
            return level switch
            {
                1 =>
                    "Mỗi 5 giây gọi 1 quả bom xuống một mục tiêu ngẫu nhiên, gây 180% sát thương diện rộng.",
                2 =>
                    "Bom gây 240% sát thương và vùng nổ rộng hơn.",
                3 =>
                    "Mỗi lần Không Kích gọi 2 quả bom, mỗi quả gây 200% sát thương.",
                4 =>
                    "Mỗi lần Không Kích gọi 3 quả bom, mỗi quả gây 200% sát thương.",
                5 =>
                    "MAX: 3 quả bom gây 220% sát thương mỗi quả và để lại bãi lửa trong 4 giây.",
                _ =>
                    "Triệu hồi bom từ trên trời tấn công các mục tiêu ngẫu nhiên.",
            };
        }
    }
}
