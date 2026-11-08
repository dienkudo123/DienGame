using UnityEngine;

namespace DienGame.Gameplay.Combat
{
    public sealed class ProjectileVisual :
        MonoBehaviour
    {
        [Header("Rotation")]

        [SerializeField]
        private bool rotateWithMovement =
            true;

        // Ảnh viên đạn của m đang quay sang PHẢI.
        // Vì vậy mặc định = 0.
        [SerializeField]
        private float rotationOffsetDegrees =
            0f;

        [Header("Visual")]

        [SerializeField, Min(0.01f)]
        private float visualScale =
            0.15f;

        private Vector3 _lastPosition;

        private Vector2 _lastDirection =
            Vector2.right;

        private void Awake()
        {
            ApplyScale();

            _lastPosition =
                transform.position;
        }

        private void OnEnable()
        {
            _lastPosition =
                transform.position;

            ApplyScale();
        }

        private void LateUpdate()
        {
            UpdateRotation();
        }

        // =========================================
        // ROTATION
        // =========================================

        private void UpdateRotation()
        {
            if (!rotateWithMovement)
            {
                return;
            }

            Vector3 currentPosition =
                transform.position;

            Vector2 movement =
                currentPosition -
                _lastPosition;

            _lastPosition =
                currentPosition;

            // Có di chuyển:
            // lấy hướng bay mới.
            if (
                movement.sqrMagnitude >
                0.000001f
            )
            {
                _lastDirection =
                    movement.normalized;
            }

            float angle =
                Mathf.Atan2(
                    _lastDirection.y,
                    _lastDirection.x
                ) *
                Mathf.Rad2Deg;

            transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    angle +
                    rotationOffsetDegrees
                );
        }

        // =========================================
        // SCALE
        // =========================================

        private void ApplyScale()
        {
            transform.localScale =
                new Vector3(
                    visualScale,
                    visualScale,
                    1f
                );
        }
    }
}