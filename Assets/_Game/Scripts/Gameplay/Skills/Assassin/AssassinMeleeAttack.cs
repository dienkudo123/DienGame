using System.Collections.Generic;
using DienGame.Gameplay.Balance;
using DienGame.Client;
using DienGame.Gameplay.Equipment;
using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Player;
using DienGame.Gameplay.Phase13;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Assassin
{
    [RequireComponent(typeof(PlayerTargeting))]
    public sealed class AssassinMeleeAttack : MonoBehaviour
    {
        [Header("Melee arc (not a projectile)")]
        [SerializeField, Min(0.1f)] private float slashRange = 1.8f;
        [SerializeField, Range(1f, 360f)] private float slashAngle = 180f;
        [SerializeField] private LayerMask enemyLayers = ~0;

        private PlayerTargeting _targeting;
        private PlayerCharacter _character;
        private AssassinSpriteAnimator _animator;
        private AssassinCrescentVfx _vfx;
        private AssassinSkillController _assassinSkills;
        private float _cooldown;

        // These are cached across slashes: no new HashSet / physics result array per attack.
        private readonly List<Collider2D> _colliders = new List<Collider2D>(64);
        private readonly HashSet<EnemyHealth> _damagedEnemies = new HashSet<EnemyHealth>();
        private ContactFilter2D _enemyFilter;

        private void Awake()
        {
            _targeting = GetComponent<PlayerTargeting>();
            _character = GetComponent<PlayerCharacter>();
            _animator = GetComponentInChildren<AssassinSpriteAnimator>(true);
            _vfx = GetComponent<AssassinCrescentVfx>();
            _assassinSkills = GetComponent<AssassinSkillController>();

            _enemyFilter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = enemyLayers,
                useTriggers = false
            };
        }

        private void Update()
        {
            if (_cooldown > 0f)
                _cooldown -= Time.deltaTime;

            if (_cooldown > 0f || _targeting == null)
                return;

            Transform target = _targeting.CurrentTarget;
            if (target == null)
                return;

            Vector2 origin = transform.position;
            Vector2 forward = (Vector2)target.position - origin;
            float attackRange = ActiveRange();
            if (forward.sqrMagnitude < 0.0001f || forward.sqrMagnitude > attackRange * attackRange)
                return;

            forward.Normalize();
            Slash(origin, forward);
            _cooldown = _character != null
                ? Mathf.Max(0.05f, _character.AttackInterval)
                : GameBalance.PlayerAttackInterval;
        }

        private float ActiveRange()
        {
            EquipmentDefinition weapon = EquipmentInventory.Selected(PlayableClass.Assassin);
            if (weapon == null) return slashRange;
            switch (weapon.Id)
            {
                case "assassin_sickles": return slashRange * 1.12f;
                case "assassin_scythe": return slashRange * 1.30f;
                case "assassin_venom": return slashRange * .93f;
                default: return slashRange;
            }
        }

        private void Slash(Vector2 origin, Vector2 forward)
        {
            Phase13Audio.Play(Phase13Cue.Slash);
            _animator?.PlaySlash(forward);
            _vfx?.ShowSlash(forward);

            EquipmentDefinition weapon = EquipmentInventory.Selected(PlayableClass.Assassin);
            float range = ActiveRange();
            float angle = weapon != null && weapon.Id == "assassin_scythe" ? 245f :
                weapon != null && weapon.Id == "assassin_sickles" ? 205f :
                weapon != null && weapon.Id == "assassin_venom" ? 165f : slashAngle;
            _colliders.Clear();
            _damagedEnemies.Clear();
            Physics2D.OverlapCircle(origin, range, _enemyFilter, _colliders);

            float minDot = Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad);
            float damage = (_character != null ? _character.Damage : GameBalance.PlayerDamage) *
                (weapon != null ? weapon.DamageMultiplier : 1f) * EquipmentResonance.DamageFactor(weapon);
            bool hitAny = false;
            foreach (Collider2D hit in _colliders)
            {
                if (hit == null) continue;

                EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
                if (enemy == null || enemy.IsDead || !_damagedEnemies.Add(enemy))
                    continue;

                Vector2 delta = (Vector2)enemy.transform.position - origin;
                if (delta.sqrMagnitude > range * range)
                    continue;

                if (delta.sqrMagnitude > 0.0001f && Vector2.Dot(forward, delta.normalized) < minDot)
                    continue;

                float actual = _assassinSkills != null && _assassinSkills.isActiveAndEnabled
                    ? _assassinSkills.DamageAgainst(enemy, damage)
                    : damage;
                if (weapon != null)
                    EnemyEquipmentStatus.ApplyHit(enemy, weapon.Effect, actual);
                enemy.TakeDamage(actual);
                if (_assassinSkills != null && _assassinSkills.isActiveAndEnabled)
                    _assassinSkills.RegisterNormalHit(enemy, damage);
                hitAny = true;
            }

            if (hitAny && _assassinSkills != null && _assassinSkills.isActiveAndEnabled)
                _assassinSkills.RegisterSuccessfulSlash(origin, forward, damage);
        }
    }
}
