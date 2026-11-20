using UnityEngine;

namespace DienGame.Gameplay.Skills.Marksman
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class JudgementCannonAnimator : MonoBehaviour
    {
        [SerializeField, Min(0.01f)]
        private float frameDuration = 0.055f;

        private SpriteRenderer _renderer;
        private Sprite[] _frames = System.Array.Empty<Sprite>();
        private Sprite _fallbackSprite;

        private float _elapsed;
        private int _frameIndex;
        private bool _isFiring;

        public bool IsFiring => _isFiring;

        public float NormalizedFireTime
        {
            get
            {
                float duration = GetFireDuration();
                return duration <= 0.0001f
                    ? 0f
                    : Mathf.Clamp01(_elapsed / duration);
            }
        }

        public bool IsBeamActive
        {
            get
            {
                if (!_isFiring)
                {
                    return false;
                }

                float t = NormalizedFireTime;
                return t >= 0.34f && t <= 0.80f;
            }
        }

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(
            Sprite[] frames,
            Sprite fallbackSprite,
            float newFrameDuration)
        {
            if (_renderer == null)
            {
                _renderer = GetComponent<SpriteRenderer>();
            }

            _frames = frames != null
                ? frames
                : System.Array.Empty<Sprite>();

            _fallbackSprite = fallbackSprite;
            frameDuration = Mathf.Max(0.01f, newFrameDuration);

            ResetToIdle();
        }

        public void PlayFire()
        {
            _elapsed = 0f;
            _frameIndex = 0;
            _isFiring = true;
            RefreshSprite();
        }

        public void Tick(float deltaTime)
        {
            if (!_isFiring)
            {
                return;
            }

            _elapsed += Mathf.Max(0f, deltaTime);

            if (_frames == null || _frames.Length == 0)
            {
                if (_elapsed >= frameDuration * 5f)
                {
                    ResetToIdle();
                }

                return;
            }

            int index = Mathf.FloorToInt(_elapsed / frameDuration);

            if (index >= _frames.Length)
            {
                ResetToIdle();
                return;
            }

            if (index != _frameIndex)
            {
                _frameIndex = index;
                RefreshSprite();
            }
        }

        public void SetAlpha(float alpha)
        {
            if (_renderer == null)
            {
                return;
            }

            Color color = _renderer.color;
            color.a = Mathf.Clamp01(alpha);
            _renderer.color = color;
        }

        public void SetWorldWidth(float worldWidth)
        {
            if (_renderer == null)
            {
                return;
            }

            Sprite sprite = _renderer.sprite;

            if (sprite == null || sprite.bounds.size.x <= 0.0001f)
            {
                transform.localScale = Vector3.one * Mathf.Max(0.01f, worldWidth);
                return;
            }

            float scale = Mathf.Max(0.01f, worldWidth) / sprite.bounds.size.x;
            transform.localScale = new Vector3(scale, scale, 1f);
        }

        private float GetFireDuration()
        {
            int frameCount = _frames != null && _frames.Length > 0
                ? _frames.Length
                : 5;

            return frameCount * frameDuration;
        }

        private void ResetToIdle()
        {
            _elapsed = 0f;
            _frameIndex = 0;
            _isFiring = false;
            RefreshSprite();
        }

        private void RefreshSprite()
        {
            if (_renderer == null)
            {
                return;
            }

            if (_frames != null && _frames.Length > 0)
            {
                int safeIndex = Mathf.Clamp(_frameIndex, 0, _frames.Length - 1);
                Sprite frame = _frames[safeIndex];

                if (frame != null)
                {
                    _renderer.sprite = frame;
                    return;
                }
            }

            _renderer.sprite = _fallbackSprite;
        }
    }
}
