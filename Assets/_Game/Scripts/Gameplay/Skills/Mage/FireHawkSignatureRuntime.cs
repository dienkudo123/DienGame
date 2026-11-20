using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Equipment;
using DienGame.Gameplay.Map;
using DienGame.Gameplay.Skills;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Mage
{
    /// <summary>
    /// Phoenix-only signature: Song Trung Oanh Kich. At most two enemies, separated
    /// from normal hits/chips; never triggers Mage combo recursively. A finite cooldown
    /// and the regular attack's slower base interval keep the skill within budget.
    /// </summary>
    public sealed class FireHawkSignatureRuntime
    {
        public const float Duration = .70f;
        private const float Cooldown = 7.0f;
        private const int HitsNeeded = 4;

        private int _normalHits;
        private float _readyAt;
        private float _elapsed = Duration;
        private bool _launched;
        private bool _applied;
        private int _level;
        private EnemyHealth _primary;
        private EnemyHealth _secondary;
        private MageSkillController _skills;
        private float _attackDamage;
        private Vector2 _origin;
        private Vector2 _impact;
        private bool _preview;

        public bool IsCasting => _elapsed < Duration;
        public float CastRemainingFraction => Mathf.Clamp01(1f - _elapsed / Duration);
        public bool IsLevel5 => _level >= 5;
        public int CastFrame => Mathf.Clamp(Mathf.FloorToInt(_elapsed / Duration * 5f), 0, 4);

        public void RegisterNormalHit(EnemyHealth primary, float attackDamage,
            MageSkillController skills, Vector2 origin)
        {
            if (IsCasting || primary == null || primary.IsDead || attackDamage <= 0f) return;
            _normalHits++;
            if (_normalHits < HitsNeeded || Time.time < _readyAt) return;
            _normalHits = 0;
            _readyAt = Time.time + Cooldown;
            _primary = primary;
            _skills = skills;
            _attackDamage = attackDamage;
            _origin = origin;
            _impact = primary.transform.position;
            _secondary = FindSecond(_impact, primary);
            // Lv1 baseline; Lv5 uses different painted cast frames, two vivid fire trails.
            _level = skills != null ? Mathf.Clamp(skills.Level(SkillId.MageCrystalStrike), 1, 5) : 1;
            _preview = false;
            BeginCast();
        }

        public void Preview(Vector2 origin, Vector2 forward, bool level5)
        {
            _origin = origin;
            if (forward.sqrMagnitude < .001f) forward = Vector2.right;
            _impact = origin + forward.normalized * 2.0f;
            _primary = null;
            _secondary = null;
            _skills = null;
            _level = level5 ? 5 : 1;
            _preview = true;
            BeginCast();
        }

        private void BeginCast()
        {
            _elapsed = 0f;
            _launched = false;
            _applied = false;
        }

        public void Tick(float seconds, Vector2 petPosition)
        {
            if (!IsCasting) return;
            _elapsed += Mathf.Max(0f, seconds);
            _origin = petPosition;

            if (!_launched && _elapsed >= .28f)
            {
                _launched = true;
                Vector2 target = _primary != null && !_primary.IsDead ? (Vector2)_primary.transform.position : _impact;
                _impact = target;
                FireHawkTwinFlameVfx.Launch(_origin, target, IsLevel5, 0);
                // Second illustrated trail diverges toward a second enemy if available.
                Vector2 targetTwo = _secondary != null && !_secondary.IsDead ? (Vector2)_secondary.transform.position
                    : target + new Vector2(-(_impact - _origin).y, (_impact - _origin).x).normalized * .28f;
                FireHawkTwinFlameVfx.Launch(_origin, targetTwo, IsLevel5, 1);
            }
            if (!_applied && _elapsed >= .58f)
            {
                _applied = true;
                ApplyImpact();
            }
        }

        private void ApplyImpact()
        {
            float growth = Mathf.Lerp(1f, 1.26f, Mathf.Clamp01((_level - 1f) / 4f));
            float primaryPower = _attackDamage * .31f * growth;
            float secondaryPower = _attackDamage * .15f * growth;
            if (!_preview && _primary != null && !_primary.IsDead)
            {
                EnemyEquipmentStatus.ApplyHit(_primary, EquipmentEffect.Burn, primaryPower);
                Deal(_primary, primaryPower);
            }
            if (!_preview && _secondary != null && !_secondary.IsDead)
            {
                EnemyEquipmentStatus.ApplyHit(_secondary, EquipmentEffect.Burn, secondaryPower);
                Deal(_secondary, secondaryPower);
            }
            // Orange illustrated impact, not the old purple generic Mage skill ring.
            FireHawkImpactAnimation.Spawn(_impact, IsLevel5);
            if (_secondary != null && !_secondary.IsDead)
                FireHawkImpactAnimation.Spawn(_secondary.transform.position, IsLevel5);
        }

        private void Deal(EnemyHealth enemy, float amount)
        {
            if (_skills != null) _skills.DealDamage(enemy, amount, true, false);
            else enemy.TakeDamage(amount);
        }

        private static EnemyHealth FindSecond(Vector2 origin, EnemyHealth exclude)
        {
            EnemyHealth best = null;
            float bestSq = 1.85f * 1.85f;
            foreach (EnemyHealth e in EnemyHealth.Active)
            {
                if (e == null || e.IsDead || e == exclude) continue;
                Vector2 diff = (Vector2)e.transform.position - origin;
                if (diff.sqrMagnitude >= bestSq || Blocked(origin, diff)) continue;
                bestSq = diff.sqrMagnitude;
                best = e;
            }
            return best;
        }

        private static bool Blocked(Vector2 origin, Vector2 direction)
        {
            if (direction.sqrMagnitude < .001f) return false;
            foreach (RaycastHit2D hit in Physics2D.RaycastAll(origin, direction.normalized, direction.magnitude))
            {
                if (hit.collider == null || hit.collider.isTrigger) continue;
                MapObstacle solid = hit.collider.GetComponentInParent<MapObstacle>();
                if (solid != null && solid.BlockProjectile) return true;
            }
            return false;
        }
    }
}
