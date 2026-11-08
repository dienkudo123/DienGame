using DienGame.Gameplay.Character;
using UnityEngine;

namespace DienGame.Gameplay.Enemy
{
    public sealed class EnemyCharacter : CharacterBase
    {
        [Header("Enemy")]
        [SerializeField] private bool hasSkill;
        [SerializeField] private bool canRangedAttack;

        [Header("Attack")]
        [SerializeField, Min(0.01f)] private float attackInterval = 1f;
        [SerializeField, Min(0f)] private float meleeAttackRange = 0.9f;
        [SerializeField, Min(0f)] private float rangedAttackRange = 5f;

        public bool HasSkill => hasSkill;
        public bool CanRangedAttack => canRangedAttack;
        public float AttackInterval => attackInterval;
        public float AttackRange => canRangedAttack ? rangedAttackRange : meleeAttackRange;
    }
}
