using DienGame.Gameplay.Balance;
using DienGame.Gameplay.Combat;
using DienGame.Gameplay.Core.Pooling;
using DienGame.Gameplay.Skills;
using DienGame.Gameplay.Skills.Marksman;
using UnityEngine;

namespace DienGame.Gameplay.Player
{
    [RequireComponent(typeof(PlayerTargeting))]
    [RequireComponent(typeof(PlayerShootVisualSignal))]
    [RequireComponent(typeof(PlayerSkillManager))]
    [RequireComponent(typeof(MarksmanSkillController))]
    public sealed class PlayerAutoAttack : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerCharacter playerCharacter;
        [SerializeField] private GameObject projectilePrefab;

        [Header("Projectile")]
        [SerializeField, Min(0f)] private float projectileSpawnDistance = 0.6f;

        [Header("Pooling")]
        [SerializeField, Min(0)] private int projectilePrewarmCount = 32;

        private PlayerTargeting _targeting;
        private PlayerShootVisualSignal _shootVisualSignal;
        private MarksmanSkillController _marksmanSkills;
        private float _attackTimer;

        public float AttackDamage =>
            playerCharacter != null
                ? playerCharacter.Damage
                : GameBalance.PlayerDamage;

        public float AttackInterval =>
            playerCharacter != null
                ? playerCharacter.AttackInterval
                : GameBalance.PlayerAttackInterval;

        public float ProjectileSpeed =>
            GameBalance.PlayerProjectileSpeed;

        public GameObject ProjectilePrefab =>
            projectilePrefab;

        public float ProjectileSpawnDistance =>
            projectileSpawnDistance;

        private void Awake()
        {
            _targeting = GetComponent<PlayerTargeting>();
            _shootVisualSignal = GetComponent<PlayerShootVisualSignal>();
            _marksmanSkills = GetComponent<MarksmanSkillController>();

            if (playerCharacter == null)
            {
                playerCharacter = GetComponent<PlayerCharacter>();
            }

            if (projectilePrefab != null && projectilePrewarmCount > 0)
            {
                GameObjectPoolService.Warm(
                    projectilePrefab,
                    projectilePrewarmCount
                );
            }
        }

        private void Update()
        {
            if (_attackTimer > 0f)
            {
                _attackTimer -= Time.deltaTime;
            }

            TryAttack();
        }

        private void TryAttack()
        {
            Transform target =
                _targeting != null
                    ? _targeting.CurrentTarget
                    : null;

            if (target == null || _attackTimer > 0f)
            {
                return;
            }

            Shoot(target);
            _attackTimer = AttackInterval;
        }

        private void Shoot(Transform target)
        {
            if (projectilePrefab == null)
            {
                Debug.LogError(
                    "PlayerAutoAttack: Chưa gán Projectile Prefab.",
                    this
                );
                return;
            }

            Vector2 direction =
                (Vector2)target.position -
                (Vector2)transform.position;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            direction.Normalize();

            Vector2 spawnPosition =
                (Vector2)transform.position +
                direction * projectileSpawnDistance;

            int thunderLevel = 0;

            if (_marksmanSkills != null)
            {
                thunderLevel =
                    _marksmanSkills.RegisterNormalShotAndGetThunderLevel();
            }

            GameObject projectileObject =
                GameObjectPoolService.Spawn(
                    projectilePrefab,
                    spawnPosition,
                    Quaternion.identity
                );

            if (projectileObject == null)
            {
                return;
            }

            if (projectileObject.TryGetComponent(out Projectile projectile))
            {
                projectile.Initialize(
                    direction,
                    ProjectileSpeed,
                    AttackDamage,
                    _marksmanSkills,
                    thunderLevel
                );
            }
            else
            {
                Debug.LogError(
                    "Projectile Prefab không có Projectile.",
                    projectileObject
                );

                GameObjectPoolService.Release(projectileObject);
            }

            _shootVisualSignal?.TriggerShot();
        }

        public void MultiplyAttackDamage(float multiplier)
        {
            if (multiplier <= 0f || playerCharacter == null)
            {
                return;
            }

            playerCharacter.MultiplyDamage(multiplier);
        }

        public void MultiplyProjectileSpeed(float multiplier)
        {
            // Projectile speed hiện được quản lý tập trung trong GameBalance.
            // Giữ method để tương thích API cũ.
        }
    }
}
