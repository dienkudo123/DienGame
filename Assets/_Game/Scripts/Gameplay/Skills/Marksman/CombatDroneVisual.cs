using UnityEngine;

namespace DienGame.Gameplay.Skills.Marksman
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class CombatDroneVisual : MonoBehaviour
    {
        // =========================================
        // ANIMATION
        // =========================================

        [SerializeField, Min(1f)]
        private float framesPerSecond = 8f;

        // =========================================
        // MUZZLE
        // =========================================

        // Khoảng cách từ tâm Drone đến đầu nòng súng.
        // Đây là giá trị local trước khi tính scale.
        [SerializeField, Min(0f)]
        private float muzzleDistance = 1.5f;

        // =========================================
        // REFERENCES
        // =========================================

        private SpriteRenderer _spriteRenderer;

        private Transform _muzzlePoint;

        // =========================================
        // RUNTIME
        // =========================================

        private Sprite[] _frames;

        private int _frameIndex;

        private int _animationDirection = 1;

        private float _frameTimer;

        // =========================================
        // PUBLIC
        // =========================================

        public Transform MuzzlePoint =>
            _muzzlePoint;

        public Vector3 MuzzlePosition =>
            _muzzlePoint != null
                ? _muzzlePoint.position
                : transform.position;

        // =========================================
        // UNITY
        // =========================================

        private void Awake()
        {
            _spriteRenderer =
                GetComponent<SpriteRenderer>();

            CreateMuzzlePoint();
        }

        private void Update()
        {
            UpdateAnimation();
        }

        // =========================================
        // INITIALIZE
        // =========================================

        public void Initialize(
            Sprite[] frames,
            float fps,
            float scale)
        {
            _frames = frames;

            framesPerSecond =
                Mathf.Max(1f, fps);

            transform.localScale =
                Vector3.one *
                Mathf.Max(0.01f, scale);

            _frameIndex = 0;

            _animationDirection = 1;

            _frameTimer = 0f;

            if (
                _spriteRenderer != null &&
                _frames != null &&
                _frames.Length > 0
            )
            {
                _spriteRenderer.sprite =
                    _frames[0];
            }

            UpdateMuzzlePoint();
        }

        // =========================================
        // CHANGE SPRITES
        // =========================================

        public void SetFrames(
            Sprite[] frames,
            float scale)
        {
            _frames = frames;

            _frameIndex = 0;

            _animationDirection = 1;

            _frameTimer = 0f;

            transform.localScale =
                Vector3.one *
                Mathf.Max(0.01f, scale);

            if (
                _spriteRenderer != null &&
                _frames != null &&
                _frames.Length > 0
            )
            {
                _spriteRenderer.sprite =
                    _frames[0];
            }

            UpdateMuzzlePoint();
        }

        // =========================================
        // AIM
        // =========================================

        public void AimAt(Vector3 targetPosition)
        {
            Vector2 direction =
                (Vector2)(
                    targetPosition -
                    transform.position
                );

            if (
                direction.sqrMagnitude <=
                0.0001f
            )
            {
                return;
            }

            direction.Normalize();

            UpdateMuzzlePoint(direction);
        }

        // =========================================
        // CREATE MUZZLE
        // =========================================

        private void CreateMuzzlePoint()
        {
            GameObject muzzleObject =
                new GameObject(
                    "MuzzlePoint"
                );

            _muzzlePoint =
                muzzleObject.transform;

            _muzzlePoint.SetParent(
                transform,
                false
            );

            UpdateMuzzlePoint();
        }

        // =========================================
        // UPDATE MUZZLE
        // =========================================

        private void UpdateMuzzlePoint()
        {
            _muzzlePoint.localPosition =
                new Vector3(
                    0f,
                    muzzleDistance,
                    0f
                );
        }

        private void UpdateMuzzlePoint(
            Vector2 direction)
        {
            if (_muzzlePoint == null)
            {
                return;
            }

            // Chuyển hướng world-space thành
            // local-space của Drone.
            Vector3 localDirection =
                transform.InverseTransformDirection(
                    new Vector3(
                        direction.x,
                        direction.y,
                        0f
                    )
                );

            localDirection.Normalize();

            _muzzlePoint.localPosition =
                localDirection *
                muzzleDistance;
        }

        // =========================================
        // ANIMATION
        // =========================================

        private void UpdateAnimation()
        {
            if (
                _frames == null ||
                _frames.Length == 0 ||
                _spriteRenderer == null
            )
            {
                return;
            }

            if (_frames.Length == 1)
            {
                _spriteRenderer.sprite =
                    _frames[0];

                return;
            }

            float frameDuration =
                1f /
                framesPerSecond;

            _frameTimer +=
                Time.deltaTime;

            if (_frameTimer < frameDuration)
            {
                return;
            }

            _frameTimer -=
                frameDuration;

            AdvanceFrame();

            _spriteRenderer.sprite =
                _frames[_frameIndex];
        }

        // =========================================
        // PING-PONG
        // =========================================

        private void AdvanceFrame()
        {
            _frameIndex +=
                _animationDirection;

            if (_frameIndex >= _frames.Length)
            {
                _frameIndex =
                    _frames.Length - 2;

                _animationDirection = -1;
            }
            else if (_frameIndex < 0)
            {
                _frameIndex = 1;

                _animationDirection = 1;
            }

            _frameIndex =
                Mathf.Clamp(
                    _frameIndex,
                    0,
                    _frames.Length - 1
                );
        }
    }
}