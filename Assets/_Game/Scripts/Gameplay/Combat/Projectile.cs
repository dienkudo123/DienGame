using System.Collections.Generic;
using DienGame.Client;
using DienGame.Gameplay.Equipment;
using DienGame.Gameplay.Core.Pooling;
using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Map;
using DienGame.Gameplay.Skills.Marksman;
using UnityEngine;

namespace DienGame.Gameplay.Combat
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(ProjectileSpriteAnimator))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Projectile : MonoBehaviour, IPoolLifecycle
    {
        [Header("Lifetime")]
        [SerializeField, Min(0.1f)] private float lifeTime = 5f;

        [Header("Piercing Normal Frames")]
        [SerializeField] private Sprite[] piercingNormalFrames;

        [Header("Piercing MAX Frames")]
        [SerializeField] private Sprite[] piercingMaxFrames;

        [Header("Piercing Animation")]
        [SerializeField, Min(1f)] private float normalAnimationFPS = 12f;
        [SerializeField, Min(1f)] private float maxAnimationFPS = 10f;

        private Vector2 _direction;
        private float _moveSpeed;
        private float _damage;
        private float _baseDamage;
        private float _remainingLife;
        private bool _initialized;
        private bool _infinitePiercing;
        private int _piercingLevel;

        private Rigidbody2D _rigidbody;
        private ProjectileSpriteAnimator _spriteAnimator;
        private SpriteRenderer _rootSpriteRenderer;
        private SpriteRenderer _visualSpriteRenderer;
        private ProjectileVisual _projectileVisual;
        private Sprite _defaultSprite;
        private Color _defaultVisualColor = Color.white;
        private Vector3 _defaultVisualScale = Vector3.one;
        private bool _visualStateCaptured;

        private readonly HashSet<EnemyHealth> _hitEnemies = new();
        private static readonly List<Collider2D> s_ricochetSearchResults = new(32);

        private MarksmanSkillController _marksmanSkillController;
        private int _thunderLevel;
        private int _ricochetLevel;
        private int _ricochetRemaining;
        private EquipmentDefinition _equippedRound;
        private int _equipmentPiercesRemaining;
        private bool _equipmentSplashTriggered;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _spriteAnimator = GetComponent<ProjectileSpriteAnimator>();
            _rootSpriteRenderer = GetComponent<SpriteRenderer>();

            ConfigureRigidbody();
            ResolveVisualRenderer();
        }

        private void OnEnable()
        {
            _remainingLife = lifeTime;
            _hitEnemies.Clear();
        }

        private void Update()
        {
            if (!_initialized)
            {
                return;
            }

            _remainingLife -= Time.deltaTime;

            if (_remainingLife <= 0f)
            {
                ReleaseSelf();
            }
        }

        private void FixedUpdate()
        {
            if (!_initialized || _rigidbody == null)
            {
                return;
            }

            Vector2 nextPosition =
                _rigidbody.position +
                _direction * (_moveSpeed * Time.fixedDeltaTime);

            _rigidbody.MovePosition(nextPosition);
        }

        public void Initialize(
            Vector2 direction,
            float speed,
            float damage,
            MarksmanSkillController marksmanSkillController = null,
            int thunderLevel = 0,
            int ricochetLevel = 0)
        {
            _marksmanSkillController = marksmanSkillController;
            _thunderLevel = Mathf.Max(0, thunderLevel);
            _ricochetLevel = Mathf.Max(0, ricochetLevel);
            _ricochetRemaining = RicochetBulletSkill.GetBounceCount(_ricochetLevel);
            _equippedRound = EquipmentInventory.Selected(PlayableClass.Marksman);
            _equipmentPiercesRemaining = _equippedRound != null &&
                _equippedRound.Effect == EquipmentEffect.Piercing ? 1 : 0;

            InitializeInternal(direction, speed,
                damage * (_equippedRound != null ? _equippedRound.DamageMultiplier : 1f)
                    * EquipmentResonance.DamageFactor(_equippedRound), false, 0);
        }

        public void InitializePiercing(
            Vector2 direction,
            float speed,
            float damage,
            int piercingLevel)
        {
            _marksmanSkillController = null;
            _thunderLevel = 0;
            _ricochetLevel = 0;
            _ricochetRemaining = 0;
            _equippedRound = null; // Active skill piercing keeps its original animation and tuning.
            _equipmentPiercesRemaining = 0;

            InitializeInternal(direction, speed, damage, true, piercingLevel);
        }

        private void InitializeInternal(
            Vector2 direction,
            float speed,
            float damage,
            bool infinitePiercing,
            int piercingLevel)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                ReleaseSelf();
                return;
            }

            CaptureDefaultVisualStateIfNeeded();

            _direction = direction.normalized;
            _moveSpeed = Mathf.Max(0f, speed);
            _damage = Mathf.Max(0f, damage);
            _baseDamage = _damage;
            _infinitePiercing = infinitePiercing;
            _piercingLevel = Mathf.Max(0, piercingLevel);
            _remainingLife = lifeTime;
            _hitEnemies.Clear();
            _equipmentSplashTriggered = false;

            SetupRotation();
            SetupVisual();
            _initialized = true;
        }

        private void ConfigureRigidbody()
        {
            if (_rigidbody == null)
            {
                return;
            }

            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody.gravityScale = 0f;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rigidbody.freezeRotation = true;
            _rigidbody.linearVelocity = Vector2.zero;
        }

        private void ResolveVisualRenderer()
        {
            _projectileVisual = GetComponentInChildren<ProjectileVisual>(true);

            if (_projectileVisual != null)
            {
                _projectileVisual.RestoreVisualState();
                _visualSpriteRenderer = _projectileVisual.GetComponent<SpriteRenderer>();
            }

            if (_visualSpriteRenderer == null)
            {
                SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);

                for (int i = 0; i < renderers.Length; i++)
                {
                    SpriteRenderer candidate = renderers[i];

                    if (candidate != null && candidate != _rootSpriteRenderer)
                    {
                        _visualSpriteRenderer = candidate;
                        break;
                    }
                }
            }

            if (_visualSpriteRenderer == null)
            {
                _visualSpriteRenderer = _rootSpriteRenderer;
            }

            if (_rootSpriteRenderer != null && _rootSpriteRenderer != _visualSpriteRenderer)
            {
                _rootSpriteRenderer.enabled = false;
                _rootSpriteRenderer.sprite = null;
            }

            if (_visualSpriteRenderer != null)
            {
                _spriteAnimator?.SetTarget(_visualSpriteRenderer);
            }
        }

        private void CaptureDefaultVisualStateIfNeeded()
        {
            if (_visualStateCaptured)
            {
                return;
            }

            ResolveVisualRenderer();
            _projectileVisual?.RestoreVisualState();

            if (_visualSpriteRenderer != null)
            {
                _defaultSprite = _visualSpriteRenderer.sprite;
                _defaultVisualColor = _visualSpriteRenderer.color;
                _defaultVisualScale = _visualSpriteRenderer.transform.localScale;
            }

            _visualStateCaptured = true;
        }

        private void SetupRotation()
        {
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            _projectileVisual?.SetDirection(_direction);
        }

        private void SetupVisual()
        {
            if (_visualSpriteRenderer == null)
            {
                return;
            }

            if (!_infinitePiercing)
            {
                _spriteAnimator?.Stop(_defaultSprite, _defaultVisualScale);
                _projectileVisual?.RestoreVisualState();
                if (_equippedRound != null)
                {
                    Sprite skin = EquipmentEvolutionArt.ProjectileFor(_equippedRound);
                    if (skin != null)
                    {
                        _visualSpriteRenderer.sprite = skin;
                        _visualSpriteRenderer.color = EquipmentResonance.VisualTint(_equippedRound);
                        // The prefab's native Visual scale is 0.011 for a giant atlas.
                        // Isolated equipment sprites are ~240px @260PPU: use a
                        // dedicated scale on the child only, never the root collider.
                        if (_visualSpriteRenderer != _rootSpriteRenderer)
                            _visualSpriteRenderer.transform.localScale = Vector3.one * .45f;
                    }
                }
                return;
            }

            bool maxForm = PiercingBulletSkill.IsMaxForm(_piercingLevel);
            float visualScale = PiercingBulletSkill.GetVisualScale(_piercingLevel);
            Sprite[] frames = maxForm ? piercingMaxFrames : piercingNormalFrames;
            float fps = maxForm ? maxAnimationFPS : normalAnimationFPS;

            if (frames == null || frames.Length == 0)
            {
                _spriteAnimator?.Stop(_defaultSprite, _defaultVisualScale);

                Debug.LogWarning(
                    maxForm
                        ? "Projectile: Chưa gán Piercing MAX Frames."
                        : "Projectile: Chưa gán Piercing Normal Frames.",
                    this
                );
                return;
            }

            Vector3 piercingVisualScale = new Vector3(
                visualScale,
                visualScale,
                1f
            );

            // Chỉ scale child Visual. Root/collider luôn giữ scale cố định,
            // nên frame có rect khác nhau không làm hitbox hoặc normal bullet co giãn.
            _spriteAnimator?.Play(
                frames,
                fps,
                true,
                piercingVisualScale
            );
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_initialized || other == null)
            {
                return;
            }

            MapObstacle obstacle = other.GetComponentInParent<MapObstacle>();

            if (obstacle != null && obstacle.BlockProjectile)
            {
                ReleaseSelf();
                return;
            }

            EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();

            // Add returns false for targets already hit by this projectile.
            // This avoids performing Contains() and Add() for the same collision.
            if (enemy == null || enemy.IsDead || !_hitEnemies.Add(enemy))
            {
                return;
            }

            // Thunder flag được gắn vào đúng viên đạn đã đủ shot-counter.
            // Ricochet giữ flag này, nhưng bản thân bounce không tăng counter.
            if (!_infinitePiercing &&
                _thunderLevel > 0 &&
                _marksmanSkillController != null)
            {
                _marksmanSkillController.TriggerThunderFromHit(
                    enemy,
                    _baseDamage,
                    _thunderLevel
                );
            }

            // Apply exactly one equipment effect per target, without stacking poison.
            if (!_infinitePiercing && _equippedRound != null)
            {
                // Ricochet and equipment pierce do not spawn several explosions
                // from one projectile; area damage has a strict one-hit budget.
                bool splash = _equippedRound.Effect == EquipmentEffect.Splash;
                if (!splash || !_equipmentSplashTriggered)
                    EnemyEquipmentStatus.ApplyHit(enemy, _equippedRound.Effect, _damage);
                if (splash) _equipmentSplashTriggered = true;
            }
            enemy.TakeDamage(_damage);

            if (_infinitePiercing)
            {
                return;
            }

            if (_equipmentPiercesRemaining > 0)
            {
                _equipmentPiercesRemaining--;
                _damage = _baseDamage * .60f;
                return;
            }
            if (TryContinueRicochet())
            {
                return;
            }

            ReleaseSelf();
        }

        private bool TryContinueRicochet()
        {
            if (_ricochetLevel <= 0 || _ricochetRemaining <= 0)
            {
                return false;
            }

            Vector3 searchOrigin = transform.position;
            EnemyHealth nextEnemy = FindNearestRicochetTarget(
                searchOrigin,
                RicochetBulletSkill.BounceRange
            );

            if (nextEnemy == null)
            {
                return false;
            }

            Vector2 nextDirection =
                (Vector2)nextEnemy.transform.position -
                (Vector2)transform.position;

            if (nextDirection.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            _ricochetRemaining--;
            _damage = _baseDamage * RicochetBulletSkill.GetBounceDamageMultiplier(_ricochetLevel);
            _direction = nextDirection.normalized;

            // Không teleport vào tâm enemy cũ. Giữ viên đạn tại điểm va chạm
            // để đường ricochet nhìn liền mạch và tránh nhảy trên enemy lớn.
            if (_rigidbody != null)
            {
                _rigidbody.position += _direction * 0.03f;
            }
            else
            {
                transform.position += (Vector3)(_direction * 0.03f);
            }

            SetupRotation();
            return true;
        }

        private EnemyHealth FindNearestRicochetTarget(Vector3 origin, float maxRange)
        {
            s_ricochetSearchResults.Clear();

            ContactFilter2D filter = new ContactFilter2D();
            filter.NoFilter();

            Physics2D.OverlapCircle(
                origin,
                maxRange,
                filter,
                s_ricochetSearchResults
            );

            EnemyHealth nearest = null;
            float nearestDistanceSquared = maxRange * maxRange;

            for (int i = 0; i < s_ricochetSearchResults.Count; i++)
            {
                Collider2D hit = s_ricochetSearchResults[i];

                if (hit == null)
                {
                    continue;
                }

                EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();

                if (enemy == null || enemy.IsDead || _hitEnemies.Contains(enemy))
                {
                    continue;
                }

                float distanceSquared =
                    (enemy.transform.position - origin).sqrMagnitude;

                if (distanceSquared > nearestDistanceSquared)
                {
                    continue;
                }

                nearestDistanceSquared = distanceSquared;
                nearest = enemy;
            }

            return nearest;
        }

        private void ReleaseSelf()
        {
            if (!_initialized && !gameObject.activeSelf)
            {
                return;
            }

            _initialized = false;
            GameObjectPoolService.Release(gameObject);
        }

        public void OnSpawnedFromPool()
        {
            _initialized = false;
            _remainingLife = lifeTime;
            _hitEnemies.Clear();

            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector2.zero;
            }

            CaptureDefaultVisualStateIfNeeded();
            RestoreDefaultVisual();
        }

        public void OnReturnedToPool()
        {
            _initialized = false;
            _direction = Vector2.zero;
            _moveSpeed = 0f;
            _damage = 0f;
            _baseDamage = 0f;
            _infinitePiercing = false;
            _piercingLevel = 0;
            _marksmanSkillController = null;
            _thunderLevel = 0;
            _ricochetLevel = 0;
            _ricochetRemaining = 0;
            _hitEnemies.Clear();
            _equippedRound = null;
            _equipmentPiercesRemaining = 0;
            _equipmentSplashTriggered = false;

            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector2.zero;
            }

            transform.localScale = Vector3.one;
            transform.rotation = Quaternion.identity;
            RestoreDefaultVisual();
        }

        private void RestoreDefaultVisual()
        {
            if (!_visualStateCaptured)
            {
                return;
            }

            _spriteAnimator?.Stop(_defaultSprite, _defaultVisualScale);
            _projectileVisual?.RestoreVisualState();
            // Projectile pooling must never leak item-color tint into the next bullet/active skill.
            if (_visualSpriteRenderer != null) _visualSpriteRenderer.color = _defaultVisualColor;

            if (_rootSpriteRenderer != null && _rootSpriteRenderer != _visualSpriteRenderer)
            {
                _rootSpriteRenderer.enabled = false;
                _rootSpriteRenderer.sprite = null;
            }
        }
    }
}
