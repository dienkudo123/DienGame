using UnityEngine;

namespace DienGame.Gameplay.Combat
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ProjectileSpriteAnimator : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField, Min(1f)] private float framesPerSecond = 12f;

        private SpriteRenderer _spriteRenderer;
        private Transform _scaleTarget;
        private Sprite[] _frames;
        private int _frameIndex;
        private float _frameTimer;

        private bool _normalizeFrameSize;
        private Vector2 _referenceSpriteSize;
        private Vector3 _animationBaseScale = Vector3.one;

        private void Awake()
        {
            SetTarget(GetComponent<SpriteRenderer>());
        }

        private void Update()
        {
            UpdateAnimation();
        }

        public void SetTarget(SpriteRenderer spriteRenderer)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            _spriteRenderer = spriteRenderer;
            _scaleTarget = spriteRenderer.transform;
        }

        public void Play(Sprite[] frames, float fps)
        {
            Play(frames, fps, false, _scaleTarget != null ? _scaleTarget.localScale : Vector3.one);
        }

        public void Play(Sprite[] frames, float fps, bool normalizeFrameSize)
        {
            Play(
                frames,
                fps,
                normalizeFrameSize,
                _scaleTarget != null ? _scaleTarget.localScale : Vector3.one
            );
        }

        public void Play(
            Sprite[] frames,
            float fps,
            bool normalizeFrameSize,
            Vector3 baseScale)
        {
            _frames = frames;
            framesPerSecond = Mathf.Max(1f, fps);
            _frameIndex = 0;
            _frameTimer = 0f;
            _normalizeFrameSize = normalizeFrameSize;
            _animationBaseScale = baseScale;
            _referenceSpriteSize = FindReferenceSize(frames);

            if (_scaleTarget != null)
            {
                _scaleTarget.localScale = _animationBaseScale;
            }

            RefreshFrame();
        }

        public void Stop(Sprite restoreSprite = null, Vector3? restoreScale = null)
        {
            _frames = null;
            _frameIndex = 0;
            _frameTimer = 0f;
            _normalizeFrameSize = false;
            _referenceSpriteSize = Vector2.zero;

            if (_spriteRenderer != null && restoreSprite != null)
            {
                _spriteRenderer.sprite = restoreSprite;
            }

            if (_scaleTarget != null)
            {
                _scaleTarget.localScale = restoreScale ?? _animationBaseScale;
            }
        }

        private void UpdateAnimation()
        {
            if (_spriteRenderer == null || _frames == null || _frames.Length <= 1)
            {
                return;
            }

            float frameDuration = 1f / framesPerSecond;
            _frameTimer += Time.deltaTime;

            while (_frameTimer >= frameDuration)
            {
                _frameTimer -= frameDuration;
                _frameIndex = (_frameIndex + 1) % _frames.Length;
                RefreshFrame();
            }
        }

        private void RefreshFrame()
        {
            if (_spriteRenderer == null || _frames == null || _frames.Length == 0)
            {
                return;
            }

            Sprite sprite = _frames[Mathf.Clamp(_frameIndex, 0, _frames.Length - 1)];

            if (sprite == null)
            {
                return;
            }

            if (_spriteRenderer.sprite != sprite)
            {
                _spriteRenderer.sprite = sprite;
            }

            if (_normalizeFrameSize)
            {
                ApplyNormalizedScale(sprite);
            }
            else if (_scaleTarget != null && !_scaleTarget.localScale.Equals(_animationBaseScale))
            {
                _scaleTarget.localScale = _animationBaseScale;
            }
        }

        private void ApplyNormalizedScale(Sprite sprite)
        {
            if (_scaleTarget == null)
            {
                return;
            }

            Vector2 currentSize = sprite.bounds.size;

            if (_referenceSpriteSize.x <= 0.0001f ||
                _referenceSpriteSize.y <= 0.0001f ||
                currentSize.x <= 0.0001f ||
                currentSize.y <= 0.0001f)
            {
                _scaleTarget.localScale = _animationBaseScale;
                return;
            }

            float xRatio = _referenceSpriteSize.x / currentSize.x;
            float yRatio = _referenceSpriteSize.y / currentSize.y;

            Vector3 normalizedScale = new Vector3(
                _animationBaseScale.x * xRatio,
                _animationBaseScale.y * yRatio,
                _animationBaseScale.z
            );

            if (!_scaleTarget.localScale.Equals(normalizedScale))
            {
                _scaleTarget.localScale = normalizedScale;
            }
        }

        private static Vector2 FindReferenceSize(Sprite[] frames)
        {
            if (frames == null)
            {
                return Vector2.zero;
            }

            for (int i = 0; i < frames.Length; i++)
            {
                Sprite sprite = frames[i];

                if (sprite != null)
                {
                    return sprite.bounds.size;
                }
            }

            return Vector2.zero;
        }
    }
}
