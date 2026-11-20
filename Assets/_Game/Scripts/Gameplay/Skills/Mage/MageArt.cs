using System.Collections.Generic;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Mage
{
    /// <summary>Runtime asset loader/slicer: no Inspector references or Sprite Editor clicks needed.</summary>
    public static class MageArt
    {
        private static readonly Dictionary<string, Sprite[]> Sheets = new Dictionary<string, Sprite[]>();
        public static Sprite Fly(int direction, int frame) => Frame("Mage/MageFly8x4", 4, 8, direction, frame, 145f);
        public static Sprite Chimera(int direction, int frame) => Frame("Mage/ChimeraWalk4x4", 4, 4, direction, frame, 185f);
        public static Sprite ChimeraAttack(int direction, int frame) => Frame("Mage/ChimeraAttack4x5", 5, 4, direction, frame, 185f);
        /// <summary>Uses Lv5 artwork only when the corresponding passive reaches level five.
        /// Both normal and evolved sprites represent the same world-space size regardless of PNG resolution.
        /// </summary>
        public static Sprite Effect(string name, int frame, bool evolved = false)
        {
            string path = "Mage/Vfx/" + name + (evolved ? "_Lv5" : "");
            // Ordinary VFX atlases are 256px cells; evolved atlases are 512px.
            // The gravity art intentionally uses a larger baseline at its 512px resolution.
            float ppu = name == "TrongTruongDiGioi" ? 160f : (evolved ? 320f : 160f);
            return Frame(path, 4, 2, frame / 4, frame % 4, ppu);
        }

        private static Sprite Frame(string resource, int columns, int rows, int row, int column, float ppu)
        {
            string key = resource + ":" + ppu;
            if (!Sheets.TryGetValue(key, out Sprite[] frames))
            {
                Texture2D texture = Resources.Load<Texture2D>(resource);
                if (texture == null)
                {
                    Debug.LogError("Mage sprite texture missing: Resources/" + resource);
                    Sheets[key] = new Sprite[columns * rows];
                    return null;
                }
                frames = new Sprite[columns * rows];
                if (texture.width % columns != 0 || texture.height % rows != 0)
                    Debug.LogWarning("Mage sprite atlas size is not divisible by its grid: " + resource +
                        " (" + texture.width + "x" + texture.height + ").");
                int cellWidth = texture.width / columns;
                int cellHeight = texture.height / rows;
                for (int y = 0; y < rows; y++)
                for (int x = 0; x < columns; x++)
                {
                    Rect rect = new Rect(x * cellWidth, (rows - 1 - y) * cellHeight, cellWidth, cellHeight);
                    frames[y * columns + x] = Sprite.Create(texture, rect, new Vector2(.5f, .5f), ppu, 0, SpriteMeshType.FullRect);
                }
                Sheets.Add(key, frames);
            }
            int index = Mathf.Clamp(row, 0, rows - 1) * columns + Mathf.Clamp(column, 0, columns - 1);
            return frames[index];
        }
    }

    public sealed class MageSpriteAnimator : MonoBehaviour
    {
        private SpriteRenderer _renderer;
        private DienGame.Gameplay.Player.PlayerMovement _movement;
        private Vector2 _lastFacing = Vector2.down;
        private float _time;
        private Animator _legacyAnimator;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _movement = GetComponentInParent<DienGame.Gameplay.Player.PlayerMovement>();
            _legacyAnimator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            if (_legacyAnimator != null) _legacyAnimator.enabled = false;
            transform.localScale = Vector3.one;
        }

        private void Update()
        {
            if (_renderer == null) return;
            Vector2 direction = _movement != null ? _movement.MoveDirection : Vector2.zero;
            if (direction.sqrMagnitude > .02f) _lastFacing = direction.normalized;
            _time += Time.deltaTime;
            int sector = Mathf.RoundToInt(Mathf.Atan2(_lastFacing.y, _lastFacing.x) * Mathf.Rad2Deg / 45f);
            // image rows top->bottom: up, up-right, right, down-right, down, down-left, left, up-left
            int row = (2 - sector + 8) % 8;
            int frame = Mathf.FloorToInt(_time * (direction.sqrMagnitude > .02f ? 7f : 3f)) % 4;
            Sprite sprite = MageArt.Fly(row, frame);
            if (sprite != null) _renderer.sprite = sprite;
        }
    }

    /// <summary>
    /// Shared animation renderer for all seven Mage skills.
    /// Tune WORLD SCALE and animation FPS in GetProfile(), not in the Inspector.
    /// The caller's size remains a relative multiplier to preserve existing skill logic.
    /// </summary>
    public sealed class MageVfx : MonoBehaviour
    {
        private readonly struct Profile
        {
            public readonly float Fps;
            public readonly float Scale;
            public Profile(float fps, float scale) { Fps = fps; Scale = scale; }
        }

        private static Material s_fxMaterial;
        private string _name;
        private SpriteRenderer _renderer;
        private Transform _follow;
        private Vector3 _followOffset;
        private float _age;
        private float _duration;
        private float _fps;
        private bool _loop;
        private bool _evolved;

        // 7 frames each; final scale = caller's size * these carefully reduced values.
        // Bigger areas (gravity) retain enough world coverage, small hits stay compact.
        private static Profile GetProfile(string name)
        {
            switch (name)
            {
                case "TinhChamPhanLiet": return new Profile(18f, .78f);
                case "TrongTruongDiGioi": return new Profile(11f, .325f); // 50% of previous display scale
                case "TraoLietHopKim": return new Profile(18f, .74f);
                case "PhaoSinhThe": return new Profile(16f, .49f); // 50% of previous display scale
                case "SongTrungOanhKich": return new Profile(14f, .85f);
                case "CongHuongSanMoi": return new Profile(16f, .80f);
                case "ManChanSongThe": return new Profile(10f, .88f);
                default: return new Profile(14f, .85f);
            }
        }

        public static MageVfx Play(string name, Vector2 position, float size = 1f,
            float duration = .38f, bool loop = false, float angle = 0f,
            Transform followTarget = null, bool evolved = false)
        {
            Profile p = GetProfile(name);
            GameObject go = new GameObject("MageVFX_" + name);
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            go.transform.localScale = Vector3.one * Mathf.Max(.05f, size * p.Scale);
            SpriteRenderer r = go.AddComponent<SpriteRenderer>();
            // Only the gravity well is a ground-field effect. All other Mage
            // spell effects retain their previous foreground sorting.
            if (name == "TrongTruongDiGioi")
            {
                r.sortingLayerID = 0;
                r.sortingOrder = -12;
            }
            else
            {
                r.sortingOrder = 55;
            }
            if (s_fxMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader != null) s_fxMaterial = new Material(shader) { name = "MageVFXShared" };
            }
            if (s_fxMaterial != null) r.sharedMaterial = s_fxMaterial;
            MageVfx v = go.AddComponent<MageVfx>();
            v._name = name;
            v._renderer = r;
            v._fps = evolved ? p.Fps + 2f : p.Fps;
            v._evolved = evolved;
            v._loop = loop;
            // Always show every frame in one-shot VFX; FPS is independent of call duration.
            v._duration = loop ? Mathf.Max(.08f, duration)
                : Mathf.Max(Mathf.Max(.08f, duration), 7f / v._fps);
            v._follow = followTarget;
            v._followOffset = followTarget != null ? go.transform.position - followTarget.position : Vector3.zero;
            r.sprite = MageArt.Effect(name, 0, evolved);
            return v;
        }

        private void LateUpdate()
        {
            if (_follow != null) transform.position = _follow.position + _followOffset;
            _age += Time.deltaTime;
            if (!_loop && _age >= _duration)
            {
                Destroy(gameObject);
                return;
            }
            int frame = _loop
                ? Mathf.FloorToInt(_age * _fps) % 7
                : Mathf.Clamp(Mathf.FloorToInt(_age * _fps), 0, 6);
            _renderer.sprite = MageArt.Effect(_name, frame, _evolved);
        }
    }
}
