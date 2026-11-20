using DienGame.Gameplay.Balance;
using DienGame.Gameplay.Player;
using DienGame.Gameplay.Skills.Assassin;
using UnityEngine;

namespace DienGame.Gameplay.Enemy
{
    public sealed class EnemyAttack : MonoBehaviour
    {
        [SerializeField] private EnemyCharacter enemyCharacter;

        private PlayerHealth _playerHealth;
        private float _timer;
        private float _runtimeDamage = GameBalance.EnemyBaseDamage;

        private float Damage =>
            enemyCharacter != null
                ? enemyCharacter.Damage
                : _runtimeDamage;

        private float AttackInterval =>
            enemyCharacter != null
                ? enemyCharacter.AttackInterval
                : GameBalance.EnemyAttackInterval;

        private float AttackRange =>
            enemyCharacter != null
                ? enemyCharacter.AttackRange
                : GameBalance.EnemyAttackRange;

        private void Awake()
        {
            if (enemyCharacter == null)
            {
                enemyCharacter = GetComponent<EnemyCharacter>();
            }
        }

        private void Start()
        {
            _playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        private void Update()
        {
            if (_timer > 0f)
            {
                _timer -= Time.deltaTime;
            }

            TryAttack();
        }

        public void SetDamage(float damage)
        {
            damage = Mathf.Max(0f, damage);

            if (enemyCharacter != null)
            {
                enemyCharacter.SetDamage(damage);
            }
            else
            {
                _runtimeDamage = damage;
            }
        }

        private void TryAttack()
        {
            if (_playerHealth == null || _playerHealth.IsDead)
            {
                return;
            }

            AssassinSoulPillar pillar = AssassinSoulPillar.GetAggroTarget(
                transform.position, _playerHealth.transform.position);
            Vector2 targetPosition = pillar != null ?
                (Vector2)pillar.transform.position : (Vector2)_playerHealth.transform.position;
            float distanceSquared =
                ((Vector2)transform.position - targetPosition).sqrMagnitude;

            float attackRangeSquared = AttackRange * AttackRange;

            if (_timer > 0f || distanceSquared > attackRangeSquared) return;
            if (pillar != null)
                pillar.TakeDamage(Damage);
            else
                _playerHealth.TakeDamage(Damage);
            _timer = AttackInterval;
        }
    }
}
