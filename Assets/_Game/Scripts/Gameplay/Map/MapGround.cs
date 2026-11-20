using UnityEngine;

namespace DienGame.Gameplay.Map
{
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class MapGround : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private RectangularMapBoundary mapBoundary;

        [Header("Ground Size")]
        [SerializeField]
        [Min(0f)]
        private float outsidePaddingX = 2f;

        [SerializeField]
        [Min(0f)]
        private float outsidePaddingY = 2f;

        [Header("Rendering")]
        [SerializeField]
        private int sortingOrder = -10;

        [SerializeField]
        private bool autoCenterToBoundary = true;

        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            Build();
        }

        private void OnEnable()
        {
            Build();
        }

        private void OnValidate()
        {
            Build();
        }

        [ContextMenu("Build Ground")]
        public void Build()
        {
            EnsureReferences();

            if (_spriteRenderer == null)
            {
                return;
            }

            if (mapBoundary == null)
            {
                Debug.LogWarning("MapGround: Không tìm thấy RectangularMapBoundary.");
                return;
            }

            if (_spriteRenderer.sprite == null)
            {
                Debug.LogWarning("MapGround: Chưa gán sprite nền cho SpriteRenderer.");
                return;
            }

            Vector2 worldSize = mapBoundary.WorldSize;

            float finalWidth = worldSize.x + outsidePaddingX * 2f;
            float finalHeight = worldSize.y + outsidePaddingY * 2f;

            _spriteRenderer.drawMode = SpriteDrawMode.Tiled;
            _spriteRenderer.size = new Vector2(finalWidth, finalHeight);
            _spriteRenderer.sortingOrder = sortingOrder;

            if (autoCenterToBoundary)
            {
                Vector2 center = mapBoundary.Center;
                transform.position = new Vector3(center.x, center.y, transform.position.z);
            }

            transform.localScale = Vector3.one;
        }

        private void EnsureReferences()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (mapBoundary == null)
            {
                mapBoundary = FindFirstObjectByType<RectangularMapBoundary>();
            }
        }
    }
}