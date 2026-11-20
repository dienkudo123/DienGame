using System.Collections.Generic;
using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Player;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Assassin
{
    /// <summary>Phi Nhan: sprite spins by code while projectile travels. At Lv5 it may ricochet once.</summary>
    public sealed class AssassinThrowingKnife : MonoBehaviour
    {
        private const float Speed = 12f;
        private const float Lifetime = .75f;
        private const float SpinSpeed = -720f;
        private readonly RaycastHit2D[] _hits = new RaycastHit2D[24];
        private readonly Collider2D[] _targets = new Collider2D[40];
        private ContactFilter2D _filter;
        private Vector2 _heading;
        private float _remaining, _damage;
        private Transform _owner;
        private AssassinSkillController _skills;
        private int[] _snapshot;
        private SpriteRenderer _renderer;
        private int _level;
        private bool _hasBounced;
        private EnemyHealth _ignoredEnemy;

        public static void Create(Vector2 origin, Vector2 direction, float damage,
            Transform owner, AssassinSkillController skills, int[] snapshot,
            LayerMask layers, int skillLevel = 1)
        {
            CreateInternal(origin, direction, damage, owner, skills, snapshot, skillLevel, false, null);
        }

        private static void CreateInternal(Vector2 origin, Vector2 direction, float damage,
            Transform owner, AssassinSkillController skills, int[] snapshot,
            int level, bool bounced, EnemyHealth ignored)
        {
            Sprite[] frames = AssassinVfxBank.Frames(level >= 5 ?
                AssassinVfxKind.KnifeLv5 : AssassinVfxKind.KnifeNormal);
            if (frames == null || frames.Length < 1) return;
            GameObject obj = new GameObject(bounced ? "Phi Nhan - Lv5 rebound" : "Phi Nhan");
            obj.transform.position = origin;
            obj.transform.localScale = Vector3.one * (level >= 5 ? 1.0f : .85f);
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = frames[Mathf.Min(3, frames.Length - 1)];
            sr.sortingOrder = 75;
            AssassinThrowingKnife projectile = obj.AddComponent<AssassinThrowingKnife>();
            projectile._renderer = sr;
            projectile._heading = direction.sqrMagnitude > .0001f ? direction.normalized : Vector2.right;
            projectile._damage = damage;
            projectile._owner = owner;
            projectile._skills = skills;
            projectile._snapshot = snapshot;
            projectile._level = level;
            projectile._hasBounced = bounced;
            projectile._ignoredEnemy = ignored;
            projectile._remaining = Lifetime;
            projectile._filter = new ContactFilter2D { useTriggers = false, useLayerMask = false };
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            _remaining -= dt;
            if (_remaining <= 0f) { Destroy(gameObject); return; }
            _renderer.transform.Rotate(0, 0, SpinSpeed * dt);
            Vector2 start = transform.position;
            float step = Speed * dt;
            int count = Physics2D.CircleCast(start, .12f, _heading, _filter, _hits, step);
            float nearest = step;
            Collider2D struck = null;
            for (int i = 0; i < count; i++)
            {
                Collider2D c = _hits[i].collider;
                if (c == null || c.isTrigger || (_owner != null && c.transform.IsChildOf(_owner))) continue;
                if (c.GetComponentInParent<PlayerHealth>() != null) continue;
                if (c.GetComponentInParent<AssassinSoulPillar>() != null) continue;
                if (_ignoredEnemy != null && c.GetComponentInParent<EnemyHealth>() == _ignoredEnemy) continue;
                if (_hits[i].distance > nearest) continue;
                struck = c;
                nearest = _hits[i].distance;
            }
            transform.position = start + _heading * nearest;
            if (struck == null) return;
            EnemyHealth enemy = struck.GetComponentInParent<EnemyHealth>();
            if (enemy != null && !enemy.IsDead)
            {
                float damage = _skills != null ? _skills.DamageAgainst(enemy, _damage, _snapshot) : _damage;
                enemy.TakeDamage(damage);
                if (_level >= 5 && !_hasBounced)
                {
                    EnemyHealth next = FindNextEnemy(transform.position, enemy);
                    if (next != null)
                    {
                        Vector2 heading = (Vector2)next.transform.position - (Vector2)transform.position;
                        CreateInternal(transform.position, heading, _damage,
                            _owner, _skills, _snapshot, _level, true, enemy);
                        AssassinVfxBank.Play(AssassinVfxKind.KnifeLv5,
                            transform.position, Vector2.right, _renderer, 1.25f, .15f);
                    }
                }
            }
            Destroy(gameObject);
        }

        private EnemyHealth FindNextEnemy(Vector2 origin, EnemyHealth excluded)
        {
            int count = Physics2D.OverlapCircle(origin, 4f, _filter, _targets);
            EnemyHealth closest = null;
            float bestSqr = 16f;
            for (int i = 0; i < count; i++)
            {
                Collider2D c = _targets[i];
                if (c == null) continue;
                EnemyHealth e = c.GetComponentInParent<EnemyHealth>();
                if (e == null || e == excluded || e.IsDead) continue;
                Vector2 delta = (Vector2)e.transform.position - origin;
                float sqr = delta.sqrMagnitude;
                if (sqr >= bestSqr || sqr < .04f) continue;
                closest = e;
                bestSqr = sqr;
            }
            return closest;
        }
    }
}
