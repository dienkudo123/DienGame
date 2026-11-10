using UnityEngine;

namespace DienGame.Gameplay.Enemy
{
    [DisallowMultipleComponent]
    public sealed class EnemyProfile : MonoBehaviour
    {
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
            SpriteRenderer mainRenderer = GetComponent<SpriteRenderer>();

            if (mainRenderer != null)
            {
                // Art thật luôn giữ màu gốc. Accent chỉ dùng cho VFX.
                mainRenderer.color = Color.white;
            }

            EnemyVisualSet visual = _definition.GetVisual(_isElite);

            if (visual == null)
            {
                return;
            }

            float scale = visual.VisualScale;

            EnemySlimeAnimator spriteAnimator =
                GetComponent<EnemySlimeAnimator>();

            if (visual.HasFrames)
            {
                if (spriteAnimator == null)
                {
                    spriteAnimator = gameObject.AddComponent<EnemySlimeAnimator>();
                }

                spriteAnimator.Configure(
                    visual.Frames,
                    visual.FramesPerSecond,
                    scale
                );
            }
            else
            {
                // Grunt vẫn có thể dùng sprite Slime cũ trên prefab.
                transform.localScale = Vector3.one * scale;
            }
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
