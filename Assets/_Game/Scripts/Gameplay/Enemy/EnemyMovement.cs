using DienGame.Gameplay.Balance;
using DienGame.Gameplay.Equipment;
using DienGame.Gameplay.Skills.Mage;
using DienGame.Gameplay.Player;
using DienGame.Gameplay.Skills.Assassin;
using UnityEngine;

namespace DienGame.Gameplay.Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class EnemyMovement : MonoBehaviour
    {
        [SerializeField] private EnemyCharacter enemyCharacter;
        [SerializeField, Min(0f)] private float stopDistance = 0.8f;

        private Rigidbody2D _rigidbody;
        private Collider2D _bodyCollider;
        private PlayerMovement _player;
        private float _runtimeMoveSpeed = GameBalance.EnemyBaseMoveSpeed;
        private float _runtimeStopDistance = 0.8f;
        private float _runtimeRetreatDistance;
        private bool _isChasing = true;

        private float MoveSpeed =>
            enemyCharacter != null
                ? enemyCharacter.MoveSpeed
                : _runtimeMoveSpeed;

        public Rigidbody2D Rigidbody => _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _bodyCollider = GetComponent<Collider2D>();

            if (enemyCharacter == null)
            {
                enemyCharacter = GetComponent<EnemyCharacter>();
            }

            _runtimeStopDistance = Mathf.Max(0f, stopDistance);

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

            Vector2 playerPosition = _player.transform.position;
            AssassinSoulPillar pillar = AssassinSoulPillar.GetAggroTarget(
                _rigidbody.position, playerPosition);
            Vector2 targetPosition = pillar != null ?
                (Vector2)pillar.transform.position : playerPosition;

            // Never chase summons: Player is the default aggro target.
            // Assassin Soul Pillar is intentionally preserved for that class.

            Vector2 difference = targetPosition - _rigidbody.position;

            // Status effects do not mutate EnemyCharacter's permanent move speed.
            EnemyEquipmentStatus status = GetComponent<EnemyEquipmentStatus>();
            float effectiveSpeed = MoveSpeed * (status != null ? status.MoveMultiplier : 1f);
            float distance = difference.magnitude;

            if (_runtimeRetreatDistance > 0f &&
                distance < _runtimeRetreatDistance)
            {
                _rigidbody.linearVelocity =
                    -difference.normalized * effectiveSpeed;
                return;
            }

            // Two different thresholds stop the "move-stop-move" oscillation at
            // close range, without changing the configured preferred distance.
            float stopAt = Mathf.Max(0f, _runtimeStopDistance - .06f);
            float resumeAt = _runtimeStopDistance + .18f;
            if (_isChasing)
            {
                if (distance <= stopAt) _isChasing = false;
            }
            else if (distance >= resumeAt)
            {
                _isChasing = true;
            }

            if (!_isChasing)
            {
                _rigidbody.linearVelocity = Vector2.zero;
                return;
            }

            _rigidbody.linearVelocity = difference.normalized * effectiveSpeed;
        }

        public void SetMoveSpeed(float moveSpeed)
        {
            moveSpeed = Mathf.Max(0f, moveSpeed);

            if (enemyCharacter != null)
            {
                enemyCharacter.SetMoveSpeed(moveSpeed);
            }
            else
            {
                _runtimeMoveSpeed = moveSpeed;
            }
        }

        public void ConfigureDistances(
            float preferredStopDistance,
            float retreatDistance = 0f)
        {
            _runtimeStopDistance =
                Mathf.Max(0f, preferredStopDistance);

            _runtimeRetreatDistance =
                Mathf.Clamp(
                    retreatDistance,
                    0f,
                    _runtimeStopDistance
                );
        }

        public void StopImmediately()
        {
            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector2.zero;
            }
        }

        private void IgnorePlayerCollision()
        {
            if (_player == null || _bodyCollider == null)
            {
                return;
            }

            foreach (
                Collider2D playerCollider
                in _player.GetComponentsInChildren<Collider2D>())
            {
                if (playerCollider != null)
                {
                    Physics2D.IgnoreCollision(
                        _bodyCollider,
                        playerCollider,
                        true
                    );
                }
            }

            // Never let an immortal Chimera physically block a monster from
            // reaching the Player, including right after the monster spawns.
            ChimeraCompanion companion = FindFirstObjectByType<ChimeraCompanion>();
            if (companion != null)
            {
                foreach (Collider2D petCollider in companion.GetComponentsInChildren<Collider2D>())
                {
                    if (petCollider != null && !petCollider.isTrigger)
                        Physics2D.IgnoreCollision(_bodyCollider, petCollider, true);
                }
            }
        }

        private void OnDisable()
        {
            StopImmediately();
        }
    }
}
