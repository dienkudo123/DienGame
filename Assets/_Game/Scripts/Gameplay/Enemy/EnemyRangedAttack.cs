using DienGame.Gameplay.Balance;
using DienGame.Gameplay.Player;
using UnityEngine;

namespace DienGame.Gameplay.Enemy
{
    public sealed class EnemyRangedAttack : MonoBehaviour
    {
        private PlayerHealth _playerHealth;
        private float _damage = GameBalance.RangedProjectileDamage;
        private float _attackInterval = GameBalance.RangedAttackInterval;
        private float _projectileSpeed = GameBalance.RangedProjectileSpeed;
        private float _maxAttackDistance = GameBalance.RangedMaxAttackDistance;
        private float _timer;
        private bool _configured;

        private void Start()
        {
            _playerHealth = FindFirstObjectByType<PlayerHealth>();
            _timer = Random.Range(0.15f, _attackInterval);
        }

        private void Update()
        {
            if (!_configured ||
                _playerHealth == null ||
                _playerHealth.IsDead)
            {
                return;
            }

            _timer -= Time.deltaTime;

            if (_timer > 0f)
            {
                return;
            }

            Vector2 difference =
                (Vector2)_playerHealth.transform.position -
                (Vector2)transform.position;

            if (difference.sqrMagnitude >
                _maxAttackDistance * _maxAttackDistance)
            {
                return;
            }

            Fire(difference.normalized);
            _timer = _attackInterval;
        }

        public void Configure(
            float damage,
            float attackInterval,
            float projectileSpeed,
            float maxAttackDistance)
        {
            _damage = Mathf.Max(0f, damage);
            _attackInterval = Mathf.Max(0.1f, attackInterval);
            _projectileSpeed = Mathf.Max(0.1f, projectileSpeed);
            _maxAttackDistance = Mathf.Max(0.5f, maxAttackDistance);
            _configured = true;
        }

        private void Fire(Vector2 direction)
        {
            Vector2 spawnPosition =
                (Vector2)transform.position + direction * 0.55f;

            EnemyProjectile.Create(
                spawnPosition,
                direction,
                _projectileSpeed,
                _damage,
                GameBalance.RangedProjectileLifetime,
                new Color(0.82f, 0.55f, 1f, 1f),
                0.11f
            );
        }
    }
}
