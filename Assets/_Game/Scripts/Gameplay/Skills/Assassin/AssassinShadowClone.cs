using System.Collections.Generic;
using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Skills;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Assassin
{
    /// <summary>
    /// Anh Phan Than / Shadow Clone.
    /// - Stationary, lasts exactly two scaled game seconds.
    /// - Own attack timer and passive state; shares the player's snapshotted combat stats.
    /// - Never spawns another clone and never uses the active wall hook.
    /// - Lv5: one last 180-degree slash (50% ATK) just before vanishing.
    /// This replaces the existing file WITHOUT changing AssassinSkillController.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AssassinShadowClone : MonoBehaviour
    {
        private const float Lifetime = 2f;
        private const float AppearanceSeconds = 0.12f;
        private const float DisappearanceSeconds = 0.16f;
        private const float FinisherLeadSeconds = 0.145f;
        private const float FinisherAttackRatio = 0.50f;
        private const float Epsilon = 0.0001f;

        private readonly List<Collider2D> _hits = new List<Collider2D>(48);
        private readonly HashSet<EnemyHealth> _unique = new HashSet<EnemyHealth>();
        private readonly AssassinSkillController.AttackState _state =
            new AssassinSkillController.AttackState();

        private ContactFilter2D _filter;
        private AssassinSkillController _owner;
        private AssassinSpriteAnimator _animator;
        private AssassinCrescentVfx _slashVfx;
        private SpriteRenderer _renderer;
        private Color _baseTint;
        private int[] _levels;
        private float _damage;
        private float _attackInterval;
        private float _range;
        private float _remaining;
        private float _attackTimer;
        private Vector2 _lastFacing = Vector2.down;
        private bool _finisherTriggered;
        private bool _initialized;

        // This signature is kept exactly compatible with existing SpawnClone().
        public void Initialize(
            AssassinSkillController owner,
            AssassinSpriteAnimator animator,
            AssassinCrescentVfx slashVfx,
            int[] levels,
            float damage,
            float attackInterval,
            float range)
        {
            _owner = owner;
            _animator = animator;
            _slashVfx = slashVfx;
            _levels = levels;
            _damage = Mathf.Max(0f, damage);
            _attackInterval = Mathf.Max(0.05f, attackInterval);
            _range = Mathf.Max(0.1f, range);
            _remaining = Lifetime;
            _attackTimer = 0f;
            _finisherTriggered = false;
            _initialized = true;

            _filter = new ContactFilter2D
            {
                useLayerMask = false,
                useTriggers = false
            };

            _renderer = animator != null ? animator.GetComponent<SpriteRenderer>() : null;
            _baseTint = _renderer != null
                ? _renderer.color
                : new Color(0.65f, 0.42f, 1f, 0.8f);

            SetOpacity(0f);
            AssassinCloneSkillVfx.PlaySpawn((Vector2)transform.position, _renderer);
        }

        private void Update()
        {
            if (!_initialized) return;
            if (_owner == null || _levels == null)
            {
                Destroy(gameObject);
                return;
            }

            // Unity's deltaTime stops on pause / level-up; clone time stops as well.
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            _remaining = Mathf.Max(0f, _remaining - dt);
            UpdateOpacity();

            if (_remaining <= 0f)
            {
                // Protection for a frame longer than the finisher lead window.
                if (HasLevelFive && !_finisherTriggered)
                    ExecuteFinalSlash();
                Destroy(gameObject);
                return;
            }

            if (HasLevelFive && !_finisherTriggered &&
                _remaining <= FinisherLeadSeconds)
            {
                ExecuteFinalSlash();
            }

            // No additional ordinary attack in the last sliver of its lifetime:
            // the Lv5 finisher has priority and the animation remains readable.
            if (_remaining <= FinisherLeadSeconds) return;

            // Inherited non-clone passives: knife, orbit, their independent timers.
            _owner.TickActor(_state, transform.position, transform,
                _levels, _damage, dt);

            _attackTimer -= dt;
            if (_attackTimer > 0f) return;

            EnemyHealth target = FindNearestEnemy(_range);
            if (target == null) return;

            Vector2 direction = (Vector2)target.transform.position -
                                (Vector2)transform.position;
            if (direction.sqrMagnitude <= Epsilon) return;

            _lastFacing = direction.normalized;
            PerformNormalSlash(_lastFacing);
            _attackTimer = _attackInterval;
        }

        private bool HasLevelFive
        {
            get
            {
                int index = (int)SkillId.AssassinShadowClone -
                            (int)SkillId.AssassinBloodSlash;
                return _levels != null && index >= 0 &&
                       index < _levels.Length && _levels[index] >= 5;
            }
        }

        private void UpdateOpacity()
        {
            float elapsed = Lifetime - _remaining;
            float enter = Mathf.Clamp01(elapsed / AppearanceSeconds);
            float exit = Mathf.Clamp01(_remaining / DisappearanceSeconds);
            SetOpacity(Mathf.Min(enter, exit));
        }

        private void SetOpacity(float factor)
        {
            if (_renderer == null) return;
            Color tint = _baseTint;
            tint.a = _baseTint.a * Mathf.Clamp01(factor);
            _renderer.color = tint;
        }

        private EnemyHealth FindNearestEnemy(float maxRange)
        {
            _hits.Clear();
            Physics2D.OverlapCircle(transform.position, maxRange,
                _filter, _hits);

            EnemyHealth nearest = null;
            float bestDistanceSquared = maxRange * maxRange;
            Vector2 origin = transform.position;

            for (int i = 0; i < _hits.Count; i++)
            {
                Collider2D hit = _hits[i];
                if (hit == null) continue;

                EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
                if (enemy == null || enemy.IsDead) continue;

                float distanceSquared =
                    ((Vector2)enemy.transform.position - origin).sqrMagnitude;
                if (distanceSquared > bestDistanceSquared) continue;

                nearest = enemy;
                bestDistanceSquared = distanceSquared;
            }

            return nearest;
        }

        private void PerformNormalSlash(Vector2 direction)
        {
            _animator?.PlaySlash(direction);
            _slashVfx?.ShowSlash(direction);

            bool connected = DealSlashDamage(direction, _range,
                _damage, true);

            // Each clone has its OWN 3-successful-slashes counter.
            if (connected)
            {
                _owner.RegisterSuccessfulSlash((Vector2)transform.position,
                    direction, _damage, _state, _levels);
            }
        }

        /// <param name="canBuildStacks">
        /// Only normal attacks can add Death Marks; a finisher is bonus damage.
        /// </param>
        private bool DealSlashDamage(Vector2 direction, float range,
            float damage, bool canBuildStacks)
        {
            _hits.Clear();
            _unique.Clear();
            Physics2D.OverlapCircle(transform.position, range,
                _filter, _hits);

            Vector2 origin = transform.position;
            float rangeSquared = range * range;
            bool connected = false;

            for (int i = 0; i < _hits.Count; i++)
            {
                Collider2D hit = _hits[i];
                if (hit == null) continue;

                EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
                if (enemy == null || enemy.IsDead || !_unique.Add(enemy))
                    continue;

                Vector2 offset = (Vector2)enemy.transform.position - origin;
                if (offset.sqrMagnitude > rangeSquared) continue;
                if (Vector2.Dot(direction, offset) < 0f) continue;

                // The clone snapshots the player's ATK and its unlocked passives.
                float actualDamage = _owner.DamageAgainst(enemy, damage, _levels);
                if (actualDamage <= 0f) continue;

                enemy.TakeDamage(actualDamage);
                if (canBuildStacks)
                    _owner.RegisterNormalHit(enemy, _damage, _levels);
                connected = true;
            }

            return connected;
        }

        private void ExecuteFinalSlash()
        {
            if (_finisherTriggered) return;
            _finisherTriggered = true;

            EnemyHealth nearest = FindNearestEnemy(_range);
            if (nearest != null)
            {
                Vector2 delta = (Vector2)nearest.transform.position -
                                (Vector2)transform.position;
                if (delta.sqrMagnitude > Epsilon)
                    _lastFacing = delta.normalized;
            }

            // Independent burst VFX survives the clone's own destruction.
            AssassinCloneSkillVfx.PlayFinisher(
                (Vector2)transform.position, _lastFacing, _renderer);
            _animator?.PlaySlash(_lastFacing);

            // Lv5 bonus, hits ALL enemies in front within the ordinary melee arc.
            // Does NOT add marks, increment combos or spawn further passives.
            DealSlashDamage(_lastFacing, _range,
                _damage * FinisherAttackRatio, false);
        }
    }
}
