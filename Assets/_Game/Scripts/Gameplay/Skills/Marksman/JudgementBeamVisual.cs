using UnityEngine;

namespace DienGame.Gameplay.Skills.Marksman
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class JudgementBeamVisual : MonoBehaviour
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

            Hide();
        }

        public void Show(
            Vector3 localStart,
            Vector3 localEnd,
            float normalizedTime,
            float worldHeight,
            float alpha)
        {
            if (_renderer == null || _frames == null || _frames.Length == 0)
            {
                Hide();
                return;
            }

            Vector3 delta = localEnd - localStart;
            float length = delta.magnitude;

            if (length <= 0.001f)
            {
                Hide();
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
                Hide();
                return;
            }

            _renderer.sprite = sprite;
            _renderer.enabled = true;

            Color color = Color.white;
            color.a = Mathf.Clamp01(alpha);
            _renderer.color = color;

            transform.localPosition = (localStart + localEnd) * 0.5f;

            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            transform.localRotation = Quaternion.Euler(0f, 0f, angle);

            float spriteWidth = Mathf.Max(0.0001f, sprite.bounds.size.x);
            float spriteHeight = Mathf.Max(0.0001f, sprite.bounds.size.y);

            float xScale = length / spriteWidth;
            float yScale = Mathf.Max(0.01f, worldHeight) / spriteHeight;

            transform.localScale = new Vector3(xScale, yScale, 1f);
        }

        public void Hide()
        {
            if (_renderer != null)
            {
                _renderer.enabled = false;
            }
        }
    }
}
