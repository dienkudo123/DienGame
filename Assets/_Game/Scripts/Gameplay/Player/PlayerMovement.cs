using DienGame.Gameplay.Balance;
using DienGame.Gameplay.Input;
using DienGame.Gameplay.Skills.Marksman;
using DienGame.Gameplay.Skills.Assassin;
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
        private MarksmanSkillController _marksmanSkills;
        private AssassinSkillController _assassinSkills;
        private bool _isAssassin;
        private bool _externalMovementControl;

        public Vector2 MoveDirection => _moveInput;
        public bool IsMoving => _moveInput.sqrMagnitude > 0.001f;
        public bool IsExternallyControlled => _externalMovementControl;

        public float MoveSpeed
        {
            get
            {
                float baseSpeed = playerCharacter != null
                    ? playerCharacter.MoveSpeed
                    : GameBalance.PlayerMoveSpeed;

                float multiplier = _marksmanSkills != null
                    ? _marksmanSkills.MoveSpeedMultiplier
                    : 1f;

                if (_isAssassin && _assassinSkills == null)
                    _assassinSkills = GetComponent<AssassinSkillController>();
                if (_assassinSkills != null && _assassinSkills.IsAfterimageSpeedBoostActive)
                    multiplier *= 1.2f;
                return baseSpeed * multiplier;
            }
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _marksmanSkills = GetComponent<MarksmanSkillController>();
            _isAssassin = GetComponent<AssassinMeleeAttack>() != null;
            _assassinSkills = GetComponent<AssassinSkillController>();

            if (playerCharacter == null)
                playerCharacter = GetComponent<PlayerCharacter>();
        }

        private void Update() => ReadInput();

        private void FixedUpdate()
        {
            // A class-specific active skill can temporarily steer the Rigidbody.
            // Without that skill, this is identical to the original 360-degree movement.
            if (!_externalMovementControl) Move();
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
                _moveInput.Normalize();
        }

        private void Move() => _rigidbody.linearVelocity = _moveInput * MoveSpeed;

        /// <summary>
        /// For exclusive movement states such as an Assassin's wall hook/dash.
        /// Does not change the input vector or base movement speed.
        /// </summary>
        public void SetExternalMovementControl(bool active)
        {
            if (_externalMovementControl == active) return;
            _externalMovementControl = active;
            if (_rigidbody != null) _rigidbody.linearVelocity = Vector2.zero;
        }

        public void MultiplyMoveSpeed(float multiplier)
        {
            if (multiplier <= 0f || playerCharacter == null) return;
            playerCharacter.MultiplyMoveSpeed(multiplier);
        }
    }
}
