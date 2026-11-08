using DienGame.Gameplay.Player;
using UnityEngine;

namespace DienGame.Gameplay.Enemy
{
    public sealed class EnemyAttack : MonoBehaviour
    {
        [SerializeField] private EnemyCharacter enemyCharacter;
        [SerializeField, Min(0f)] private float attackDamage = 10f;
        [SerializeField, Min(0.01f)] private float attackInterval = 1f;
        [SerializeField, Min(0f)] private float attackRange = 0.9f;

        private PlayerHealth _playerHealth;
        private float _timer;

        private float Damage => enemyCharacter != null ? enemyCharacter.Damage : attackDamage;
        private float AttackInterval => enemyCharacter != null ? enemyCharacter.AttackInterval : attackInterval;
        private float AttackRange => enemyCharacter != null ? enemyCharacter.AttackRange : attackRange;

        private void Awake()
        {
            if (enemyCharacter == null) enemyCharacter = GetComponent<EnemyCharacter>();
        }

        private void Start()
        {
            _playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        private void Update()
        {
            if (_timer > 0f) _timer -= Time.deltaTime;
            TryAttack();
        }

        private void TryAttack()
        {
            if (_playerHealth == null || _playerHealth.IsDead) return;

            float distance = Vector2.Distance(
                transform.position,
                _playerHealth.transform.position
            );

            if (distance > AttackRange || _timer > 0f) return;

            _playerHealth.TakeDamage(Damage);
            _timer = AttackInterval;
        }
    }
}
