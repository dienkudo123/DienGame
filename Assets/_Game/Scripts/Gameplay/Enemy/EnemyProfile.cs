using UnityEngine;

namespace DienGame.Gameplay.Enemy
{
    [DisallowMultipleComponent]
    public sealed class EnemyProfile : MonoBehaviour
    {
        private const string PreferredVisualChildName = "Visual";
        private const float AnimatedEnemyVisualScaleMultiplier = 0.20f;

        private EnemyDefinition _definition;
        private EnemyArchetype _archetype;
        private bool _isElite;
        private Color _displayColor = Color.white;

        public EnemyDefinition Definition => _definition;
        public EnemyArchetype Archetype => _archetype;
        public bool IsElite => _isElite;
        public Color DisplayColor => _displayColor;

        public void Configure(
            EnemyDefinition definition,
            bool isElite)
        {
            if (definition == null)
            {
                Debug.LogError("EnemyProfile: definition đang null.", this);
                return;
            }

            _definition = definition;
            _archetype = definition.Archetype;
            _isElite = isElite;
            _displayColor = GetAccentColor(_archetype, _isElite);

            ApplyArt();
            EnsureVisualPolish();
        }

        // Giữ overload này để code cũ vẫn chạy nếu còn chỗ gọi trực tiếp.
        public void Configure(
            EnemyArchetype archetype,
            bool isElite)
        {
            EnemyDefinition definition = EnemyCatalogProvider.Get(archetype);
            Configure(definition, isElite);
        }

        private void ApplyArt()
        {
            if (_definition == null)
            {
                return;
            }

            Transform visualRoot = ResolveVisualRoot();

            if (visualRoot == null)
            {
                Debug.LogWarning(
                    $"EnemyProfile: Không tìm thấy visual root cho {_definition.DisplayName}.",
                    this
                );
                return;
            }

            SpriteRenderer visualRenderer =
                visualRoot.GetComponent<SpriteRenderer>();

            if (visualRenderer == null)
            {
                visualRenderer =
                    visualRoot.GetComponentInChildren<SpriteRenderer>(true);
            }

            if (visualRenderer != null)
            {
                visualRenderer.enabled = true;
                // Art thật luôn giữ màu gốc. Accent chỉ dùng cho VFX.
                visualRenderer.color = Color.white;
            }

            EnemyVisualSet visual = _definition.GetVisual(_isElite);

            if (visual == null)
            {
                return;
            }

            EnemySlimeAnimator spriteAnimator =
                visualRoot.GetComponent<EnemySlimeAnimator>();

            if (visual.HasFrames)
            {
                if (spriteAnimator == null)
                {
                    spriteAnimator =
                        visualRoot.gameObject.AddComponent<EnemySlimeAnimator>();
                }

                spriteAnimator.enabled = true;
                spriteAnimator.Configure(
                    visual.Frames,
                    visual.FramesPerSecond,
                    visual.VisualScale * AnimatedEnemyVisualScaleMultiplier
                );

                return;
            }

            // Grunt hiện chưa có sprite sheet riêng trong project.
            // Vì vậy giữ nguyên animation Slime có sẵn ở child Visual.
            // Không scale lại root/Visual tại đây vì Slime prefab đang dùng
            // visualScale riêng (0.05) để khớp PPU/kích thước sprite gốc.
        }

        private Transform ResolveVisualRoot()
        {
            Transform namedVisual = transform.Find(PreferredVisualChildName);

            if (namedVisual != null &&
                namedVisual.GetComponent<SpriteRenderer>() != null)
            {
                return namedVisual;
            }

            SpriteRenderer[] renderers =
                GetComponentsInChildren<SpriteRenderer>(true);

            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer renderer = renderers[i];

                if (renderer == null)
                {
                    continue;
                }

                // Prefab Enemy hiện có SpriteRenderer root bị disable và
                // SpriteRenderer thật nằm ở child Visual. Ưu tiên renderer
                // đang enabled để tránh bind animation vào renderer ẩn.
                if (renderer.enabled)
                {
                    return renderer.transform;
                }
            }

            if (renderers.Length > 0 && renderers[0] != null)
            {
                return renderers[0].transform;
            }

            return null;
        }

        private void EnsureVisualPolish()
        {
            EnemyVisualPolish polish = GetComponent<EnemyVisualPolish>();

            if (polish == null)
            {
                polish = gameObject.AddComponent<EnemyVisualPolish>();
            }

            polish.Configure(this);
        }

        private static Color GetAccentColor(
            EnemyArchetype archetype,
            bool isElite)
        {
            if (isElite && archetype != EnemyArchetype.Boss)
            {
                return new Color(1f, 0.72f, 0.12f, 1f);
            }

            return archetype switch
            {
                EnemyArchetype.Runner => new Color(0.2f, 0.9f, 1f, 1f),
                EnemyArchetype.Tank => new Color(1f, 0.34f, 0.18f, 1f),
                EnemyArchetype.Ranged => new Color(0.78f, 0.32f, 1f, 1f),
                EnemyArchetype.Swarm => new Color(0.78f, 1f, 0.16f, 1f),
                EnemyArchetype.Boss => new Color(1f, 0.12f, 0.32f, 1f),
                _ => Color.white,
            };
        }
    }
}
