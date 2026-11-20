using UnityEngine;

namespace DienGame.Gameplay.Skills.Marksman
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class JudgementShockwaveAnimator : MonoBehaviour
    {
        private SpriteRenderer _renderer;
        private Sprite[] _frames = System.Array.Empty<Sprite>();

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _renderer.enabled = false;
        }

        public void Initialize(Sprite[] frames)
        {
            if (_renderer == null)
            {
                _renderer = GetComponent<SpriteRenderer>();
            }

            _frames = frames != null
                ? frames
                : System.Array.Empty<Sprite>();

            _renderer.enabled = _frames.Length > 0;
        }

        public void SetProgress(
            float normalizedTime,
            float worldDiameter,
            float alpha)
        {
            if (_renderer == null || _frames == null || _frames.Length == 0)
            {
                return;
            }

            float t = Mathf.Clamp01(normalizedTime);
            int frameIndex = Mathf.Clamp(
                Mathf.FloorToInt(t * _frames.Length),
                0,
                _frames.Length - 1
            );

            Sprite sprite = _frames[frameIndex];

            if (sprite == null)
            {
                return;
            }

            _renderer.sprite = sprite;
            _renderer.enabled = true;

            Color color = Color.white;
            color.a = Mathf.Clamp01(alpha);
            _renderer.color = color;

            float spriteWidth = Mathf.Max(0.0001f, sprite.bounds.size.x);
            float scale = Mathf.Max(0.01f, worldDiameter) / spriteWidth;
            transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
