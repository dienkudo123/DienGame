using DienGame.Gameplay.Balance;
using UnityEngine;

namespace DienGame.Gameplay.Enemy
{
    public sealed class EnemyFactory
    {
        private readonly GameObject _enemyPrefab;
        private readonly GameObject _experienceOrbPrefab;
        private readonly int _stage;
        private readonly float _startingMarksmanDamage;

        public EnemyFactory(
            GameObject enemyPrefab,
            GameObject experienceOrbPrefab, int stage = 1,
            float startingMarksmanDamage = GameBalance.PlayerDamage)
        {
            _enemyPrefab = enemyPrefab;
            _experienceOrbPrefab = experienceOrbPrefab;
            _stage = Mathf.Clamp(stage, 1, 5);
            _startingMarksmanDamage = Mathf.Max(1f, startingMarksmanDamage);
        }

        public GameObject Spawn(
            Vector2 position,
            EnemyArchetype archetype,
            bool isElite,
            float difficultyProgress)
        {
            if (_enemyPrefab == null)
            {
                Debug.LogError("EnemyFactory: Enemy Prefab đang null.");
                return null;
            }

            EnemyDefinition definition = EnemyCatalogProvider.Get(archetype);

            if (definition == null)
            {
                return null;
            }

            GameObject enemy = Object.Instantiate(
                _enemyPrefab,
                position,
                Quaternion.identity
            );

            enemy.name = isElite && archetype != EnemyArchetype.Boss
                ? $"Elite_{definition.DisplayName}"
                : definition.DisplayName;

            EnemyProfile profile = enemy.GetComponent<EnemyProfile>();

            if (profile == null)
            {
                profile = enemy.AddComponent<EnemyProfile>();
            }

            profile.Configure(definition, isElite);

            ApplyRuntimeStats(
                enemy,
                definition,
                isElite,
                difficultyProgress, _stage, _startingMarksmanDamage
            );

            ConfigureExperienceDrop(enemy, definition, isElite);
            return enemy;
        }

        private static void ApplyRuntimeStats(
            GameObject enemy,
            EnemyDefinition definition,
            bool isElite,
            float progress, int stage, float openingShotDamage)
        {
            progress = Mathf.Clamp01(progress);

            // A monster archetype has fixed HP for the entire island run.
            // Only the island index and the elite flag can change its max HP.
            float healthMultiplier = 1f +
                GameBalance.EnemyHealthBonusPerIsland * (stage - 1);

            float damageMultiplier = Mathf.Lerp(
                1f,
                GameBalance.MaxEnemyDamageMultiplier,
                progress
            );

            float moveSpeedMultiplier = Mathf.Lerp(
                1f,
                GameBalance.MaxEnemyMoveSpeedMultiplier,
                progress
            );

            float stageDamageFactor = 1f + 0.045f * (stage - 1);
            damageMultiplier *= stageDamageFactor;

            if (isElite && definition.Archetype != EnemyArchetype.Boss)
            {
                healthMultiplier *= GameBalance.EliteHealthMultiplier;
                damageMultiplier *= GameBalance.EliteDamageMultiplier;
                moveSpeedMultiplier *= GameBalance.EliteMoveSpeedMultiplier;
            }

            EnemyHealth health = enemy.GetComponent<EnemyHealth>();

            if (health != null)
            {
                float baseHp = definition.Archetype switch
                {
                    EnemyArchetype.Grunt => Mathf.Clamp(openingShotDamage, 6f, GameBalance.PlayerDamage) * 1.5f,
                    EnemyArchetype.Runner => GameBalance.RunnerHealth,
                    EnemyArchetype.Tank => GameBalance.TankHealth,
                    EnemyArchetype.Ranged => GameBalance.RangedHealth,
                    EnemyArchetype.Swarm => GameBalance.SwarmHealth,
                    EnemyArchetype.Boss => GameBalance.BossMaxHealth,
                    _ => definition.MaxHealth
                };
                health.SetMaxHealth(baseHp * (definition.Archetype == EnemyArchetype.Boss
                    ? 1f : healthMultiplier));
            }

            EnemyMovement movement = enemy.GetComponent<EnemyMovement>();

            if (movement != null)
            {
                movement.SetMoveSpeed(
                    (definition.Archetype == EnemyArchetype.Runner
                        ? GameBalance.RunnerMoveSpeed : definition.MoveSpeed) * moveSpeedMultiplier
                );

                movement.ConfigureDistances(
                    definition.PreferredDistance,
                    definition.RetreatDistance
                );
            }

            ConfigureAttack(
                enemy,
                definition,
                damageMultiplier
            );
        }

        private static void ConfigureAttack(
            GameObject enemy,
            EnemyDefinition definition,
            float damageMultiplier)
        {
            EnemyAttack meleeAttack = enemy.GetComponent<EnemyAttack>();
            EnemyRangedAttack rangedAttack = enemy.GetComponent<EnemyRangedAttack>();

            switch (definition.AttackKind)
            {
                case EnemyAttackKind.Ranged:
                {
                    if (meleeAttack != null)
                    {
                        meleeAttack.enabled = false;
                    }

                    if (rangedAttack == null)
                    {
                        rangedAttack = enemy.AddComponent<EnemyRangedAttack>();
                    }

                    rangedAttack.enabled = true;
                    rangedAttack.Configure(
                        GetTunedDamage(definition) * damageMultiplier,
                        definition.AttackInterval,
                        definition.ProjectileSpeed,
                        definition.MaxAttackDistance
                    );
                    break;
                }

                case EnemyAttackKind.Boss:
                {
                    if (rangedAttack != null)
                    {
                        rangedAttack.enabled = false;
                    }

                    if (meleeAttack != null)
                    {
                        // No invisible boss contact damage: ALL boss hits need a warning zone.
                        meleeAttack.enabled = false;
                    }

                    if (enemy.GetComponent<BossCombatController>() == null)
                    {
                        enemy.AddComponent<BossCombatController>();
                    }

                    break;
                }

                default:
                {
                    if (rangedAttack != null)
                    {
                        rangedAttack.enabled = false;
                    }

                    if (meleeAttack != null)
                    {
                        meleeAttack.enabled = true;
                        meleeAttack.SetDamage(
                            GetTunedDamage(definition) * damageMultiplier
                        );
                    }

                    break;
                }
            }
        }

        private static float GetTunedDamage(EnemyDefinition definition)
        {
            return definition.Archetype switch
            {
                EnemyArchetype.Grunt => GameBalance.GruntDamage,
                EnemyArchetype.Runner => GameBalance.RunnerDamage,
                EnemyArchetype.Tank => GameBalance.TankDamage,
                EnemyArchetype.Ranged => GameBalance.RangedProjectileDamage,
                EnemyArchetype.Swarm => GameBalance.SwarmDamage,
                EnemyArchetype.Boss => 0f,
                _ => definition.Damage
            };
        }

        private void ConfigureExperienceDrop(
            GameObject enemy,
            EnemyDefinition definition,
            bool isElite)
        {
            int experience = GetTunedExperience(definition);
            if (_experienceOrbPrefab == null || experience <= 0)
            {
                return;
            }

            EnemyExperienceDrop drop = enemy.GetComponent<EnemyExperienceDrop>();

            if (drop == null)
            {
                drop = enemy.AddComponent<EnemyExperienceDrop>();
            }

            if (isElite && definition.Archetype != EnemyArchetype.Boss)
            {
                experience = Mathf.RoundToInt(
                    experience * GameBalance.EliteExperienceMultiplier
                );
            }

            drop.Initialize(
                _experienceOrbPrefab,
                Mathf.Max(1, experience)
            );
        }
        private static int GetTunedExperience(EnemyDefinition definition)
        {
            // Explicit per-archetype EXP so the ScriptableObject catalog's
            // older EXP values cannot override the Phase 13 progression target.
            return definition.Archetype switch
            {
                EnemyArchetype.Grunt => GameBalance.GruntExperience,
                EnemyArchetype.Runner => GameBalance.RunnerExperience,
                EnemyArchetype.Tank => GameBalance.TankExperience,
                EnemyArchetype.Ranged => GameBalance.RangedExperience,
                EnemyArchetype.Swarm => GameBalance.SwarmExperience,
                EnemyArchetype.Boss => 0,
                _ => definition.Experience
            };
        }

    }
}
