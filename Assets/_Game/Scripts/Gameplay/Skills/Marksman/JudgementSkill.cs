namespace DienGame.Gameplay.Skills.Marksman
{
    public static class JudgementSkill
    {
        // =========================================
        // COOLDOWN
        // =========================================

        public const float Cooldown = 30f;

        // =========================================
        // TARGET
        // =========================================

        public const float TargetRange = 12f;

        // =========================================
        // BOMB
        // =========================================

        public const float BombStartHeight = 7f;

        public const float BombFallDuration = 0.55f;

        // =========================================
        // LEVEL
        // =========================================

        public static bool IsMaxLevel(int level)
        {
            return level >= 5;
        }

        // =========================================
        // DAMAGE
        // =========================================

        public static float GetDamageMultiplier(int level)
        {
            return level switch
            {
                <= 1 => 4.0f,
                2 => 5.0f,
                3 => 6.5f,
                4 => 8.0f,
                _ => 12.0f,
            };
        }

        // =========================================
        // DAMAGE RADIUS
        // =========================================

        public static float GetExplosionRadius(int level)
        {
            return level switch
            {
                <= 1 => 3.0f,
                2 => 3.4f,
                3 => 3.8f,
                4 => 4.3f,
                _ => 5.5f,
            };
        }

        // =========================================
        // BOMB VISUAL
        // =========================================

        public static float GetBombVisualMultiplier(int level)
        {
            return level switch
            {
                <= 1 => 1.0f,
                2 => 1.1f,
                3 => 1.25f,
                4 => 1.45f,

                // MAX: bom khổng lồ.
                _ => 2.4f,
            };
        }

        // =========================================
        // EXPLOSION VISUAL
        // =========================================

        public static float GetExplosionVisualMultiplier(int level)
        {
            return level switch
            {
                <= 1 => 1.0f,
                2 => 1.05f,
                3 => 1.15f,
                4 => 1.30f,

                // MAX
                _ => 1.65f,
            };
        }

        // =========================================
        // MAX PLANE
        // =========================================

        public const float MaxPlaneDuration = 0.7f;

        public const float MaxPlaneTravelDistance = 16f;

        // =========================================
        // MAX CAMERA SHAKE
        // =========================================

        public const float MaxShakeDuration = 0.55f;

        public const float MaxShakeStrength = 0.28f;
    }
}