using System;
using UnityEngine;

namespace DienGame.Gameplay.Enemy
{
    [Serializable]
    public sealed class EnemyDefinition
    {
        [Header("Identity")]
        [SerializeField] private EnemyArchetype archetype;
        [SerializeField] private string displayName;
        [SerializeField] private EnemyAttackKind attackKind;

        [Header("Base Stats")]
        [SerializeField, Min(1f)] private float maxHealth = 30f;
        [SerializeField, Min(0f)] private float damage = 8f;
        [SerializeField, Min(0f)] private float moveSpeed = 2f;
        [SerializeField, Min(0)] private int experience = 1;

        [Header("Movement / Attack")]
        [SerializeField, Min(0f)] private float preferredDistance = 0.8f;
        [SerializeField, Min(0f)] private float retreatDistance;
        [SerializeField, Min(0.05f)] private float attackInterval = 1f;
        [SerializeField, Min(0f)] private float projectileSpeed = 7f;
        [SerializeField, Min(0f)] private float maxAttackDistance = 8f;

        [Header("Visual")]
        [SerializeField] private EnemyVisualSet visual = new();
        [SerializeField] private EnemyVisualSet eliteVisual = new();

        public EnemyArchetype Archetype => archetype;
        public string DisplayName => displayName;
        public EnemyAttackKind AttackKind => attackKind;
        public float MaxHealth => maxHealth;
        public float Damage => damage;
        public float MoveSpeed => moveSpeed;
        public int Experience => experience;
        public float PreferredDistance => preferredDistance;
        public float RetreatDistance => retreatDistance;
        public float AttackInterval => attackInterval;
        public float ProjectileSpeed => projectileSpeed;
        public float MaxAttackDistance => maxAttackDistance;
        public EnemyVisualSet Visual => visual;
        public EnemyVisualSet EliteVisual => eliteVisual;

        public EnemyVisualSet GetVisual(bool isElite)
        {
            if (isElite && eliteVisual != null && eliteVisual.HasFrames)
            {
                return eliteVisual;
            }

            return visual;
        }

        public void Configure(
            EnemyArchetype newArchetype,
            string newDisplayName,
            EnemyAttackKind newAttackKind,
            float newMaxHealth,
            float newDamage,
            float newMoveSpeed,
            int newExperience,
            float newPreferredDistance,
            float newRetreatDistance,
            float newAttackInterval,
            float newProjectileSpeed,
            float newMaxAttackDistance,
            EnemyVisualSet newVisual,
            EnemyVisualSet newEliteVisual)
        {
            archetype = newArchetype;
            displayName = newDisplayName;
            attackKind = newAttackKind;
            maxHealth = Mathf.Max(1f, newMaxHealth);
            damage = Mathf.Max(0f, newDamage);
            moveSpeed = Mathf.Max(0f, newMoveSpeed);
            experience = Mathf.Max(0, newExperience);
            preferredDistance = Mathf.Max(0f, newPreferredDistance);
            retreatDistance = Mathf.Clamp(newRetreatDistance, 0f, preferredDistance);
            attackInterval = Mathf.Max(0.05f, newAttackInterval);
            projectileSpeed = Mathf.Max(0f, newProjectileSpeed);
            maxAttackDistance = Mathf.Max(0f, newMaxAttackDistance);
            visual = newVisual ?? new EnemyVisualSet();
            eliteVisual = newEliteVisual ?? new EnemyVisualSet();
        }
    }
}
