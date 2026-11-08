using System;
using UnityEngine;

namespace DienGame.Gameplay.Enemy
{
    public sealed class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private EnemyCharacter enemyCharacter;
        [SerializeField, Min(1f)] private float maxHealth = 100f;

        private float _currentHealth;

        public float MaxHealth => enemyCharacter != null ? enemyCharacter.MaxHealth : maxHealth;
        public float CurrentHealth => _currentHealth;
        public bool IsDead => _currentHealth <= 0f;

        public event Action<EnemyHealth> Died;

        private void Awake()
        {
            if (enemyCharacter == null) enemyCharacter = GetComponent<EnemyCharacter>();
            _currentHealth = MaxHealth;
        }

        public void SetMaxHealth(float value)
        {
            value = Mathf.Max(1f, value);

            if (enemyCharacter != null) enemyCharacter.SetMaxHealth(value);
            else maxHealth = value;

            _currentHealth = MaxHealth;
        }

        public void TakeDamage(float damage)
        {
            if (IsDead || damage <= 0f) return;

            _currentHealth = Mathf.Max(0f, _currentHealth - damage);
            if (IsDead) Die();
        }

        private void Die()
        {
            Died?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
