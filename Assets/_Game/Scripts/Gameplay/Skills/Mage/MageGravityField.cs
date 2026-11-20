using DienGame.Gameplay.Enemy;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Mage
{
    /// <summary>Duration-based AoE, collision-aware movement remains handled by each Enemy Rigidbody.</summary>
    public sealed class MageGravityField : MonoBehaviour
    {
        private float _remaining;
        private float _tick;
        private float _damage;
        private bool _evolved;
        private MageSkillController _owner;
        private const float Radius = 2.15f;

        public static void Create(Vector2 at, float damage, bool evolved, MageSkillController owner)
        {
            GameObject field = new GameObject("Mage_GravityField");
            field.transform.position = at;
            MageGravityField logic = field.AddComponent<MageGravityField>();
            logic._remaining = 2f;
            logic._damage = damage;
            logic._evolved = evolved;
            logic._owner = owner;
            MageVfx.Play("TrongTruongDiGioi", at, 2.6f, 2f, true, evolved: evolved);
        }

        private void Update()
        {
            _remaining -= Time.deltaTime;
            _tick -= Time.deltaTime;
            if (_tick <= 0f)
            {
                _tick += .35f;
                foreach (EnemyHealth e in EnemyHealth.Active)
                {
                    if (e == null || e.IsDead) continue;
                    Vector2 delta = (Vector2)transform.position - (Vector2)e.transform.position;
                    if (delta.sqrMagnitude > Radius * Radius) continue;
                    _owner?.DealDamage(e, _damage * .35f, false, false);
                    // Impulse, not teleport: enemies still collide with actual solid terrain.
                    Rigidbody2D rb = e.GetComponent<Rigidbody2D>();
                    if (rb != null && delta.sqrMagnitude > .15f)
                        rb.AddForce(delta.normalized * 4f, ForceMode2D.Impulse);
                }
            }
            if (_remaining > 0f) return;
            if (_evolved && _owner != null)
            {
                _owner.DamageArea(transform.position, 2.3f, _damage * 1.4f, false);
                MageVfx.Play("TrongTruongDiGioi", transform.position, 2.9f, .38f, evolved: true);
            }
            Destroy(gameObject);
        }
    }
}
