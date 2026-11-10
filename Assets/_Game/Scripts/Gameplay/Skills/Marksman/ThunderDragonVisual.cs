using System.Collections.Generic;
using DienGame.Gameplay.Enemy;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Marksman
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ThunderDragonVisual : MonoBehaviour
    {
        private static readonly Stack<ThunderDragonVisual> Pool = new();
        private static Transform s_poolRoot;

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
        private bool _inPool;

        private readonly HashSet<EnemyHealth> _hitEnemies = new();
        private readonly List<Collider2D> _physicsResults = new(32);
        private ContactFilter2D _physicsFilter;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Pool.Clear();
            s_poolRoot = null;
        }

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _physicsFilter = new ContactFilter2D();
            _physicsFilter.NoFilter();
        }

        private void Update()
        {
            UpdateAnimation();
            Move();
            CheckEnemyHits();
        }

        public static ThunderDragonVisual Spawn(
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
            ThunderDragonVisual dragon = Acquire();
            dragon.Initialize(
                frames,
                animationFPS,
                startPosition,
                endPosition,
                moveSpeed,
                damage,
                visualScale,
                hitboxScale,
                sortingOrder
            );
            return dragon;
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
            _framesPerSecond = Mathf.Max(1f, animationFPS);
            _moveSpeed = Mathf.Max(0.1f, moveSpeed);
            _damage = Mathf.Max(0f, damage);
            _hitboxScale = Mathf.Max(0.05f, hitboxScale);
            _endPosition = endPosition;

            Vector2 direction = endPosition - startPosition;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                Release();
                return;
            }

            _direction = direction.normalized;
            transform.position = startPosition;
            transform.localScale = Vector3.one * visualScale;

            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

            _spriteRenderer.sortingOrder = sortingOrder;
            _frameIndex = 0;
            _frameTimer = 0f;
            _hitEnemies.Clear();

            if (_frames == null || _frames.Length == 0)
            {
                Release();
                return;
            }

            _spriteRenderer.sprite = _frames[0];
            CalculateHitbox();
        }

        private void CalculateHitbox()
        {
            Sprite sprite = _spriteRenderer.sprite;
            if (sprite == null)
            {
                _hitboxSize = Vector2.one;
                return;
            }

            _hitboxSize = sprite.bounds.size * _hitboxScale * (Vector2)transform.localScale;
        }

        private void Move()
        {
            transform.position += (Vector3)(_direction * _moveSpeed * Time.deltaTime);

            Vector2 toEnd = _endPosition - (Vector2)transform.position;
            if (Vector2.Dot(toEnd, _direction) <= 0f)
            {
                Release();
            }
        }

        private void CheckEnemyHits()
        {
            if (_inPool)
            {
                return;
            }

            _physicsResults.Clear();

            Physics2D.OverlapBox(
                transform.position,
                _hitboxSize,
                transform.eulerAngles.z,
                _physicsFilter,
                _physicsResults
            );

            for (int i = 0; i < _physicsResults.Count; i++)
            {
                Collider2D hit = _physicsResults[i];
                if (hit == null)
                {
                    continue;
                }

                EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
                if (enemy == null || enemy.IsDead || _hitEnemies.Contains(enemy))
                {
                    continue;
                }

                _hitEnemies.Add(enemy);
                enemy.TakeDamage(_damage);
            }
        }

        private void UpdateAnimation()
        {
            if (_frames == null || _frames.Length <= 1)
            {
                return;
            }

            float frameDuration = 1f / _framesPerSecond;
            _frameTimer += Time.deltaTime;

            while (_frameTimer >= frameDuration)
            {
                _frameTimer -= frameDuration;
                _frameIndex = (_frameIndex + 1) % _frames.Length;
                _spriteRenderer.sprite = _frames[_frameIndex];
            }
        }

        private static ThunderDragonVisual Acquire()
        {
            ThunderDragonVisual dragon = null;

            while (Pool.Count > 0 && dragon == null)
            {
                dragon = Pool.Pop();
            }

            if (dragon == null)
            {
                GameObject go = new GameObject("ThunderDragon_MAX");
                go.AddComponent<SpriteRenderer>();
                dragon = go.AddComponent<ThunderDragonVisual>();
            }

            dragon._inPool = false;
            dragon.transform.SetParent(null, false);
            dragon.gameObject.SetActive(true);
            return dragon;
        }

        private void Release()
        {
            if (_inPool)
            {
                return;
            }

            _inPool = true;
            _frames = null;
            _direction = Vector2.zero;
            _moveSpeed = 0f;
            _damage = 0f;
            _hitEnemies.Clear();
            _physicsResults.Clear();
            _spriteRenderer.sprite = null;
            transform.localScale = Vector3.one;
            transform.rotation = Quaternion.identity;

            gameObject.SetActive(false);
            transform.SetParent(GetPoolRoot(), false);
            Pool.Push(this);
        }

        private static Transform GetPoolRoot()
        {
            if (s_poolRoot != null)
            {
                return s_poolRoot;
            }

            GameObject root = new GameObject("ThunderDragonPool");
            s_poolRoot = root.transform;
            return s_poolRoot;
        }
    }
}
