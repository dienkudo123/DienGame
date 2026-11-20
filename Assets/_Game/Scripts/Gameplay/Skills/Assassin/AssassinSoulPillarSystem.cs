using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Player;
using DienGame.Gameplay.Skills;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Assassin
{
    /// <summary>One soul per defeated enemy, collect 20 to summon a pillar at the player's current position.</summary>
    [DisallowMultipleComponent]
    public sealed class AssassinSoulPillarSystem : MonoBehaviour
    {
        private const int SoulsPerPillar = 20;
        private const int MaximumPillars = 4;
        private PlayerSkillManager _skills;
        private PlayerHealth _health;
        private PlayerCharacter _character;
        private int _souls;
        public int SoulCount => _souls;

        private void Awake()
        {
            _skills = GetComponent<PlayerSkillManager>();
            _health = GetComponent<PlayerHealth>();
            _character = GetComponent<PlayerCharacter>();
        }
        private void OnEnable() { EnemyHealth.AnyEnemyDied += OnAnyEnemyDied; }
        private void OnDisable() { EnemyHealth.AnyEnemyDied -= OnAnyEnemyDied; }
        private void Update()
        {
            if (_souls >= SoulsPerPillar) TrySummon();
        }
        private void OnAnyEnemyDied(EnemyHealth enemy)
        {
            if (enemy == null || _skills == null ||
                _skills.GetSkillLevel(SkillId.AssassinExecution) <= 0) return;
            AssassinSoulOrb.Spawn(enemy.transform.position, this);
        }
        public void CollectSoul()
        {
            if (_skills == null || _skills.GetSkillLevel(SkillId.AssassinExecution) <= 0) return;
            _souls = Mathf.Min(SoulsPerPillar, _souls + 1);
            if (_souls >= SoulsPerPillar) TrySummon();
        }
        private void TrySummon()
        {
            if (_souls < SoulsPerPillar || _skills == null ||
                _skills.GetSkillLevel(SkillId.AssassinExecution) <= 0) return;
            if (AssassinSoulPillar.ActivePillarCount >= MaximumPillars) return;
            int lvl = _skills.GetSkillLevel(SkillId.AssassinExecution);
            float hp = Mathf.Max(25f, (_health != null ? _health.MaxHealth : 100f) *
                (.40f + .045f * lvl));
            float atk = _character != null ? _character.Damage : 10f;
            AssassinSoulPillar.Create(transform.position, this, lvl, hp, atk);
            _souls = 0; // Additional souls NEVER damage existing pillars.
        }
    }

    public sealed class AssassinSoulOrb : MonoBehaviour
    {
        private static Sprite s_sprite;
        private AssassinSoulPillarSystem _owner;
        private float _life = 16f;
        public static void Spawn(Vector2 position, AssassinSoulPillarSystem owner)
        {
            if (owner == null) return;
            GameObject obj = new GameObject("Soul (1 of 20)");
            obj.transform.position = position;
            obj.transform.localScale = Vector3.one * .25f;
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = GetSprite();
            sr.color = new Color(.65f, .32f, 1f, .95f);
            sr.sortingOrder = 78;
            obj.AddComponent<AssassinSoulOrb>()._owner = owner;
        }
        private static Sprite GetSprite()
        {
            if (s_sprite != null) return s_sprite;
            const int n = 32;
            Texture2D t = new Texture2D(n, n, TextureFormat.RGBA32, false);
            t.name = "RuntimeSoulOrb";
            Color32[] pixels = new Color32[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float dx = (x - 15.5f) / 16f;
                float dy = (y - 15.5f) / 16f;
                float a = Mathf.Clamp01((1f - Mathf.Sqrt(dx * dx + dy * dy)) * 4f);
                pixels[y * n + x] = new Color32(255, 255, 255, (byte)(255 * a));
            }
            t.SetPixels32(pixels);
            t.Apply(false, false);
            s_sprite = Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(.5f, .5f), n);
            return s_sprite;
        }
        private void Update()
        {
            if (_owner == null) { Destroy(gameObject); return; }
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            _life -= dt;
            if (_life <= 0f) { Destroy(gameObject); return; }
            Vector2 offset = (Vector2)_owner.transform.position - (Vector2)transform.position;
            if (offset.sqrMagnitude <= .55f * .55f)
            {
                _owner.CollectSoul();
                Destroy(gameObject);
                return;
            }
            if (offset.sqrMagnitude < 2.6f * 2.6f)
                transform.position = Vector2.MoveTowards(transform.position,
                    _owner.transform.position, 6f * dt);
        }
    }
}
