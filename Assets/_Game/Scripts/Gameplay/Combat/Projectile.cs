using System.Collections.Generic;
using DienGame.Gameplay.Core.Pooling;
using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Skills.Marksman;
using UnityEngine;

namespace DienGame.Gameplay.Combat
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(ProjectileSpriteAnimator))]
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
        private float _remainingLife;
        private bool _initialized;
        private bool _infinitePiercing;
        private int _piercingLevel;
        private Vector3 _originalScale;
        private ProjectileSpriteAnimator _spriteAnimator;
        private SpriteRenderer _spriteRenderer;
        private Sprite _defaultSprite;

        private readonly HashSet<EnemyHealth> _hitEnemies = new();

        private MarksmanSkillController _marksmanSkillController;
        private int _thunderLevel;
        private bool _thunderTriggered;

        private void Awake()
        {
            _originalScale = transform.localScale;
            _spriteAnimator = GetComponent<ProjectileSpriteAnimator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _defaultSprite = _spriteRenderer != null ? _spriteRenderer.sprite : null;
        }

        private void OnEnable()
        {
            _remainingLife = lifeTime;
            _hitEnemies.Clear();
            _thunderTriggered = false;
        }

        private void Update()
        {
            if (!_initialized)
            {
                return;
            }

            transform.position += (Vector3)(_direction * _moveSpeed * Time.deltaTime);

            _remainingLife -= Time.deltaTime;
            if (_remainingLife <= 0f)
            {
                ReleaseSelf();
            }
        }

        public void Initialize(
            Vector2 direction,
            float speed,
            float damage,
            MarksmanSkillController marksmanSkillController = null,
            int thunderLevel = 0)
        {
            _marksmanSkillController = marksmanSkillController;
            _thunderLevel = thunderLevel;

            InitializeInternal(direction, speed, damage, false, 0);
        }

        public void InitializePiercing(
            Vector2 direction,
            float speed,
            float damage,
            int piercingLevel)
        {
            _marksmanSkillController = null;
            _thunderLevel = 0;

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

            _direction = direction.normalized;
            _moveSpeed = Mathf.Max(0f, speed);
            _damage = Mathf.Max(0f, damage);
            _infinitePiercing = infinitePiercing;
            _piercingLevel = Mathf.Max(0, piercingLevel);
            _remainingLife = lifeTime;
            _hitEnemies.Clear();
            _thunderTriggered = false;

            SetupRotation();
            SetupVisual();
            _initialized = true;
        }

        private void SetupRotation()
        {
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void SetupVisual()
        {
            if (!_infinitePiercing)
            {
                transform.localScale = _originalScale;
                _spriteAnimator?.Stop(_defaultSprite);
                return;
            }

            bool maxForm = PiercingBulletSkill.IsMaxForm(_piercingLevel);
            float visualScale = PiercingBulletSkill.GetVisualScale(_piercingLevel);
            transform.localScale = _originalScale * visualScale;

            Sprite[] frames = maxForm ? piercingMaxFrames : piercingNormalFrames;
            float fps = maxForm ? maxAnimationFPS : normalAnimationFPS;

            if (frames == null || frames.Length == 0)
            {
                Debug.LogWarning(
                    maxForm
                        ? "Projectile: Chưa gán Piercing MAX Frames."
                        : "Projectile: Chưa gán Piercing Normal Frames.",
                    this
                );
                return;
            }

            _spriteAnimator?.Play(frames, fps);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_initialized)
            {
                return;
            }

            EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();

            if (enemy == null || enemy.IsDead || _hitEnemies.Contains(enemy))
            {
                return;
            }

            _hitEnemies.Add(enemy);

            if (!_infinitePiercing &&
                !_thunderTriggered &&
                _thunderLevel > 0 &&
                _marksmanSkillController != null)
            {
                _thunderTriggered = true;
                _marksmanSkillController.TriggerThunderFromHit(
                    enemy,
                    _damage,
                    _thunderLevel
                );
            }

            enemy.TakeDamage(_damage);

            if (!_infinitePiercing)
            {
                ReleaseSelf();
            }
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
            _thunderTriggered = false;
        }

        public void OnReturnedToPool()
        {
            _initialized = false;
            _direction = Vector2.zero;
            _moveSpeed = 0f;
            _damage = 0f;
            _infinitePiercing = false;
            _piercingLevel = 0;
            _marksmanSkillController = null;
            _thunderLevel = 0;
            _thunderTriggered = false;
            _hitEnemies.Clear();

            transform.localScale = _originalScale;
            transform.rotation = Quaternion.identity;
            _spriteAnimator?.Stop(_defaultSprite);
        }
    }
}
