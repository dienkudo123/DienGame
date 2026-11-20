using System.Collections.Generic;
using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Map;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Mage
{
    /// <summary>Small class-specific homing bolt; no Marksman Projectile/weapon coupling.</summary>
    public sealed class MageProjectile : MonoBehaviour
    {
        private EnemyHealth _target;
        private Vector2 _direction;
        private float _speed;
        private float _damage;
        private int _pierce;
        private float _age;
        private MageSkillController _skills;
        private SpriteRenderer _renderer;
        private bool _normalAttack;
        private bool _evolved;
        private readonly HashSet<int> _hitIds = new HashSet<int>();

        public static MageProjectile Spawn(Vector2 start, EnemyHealth target, Vector2 direction,
            float damage, float speed, int pierce, MageSkillController skills, bool normalAttack, bool evolved = false)
        {
            GameObject obj = new GameObject(normalAttack ? "Mage_Attack" : "Mage_CrystalNeedle");
            obj.transform.position = start;
            SpriteRenderer r = obj.AddComponent<SpriteRenderer>();
            r.sortingOrder = 45;
            r.sprite = MageArt.Effect("TinhChamPhanLiet", normalAttack ? 2 : 4, evolved);
            obj.transform.localScale = Vector3.one * (normalAttack ? .48f : .7f);
            MageProjectile bolt = obj.AddComponent<MageProjectile>();
            bolt._renderer = r;
            bolt._target = target;
            bolt._direction = direction.sqrMagnitude > .01f ? direction.normalized : Vector2.right;
            bolt._speed = Mathf.Max(2f, speed);
            bolt._damage = damage;
            bolt._pierce = Mathf.Max(1, pierce);
            bolt._skills = skills;
            bolt._normalAttack = normalAttack;
            bolt._evolved = evolved;
            return bolt;
        }

        private void Update()
        {
            _age += Time.deltaTime;
            if (_age > 2.8f) { Destroy(gameObject); return; }
            if (_target != null && !_target.IsDead)
            {
                Vector2 to = (Vector2)_target.transform.position - (Vector2)transform.position;
                if (to.sqrMagnitude > .002f) _direction = to.normalized;
            }
            float step = _speed * Time.deltaTime;
            Vector2 start = transform.position;
            Vector2 next = start + _direction * step;
            foreach (RaycastHit2D hit in Physics2D.LinecastAll(start, next))
            {
                if (hit.collider == null || hit.collider.isTrigger) continue;
                EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
                if (enemy != null && !enemy.IsDead)
                {
                    if (_hitIds.Add(enemy.GetInstanceID()))
                    {
                        _skills?.DealDamage(enemy, _damage, false);
                        MageVfx.Play("TinhChamPhanLiet", hit.point, .55f, .24f, evolved: _evolved);
                        _pierce--;
                        if (_pierce <= 0) { Destroy(gameObject); return; }
                        // Don't home back to a previously hit enemy.
                        _target = MageTargeting.Nearest(next, 3f, enemy);
                    }
                    continue;
                }
                MapObstacle obstacle = hit.collider.GetComponentInParent<MapObstacle>();
                if (obstacle != null && obstacle.BlockProjectile) { Destroy(gameObject); return; }
            }
            transform.position = next;
            float degrees = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, degrees);
        }
    }
}
