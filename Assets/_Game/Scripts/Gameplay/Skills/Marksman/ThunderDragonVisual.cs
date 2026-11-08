using System.Collections.Generic;
using DienGame.Gameplay.Enemy;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Marksman
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ThunderDragonVisual : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;

        private Sprite[] _frames;

        private int _frameIndex;

        private float _frameTimer;

        private float _framesPerSecond;

        private Vector2 _direction;

        private Vector2 _endPosition;

        private float _moveSpeed;

        private float _damage;

        private float _hitboxScale;

        private Vector2 _hitboxSize;

        private readonly HashSet<EnemyHealth>
            _hitEnemies = new();

        private void Awake()
        {
            _spriteRenderer =
                GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            UpdateAnimation();

            Move();

            CheckEnemyHits();
        }

        public void Initialize(
            Sprite[] frames,
            float animationFPS,
            Vector2 startPosition,
            Vector2 endPosition,
            float moveSpeed,
            float damage,
            float visualScale,
            float hitboxScale,
            int sortingOrder)
        {
            _frames = frames;

            _framesPerSecond =
                Mathf.Max(
                    1f,
                    animationFPS
                );

            _moveSpeed =
                Mathf.Max(
                    0.1f,
                    moveSpeed
                );

            _damage =
                Mathf.Max(
                    0f,
                    damage
                );

            _hitboxScale =
                Mathf.Max(
                    0.05f,
                    hitboxScale
                );

            _endPosition =
                endPosition;

            Vector2 direction =
                endPosition -
                startPosition;

            if (
                direction.sqrMagnitude <=
                0.0001f
            )
            {
                Destroy(gameObject);

                return;
            }

            _direction =
                direction.normalized;

            transform.position =
                startPosition;

            transform.localScale =
                Vector3.one *
                visualScale;

            float angle =
                Mathf.Atan2(
                    _direction.y,
                    _direction.x
                ) *
                Mathf.Rad2Deg;

            // Sprite rồng gốc hướng lên trên.
            transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    angle - 90f
                );

            _spriteRenderer.sortingOrder =
                sortingOrder;

            _frameIndex = 0;

            _frameTimer = 0f;

            _hitEnemies.Clear();

            if (
                _frames == null ||
                _frames.Length == 0
            )
            {
                Debug.LogWarning(
                    "ThunderDragonVisual: Chưa gán Dragon Frames.",
                    this
                );

                Destroy(gameObject);

                return;
            }

            _spriteRenderer.sprite =
                _frames[0];

            CalculateHitbox();
        }

        private void CalculateHitbox()
        {
            Sprite sprite =
                _spriteRenderer.sprite;

            if (sprite == null)
            {
                return;
            }

            Vector2 spriteSize =
                sprite.bounds.size;

            _hitboxSize =
                spriteSize *
                _hitboxScale *
                (Vector2)transform.localScale;
        }

        private void Move()
        {
            float moveDistance =
                _moveSpeed *
                Time.deltaTime;

            transform.position +=
                (Vector3)(
                    _direction *
                    moveDistance
                );

            Vector2 toEnd =
                _endPosition -
                (Vector2)transform.position;

            if (
                Vector2.Dot(
                    toEnd,
                    _direction
                ) <= 0f
            )
            {
                Destroy(gameObject);
            }
        }

        private void CheckEnemyHits()
        {
            float angle =
                transform.eulerAngles.z;

            Collider2D[] hits =
                Physics2D.OverlapBoxAll(
                    transform.position,
                    _hitboxSize,
                    angle
                );

            foreach (
                Collider2D hit
                in hits
            )
            {
                EnemyHealth enemy =
                    hit.GetComponentInParent<
                        EnemyHealth
                    >();

                if (
                    enemy == null ||
                    enemy.IsDead
                )
                {
                    continue;
                }

                if (
                    _hitEnemies.Contains(
                        enemy
                    )
                )
                {
                    continue;
                }

                _hitEnemies.Add(
                    enemy
                );

                enemy.TakeDamage(
                    _damage
                );
            }
        }

        private void UpdateAnimation()
        {
            if (
                _frames == null ||
                _frames.Length <= 1
            )
            {
                return;
            }

            float frameDuration =
                1f /
                _framesPerSecond;

            _frameTimer +=
                Time.deltaTime;

            while (
                _frameTimer >=
                frameDuration
            )
            {
                _frameTimer -=
                    frameDuration;

                _frameIndex++;

                if (
                    _frameIndex >=
                    _frames.Length
                )
                {
                    _frameIndex = 0;
                }

                _spriteRenderer.sprite =
                    _frames[_frameIndex];
            }
        }
    }
}