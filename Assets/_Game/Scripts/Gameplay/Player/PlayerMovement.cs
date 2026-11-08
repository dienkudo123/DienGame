using DienGame.Gameplay.Input;
using UnityEngine;

namespace DienGame.Gameplay.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PlayerCharacter playerCharacter;
        [SerializeField, Min(0f)] private float moveSpeed = 5f;

        private Rigidbody2D _rigidbody;
        private Vector2 _moveInput;

        public Vector2 MoveDirection => _moveInput;
        public bool IsMoving => _moveInput.sqrMagnitude > 0.001f;
        public float MoveSpeed => playerCharacter != null ? playerCharacter.MoveSpeed : moveSpeed;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            if (playerCharacter == null) playerCharacter = GetComponent<PlayerCharacter>();
        }

        private void Update() => ReadInput();
        private void FixedUpdate() => Move();

        private void ReadInput()
        {
            if (inputReader == null)
            {
                _moveInput = Vector2.zero;
                return;
            }

            _moveInput = inputReader.MoveDirection;
            if (_moveInput.sqrMagnitude > 1f) _moveInput.Normalize();
        }

        private void Move()
        {
            _rigidbody.linearVelocity = _moveInput * MoveSpeed;
        }

        public void MultiplyMoveSpeed(float multiplier)
        {
            if (multiplier <= 0f) return;

            if (playerCharacter != null)
            {
                playerCharacter.MultiplyMoveSpeed(multiplier);
                return;
            }

            moveSpeed *= multiplier;
        }
    }
}
