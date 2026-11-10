using UnityEngine;

namespace DienGame.Gameplay.Combat
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ProjectileSpriteAnimator : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField, Min(1f)] private float framesPerSecond = 12f;

        private SpriteRenderer _spriteRenderer;
        private Sprite[] _frames;
        private int _frameIndex;
        private float _frameTimer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            UpdateAnimation();
        }

        public void Play(Sprite[] frames, float fps)
        {
            _frames = frames;
            framesPerSecond = Mathf.Max(1f, fps);
            _frameIndex = 0;
            _frameTimer = 0f;

            if (_spriteRenderer != null && _frames != null && _frames.Length > 0)
            {
                _spriteRenderer.sprite = _frames[0];
            }
        }

        public void Stop(Sprite restoreSprite = null)
        {
            _frames = null;
            _frameIndex = 0;
            _frameTimer = 0f;

            if (_spriteRenderer != null && restoreSprite != null)
            {
                _spriteRenderer.sprite = restoreSprite;
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
                _spriteRenderer.sprite = _frames[_frameIndex];
            }
        }
    }
}
