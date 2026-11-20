namespace DienGame.Gameplay.Balance
{
    public static class GameBalance
    {
        // =========================================
        // PLAYER
        // =========================================

        public const float PlayerMaxHealth = 100f;
        public const float PlayerDamage = 15f;
        public const float PlayerMoveSpeed = 4f;
        public const float PlayerAttackInterval = 0.55f;
        public const float PlayerProjectileSpeed = 12f;
        public const float PlayerTargetRange = 7f;
        public const float PlayerDamageInvulnerability = 0.65f;

        // =========================================
        // EXPERIENCE
        // =========================================

        public const int ExperiencePerEnemy = 3;
        public const int BaseExperienceToLevel = 12;
        public const int ExperienceIncreasePerLevel = 2;

        public const int GruntExperience = 3;
        public const int RunnerExperience = 3;
        public const int TankExperience = 5;
        public const int RangedExperience = 3;
        public const int SwarmExperience = 2;
        public const float EliteExperienceMultiplier = 6f;

        // A full 10-minute survival run targets approximately level 25 for an
        // actively fighting player collecting most of the dropped EXP.
        // Actual level depends on kills, drop collection and combat performance.
        public const int TargetLevelAtEndOfRun = 25;

        public static int CumulativeExperienceToReachLevel(int level)
        {
            int levelUps = level > 1 ? level - 1 : 0;
            return levelUps * BaseExperienceToLevel +
                   (levelUps * (levelUps - 1) / 2) * ExperienceIncreasePerLevel;
        }


        // =========================================
        // RUN
        // =========================================

        public const float SurvivalDuration = 600f;

        // =========================================
        // ENEMY - SHARED
        // =========================================

        public const float EnemyBaseHealth = 23f;
        public const float EnemyBaseDamage = 5f;
        public const float EnemyBaseMoveSpeed = 1.75f;
        public const float EnemyAttackInterval = 1.15f;
        public const float EnemyAttackRange = 0.9f;

        // Grunt
        public const float GruntHealth = 23f;
        public const float GruntDamage = 5f;
        public const float GruntMoveSpeed = 1.75f;

        // Runner
        public const float RunnerHealth = 17f;
        public const float RunnerDamage = 4f;
        public const float RunnerMoveSpeed = 2.35f;

        // Tank
        public const float TankHealth = 70f;
        public const float TankDamage = 9f;
        public const float TankMoveSpeed = 1.15f;

        // Ranged
        public const float RangedHealth = 24f;
        public const float RangedProjectileDamage = 5f;
        public const float RangedMoveSpeed = 1.45f;
        public const float RangedAttackInterval = 1.6f;
        public const float RangedProjectileSpeed = 7f;
        public const float RangedProjectileLifetime = 3.5f;
        public const float RangedPreferredDistance = 5.25f;
        public const float RangedRetreatDistance = 3.25f;
        public const float RangedMaxAttackDistance = 8f;

        // Swarm
        public const float SwarmHealth = 11f;
        public const float SwarmDamage = 3f;
        public const float SwarmMoveSpeed = 2.3f;

        // =========================================
        // DIFFICULTY RAMP
        // =========================================

        public const float DifficultyRampDuration = SurvivalDuration;

        public const int BaseEnemiesPerWave = 3;
        public const int MaxEnemiesPerWave = 5;

        public const float BaseWaveInterval = 4.0f;
        public const float MinWaveInterval = 2.75f;
        public const float FirstWaveDelay = 1.8f;

        public const int MaxAliveEnemies = 20;

        public const float MaxEnemyHealthMultiplier = 1f; // No HP ramp within one island.
        public const float EnemyHealthBonusPerIsland = 0.08f;
        public const float MaxEnemyDamageMultiplier = 1.18f;
        public const float MaxEnemyMoveSpeedMultiplier = 1.05f;

        // =========================================
        // ELITE
        // =========================================

        public const float EliteStartProgress = 0.52f;
        public const float EliteMaxSpawnChance = 0.035f;
        public const float EliteMinimumInterval = 50f;
        public const float EliteHealthMultiplier = 1.85f;
        public const float EliteDamageMultiplier = 1.20f;
        public const float EliteMoveSpeedMultiplier = 1.08f;
        public const float EliteScaleMultiplier = 1.22f;

        // =========================================
        // BOSS
        // =========================================

        public const float BossMaxHealth = 900f;
        public const float BossScale = 1.75f;
        public const float BossContactDamage = 0f;
        public const float BossMoveSpeed = 1.45f;

        public const float BossPhase2HealthNormalized = 0.70f;
        public const float BossPhase3HealthNormalized = 0.35f;

        public const float BossPhase1SpecialInterval = 6.8f;
        public const float BossPhase2SpecialInterval = 6f;
        public const float BossPhase3SpecialInterval = 5.3f;

        public const float BossSlamRadius = 2.7f;
        public const float BossSlamDamage = 15f;
        public const float BossSlamWarningDuration = 1.35f;

        public const float BossBurstProjectileDamage = 7f;
        public const float BossBurstProjectileSpeed = 6f;
        public const int BossPhase2ProjectileCount = 7;
        public const int BossPhase3ProjectileCount = 9;

        public const float BossChargeSpeed = 6.8f;
        public const float BossChargeDuration = 0.65f;
        public const float BossChargeDamage = 16f;
        public const float BossChargeWarningDuration = 1.35f;
        public const float BossChargeHitRadius = 1.1f;
    }
}
