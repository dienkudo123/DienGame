using UnityEngine;

namespace DienGame.Gameplay.Enemy
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class EnemySlimeAnimator : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField] private Sprite[] frames = new Sprite[4];
        [SerializeField, Min(0.1f)] private float framesPerSecond = 6f;

        [Header("Visual")]
        [SerializeField, Min(0.01f)] private float visualScale = 1f;

        private SpriteRenderer _spriteRenderer;
        private int _currentFrame;
        private float _timer;
        private Vector3 _baseScale = Vector3.one;
        private bool _baseScaleCaptured;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            CaptureBaseScale();
            ApplyScale();
        }

        private void OnEnable()
        {
            if (frames != null && frames.Length > 0)
            {
                _currentFrame = Random.Range(0, frames.Length);
            }

            _timer = 0f;
            RefreshSprite();
        }

        private void Update()
        {
            if (frames == null || frames.Length == 0)
            {
                return;
            }

            float frameDuration = 1f / Mathf.Max(0.1f, framesPerSecond);
            _timer += Time.deltaTime;

            while (_timer >= frameDuration)
            {
                _timer -= frameDuration;
                _currentFrame = (_currentFrame + 1) % frames.Length;
                RefreshSprite();
            }
        }

        public void Configure(
            Sprite[] newFrames,
            float newFramesPerSecond,
            float newVisualScale)
        {
            frames = newFrames;
            framesPerSecond = Mathf.Max(0.1f, newFramesPerSecond);
            visualScale = Mathf.Max(0.01f, newVisualScale);

            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            CaptureBaseScale();
            _currentFrame = 0;
            _timer = 0f;
            ApplyScale();
            RefreshSprite();
        }

        private void RefreshSprite()
        {
            if (_spriteRenderer == null || frames == null || frames.Length == 0)
            {
                return;
            }

            Sprite sprite = frames[Mathf.Clamp(_currentFrame, 0, frames.Length - 1)];

            if (sprite != null)
            {
                _spriteRenderer.sprite = sprite;
            }
        }

        private void CaptureBaseScale()
        {
            if (_baseScaleCaptured)
            {
                return;
            }

            _baseScale = transform.localScale;
            _baseScaleCaptured = true;
        }

        private void ApplyScale()
        {
            CaptureBaseScale();
            transform.localScale = new Vector3(
                _baseScale.x * visualScale,
                _baseScale.y * visualScale,
                _baseScale.z
            );
        }
    }
}
