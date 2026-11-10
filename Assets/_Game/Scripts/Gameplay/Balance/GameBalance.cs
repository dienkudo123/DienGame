namespace DienGame.Gameplay.Balance
{
    public static class GameBalance
    {
        // =========================================
        // PLAYER
        // =========================================

        public const float PlayerMaxHealth = 100f;
        public const float PlayerDamage = 15f;
        public const float PlayerMoveSpeed = 5f;
        public const float PlayerAttackInterval = 0.55f;
        public const float PlayerProjectileSpeed = 12f;
        public const float PlayerTargetRange = 7f;
        public const float PlayerDamageInvulnerability = 0.35f;

        // =========================================
        // EXPERIENCE
        // =========================================

        public const int ExperiencePerEnemy = 2;
        public const int BaseExperienceToLevel = 20;
        public const int ExperienceIncreasePerLevel = 8;

        public const int GruntExperience = 2;
        public const int RunnerExperience = 2;
        public const int TankExperience = 6;
        public const int RangedExperience = 4;
        public const int SwarmExperience = 1;
        public const float EliteExperienceMultiplier = 6f;

        // =========================================
        // RUN
        // =========================================

        public const float SurvivalDuration = 600f;

        // =========================================
        // ENEMY - SHARED
        // =========================================

        public const float EnemyBaseHealth = 30f;
        public const float EnemyBaseDamage = 8f;
        public const float EnemyBaseMoveSpeed = 2.2f;
        public const float EnemyAttackInterval = 1.15f;
        public const float EnemyAttackRange = 0.9f;

        // Grunt
        public const float GruntHealth = 30f;
        public const float GruntDamage = 8f;
        public const float GruntMoveSpeed = 2.2f;

        // Runner
        public const float RunnerHealth = 18f;
        public const float RunnerDamage = 6f;
        public const float RunnerMoveSpeed = 3.7f;

        // Tank
        public const float TankHealth = 110f;
        public const float TankDamage = 16f;
        public const float TankMoveSpeed = 1.45f;

        // Ranged
        public const float RangedHealth = 28f;
        public const float RangedProjectileDamage = 7f;
        public const float RangedMoveSpeed = 1.8f;
        public const float RangedAttackInterval = 1.6f;
        public const float RangedProjectileSpeed = 7f;
        public const float RangedProjectileLifetime = 3.5f;
        public const float RangedPreferredDistance = 5.25f;
        public const float RangedRetreatDistance = 3.25f;
        public const float RangedMaxAttackDistance = 8f;

        // Swarm
        public const float SwarmHealth = 12f;
        public const float SwarmDamage = 4f;
        public const float SwarmMoveSpeed = 3f;

        // =========================================
        // DIFFICULTY RAMP
        // =========================================

        public const float DifficultyRampDuration = SurvivalDuration;

        public const int BaseEnemiesPerWave = 4;
        public const int MaxEnemiesPerWave = 9;

        public const float BaseWaveInterval = 4f;
        public const float MinWaveInterval = 2.6f;
        public const float FirstWaveDelay = 1f;

        public const int MaxAliveEnemies = 60;

        public const float MaxEnemyHealthMultiplier = 4f;
        public const float MaxEnemyDamageMultiplier = 2f;
        public const float MaxEnemyMoveSpeedMultiplier = 1.25f;

        // =========================================
        // ELITE
        // =========================================

        public const float EliteStartProgress = 0.20f;
        public const float EliteMaxSpawnChance = 0.08f;
        public const float EliteMinimumInterval = 30f;
        public const float EliteHealthMultiplier = 5f;
        public const float EliteDamageMultiplier = 1.6f;
        public const float EliteMoveSpeedMultiplier = 1.08f;
        public const float EliteScaleMultiplier = 1.22f;

        // =========================================
        // BOSS
        // =========================================

        public const float BossMaxHealth = 6000f;
        public const float BossScale = 1.75f;
        public const float BossContactDamage = 20f;
        public const float BossMoveSpeed = 1.85f;

        public const float BossPhase2HealthNormalized = 0.70f;
        public const float BossPhase3HealthNormalized = 0.35f;

        public const float BossPhase1SpecialInterval = 5.5f;
        public const float BossPhase2SpecialInterval = 4.4f;
        public const float BossPhase3SpecialInterval = 3.4f;

        public const float BossSlamRadius = 2.7f;
        public const float BossSlamDamage = 22f;
        public const float BossSlamWarningDuration = 0.7f;

        public const float BossBurstProjectileDamage = 12f;
        public const float BossBurstProjectileSpeed = 7.5f;
        public const int BossPhase2ProjectileCount = 8;
        public const int BossPhase3ProjectileCount = 12;

        public const float BossChargeSpeed = 8f;
        public const float BossChargeDuration = 0.65f;
        public const float BossChargeDamage = 24f;
        public const float BossChargeWarningDuration = 0.45f;
        public const float BossChargeHitRadius = 1.1f;
    }
}
