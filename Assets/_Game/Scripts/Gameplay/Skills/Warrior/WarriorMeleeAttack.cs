using System.Collections.Generic;
using DienGame.Client;
using DienGame.Gameplay.Balance;
using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Equipment;
using DienGame.Gameplay.Map;
using DienGame.Gameplay.Player;
using UnityEngine;
using DienGame.Gameplay.Phase13;

namespace DienGame.Gameplay.Skills.Warrior
{
    /// <summary>
    /// Three-beat melee combo; 360-degree joystick movement, eight-direction art.
    /// Sword styles exchange single-target DPS against reach, arc, slow or burn.
    /// Collider overlap plus LOS checks prevent slashing through walls.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerTargeting))]
    public sealed class WarriorMeleeAttack : MonoBehaviour
    {
        private PlayerTargeting _targeting;
        private PlayerCharacter _character;
        private WarriorVisual _visual;
        private WarriorSkillController _skills;
        private PlayerHealth _health;
        private float _cooldown, _lastSwingAt;
        private int _combo;
        private readonly List<EnemyHealth> _candidates = new List<EnemyHealth>(24);
        public int ComboStep => _combo;
        private void Awake()
        {
            _targeting = GetComponent<PlayerTargeting>();
            _character = GetComponent<PlayerCharacter>();
            _health = GetComponent<PlayerHealth>();
            _visual = GetComponentInChildren<WarriorVisual>(true);
            _skills = GetComponent<WarriorSkillController>();
        }
        private void Start()
        {
            if (_character == null) return;
            // Durable and slower than Assassin, but no free overpowering DPS.
            float oldMax = _character.MaxHealth;
            _character.AddMaxHealth(25f);
            _character.MultiplyMoveSpeed(.94f);
            _character.MultiplyAttackInterval(1.12f);
            if (_health != null) _health.Heal(_character.MaxHealth - oldMax);
        }
        private void Update()
        {
            if (_cooldown > 0f) _cooldown -= Time.deltaTime;
            if (_cooldown > 0f || _targeting == null || (_health != null && _health.IsDead)) return;
            EquipmentDefinition sword = EquipmentInventory.Selected(PlayableClass.Warrior);
            float reach = Reach(sword);
            Vector2 origin = transform.position;
            // PlayerTargeting was designed around ranged attacks. If its nearest
            // enemy is behind a wall, pick another valid enemy within MELEE reach.
            EnemyHealth target = FindReachableEnemy(origin, reach);
            if (target == null) return;
            Vector2 direction = (Vector2)target.transform.position - origin;
            if (Time.time - _lastSwingAt > 2.1f) _combo = 0;
            _combo = _combo % 3 + 1;
            _lastSwingAt = Time.time;
            direction.Normalize();
            Slash(direction, sword, reach);
            float interval = _character != null ? _character.AttackInterval : GameBalance.PlayerAttackInterval;
            // Phase08 resonance interval was already applied to PlayerCharacter
            // by WorldEquipmentStatApplier; do not multiply it a second time.
            interval *= _skills != null ? _skills.IntervalFactor : 1f;
            if (sword != null && sword.Id == "warrior_great") interval *= 1.18f;
            _cooldown = Mathf.Max(.22f, interval);
        }
        private static float Reach(EquipmentDefinition sword) =>
            sword != null && sword.Id == "warrior_great" ? 2.45f : 1.85f;
        private static float Arc(EquipmentDefinition sword) =>
            sword != null && sword.Id == "warrior_great" ? 225f : 150f;
        private EnemyHealth FindReachableEnemy(Vector2 origin, float reach)
        {
            EnemyHealth best = null;
            float bestDistance = reach * reach;
            foreach (EnemyHealth enemy in EnemyHealth.Active)
            {
                if (enemy == null || enemy.IsDead) continue;
                Vector2 position = enemy.transform.position;
                float sqr = (position - origin).sqrMagnitude;
                if (sqr <= .002f || sqr >= bestDistance || HasSolidObstacle(origin, position)) continue;
                best = enemy;
                bestDistance = sqr;
            }
            return best;
        }

        private void Slash(Vector2 forward, EquipmentDefinition sword, float reach)
        {
            if (_visual == null) _visual = GetComponentInChildren<WarriorVisual>(true);
            Phase13Audio.Play(Phase13Cue.Slash);
            _visual?.PlaySlash(forward, _combo);
            float baseDamage = (_character != null ? _character.Damage : GameBalance.PlayerDamage) *
                (sword != null ? sword.DamageMultiplier : 1f) * EquipmentResonance.DamageFactor(sword);
            float minDot = Mathf.Cos(Arc(sword) * .5f * Mathf.Deg2Rad);
            Vector2 origin = transform.position;
            _candidates.Clear();
            foreach (EnemyHealth enemy in EnemyHealth.Active)
            {
                if (enemy == null || enemy.IsDead) continue;
                Vector2 delta = (Vector2)enemy.transform.position - origin;
                float d2 = delta.sqrMagnitude;
                if (d2 > reach * reach ||
                    (d2 > .001f && Vector2.Dot(forward, delta.normalized) < minDot)) continue;
                if (HasSolidObstacle(origin, enemy.transform.position)) continue;
                _candidates.Add(enemy);
            }
            if (_candidates.Count == 0) return;
            // Nearest-first ensures the target cap does not depend on an enemy's
            // spawn/order in EnemyHealth.Active. List is cached; no per-slash alloc.
            _candidates.Sort((a, b) =>
                ((Vector2)a.transform.position - origin).sqrMagnitude.CompareTo(
                ((Vector2)b.transform.position - origin).sqrMagnitude));
            int count = Mathf.Min(_candidates.Count, sword != null && sword.Id == "warrior_great" ? 4 : 2);
            EnemyHealth primary = _candidates[0];
            for (int i = 0; i < count; i++) HitEnemy(_candidates[i], sword, baseDamage);
            _skills?.OnPrimaryHit(primary, baseDamage, _combo);
        }

        private void HitEnemy(EnemyHealth enemy, EquipmentDefinition sword, float baseDamage)
        {
            if (enemy == null || enemy.IsDead) return;
            float actual = baseDamage * (_skills != null ? _skills.DamageFactor(enemy, _combo) :
                (_combo == 3 ? 1.16f : 1f));
            if (sword != null) EnemyEquipmentStatus.ApplyHit(enemy, sword.Effect, actual);
            enemy.TakeDamage(actual);
        }
        public static bool HasSolidObstacle(Vector2 origin, Vector2 target)
        {
            Vector2 delta = target - origin;
            float distance = delta.magnitude;
            if (distance < .05f) return false;
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, delta / distance, distance);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null || hit.collider.isTrigger) continue;
                MapObstacle obstacle = hit.collider.GetComponentInParent<MapObstacle>();
                if (obstacle != null && obstacle.BlockProjectile) return true;
            }
            return false;
        }
    }
}
