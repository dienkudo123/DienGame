using DienGame.Gameplay.Balance;
using DienGame.Client;
using DienGame.Gameplay.Equipment;
using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Player;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Mage
{
    /// <summary>Playable Mage combat root. Reuses PlayerMovement/HP/EXP/SkillManager.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerHealth))]
    public sealed class MageClassController : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float leashRadius = 5f;
        [SerializeField, Min(1f)] private float attackRadius = 6f;
        [SerializeField, Min(.1f)] private float magicProjectileSpeed = 12f;
        [SerializeField, Min(.1f)] private float companionRespawn = 10f;
        [SerializeField, Min(.1f)] private float companionBaseHpRatio = .8f;

        private PlayerCharacter _stats;
        private PlayerHealth _health;
        private MageSkillController _skills;
        private ChimeraCompanion _companion;
        private float _attackTimer;

        public float LeashRadius => leashRadius;
        public float AttackRadius => attackRadius;
        public float AttackDamage => _stats != null ? _stats.Damage : GameBalance.PlayerDamage;
        public float CompanionMaxHealth => (_health != null ? _health.MaxHealth : GameBalance.PlayerMaxHealth) * companionBaseHpRatio;
        public float CompanionRespawn => companionRespawn;
        public ChimeraCompanion Companion => _companion;

        private void Awake()
        {
            _stats = GetComponent<PlayerCharacter>();
            _health = GetComponent<PlayerHealth>();
            _skills = GetComponent<MageSkillController>();
        }

        private void Start()
        {
            CreateCompanion();
        }

        private void Update()
        {
            if (_health != null && _health.IsDead) return;
            if (_skills == null) _skills = GetComponent<MageSkillController>();
            _attackTimer -= Time.deltaTime;
            if (_attackTimer > 0f) return;
            EnemyHealth nearest = MageTargeting.Nearest(transform.position, attackRadius);
            if (nearest == null) return;
            Vector2 origin = transform.position;
            Vector2 target = nearest.transform.position;
            Vector2 forward = (target - origin).normalized;
            MageProjectile.Spawn(origin + forward * .4f, nearest, forward,
                AttackDamage * EquipmentResonance.DamageFactor(EquipmentInventory.Selected(PlayableClass.Mage)),
                magicProjectileSpeed, 1, _skills, true);
            _attackTimer = _stats != null ? Mathf.Max(.15f, _stats.AttackInterval) : 1f;
        }

        private void CreateCompanion()
        {
            GameObject pet = new GameObject("Chimera_Summoned_MageCompanion");
            pet.transform.position = transform.position + Vector3.right * 1.3f;
            Rigidbody2D rb = pet.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            CircleCollider2D collider = pet.AddComponent<CircleCollider2D>();
            collider.radius = .34f;
            SpriteRenderer r = pet.AddComponent<SpriteRenderer>();
            r.sortingOrder = 20;
            r.sprite = MageArt.Chimera(2, 0);
            foreach (Collider2D own in GetComponentsInChildren<Collider2D>(true))
                if (own != null) Physics2D.IgnoreCollision(collider, own, true);
            _companion = pet.AddComponent<ChimeraCompanion>();
            _companion.Initialize(this, _skills);
        }

        private void OnDestroy()
        {
            if (_companion != null) Destroy(_companion.gameObject);
        }
    }

    /// <summary>Shared target lookup, only searches real EnemyHealth objects.</summary>
    internal static class MageTargeting
    {
        public static EnemyHealth Nearest(Vector2 origin, float radius, EnemyHealth exclude = null,
            Vector2? leashOrigin = null, float leashRadius = 0f)
        {
            float best = radius * radius;
            EnemyHealth result = null;
            foreach (EnemyHealth e in EnemyHealth.Active)
            {
                if (e == null || e.IsDead || e == exclude) continue;
                Vector2 p = e.transform.position;
                if (leashOrigin.HasValue && (p - leashOrigin.Value).sqrMagnitude > leashRadius * leashRadius) continue;
                float sqr = (p - origin).sqrMagnitude;
                if (sqr >= best) continue;
                best = sqr;
                result = e;
            }
            return result;
        }
    }
}
