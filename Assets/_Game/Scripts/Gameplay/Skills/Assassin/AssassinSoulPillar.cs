using System.Collections.Generic;
using DienGame.Gameplay.Enemy;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Assassin
{
    /// <summary>Solid hookable Soul Pillar: owns HP, aggro, Lv5 counter-damage and its seven-frame VFX.</summary>
    public sealed class AssassinSoulPillar : MonoBehaviour
    {
        private const float AggroRadius = 4.25f;
        private const float MaxLifetime = 18f; // emergency expiry to prevent unbounded clutter
        private static readonly List<AssassinSoulPillar> Active = new List<AssassinSoulPillar>(4);
        private static Sprite s_barSprite;
        private readonly Collider2D[] _targets = new Collider2D[40];
        private ContactFilter2D _filter;
        private float _hp, _maxHp, _aliveTime, _counterTick, _damage;
        private int _level;
        private Sprite[] _frames;
        private SpriteRenderer _sprite;
        private Transform _fill;
        private float _spawnTime;
        private bool _destroying;
        public static int ActivePillarCount => Active.Count;

        public static AssassinSoulPillar GetAggroTarget(Vector2 enemyPosition, Vector2 playerPosition)
        {
            AssassinSoulPillar best = null;
            float nearest = AggroRadius * AggroRadius;
            for (int i = 0; i < Active.Count; i++)
            {
                AssassinSoulPillar p = Active[i];
                if (p == null || p._destroying) continue;
                Vector2 point = p.transform.position;
                if ((point - playerPosition).sqrMagnitude > AggroRadius * AggroRadius) continue;
                float sq = (point - enemyPosition).sqrMagnitude;
                if (sq >= nearest) continue;
                nearest = sq;
                best = p;
            }
            return best;
        }
        public static void Create(Vector2 position, AssassinSoulPillarSystem owner,
            int level, float health, float damage)
        {
            GameObject obj = new GameObject("Hon Tru Dan Anh - Lv" + level);
            obj.transform.position = position;
            AssassinSoulPillar p = obj.AddComponent<AssassinSoulPillar>();
            p._level = level;
            p._maxHp = Mathf.Max(1, health);
            p._hp = p._maxHp;
            p._damage = damage;
            p._frames = AssassinVfxBank.Frames(level >= 5 ?
                AssassinVfxKind.PillarLv5 : AssassinVfxKind.PillarNormal);
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 65;
            sr.sprite = p._frames != null && p._frames.Length > 0 ? p._frames[0] : null;
            p._sprite = sr;
            obj.transform.localScale = new Vector3(1.65f, 1.65f, 1);
            CircleCollider2D body = obj.AddComponent<CircleCollider2D>();
            body.radius = .22f;
            body.isTrigger = false;
            if (owner != null)
                foreach (Collider2D ownCollider in owner.GetComponentsInChildren<Collider2D>())
                    if (ownCollider != null) Physics2D.IgnoreCollision(body, ownCollider, true);
            p._filter = new ContactFilter2D { useLayerMask = false, useTriggers = false };
            p.BuildHealthBar();
            Active.Add(p);
        }
        private static Sprite WhitePixel()
        {
            if (s_barSprite == null)
                s_barSprite = Sprite.Create(Texture2D.whiteTexture,
                    new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1);
            return s_barSprite;
        }
        private void BuildHealthBar()
        {
            GameObject bar = new GameObject("Pillar HP");
            bar.transform.SetParent(transform, false);
            bar.transform.localPosition = new Vector3(0f, .68f, 0f);
            SpriteRenderer bg = bar.AddComponent<SpriteRenderer>();
            bg.sprite = WhitePixel();
            bg.color = new Color(.12f, .09f, .2f, .9f);
            bg.sortingOrder = 86;
            bar.transform.localScale = new Vector3(.75f, .065f, 1f);
            GameObject fill = new GameObject("Remaining HP");
            fill.transform.SetParent(bar.transform, false);
            fill.transform.localPosition = new Vector3(0, 0, -.02f);
            SpriteRenderer fg = fill.AddComponent<SpriteRenderer>();
            fg.sprite = WhitePixel();
            fg.color = new Color(.75f, .34f, 1f, 1f);
            fg.sortingOrder = 87;
            _fill = fill.transform;
            UpdateBar();
        }
        private void UpdateBar()
        {
            if (_fill == null) return;
            float fraction = Mathf.Clamp01(_hp / _maxHp);
            _fill.localScale = new Vector3(fraction, 1f, 1f);
            _fill.localPosition = new Vector3((fraction - 1f) * .5f, 0f, -.02f);
        }
        public void TakeDamage(float damage)
        {
            if (_destroying || damage <= 0f) return;
            _hp = Mathf.Max(0f, _hp - damage);
            UpdateBar();
            if (_hp <= 0f) Despawn();
            else _spawnTime = Mathf.Max(_spawnTime, .1f);
        }
        private void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f || _destroying) return;
            _aliveTime += dt;
            _spawnTime += dt;
            if (_aliveTime >= MaxLifetime) { Despawn(); return; }
            if (_frames != null && _frames.Length > 0 && _sprite != null)
            {
                int index = Mathf.Min(3, (int)(_spawnTime * 12f));
                _sprite.sprite = _frames[Mathf.Min(index, _frames.Length - 1)];
                Color tint = _sprite.color;
                tint.a = .96f + .04f * Mathf.Sin(_aliveTime * 5f);
                _sprite.color = tint;
            }
            if (_level >= 5)
            {
                _counterTick -= dt;
                if (_counterTick <= 0f)
                {
                    _counterTick = 1.5f;
                    int count = Physics2D.OverlapCircle(transform.position, 1.5f, _filter, _targets);
                    for (int i = 0; i < count; i++)
                    {
                        Collider2D c = _targets[i];
                        if (c == null) continue;
                        EnemyHealth e = c.GetComponentInParent<EnemyHealth>();
                        if (e != null && !e.IsDead) e.TakeDamage(_damage * .35f);
                    }
                }
            }
        }
        private void Despawn()
        {
            if (_destroying) return;
            _destroying = true;
            Active.Remove(this);
            AssassinVfxBank.Play(_level >= 5 ? AssassinVfxKind.PillarLv5 :
                AssassinVfxKind.PillarNormal, transform.position, Vector2.right,
                _sprite, 1.65f, .35f);
            Destroy(gameObject);
        }
        private void OnDestroy() { Active.Remove(this); }
    }
}
