using UnityEngine;

namespace DienGame.Gameplay.Skills.Marksman
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ThunderLightningVisual : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;

        private Sprite[] _frames;

        private int _frameIndex;

        private float _frameTimer;

        private float _framesPerSecond;

        private float _lifeTimer;

        private float _distance;

        private Vector3 _from;

        private Vector3 _to;

        private void Awake()
        {
            _spriteRenderer =
                GetComponent<SpriteRenderer>();

            _spriteRenderer.enabled = true;
            _spriteRenderer.color = Color.white;

            SetupUnlitMaterial();
        }

        private void Update()
        {
            UpdateAnimation();
            UpdateLifetime();
        }

        // =========================================
        // INITIALIZE
        // =========================================

        public void Initialize(
            Sprite[] frames,
            float framesPerSecond,
            Vector3 from,
            Vector3 to,
            float widthScale,
            float lifetime,
            int sortingOrder)
        {
            _frames = frames;

            _framesPerSecond =
                Mathf.Max(
                    1f,
                    framesPerSecond
                );

            _from = from;
            _to = to;

            _lifeTimer =
                Mathf.Max(
                    0.02f,
                    lifetime
                );

            _frameIndex = 0;
            _frameTimer = 0f;

            _spriteRenderer.sortingOrder =
                sortingOrder;

            // Giữ parameter này để không phải sửa
            // MarksmanSkillController hiện tại.
            // Nhưng không dùng nó để làm sét dày hơn.
            _ = widthScale;

            if (
                _frames == null ||
                _frames.Length == 0 ||
                _frames[0] == null
            )
            {
                Debug.LogWarning(
                    "ThunderLightningVisual: Không có sprite sét.",
                    this
                );

                Destroy(gameObject);

                return;
            }

            _spriteRenderer.sprite =
                _frames[0];

            SetupTransform();
        }

        // =========================================
        // MATERIAL
        // =========================================

        private void SetupUnlitMaterial()
        {
            Shader shader =
                Shader.Find(
                    "Universal Render Pipeline/2D/Sprite-Unlit-Default"
                );

            if (shader == null)
            {
                shader =
                    Shader.Find(
                        "Sprites/Default"
                    );
            }

            if (shader == null)
            {
                return;
            }

            Material material =
                new Material(shader);

            _spriteRenderer.material =
                material;
        }

        // =========================================
        // POSITION + ROTATION
        // =========================================

        private void SetupTransform()
        {
            Vector3 direction =
                _to - _from;

            _distance =
                direction.magnitude;

            if (_distance <= 0.001f)
            {
                Destroy(gameObject);

                return;
            }

            // Đặt sét chính giữa 2 enemy.
            Vector3 middle =
                (_from + _to) *
                0.5f;

            transform.position =
                middle;

            // Xoay sprite hướng về enemy tiếp theo.
            float angle =
                Mathf.Atan2(
                    direction.y,
                    direction.x
                ) *
                Mathf.Rad2Deg;

            transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    angle
                );

            UpdateScale();
        }

        // =========================================
        // SCALE
        // =========================================

        private void UpdateScale()
        {
            Sprite sprite =
                _spriteRenderer.sprite;

            if (sprite == null)
            {
                return;
            }

            float spriteWidth =
                sprite.bounds.size.x;

            if (spriteWidth <= 0.001f)
            {
                return;
            }

            // =====================================
            // CHỈ KÉO DÀI SÉT
            // =====================================
            //
            // X = chiều dài
            // Y = độ dày
            //
            // Chỉ thay đổi X.
            // Y luôn = 1 nên tia sét không béo hơn.
            // =====================================

            float lengthScale =
                _distance /
                spriteWidth;

            transform.localScale =
                new Vector3(
                    lengthScale,
                    0.1f,
                    1f
                );
        }

        // =========================================
        // ANIMATION
        // =========================================

        private void UpdateAnimation()
        {
            if (
                _frames == null ||
                _frames.Length <= 1
            )
            {
                return;
            }

            float frameDuration =
                1f /
                _framesPerSecond;

            _frameTimer +=
                Time.deltaTime;

            while (
                _frameTimer >=
                frameDuration
            )
            {
                _frameTimer -=
                    frameDuration;

                _frameIndex++;

                if (
                    _frameIndex >=
                    _frames.Length
                )
                {
                    _frameIndex = 0;
                }

                Sprite nextSprite =
                    _frames[_frameIndex];

                if (nextSprite == null)
                {
                    continue;
                }

                _spriteRenderer.sprite =
                    nextSprite;

                // Frame có thể có kích thước khác nhau,
                // nên tính lại CHIỀU DÀI.
                UpdateScale();
            }
        }

        // =========================================
        // LIFETIME
        // =========================================

        private void UpdateLifetime()
        {
            _lifeTimer -=
                Time.deltaTime;

            if (_lifeTimer > 0f)
            {
                return;
            }

            Destroy(gameObject);
        }

        // =========================================
        // CLEANUP
        // =========================================

        private void OnDestroy()
        {
            if (
                _spriteRenderer == null ||
                _spriteRenderer.material == null
            )
            {
                return;
            }

            Destroy(
                _spriteRenderer.material
            );
        }
    }
}