using System.Collections.Generic;
using DienGame.Gameplay.Map;
using DienGame.Gameplay.Player;
using UnityEngine;

namespace DienGame.Gameplay.Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public sealed class EnemyProjectile : MonoBehaviour
    {
        private static readonly Stack<EnemyProjectile> Pool = new();
        private static Material s_sharedMaterial;
        private static Transform s_poolRoot;

        private Rigidbody2D _rigidbody;
        private CircleCollider2D _collider;
        private LineRenderer _core;
        private TrailRenderer _trail;

        private float _damage;
        private float _lifetime;
        private bool _initialized;
        private bool _inPool;
        private Color _color;
        private float _width;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Pool.Clear();
            s_poolRoot = null;
            s_sharedMaterial = null;
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<CircleCollider2D>();

            _rigidbody.bodyType = RigidbodyType2D.Dynamic;
            _rigidbody.gravityScale = 0f;
            _rigidbody.freezeRotation = true;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            _collider.isTrigger = true;
            _collider.radius = 0.12f;

            BuildVisualOnce();
        }

        private void Update()
        {
            if (!_initialized)
            {
                return;
            }

            _lifetime -= Time.deltaTime;

            if (_lifetime <= 0f)
            {
                Release();
            }
        }

        public void Initialize(
            Vector2 direction,
            float speed,
            float damage,
            float lifetime,
            Color color,
            float width = 0.12f)
        {
            _damage = Mathf.Max(0f, damage);
            _lifetime = Mathf.Max(0.1f, lifetime);
            _color = color;
            _width = Mathf.Max(0.04f, width);
            _initialized = true;
            _inPool = false;

            direction = direction.sqrMagnitude > 0.0001f
                ? direction.normalized
                : Vector2.down;

            _rigidbody.linearVelocity =
                direction * Mathf.Max(0.1f, speed);

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            ConfigureVisual(color, _width);
        }

        private void BuildVisualOnce()
        {
            _core = gameObject.AddComponent<LineRenderer>();
            _core.useWorldSpace = false;
            _core.positionCount = 2;
            _core.SetPosition(0, new Vector3(-0.18f, 0f, 0f));
            _core.SetPosition(1, new Vector3(0.18f, 0f, 0f));
            _core.sortingOrder = 20;
            _core.material = GetSharedMaterial();

            _trail = gameObject.AddComponent<TrailRenderer>();
            _trail.time = 0.16f;
            _trail.minVertexDistance = 0.035f;
            _trail.material = GetSharedMaterial();
            _trail.sortingOrder = 19;
        }

        private void ConfigureVisual(Color color, float width)
        {
            _core.startWidth = width;
            _core.endWidth = width * 0.45f;
            _core.startColor = color;
            _core.endColor = new Color(color.r, color.g, color.b, 0.18f);

            _trail.Clear();
            _trail.startWidth = width * 0.9f;
            _trail.endWidth = 0f;
            _trail.startColor = new Color(color.r, color.g, color.b, 0.55f);
            _trail.endColor = new Color(color.r, color.g, color.b, 0f);
        }

        private static Material GetSharedMaterial()
        {
            if (s_sharedMaterial != null)
            {
                return s_sharedMaterial;
            }

            Shader shader = Shader.Find("Sprites/Default");

            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Unlit");
            }

            if (shader != null)
            {
                s_sharedMaterial = new Material(shader)
                {
                    name = "EnemyProjectile_RuntimeMaterial"
                };
            }

            return s_sharedMaterial;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_initialized || other == null)
            {
                return;
            }

            PlayerHealth playerHealth =
                other.GetComponentInParent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(_damage);

                EnemyDeathVfx.SpawnHit(
                    transform.position,
                    _color,
                    Mathf.Max(0.18f, _width * 2.2f)
                );

                Release();
                return;
            }

            MapObstacle obstacle =
                other.GetComponentInParent<MapObstacle>();

            if (obstacle != null && obstacle.BlockProjectile)
            {
                EnemyDeathVfx.SpawnHit(
                    transform.position,
                    _color,
                    Mathf.Max(0.16f, _width * 1.8f)
                );

                Release();
            }
        }

        private void Release()
        {
            if (_inPool)
            {
                return;
            }

            _inPool = true;
            _initialized = false;
            _damage = 0f;
            _lifetime = 0f;
            _rigidbody.linearVelocity = Vector2.zero;
            _trail.Clear();

            gameObject.SetActive(false);
            transform.SetParent(GetPoolRoot(), false);
            Pool.Push(this);
        }

        private static EnemyProjectile Acquire()
        {
            EnemyProjectile projectile = null;

            while (Pool.Count > 0 && projectile == null)
            {
                projectile = Pool.Pop();
            }

            if (projectile == null)
            {
                GameObject projectileObject = new GameObject("EnemyProjectile");
                projectileObject.AddComponent<Rigidbody2D>();
                projectileObject.AddComponent<CircleCollider2D>();
                projectile = projectileObject.AddComponent<EnemyProjectile>();
            }

            projectile._inPool = false;
            projectile.transform.SetParent(null, false);
            projectile.gameObject.SetActive(true);
            return projectile;
        }

        private static Transform GetPoolRoot()
        {
            if (s_poolRoot != null)
            {
                return s_poolRoot;
            }

            GameObject root = new GameObject("EnemyProjectilePool");
            s_poolRoot = root.transform;
            return s_poolRoot;
        }

        public static EnemyProjectile Create(
            Vector2 position,
            Vector2 direction,
            float speed,
            float damage,
            float lifetime,
            Color color,
            float width = 0.12f)
        {
            EnemyProjectile projectile = Acquire();
            projectile.transform.position = position;

            projectile.Initialize(
                direction,
                speed,
                damage,
                lifetime,
                color,
                width
            );

            return projectile;
        }
    }
}
