namespace DienGame.Gameplay.Skills.Marksman
{
    public static class TacticalShieldSkill
    {
        public static float GetCooldown(int level)
        {
            return level switch
            {
                <= 0 => float.PositiveInfinity,
                1 => 14f,
                2 => 12f,
                3 => 10f,
                4 => 8f,
                _ => 6f,
            };
        }

        public static string GetDescription(int level)
        {
            return level switch
            {
                1 => "Tạo Khiên Chiến Thuật chặn hoàn toàn 1 đòn đánh. Khi vỡ, khiên hồi sau 14 giây.",
                2 => "Khiên tiếp tục chặn hoàn toàn 1 đòn, thời gian hồi giảm còn 12 giây.",
                3 => "Khiên tiếp tục chặn hoàn toàn 1 đòn, thời gian hồi giảm còn 10 giây.",
                4 => "Khiên tiếp tục chặn hoàn toàn 1 đòn, thời gian hồi giảm còn 8 giây.",
                5 => "MAX: Khiên chặn hoàn toàn 1 đòn và hồi lại sau 6 giây.",
                _ => "Chặn hoàn toàn đòn gây sát thương tiếp theo rồi tự hồi theo thời gian.",
            };
        }
    }
}
