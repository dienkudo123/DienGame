using UnityEngine;

namespace DienGame.Gameplay.Combat
{
    public sealed class ProjectileVisual : MonoBehaviour
    {
        [Header("Rotation")]
        [SerializeField] private bool rotateWithMovement = true;
        [SerializeField] private float rotationOffsetDegrees = 0f;

        [Header("Visual")]
        [SerializeField, Min(0.01f)] private float visualScale = 0.15f;

        private Vector3 _lastPosition;
        private Vector2 _lastDirection = Vector2.right;

        public float VisualScale => visualScale;

        private void Awake()
        {
            RestoreVisualState();
        }

        private void OnEnable()
        {
            RestoreVisualState();
        }

        private void LateUpdate()
        {
            UpdateRotation();
        }

        public void RestoreVisualState()
        {
            _lastPosition = transform.position;
            _lastDirection = Vector2.right;
            ApplyScale();
        }


        public void SetDirection(Vector2 worldDirection)
        {
            if (worldDirection.sqrMagnitude <= 0.000001f)
            {
                return;
            }

            _lastDirection = worldDirection.normalized;
            _lastPosition = transform.position;

            if (rotateWithMovement)
            {
                float angle = Mathf.Atan2(_lastDirection.y, _lastDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffsetDegrees);
            }
        }

        private void UpdateRotation()
        {
            if (!rotateWithMovement)
            {
                return;
            }

            Vector3 currentPosition = transform.position;
            Vector2 movement = currentPosition - _lastPosition;
            _lastPosition = currentPosition;

            if (movement.sqrMagnitude > 0.000001f)
            {
                _lastDirection = movement.normalized;
            }

            float angle = Mathf.Atan2(_lastDirection.y, _lastDirection.x) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(
                0f,
                0f,
                angle + rotationOffsetDegrees
            );
        }

        private void ApplyScale()
        {
            transform.localScale = new Vector3(
                visualScale,
                visualScale,
                1f
            );
        }
    }
}
