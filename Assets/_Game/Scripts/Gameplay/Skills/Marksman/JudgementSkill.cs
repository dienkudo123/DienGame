namespace DienGame.Gameplay.Skills.Marksman
{
    public static class JudgementSkill
    {
        public const float Cooldown = 28f;

        public const float ChargeDuration = 0.30f;
        public const float BarrageDuration = 1.65f;
        public const float FinishDuration = 0.48f;

        public const float RotationSpeed = 155f;
        public const float CannonFrameDuration = 0.055f;

        // Điểm bắt đầu beam tính từ tâm cannon theo hướng nòng.
        // Giá trị nhỏ hơn = beam chui sâu hơn vào cannon, nhìn liền mạch hơn.
        public const float BeamStartOffsetFactor = 0.14f;

        // Generated cannon art faces to the right by default.
        public const float CannonSpriteAngleOffset = 0f;

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

        public static float GetOrbitRadius(int level)
        {
            return level switch
            {
                <= 1 => 1.55f,
                2 => 1.60f,
                3 => 1.70f,
                4 => 1.82f,
                _ => 2.05f,
            };
        }

        public static float GetCannonWorldWidth(int level)
        {
            return level switch
            {
                <= 1 => 0.74f,
                2 => 0.76f,
                3 => 0.79f,
                4 => 0.82f,
                _ => 0.88f,
            };
        }

        public static float GetBeamWorldHeight(int level)
        {
            return IsMaxLevel(level)
                ? 0.34f
                : 0.28f;
        }

        public static float GetImpactWorldSize(int level)
        {
            return IsMaxLevel(level)
                ? 0.42f
                : 0.32f;
        }

        public static float GetShockwaveDiameter(int level)
        {
            return GetRadius(level) * 2.15f;
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
    }
}
