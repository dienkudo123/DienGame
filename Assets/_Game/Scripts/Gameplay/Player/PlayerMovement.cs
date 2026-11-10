using DienGame.Gameplay.Balance;
using DienGame.Gameplay.Input;
using UnityEngine;

namespace DienGame.Gameplay.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PlayerCharacter playerCharacter;

        private Rigidbody2D _rigidbody;
        private Vector2 _moveInput;

        public Vector2 MoveDirection => _moveInput;
        public bool IsMoving => _moveInput.sqrMagnitude > 0.001f;

        public float MoveSpeed =>
            playerCharacter != null
                ? playerCharacter.MoveSpeed
                : GameBalance.PlayerMoveSpeed;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();

            if (playerCharacter == null)
            {
                playerCharacter = GetComponent<PlayerCharacter>();
            }
        }

        private void Update()
        {
            ReadInput();
        }

        private void FixedUpdate()
        {
            Move();
        }

        private void ReadInput()
        {
            if (inputReader == null)
            {
                _moveInput = Vector2.zero;
                return;
            }

            _moveInput = inputReader.MoveDirection;

            if (_moveInput.sqrMagnitude > 1f)
            {
                _moveInput.Normalize();
            }
        }

        private void Move()
        {
            _rigidbody.linearVelocity =
                _moveInput * MoveSpeed;
        }

        public void MultiplyMoveSpeed(float multiplier)
        {
            if (multiplier <= 0f || playerCharacter == null)
            {
                return;
            }

            playerCharacter.MultiplyMoveSpeed(multiplier);
        }
    }
}
