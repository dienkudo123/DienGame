using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Player;
using DienGame.Gameplay.Skills;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Warrior
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerSkillManager))]
    public sealed class WarriorSkillController : MonoBehaviour
    {
        private PlayerSkillManager _levels;
        private PlayerHealth _health;
        private int _hitCount;
        private float _shockReadyAt, _heatReadyAt, _healReadyAt;
        public int Level(SkillId id) => _levels != null ? _levels.GetSkillLevel(id) : 0;
        private void Awake()
        {
            _levels = GetComponent<PlayerSkillManager>();
            _health = GetComponent<PlayerHealth>();
        }
        public float GuardFactor => 1f - Mathf.Min(.14f, .04f + .02f * Level(SkillId.WarriorBulwark)) *
            (Level(SkillId.WarriorBulwark) > 0 ? 1f : 0f);
        public float IntervalFactor => 1f - .03f * Level(SkillId.WarriorTempo);
        public float DamageFactor(EnemyHealth target, int comboStep)
        {
            float multiplier = comboStep == 3 ? 1.16f + .07f * Level(SkillId.WarriorDrive) : 1f;
            if (target != null && target.GetComponent<BossCombatController>() != null)
                multiplier *= 1f + .03f * Level(SkillId.WarriorBreaker);
            return multiplier;
        }
        // Called once PER normal slash with a valid primary target, never for passive hits.
        public void OnPrimaryHit(EnemyHealth primary, float baseDamage, int comboStep)
        {
            if (primary == null) return;
            _hitCount++;
            int heatLevel = Level(SkillId.WarriorHeat);
            if (heatLevel > 0 && _hitCount % 5 == 0 && Time.time >= _heatReadyAt)
            {
                _heatReadyAt = Time.time + 5f;
                // Single-target heat discharge; only a visual, not an indefinite Burn.
                if (!primary.IsDead) primary.TakeDamage(baseDamage * (.12f + .05f * heatLevel));
            }
            int regen = Level(SkillId.WarriorRegeneration);
            if (regen > 0 && _hitCount % 5 == 0 && Time.time >= _healReadyAt &&
                _health != null && !_health.IsDead && _health.CurrentHealth < _health.MaxHealth)
            {
                _healReadyAt = Time.time + 12f;
                _health.Heal(_health.MaxHealth * .0035f * regen);
            }
            int shock = Level(SkillId.WarriorShockwave);
            if (comboStep != 3 || shock == 0 || Time.time < _shockReadyAt) return;
            _shockReadyAt = Time.time + 4f;
            Vector2 center = primary.transform.position;
            int hits = 0;
            foreach (EnemyHealth other in EnemyHealth.Active)
            {
                if (other == null || other.IsDead || other == primary ||
                    ((Vector2)other.transform.position - center).sqrMagnitude > 1.65f * 1.65f) continue;
                if (WarriorMeleeAttack.HasSolidObstacle(center, other.transform.position)) continue;
                other.TakeDamage(baseDamage * (.10f + .05f * shock));
                if (++hits >= 2) break;
            }
        }
    }
}
