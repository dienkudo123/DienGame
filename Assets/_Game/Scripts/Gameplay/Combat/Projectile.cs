using System.Collections.Generic;
using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Skills.Marksman;
using UnityEngine;

namespace DienGame.Gameplay.Combat
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(ProjectileSpriteAnimator))]
    public sealed class Projectile : MonoBehaviour
    {
        [Header("Lifetime")]
        [SerializeField, Min(0.1f)]
        private float lifeTime = 5f;

        [Header("Piercing Normal Frames")]
        [SerializeField]
        private Sprite[] piercingNormalFrames;

        [Header("Piercing MAX Frames")]
        [SerializeField]
        private Sprite[] piercingMaxFrames;

        [Header("Piercing Animation")]
        [SerializeField, Min(1f)]
        private float normalAnimationFPS = 12f;

        [SerializeField, Min(1f)]
        private float maxAnimationFPS = 10f;

        private Vector2 _direction;
        private float _moveSpeed;
        private float _damage;
        private float _remainingLife;

        private bool _initialized;
        private bool _infinitePiercing;

        private int _piercingLevel;

        private Vector3 _originalScale;

        private ProjectileSpriteAnimator _spriteAnimator;

        private readonly HashSet<EnemyHealth> _hitEnemies = new();

        // =========================================
        // THUNDER
        // =========================================

        private MarksmanSkillController _marksmanSkillController;

        private int _thunderLevel;

        private bool _thunderTriggered;

        private void Awake()
        {
            _originalScale = transform.localScale;

            _spriteAnimator =
                GetComponent<ProjectileSpriteAnimator>();
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

            Move();
            UpdateLifetime();
        }

        // =========================================
        // NORMAL BULLET
        // =========================================

        public void Initialize(
            Vector2 direction,
            float speed,
            float damage,
            MarksmanSkillController marksmanSkillController = null,
            int thunderLevel = 0)
        {
            _marksmanSkillController =
                marksmanSkillController;

            _thunderLevel =
                thunderLevel;

            InitializeInternal(
                direction,
                speed,
                damage,
                false,
                0
            );
        }

        // =========================================
        // PIERCING BULLET
        // =========================================

        public void InitializePiercing(
            Vector2 direction,
            float speed,
            float damage,
            int piercingLevel)
        {
            _marksmanSkillController = null;
            _thunderLevel = 0;

            InitializeInternal(
                direction,
                speed,
                damage,
                true,
                piercingLevel
            );
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
                Destroy(gameObject);
                return;
            }

            _direction =
                direction.normalized;

            _moveSpeed =
                Mathf.Max(0f, speed);

            _damage =
                Mathf.Max(0f, damage);

            _infinitePiercing =
                infinitePiercing;

            _piercingLevel =
                Mathf.Max(0, piercingLevel);

            _remainingLife =
                lifeTime;

            _hitEnemies.Clear();

            _thunderTriggered = false;

            SetupRotation();
            SetupVisual();

            _initialized = true;
        }

        private void SetupRotation()
        {
            float angle =
                Mathf.Atan2(
                    _direction.y,
                    _direction.x
                ) *
                Mathf.Rad2Deg;

            transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    angle
                );
        }

        private void SetupVisual()
        {
            if (!_infinitePiercing)
            {
                transform.localScale =
                    _originalScale;

                return;
            }

            bool maxForm =
                PiercingBulletSkill.IsMaxForm(
                    _piercingLevel
                );

            float visualScale =
                PiercingBulletSkill.GetVisualScale(
                    _piercingLevel
                );

            transform.localScale =
                _originalScale *
                visualScale;

            Sprite[] frames =
                maxForm
                    ? piercingMaxFrames
                    : piercingNormalFrames;

            float fps =
                maxForm
                    ? maxAnimationFPS
                    : normalAnimationFPS;

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

            if (_spriteAnimator != null)
            {
                _spriteAnimator.Play(
                    frames,
                    fps
                );
            }
        }

        private void Move()
        {
            Vector2 movement =
                _direction *
                _moveSpeed *
                Time.deltaTime;

            transform.position +=
                (Vector3)movement;
        }

        private void UpdateLifetime()
        {
            _remainingLife -=
                Time.deltaTime;

            if (_remainingLife <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(
            Collider2D other)
        {
            EnemyHealth enemy =
                other.GetComponentInParent<EnemyHealth>();

            if (enemy == null || enemy.IsDead)
            {
                return;
            }

            if (_hitEnemies.Contains(enemy))
            {
                return;
            }

            _hitEnemies.Add(enemy);

            // =====================================
            // LÔI ĐẠN
            // =====================================
            //
            // Quan trọng:
            // Kích hoạt Thunder TRƯỚC khi viên đạn
            // giết Enemy 1.
            //
            // Nhờ vậy dù viên đạn one-shot Enemy 1,
            // chuỗi sét vẫn tiếp tục sang Enemy 2.
            // =====================================

            if (
                !_infinitePiercing &&
                !_thunderTriggered &&
                _thunderLevel > 0 &&
                _marksmanSkillController != null
            )
            {
                _thunderTriggered = true;

                _marksmanSkillController
                    .TriggerThunderFromHit(
                        enemy,
                        _damage,
                        _thunderLevel
                    );
            }

            // Damage của chính viên đạn.
            enemy.TakeDamage(
                _damage
            );

            // Piercing không biến mất.
            if (_infinitePiercing)
            {
                return;
            }

            Destroy(gameObject);
        }
    }
}