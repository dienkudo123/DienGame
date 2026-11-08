using DienGame.Gameplay.Player;
using UnityEngine;

namespace DienGame.Gameplay.Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class EnemyMovement : MonoBehaviour
    {
        [SerializeField] private EnemyCharacter enemyCharacter;
        [SerializeField, Min(0f)] private float moveSpeed = 2f;
        [SerializeField, Min(0f)] private float stopDistance = 0.8f;

        private Rigidbody2D _rigidbody;
        private Collider2D _bodyCollider;
        private PlayerMovement _player;

        private float MoveSpeed => enemyCharacter != null ? enemyCharacter.MoveSpeed : moveSpeed;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _bodyCollider = GetComponent<Collider2D>();

            if (enemyCharacter == null) enemyCharacter = GetComponent<EnemyCharacter>();

            _rigidbody.bodyType = RigidbodyType2D.Dynamic;
            _rigidbody.gravityScale = 0f;
            _rigidbody.freezeRotation = true;
            _rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
            _bodyCollider.isTrigger = false;
        }

        private void Start()
        {
            _player = FindFirstObjectByType<PlayerMovement>();
            IgnorePlayerCollision();
        }

        private void FixedUpdate()
        {
            if (_player == null)
            {
                _rigidbody.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 difference =
                (Vector2)_player.transform.position - _rigidbody.position;

            if (difference.magnitude <= stopDistance)
            {
                _rigidbody.linearVelocity = Vector2.zero;
                return;
            }

            _rigidbody.linearVelocity = difference.normalized * MoveSpeed;
        }

        private void IgnorePlayerCollision()
        {
            if (_player == null || _bodyCollider == null) return;

            foreach (Collider2D playerCollider in _player.GetComponentsInChildren<Collider2D>())
            {
                if (playerCollider != null)
                {
                    Physics2D.IgnoreCollision(_bodyCollider, playerCollider, true);
                }
            }
        }

        private void OnDisable()
        {
            if (_rigidbody != null) _rigidbody.linearVelocity = Vector2.zero;
        }
    }
}
