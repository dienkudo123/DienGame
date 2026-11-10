using DienGame.Gameplay.Balance;
using UnityEngine;

namespace DienGame.Gameplay.Enemy
{
    public sealed class EnemyFactory
    {
        private readonly GameObject _enemyPrefab;
        private readonly GameObject _experienceOrbPrefab;

        public EnemyFactory(
            GameObject enemyPrefab,
            GameObject experienceOrbPrefab)
        {
            _enemyPrefab = enemyPrefab;
            _experienceOrbPrefab = experienceOrbPrefab;
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
                difficultyProgress
            );

            ConfigureExperienceDrop(enemy, definition, isElite);
            return enemy;
        }

        private static void ApplyRuntimeStats(
            GameObject enemy,
            EnemyDefinition definition,
            bool isElite,
            float progress)
        {
            progress = Mathf.Clamp01(progress);

            float healthMultiplier = Mathf.Lerp(
                1f,
                GameBalance.MaxEnemyHealthMultiplier,
                progress
            );

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

            if (isElite && definition.Archetype != EnemyArchetype.Boss)
            {
                healthMultiplier *= GameBalance.EliteHealthMultiplier;
                damageMultiplier *= GameBalance.EliteDamageMultiplier;
                moveSpeedMultiplier *= GameBalance.EliteMoveSpeedMultiplier;
            }

            EnemyHealth health = enemy.GetComponent<EnemyHealth>();

            if (health != null)
            {
                health.SetMaxHealth(definition.MaxHealth * healthMultiplier);
            }

            EnemyMovement movement = enemy.GetComponent<EnemyMovement>();

            if (movement != null)
            {
                movement.SetMoveSpeed(
                    definition.MoveSpeed * moveSpeedMultiplier
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
                        definition.Damage * damageMultiplier,
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
                        meleeAttack.enabled = true;
                        meleeAttack.SetDamage(
                            definition.Damage * damageMultiplier
                        );
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
                            definition.Damage * damageMultiplier
                        );
                    }

                    break;
                }
            }
        }

        private void ConfigureExperienceDrop(
            GameObject enemy,
            EnemyDefinition definition,
            bool isElite)
        {
            if (_experienceOrbPrefab == null || definition.Experience <= 0)
            {
                return;
            }

            EnemyExperienceDrop drop = enemy.GetComponent<EnemyExperienceDrop>();

            if (drop == null)
            {
                drop = enemy.AddComponent<EnemyExperienceDrop>();
            }

            int experience = definition.Experience;

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
    }
}
