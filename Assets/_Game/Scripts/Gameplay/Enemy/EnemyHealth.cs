using System;
using System.Collections.Generic;
using DienGame.Gameplay.Balance;
using DienGame.Gameplay.Phase13;
using UnityEngine;

namespace DienGame.Gameplay.Enemy
{
    public sealed class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private EnemyCharacter enemyCharacter;

        private float _fallbackMaxHealth = GameBalance.EnemyBaseHealth;
        private float _currentHealth;

        public float MaxHealth =>
            enemyCharacter != null
                ? enemyCharacter.MaxHealth
                : _fallbackMaxHealth;

        public float CurrentHealth => _currentHealth;
        public float HealthNormalized =>
            MaxHealth > 0f
                ? Mathf.Clamp01(_currentHealth / MaxHealth)
                : 0f;

        public bool IsDead => _currentHealth <= 0f;

        private static readonly HashSet<EnemyHealth> ActiveSet = new HashSet<EnemyHealth>();
        public static IReadOnlyCollection<EnemyHealth> Active => ActiveSet;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry() => ActiveSet.Clear();

        private void OnEnable() => ActiveSet.Add(this);
        private void OnDisable() => ActiveSet.Remove(this);

        public static event Action<EnemyHealth> AnyEnemyDied;
        public event Action<EnemyHealth> Died;
        public event Action<EnemyHealth> HealthChanged;

        private void Awake()
        {
            if (enemyCharacter == null)
            {
                enemyCharacter = GetComponent<EnemyCharacter>();
            }

            if (enemyCharacter == null)
            {
                _fallbackMaxHealth = GameBalance.EnemyBaseHealth;
            }

            _currentHealth = MaxHealth;
        }

        public void SetMaxHealth(float value)
        {
            value = Mathf.Max(1f, value);

            if (enemyCharacter != null)
            {
                enemyCharacter.SetMaxHealth(value);
            }
            else
            {
                _fallbackMaxHealth = value;
            }

            _currentHealth = MaxHealth;
            HealthChanged?.Invoke(this);
        }

        public void TakeDamage(float damage)
        {
            if (IsDead || damage <= 0f)
            {
                return;
            }

            float previousHealth = _currentHealth;
            _currentHealth = Mathf.Max(0f, _currentHealth - damage);
            Phase13CombatMetrics.RecordEnemyDamage(previousHealth - _currentHealth);
            Phase13Audio.Play(Phase13Cue.Hit);

            HealthChanged?.Invoke(this);

            if (IsDead)
            {
                Die();
            }
        }

        private void Die()
        {
            Phase13CombatMetrics.RecordKill();
            AnyEnemyDied?.Invoke(this);
            Died?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
