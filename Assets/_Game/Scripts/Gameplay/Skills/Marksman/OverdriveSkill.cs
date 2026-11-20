namespace DienGame.Gameplay.Skills.Marksman
{
    public static class OverdriveSkill
    {
        public static float GetAttackSpeedBonus(int level)
        {
            return level switch
            {
                <= 0 => 0f,
                1 => 0.25f,
                2 => 0.35f,
                3 => 0.35f,
                4 => 0.45f,
                _ => 0.60f,
            };
        }

        public static float GetDamageBonus(int level)
        {
            return level switch
            {
                <= 0 => 0f,
                1 => 0f,
                2 => 0f,
                3 => 0.15f,
                4 => 0.20f,
                _ => 0.30f,
            };
        }

        public static float GetDuration(int level)
        {
            return level switch
            {
                <= 0 => 0f,
                1 => 4f,
                2 => 4f,
                3 => 5f,
                4 => 5f,
                _ => 6f,
            };
        }

        public static float GetCooldown(int level)
        {
            return level switch
            {
                <= 0 => float.PositiveInfinity,
                1 => 18f,
                2 => 16f,
                3 => 15f,
                4 => 13f,
                _ => 10f,
            };
        }

        public static string GetDescription(int level)
        {
            return level switch
            {
                1 => "Tự động Quá Tải theo chu kỳ: +25% tốc độ đánh trong 4 giây, hồi 18 giây sau khi hiệu ứng kết thúc.",
                2 => "Quá Tải: +35% tốc độ đánh trong 4 giây, hồi 16 giây.",
                3 => "Quá Tải: +35% tốc độ đánh và +15% sát thương trong 5 giây, hồi 15 giây.",
                4 => "Quá Tải: +45% tốc độ đánh và +20% sát thương trong 5 giây, hồi 13 giây.",
                5 => "MAX: +60% tốc độ đánh và +30% sát thương trong 6 giây, hồi 10 giây.",
                _ => "Định kỳ tự kích hoạt trạng thái tăng mạnh tốc độ đánh và sát thương.",
            };
        }
    }
}
