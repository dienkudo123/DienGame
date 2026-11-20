namespace DienGame.Gameplay.Skills.Marksman
{
    public static class RicochetBulletSkill
    {
        public const float BounceRange = 3f;

        public static int GetBounceCount(int level)
        {
            return level switch
            {
                <= 0 => 0,
                1 => 1,
                2 => 1,
                3 => 2,
                4 => 2,
                _ => 3,
            };
        }

        public static float GetBounceDamageMultiplier(int level)
        {
            return level switch
            {
                <= 0 => 0f,
                1 => 0.70f,
                2 => 0.90f,
                3 => 0.80f,
                4 => 1.00f,
                _ => 1.00f,
            };
        }

        public static string GetDescription(int level)
        {
            return level switch
            {
                1 => "Đạn thường sau khi trúng mục tiêu nảy thêm 1 lần, hit nảy gây 70% sát thương.",
                2 => "Đạn Nảy vẫn nảy 1 lần, hit nảy tăng lên 90% sát thương.",
                3 => "Đạn thường nảy tối đa 2 lần, mỗi hit nảy gây 80% sát thương.",
                4 => "Đạn thường nảy tối đa 2 lần, mỗi hit nảy gây 100% sát thương.",
                5 => "MAX: Đạn thường nảy tối đa 3 lần, mỗi hit nảy gây 100% sát thương. Nếu viên đạn là Lôi Đạn, mỗi điểm nảy tiếp tục kích hoạt chuỗi sét.",
                _ => "Đạn thường có thể nảy sang kẻ địch gần nhất sau khi trúng mục tiêu.",
            };
        }
    }
}
