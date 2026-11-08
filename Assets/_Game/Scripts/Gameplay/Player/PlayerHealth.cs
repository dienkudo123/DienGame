using UnityEngine;

namespace DienGame.Gameplay.Player
{
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private PlayerCharacter playerCharacter;
        [SerializeField, Min(1f)] private float maxHealth = 100f;

        private float _currentHealth;

        public float MaxHealth => playerCharacter != null ? playerCharacter.MaxHealth : maxHealth;
        public float CurrentHealth => _currentHealth;
        public bool IsDead => _currentHealth <= 0f;

        private void Awake()
        {
            if (playerCharacter == null) playerCharacter = GetComponent<PlayerCharacter>();
            _currentHealth = MaxHealth;
        }

        public void TakeDamage(float damage)
        {
            if (IsDead || damage <= 0f) return;

            _currentHealth = Mathf.Max(0f, _currentHealth - damage);

            if (IsDead) Die();
        }

        public void Heal(float amount)
        {
            if (amount <= 0f || IsDead) return;
            _currentHealth = Mathf.Min(_currentHealth + amount, MaxHealth);
        }

        public void AddMaxHealth(float amount)
        {
            if (amount <= 0f) return;

            if (playerCharacter != null) playerCharacter.AddMaxHealth(amount);
            else maxHealth += amount;

            _currentHealth = Mathf.Min(_currentHealth + amount, MaxHealth);
        }

        private void Die()
        {
            Debug.Log("PLAYER DEAD - GAME OVER");
        }
    }
}
