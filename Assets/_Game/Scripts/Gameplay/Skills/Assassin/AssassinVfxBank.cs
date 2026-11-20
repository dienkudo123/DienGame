using System.Collections.Generic;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Assassin
{
    public enum AssassinVfxKind
    {
        MeleeNormal, MeleeLv5, BloodNormal, BloodLv5,
        KnifeNormal, KnifeLv5, DeathMark,
        OrbitNormal, OrbitLv5, PillarNormal, PillarLv5
    }

    /// <summary>Loads one atlas per animation and creates cached sprites once (no per-frame allocations).</summary>
    public static class AssassinVfxBank
    {
        private const string CatalogPath = "Assassin/VfxCatalog";
        private static AssassinVfxCatalog s_catalog;
        private static bool s_searched;
        private static bool s_warned;
        private static readonly Dictionary<AssassinVfxKind, Sprite[]> s_cache =
            new Dictionary<AssassinVfxKind, Sprite[]>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache()
        {
            s_catalog = null;
            s_searched = false;
            s_warned = false;
            s_cache.Clear();
        }

        public static Sprite[] Frames(AssassinVfxKind kind)
        {
            if (s_cache.TryGetValue(kind, out Sprite[] frames)) return frames;
            Texture2D texture = Texture(kind);
            if (texture == null) return null;
            int n = kind == AssassinVfxKind.OrbitNormal || kind == AssassinVfxKind.OrbitLv5 ? 1 : 7;
            frames = new Sprite[n];
            if (n == 1)
            {
                frames[0] = Sprite.Create(texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(.5f, .5f), Mathf.Max(texture.width, texture.height));
            }
            else
            {
                float cellWidth = texture.width / 4f;
                float cellHeight = texture.height / 2f;
                for (int i = 0; i < n; i++)
                {
                    int col = i % 4, row = i / 4;
                    // Unity sprite rectangles use a bottom-left origin.
                    Rect rect = new Rect(col * cellWidth,
                        texture.height - (row + 1) * cellHeight, cellWidth, cellHeight);
                    frames[i] = Sprite.Create(texture, rect, new Vector2(.5f, .5f),
                        Mathf.Max(cellWidth, cellHeight));
                }
            }
            s_cache[kind] = frames;
            return frames;
        }

        private static Texture2D Texture(AssassinVfxKind kind)
        {
            if (!s_searched)
            {
                s_catalog = Resources.Load<AssassinVfxCatalog>(CatalogPath);
                s_searched = true;
                if (s_catalog == null && !s_warned)
                {
                    Debug.LogWarning("Assassin VFX Catalog missing: use DienGame > Assassin > Sync VFX from Art (auto-created on editor import).");
                    s_warned = true;
                }
            }
            if (s_catalog == null) return null;
            switch (kind)
            {
                case AssassinVfxKind.MeleeNormal: return s_catalog.meleeNormal;
                case AssassinVfxKind.MeleeLv5: return s_catalog.meleeLv5;
                case AssassinVfxKind.BloodNormal: return s_catalog.bloodNormal;
                case AssassinVfxKind.BloodLv5: return s_catalog.bloodLv5;
                case AssassinVfxKind.KnifeNormal: return s_catalog.knifeNormal;
                case AssassinVfxKind.KnifeLv5: return s_catalog.knifeLv5;
                case AssassinVfxKind.DeathMark: return s_catalog.deathMark;
                case AssassinVfxKind.OrbitNormal: return s_catalog.orbitNormal;
                case AssassinVfxKind.OrbitLv5: return s_catalog.orbitLv5;
                case AssassinVfxKind.PillarNormal: return s_catalog.pillarNormal;
                case AssassinVfxKind.PillarLv5: return s_catalog.pillarLv5;
                default: return null;
            }
        }

        public static void Play(AssassinVfxKind kind, Vector2 position, Vector2 facing,
            SpriteRenderer source = null, float worldSize = 1.8f,
            float duration = .22f, float angleOffset = 0f)
        {
            Sprite[] sprites = Frames(kind);
            if (sprites == null || sprites.Length == 0) return;
            GameObject go = new GameObject("Assassin VFX - " + kind);
            go.transform.position = position;
            float angle = facing.sqrMagnitude > .001f
                ? Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg : 0f;
            go.transform.rotation = Quaternion.Euler(0f, 0f, angle + angleOffset);
            go.transform.localScale = Vector3.one * worldSize;
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprites[0];
            renderer.sortingLayerID = source != null ? source.sortingLayerID : 0;
            renderer.sortingOrder = source != null ? source.sortingOrder + 10 : 70;
            AssassinOneShotVfx fx = go.AddComponent<AssassinOneShotVfx>();
            fx.Configure(sprites, renderer, Mathf.Max(.03f, duration));
        }
    }

    public sealed class AssassinOneShotVfx : MonoBehaviour
    {
        private Sprite[] _frames;
        private SpriteRenderer _renderer;
        private float _duration, _elapsed;
        public void Configure(Sprite[] frames, SpriteRenderer renderer, float duration)
        {
            _frames = frames;
            _renderer = renderer;
            _duration = duration;
            _elapsed = 0f;
        }
        private void Update()
        {
            if (_frames == null || _renderer == null) { Destroy(gameObject); return; }
            _elapsed += Time.deltaTime;
            if (_elapsed >= _duration) { Destroy(gameObject); return; }
            float t = _elapsed / _duration;
            _renderer.sprite = _frames[Mathf.Min(_frames.Length - 1, (int)(t * _frames.Length))];
        }
    }
}
