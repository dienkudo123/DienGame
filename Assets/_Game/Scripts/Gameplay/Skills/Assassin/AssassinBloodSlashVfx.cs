using System.Collections.Generic;
using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Player;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Assassin
{
    /// <summary>
    /// Huyet Anh Tram: after three connected basic slashes, send ONE animated
    /// sword-wave straight forward. Pierces multiple enemies, stops at terrain.
    /// Uses the seven-frame atlas from Art/Skills/Assassin/BloodNormal or BloodLv5.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AssassinBloodSlashVfx : MonoBehaviour
    {
        
        // The slash travels slower to remain readable on mobile screens.
        private const float Speed = 5.5f;
        private const float MaxDistance = 7f;
        private const float HitRadius = .35f;
        
        // Source crescent opens toward local +X; flip it so its solid edge leads.
        // Only the sprite orientation changes; movement still follows direction.
        private const float VisualFacingOffsetDegrees = 180f;
        private const int MaxRayHits = 64;

        private Sprite[] _frames;
        
        private readonly RaycastHit2D[] _rayHits = new RaycastHit2D[MaxRayHits];
        private readonly HashSet<EnemyHealth> _damaged = new HashSet<EnemyHealth>();
        private SpriteRenderer _render;
        private ContactFilter2D _filter;
        private AssassinSkillController _controller;
        private int[] _snapshot;
        private Vector2 _heading;
        private float _damage;
        private float _travelled;

        public static void Launch(Vector2 origin, Vector2 direction, float damage,
            AssassinSkillController controller, int[] snapshot,
            SpriteRenderer sourceRenderer, LayerMask obstacleLayers, int skillLevel)
        {
            if (direction.sqrMagnitude < .0001f) return;
            Sprite[] frames = AssassinVfxBank.Frames(skillLevel >= 5
                ? AssassinVfxKind.BloodLv5 : AssassinVfxKind.BloodNormal);
            if (frames == null || frames.Length == 0) return;
            direction.Normalize();
            GameObject obj = new GameObject("Assassin - Huyet Anh Tram Projectile");
            obj.transform.position = origin + direction * .60f;
            obj.transform.rotation = Quaternion.Euler(0f, 0f,
                Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg
                + VisualFacingOffsetDegrees);
            obj.transform.localScale = Vector3.one * 1.75f;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = frames[0];
            sr.sortingLayerID = sourceRenderer != null ? sourceRenderer.sortingLayerID : 0;
            sr.sortingOrder = sourceRenderer != null ? sourceRenderer.sortingOrder + 15 : 70;

            AssassinBloodSlashVfx projectile = obj.AddComponent<AssassinBloodSlashVfx>();
            projectile._render = sr;
            projectile._frames = frames;
            projectile._heading = direction;
            projectile._damage = Mathf.Max(0f, damage);
            projectile._controller = controller;
            projectile._snapshot = snapshot;
            // Obstacles and enemies are handled with a swept-circle query.
            projectile._filter = new ContactFilter2D
            {
                useTriggers = false,
                useLayerMask = false
            };
        }

        // Compatibility with old visual-only callers; new combat uses Launch().
        public static void Play(Vector2 origin, Vector2 facing, SpriteRenderer sr)
        {
            Launch(origin, facing, 0f, null, null, sr, ~0, 1);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f || _render == null || _frames == null) return;
            float step = Mathf.Min(Speed * dt, MaxDistance - _travelled);
            if (step <= 0f) { Destroy(gameObject); return; }

            Vector2 start = transform.position;
            int count = Physics2D.CircleCast(start, HitRadius, _heading,
                _filter, _rayHits, step);
            float stopAt = step;

            // Find nearest blocking TERRAIN first, ignoring enemies and Player.
            for (int i = 0; i < count; i++)
            {
                RaycastHit2D hit = _rayHits[i];
                Collider2D collider = hit.collider;
                if (collider == null || collider.isTrigger) continue;
                if (collider.GetComponentInParent<PlayerHealth>() != null) continue;
                if (collider.GetComponentInParent<AssassinShadowClone>() != null) continue;
                if (collider.GetComponentInParent<AssassinSoulPillar>() != null) continue;
                if (collider.GetComponentInParent<EnemyHealth>() != null) continue;
                if (hit.distance < stopAt) stopAt = hit.distance;
            }

            // Hit each enemy ONCE along the travelled segment (piercing).
            for (int i = 0; i < count; i++)
            {
                RaycastHit2D hit = _rayHits[i];
                if (hit.collider == null || hit.distance > stopAt + .001f) continue;
                EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
                if (enemy == null || enemy.IsDead || !_damaged.Add(enemy)) continue;
                float amount = _controller != null
                    ? _controller.DamageAgainst(enemy, _damage, _snapshot) : _damage;
                if (amount > 0f) enemy.TakeDamage(amount);
            }

            transform.position = start + _heading * stopAt;
            _travelled += stopAt;
            int frameIndex = Mathf.Clamp(
                Mathf.FloorToInt(_travelled / MaxDistance * _frames.Length),
                0, _frames.Length - 1);
            _render.sprite = _frames[frameIndex];

            // Stop at a wall (or maximum range), never pass through solid terrain.
            if (stopAt < step - .0005f || _travelled >= MaxDistance - .0005f)
                Destroy(gameObject);
        }
    }
}
