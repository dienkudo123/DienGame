namespace DienGame.Gameplay.Skills.Marksman
{
    public static class CombatDroneSkill
    {
        // =========================================
        // DRONE COUNT
        // =========================================

        public static int GetDroneCount(int level)
        {
            return level switch
            {
                <= 0 => 0,

                // Lv1
                1 => 1,

                // Lv2
                2 => 1,

                // Lv3
                3 => 2,

                // Lv4
                4 => 2,

                // Lv5 MAX:
                // 2 Drone thường hợp nhất thành
                // 1 Elite Drone.
                _ => 1,
            };
        }

        // =========================================
        // MAX FORM
        // =========================================

        public static bool IsMaxForm(int level)
        {
            return level >= 5;
        }

        // =========================================
        // ATTACK INTERVAL
        // =========================================

        public static float GetAttackInterval(int level)
        {
            return level switch
            {
                <= 0 => 999f,

                1 => 1.10f,

                2 => 0.85f,

                3 => 0.80f,

                4 => 0.65f,

                // Elite Drone
                _ => 0.40f,
            };
        }

        // =========================================
        // DAMAGE
        // =========================================

        public static float GetDamageMultiplier(int level)
        {
            return level switch
            {
                <= 0 => 0f,

                1 => 0.35f,

                2 => 0.40f,

                3 => 0.45f,

                4 => 0.55f,

                // Một Elite Drone nhưng
                // mỗi phát mạnh hơn rất nhiều.
                _ => 1.15f,
            };
        }

        // =========================================
        // RANGE
        // =========================================

        public static float GetAttackRange(int level)
        {
            return level >= 5
                ? 8f
                : 6f;
        }

        // =========================================
        // ORBIT
        // =========================================

        public static float GetOrbitRadius(int level)
        {
            return level >= 5
                ? 2f
                : 2f;
        }

        public static float GetOrbitSpeed(int level)
        {
            return level >= 5
                ? 70f
                : 90f;
        }

        // =========================================
        // VISUAL SCALE
        // =========================================

        public static float GetVisualScale(int level)
        {
            return level >= 5
                ? 0.1f
                : 0.1f;
        }

        // =========================================
        // DESCRIPTION
        // =========================================

        public static string GetDescription(int level)
        {
            return level switch
            {
                1 =>
                    "Triệu hồi 1 Drone Chiến Đấu tự động tấn công kẻ địch.",

                2 =>
                    "Drone tăng tốc độ khai hỏa.",

                3 =>
                    "Triệu hồi Drone thứ 2.",

                4 =>
                    "Hai Drone được tăng mạnh tốc độ bắn và sát thương.",

                5 =>
                    "MAX: Hai Drone hợp nhất thành 1 Elite Drone với ngoại hình mới, tầm bắn xa và hỏa lực vượt trội.",

                _ =>
                    "Drone Chiến Đấu tự động hỗ trợ Xạ thủ.",
            };
        }
    }
}