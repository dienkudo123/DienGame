using UnityEngine;
using UnityEngine.InputSystem;

namespace DienGame.Gameplay.Input
{
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [SerializeField]
        private InputActionReference moveAction;

        public Vector2 MoveDirection =>
            moveAction.action.ReadValue<Vector2>();

        private void OnEnable()
        {
            moveAction.action.Enable();
        }

        private void OnDisable()
        {
            moveAction.action.Disable();
        }
    }
}