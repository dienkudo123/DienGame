using DienGame.Gameplay.Balance;
using UnityEngine;

namespace DienGame.Gameplay.Player
{
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private PlayerCharacter playerCharacter;

        private float _currentHealth;
        private float _invulnerabilityTimer;

        public float MaxHealth =>
            playerCharacter != null
                ? playerCharacter.MaxHealth
                : GameBalance.PlayerMaxHealth;

        public float CurrentHealth => _currentHealth;
        public bool IsDead => _currentHealth <= 0f;
        public bool IsInvulnerable => _invulnerabilityTimer > 0f;

        private void Awake()
        {
            if (playerCharacter == null)
            {
                playerCharacter = GetComponent<PlayerCharacter>();
            }

            _currentHealth = MaxHealth;
        }

        private void Update()
        {
            if (_invulnerabilityTimer > 0f)
            {
                _invulnerabilityTimer -= Time.deltaTime;
            }
        }

        public void TakeDamage(float damage)
        {
            if (IsDead || IsInvulnerable || damage <= 0f)
            {
                return;
            }

            _currentHealth =
                Mathf.Max(0f, _currentHealth - damage);

            if (IsDead)
            {
                Die();
                return;
            }

            _invulnerabilityTimer =
                GameBalance.PlayerDamageInvulnerability;
        }

        public void Heal(float amount)
        {
            if (amount <= 0f || IsDead)
            {
                return;
            }

            _currentHealth =
                Mathf.Min(_currentHealth + amount, MaxHealth);
        }

        public void AddMaxHealth(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            if (playerCharacter != null)
            {
                playerCharacter.AddMaxHealth(amount);
            }

            _currentHealth =
                Mathf.Min(_currentHealth + amount, MaxHealth);
        }

        private void Die()
        {
            Debug.Log("PLAYER DEAD - GAME OVER");
        }
    }
}
