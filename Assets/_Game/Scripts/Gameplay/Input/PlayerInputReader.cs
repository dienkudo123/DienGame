using UnityEngine;
using UnityEngine.InputSystem;

namespace DienGame.Gameplay.Input
{
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionReference moveAction;

        private InputAction _moveAction;
        private bool _enabledByThisReader;

        public Vector2 MoveDirection =>
            isActiveAndEnabled && _moveAction != null && _moveAction.enabled
                ? _moveAction.ReadValue<Vector2>()
                : Vector2.zero;

        private void Awake()
        {
            ResolveMoveAction();
        }

        private void OnEnable()
        {
            ResolveMoveAction();

            if (_moveAction == null)
            {
                Debug.LogWarning(
                    "PlayerInputReader: Chưa gán Move Input Action.",
                    this
                );
                return;
            }

            // Không disable action vốn đã được một component khác bật.
            _enabledByThisReader = !_moveAction.enabled;
            if (_enabledByThisReader)
            {
                _moveAction.Enable();
            }
        }

        private void OnDisable()
        {
            if (_enabledByThisReader && _moveAction != null)
            {
                _moveAction.Disable();
            }

            _enabledByThisReader = false;
        }

        private void ResolveMoveAction()
        {
            _moveAction = moveAction != null ? moveAction.action : null;
        }
    }
}
