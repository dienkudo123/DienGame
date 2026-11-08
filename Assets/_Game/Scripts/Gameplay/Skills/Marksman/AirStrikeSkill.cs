namespace DienGame.Gameplay.Skills.Marksman
{
    public static class AirStrikeSkill
    {
        // =========================================
        // GENERAL
        // =========================================

        // Bao lâu Không Kích xuất hiện một lần.
        public const float Cooldown = 5f;

        // Tầm tìm enemy ngẫu nhiên quanh Player.
        public const float TargetRange = 10f;

        // Thời gian vòng cảnh báo tồn tại.
        public const float WarningDuration = 0.65f;

        // Thời gian bom rơi từ trên xuống.
        public const float BombFallDuration = 0.45f;

        // Độ cao lúc bom bắt đầu rơi.
        public const float BombStartHeight = 5f;

        // Nếu có nhiều bom thì chúng rơi lệch nhau một chút.
        public const float BombStagger = 0.18f;

        // =========================================
        // BOMB COUNT
        // =========================================

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

        // =========================================
        // EXPLOSION DAMAGE
        // =========================================

        public static float GetDamageMultiplier(int level)
        {
            return level switch
            {
                <= 0 => 0f,

                1 => 1.20f,
                2 => 1.50f,

                3 => 1.50f,

                4 => 1.65f,

                _ => 1.80f,
            };
        }

        // =========================================
        // EXPLOSION SIZE
        // =========================================

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

        // =========================================
        // MAX FIRE FIELD
        // =========================================

        public static bool IsMaxForm(int level)
        {
            return level >= 5;
        }

        public const float FireDuration = 4f;

        // Mỗi 0.5 giây gây damage một lần.
        public const float FireTickInterval = 0.5f;

        // Mỗi tick gây 35% Player Damage.
        public const float FireDamageMultiplier = 0.35f;

        public static float GetFireRadius(int level)
        {
            return IsMaxForm(level)
                ? 1.85f
                : 0f;
        }

        // =========================================
        // DESCRIPTION
        // =========================================

        public static string GetDescription(int level)
        {
            return level switch
            {
                1 =>
                    "Mỗi 5 giây gọi 1 quả bom từ trên trời xuống một mục tiêu ngẫu nhiên, gây sát thương diện rộng.",

                2 =>
                    "Bom gây sát thương lớn hơn và vùng nổ rộng hơn.",

                3 =>
                    "Mỗi lần Không Kích gọi 2 quả bom vào các mục tiêu ngẫu nhiên.",

                4 =>
                    "Mỗi lần Không Kích gọi 3 quả bom vào các mục tiêu ngẫu nhiên.",

                5 =>
                    "MAX: 3 quả bom oanh tạc chiến trường. Mỗi điểm nổ để lại một bãi lửa gây sát thương liên tục trong 4 giây.",

                _ =>
                    "Triệu hồi bom từ trên trời tấn công các mục tiêu ngẫu nhiên.",
            };
        }
    }
}