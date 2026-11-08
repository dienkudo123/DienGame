using UnityEngine;

namespace DienGame.Gameplay.CameraSystem
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField]
        private Transform target;

        [Header("Dead Zone")]
        [SerializeField]
        [Range(0f, 1f)]
        private float deadZoneWidth = 0.25f;

        [SerializeField]
        [Range(0f, 1f)]
        private float deadZoneHeight = 0.20f;

        [Header("Follow")]
        [SerializeField]
        [Min(0f)]
        private float smoothTime = 0.12f;

        [SerializeField]
        private bool centerOnStart = true;

        [Header("Map Bounds")]
        [SerializeField]
        private Collider2D cameraBounds;

        private Camera _camera;
        private Vector3 _velocity;
        private float _cameraZ;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _cameraZ = transform.position.z;
        }

        private void Start()
        {
            if (target == null)
            {
                Debug.LogError(
                    "CameraFollow2D: Target chưa được gán.",
                    this
                );

                enabled = false;
                return;
            }

            if (!_camera.orthographic)
            {
                Debug.LogError(
                    "CameraFollow2D chỉ dùng cho Orthographic Camera.",
                    this
                );

                enabled = false;
                return;
            }

            if (centerOnStart)
            {
                Vector3 startPosition = new Vector3(
                    target.position.x,
                    target.position.y,
                    _cameraZ
                );

                transform.position =
                    ClampToBounds(startPosition);
            }
        }

        private void LateUpdate()
        {
            FollowTarget();
        }

        private void FollowTarget()
        {
            Vector3 currentPosition = transform.position;

            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;

            float deadHalfWidth =
                halfWidth * deadZoneWidth;

            float deadHalfHeight =
                halfHeight * deadZoneHeight;

            float offsetX =
                target.position.x - currentPosition.x;

            float offsetY =
                target.position.y - currentPosition.y;

            Vector3 desiredPosition = currentPosition;

            if (Mathf.Abs(offsetX) > deadHalfWidth)
            {
                desiredPosition.x +=
                    offsetX -
                    Mathf.Sign(offsetX) * deadHalfWidth;
            }

            if (Mathf.Abs(offsetY) > deadHalfHeight)
            {
                desiredPosition.y +=
                    offsetY -
                    Mathf.Sign(offsetY) * deadHalfHeight;
            }

            desiredPosition.z = _cameraZ;

            desiredPosition =
                ClampToBounds(desiredPosition);

            transform.position = Vector3.SmoothDamp(
                currentPosition,
                desiredPosition,
                ref _velocity,
                smoothTime
            );

            transform.position = ClampToBounds(
                transform.position
            );
        }

        private Vector3 ClampToBounds(
            Vector3 position)
        {
            position.z = _cameraZ;

            if (cameraBounds == null)
                return position;

            Bounds bounds = cameraBounds.bounds;

            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;

            float minX = bounds.min.x + halfWidth;
            float maxX = bounds.max.x - halfWidth;

            float minY = bounds.min.y + halfHeight;
            float maxY = bounds.max.y - halfHeight;

            if (minX > maxX)
            {
                position.x = bounds.center.x;
            }
            else
            {
                position.x = Mathf.Clamp(
                    position.x,
                    minX,
                    maxX
                );
            }

            if (minY > maxY)
            {
                position.y = bounds.center.y;
            }
            else
            {
                position.y = Mathf.Clamp(
                    position.y,
                    minY,
                    maxY
                );
            }

            return position;
        }
    }
}