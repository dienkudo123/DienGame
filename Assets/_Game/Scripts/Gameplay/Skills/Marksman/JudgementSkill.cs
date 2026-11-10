namespace DienGame.Gameplay.Skills.Marksman
{
    public static class JudgementSkill
    {
        // =========================================
        // ACTIVE ULTIMATE
        // "Vong Vay Tu Than"
        // =========================================

        public const float Cooldown = 28f;

        public const float ChargeDuration = 0.35f;
        public const float BarrageDuration = 1.55f;
        public const float FinishDuration = 0.42f;

        public const float OrbitRadius = 1.35f;
        public const float RotationSpeed = 165f;

        public const float PulseDamageFraction = 0.55f;
        public const float FinalDamageFraction = 0.45f;

        public const float PulseShakeDuration = 0.10f;
        public const float PulseShakeStrength = 0.055f;

        public const float FinalShakeDuration = 0.48f;
        public const float FinalShakeStrength = 0.24f;

        public static bool IsMaxLevel(int level)
        {
            return level >= 5;
        }

        public static float GetRadius(int level)
        {
            return level switch
            {
                <= 1 => 4.0f,
                2 => 4.5f,
                3 => 5.0f,
                4 => 5.5f,
                _ => 6.2f,
            };
        }

        public static float GetTotalDamageMultiplier(int level)
        {
            return level switch
            {
                <= 1 => 4.5f,
                2 => 5.5f,
                3 => 7.0f,
                4 => 8.5f,
                _ => 11.5f,
            };
        }

        public static int GetCannonCount(int level)
        {
            return level switch
            {
                <= 1 => 6,
                2 => 7,
                3 => 8,
                4 => 10,
                _ => 12,
            };
        }

        public static int GetPulseCount(int level)
        {
            return level switch
            {
                <= 2 => 3,
                <= 4 => 4,
                _ => 5,
            };
        }

        public static float GetBeamWidth(int level)
        {
            return IsMaxLevel(level)
                ? 0.18f
                : 0.12f;
        }

        public static float GetNodeSize(int level)
        {
            return IsMaxLevel(level)
                ? 0.44f
                : 0.34f;
        }
    }
}
