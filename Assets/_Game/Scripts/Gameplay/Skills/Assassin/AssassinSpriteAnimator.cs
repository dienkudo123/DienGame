using DienGame.Gameplay.Player;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Assassin
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class AssassinSpriteAnimator : MonoBehaviour
    {
        [SerializeField] private AssassinSpriteAnimationData animationData;
        [SerializeField, Min(1f)] private float runFps = 8f;
        [SerializeField, Min(0.01f)] private float slashDuration = 0.15f;

        private SpriteRenderer _renderer;
        private PlayerMovement _movement;
        private PlayerTargeting _targeting;
        private Animator _unityAnimator;
        private float _runTime;
        private float _slashRemaining;
        private int _slashDirection;
        private Vector2 _lastFacing = Vector2.down;

        public AssassinSpriteAnimationData AnimationData => animationData;

        public void SetAnimationData(AssassinSpriteAnimationData data)
        {
            animationData = data;
        }

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _movement = GetComponentInParent<PlayerMovement>();
            _targeting = GetComponentInParent<PlayerTargeting>();
            _unityAnimator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            // The separate Assassin animator owns this SpriteRenderer.
            if (_unityAnimator != null)
                _unityAnimator.enabled = false;
        }

        public void PlaySlash(Vector2 direction)
        {
            if (direction.sqrMagnitude > 0.0001f)
                _lastFacing = direction.normalized;

            _slashDirection = DirectionIndex(_lastFacing);
            _slashRemaining = slashDuration;
        }

        private void Update()
        {
            if (animationData == null || _renderer == null)
                return;

            if (_slashRemaining > 0f)
            {
                // Keep facing locked through the 4-frame attack, even while the joystick moves.
                float elapsed = slashDuration - _slashRemaining;
                int frame = Mathf.Clamp(Mathf.FloorToInt(elapsed / slashDuration * 4f), 0, 3);
                SetSprite(animationData.GetSlash(_slashDirection, frame));
                _slashRemaining = Mathf.Max(0f, _slashRemaining - Time.deltaTime);
                return;
            }

            bool moving = _movement != null && _movement.IsMoving;
            if (_targeting != null && _targeting.CurrentTarget != null)
            {
                Vector2 aim = (Vector2)_targeting.CurrentTarget.position -
                              (Vector2)_targeting.transform.position;
                if (aim.sqrMagnitude > 0.0001f)
                    _lastFacing = aim.normalized;
            }
            else if (moving)
            {
                _lastFacing = _movement.MoveDirection.normalized;
            }

            if (moving)
                _runTime += Time.deltaTime * runFps;
            else
                _runTime = 0f;

            int runFrame = moving ? Mathf.FloorToInt(_runTime) % 4 : 0;
            SetSprite(animationData.GetRun(DirectionIndex(_lastFacing), runFrame));
        }

        private void SetSprite(Sprite sprite)
        {
            if (sprite != null && _renderer.sprite != sprite)
                _renderer.sprite = sprite;
        }

        private static int DirectionIndex(Vector2 dir)
        {
            // Match the existing PlayerAnimator index convention.
            float degrees = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (degrees < 0f) degrees += 360f;
            if (degrees >= 337.5f || degrees < 22.5f) return 6;  // Right
            if (degrees < 67.5f) return 5;                     // UpRight
            if (degrees < 112.5f) return 4;                    // Up
            if (degrees < 157.5f) return 3;                    // UpLeft
            if (degrees < 202.5f) return 2;                    // Left
            if (degrees < 247.5f) return 1;                    // DownLeft
            if (degrees < 292.5f) return 0;                    // Down
            return 7;                                          // DownRight
        }
    }
}
