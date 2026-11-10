namespace DienGame.Gameplay.Skills.Marksman
{
    public static class CombatDroneSkill
    {
        public static int GetDroneCount(int level)
        {
            return level switch
            {
                <= 0 => 0,
                1 => 1,
                2 => 1,
                3 => 2,
                4 => 2,
                _ => 1,
            };
        }

        public static bool IsMaxForm(int level)
        {
            return level >= 5;
        }

        public static float GetAttackInterval(int level)
        {
            return level switch
            {
                <= 0 => 999f,
                1 => 0.80f,
                2 => 0.70f,
                3 => 0.75f,
                4 => 0.65f,
                _ => 0.45f,
            };
        }

        public static float GetDamageMultiplier(int level)
        {
            return level switch
            {
                <= 0 => 0f,
                1 => 0.35f,
                2 => 0.45f,
                3 => 0.40f,
                4 => 0.50f,
                _ => 1.30f,
            };
        }

        public static float GetAttackRange(int level)
        {
            return level >= 5
                ? 8f
                : 6f;
        }

        public static float GetOrbitRadius(int level)
        {
            return 2f;
        }

        public static float GetOrbitSpeed(int level)
        {
            return level >= 5
                ? 70f
                : 90f;
        }

        public static float GetVisualScale(int level)
        {
            return 0.1f;
        }

        public static string GetDescription(int level)
        {
            return level switch
            {
                1 =>
                    "Triệu hồi 1 Drone Chiến Đấu tự động tấn công kẻ địch.",
                2 =>
                    "Drone tăng sát thương và tốc độ khai hỏa.",
                3 =>
                    "Triệu hồi Drone thứ 2.",
                4 =>
                    "Hai Drone được tăng mạnh tốc độ bắn và sát thương.",
                5 =>
                    "MAX: Hai Drone hợp nhất thành 1 Elite Drone với tầm bắn xa và hỏa lực vượt trội.",
                _ =>
                    "Drone Chiến Đấu tự động hỗ trợ Xạ thủ.",
            };
        }
    }
}
