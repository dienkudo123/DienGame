using System.Collections;
using System.Collections.Generic;
using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Player;
using DienGame.Gameplay.Skills;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Mage
{
    /// <summary>
    /// Mage combat orchestrator. Passive triggers are independent of the 2nd joystick:
    /// Chimera attacks in manual and autonomous modes. Only one skill is defensive.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerSkillManager))]
    public sealed class MageSkillController : MonoBehaviour
    {
        private sealed class ResonanceCounter
        {
            public int playerHits;
            public int petHits;
            public float lastHit;
            public float nextAllowed;
        }

        private readonly Dictionary<int, ResonanceCounter> _counters = new Dictionary<int, ResonanceCounter>();
        private readonly Dictionary<int, float> _vulnerableUntil = new Dictionary<int, float>();
        private PlayerSkillManager _levels;
        private MageClassController _mage;
        private PlayerHealth _player;
        private float _needlesTimer;
        private float _gravityTimer;
        private float _cannonTimer;
        private float _shieldCooldown;
        private float _shieldRemaining;
        private float _playerShield;
        private float _petShield;
        private int _petNormalHits;
        private int _petCannonHits;
        private float _pruneTimer;

        public int Level(SkillId id) => _levels != null ? _levels.GetSkillLevel(id) : 0;

        private void Awake()
        {
            _levels = GetComponent<PlayerSkillManager>();
            _mage = GetComponent<MageClassController>();
            _player = GetComponent<PlayerHealth>();
        }

        private void Update()
        {
            if (_mage == null || _player == null || _player.IsDead) return;
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            _needlesTimer -= dt;
            _gravityTimer -= dt;
            _cannonTimer -= dt;
            _shieldCooldown -= dt;
            _pruneTimer -= dt;

            if (_shieldRemaining > 0f)
            {
                _shieldRemaining -= dt;
                if (_shieldRemaining <= 0f)
                {
                    _playerShield = 0f;
                    _petShield = 0f;
                    if (Level(SkillId.MageDualShield) >= 5) PulseShield();
                }
            }
            TriggerShieldIfNeeded();
            if (_pruneTimer <= 0f)
            {
                _pruneTimer = 10f;
                PruneExpired();
            }

            int lvl = Level(SkillId.MageCrystalNeedles);
            if (lvl > 0 && _needlesTimer <= 0f)
            {
                EnemyHealth target = MageTargeting.Nearest(transform.position, _mage.AttackRadius);
                if (target != null)
                {
                    FireNeedles(lvl, target);
                    _needlesTimer = MageSkillCatalog.NeedleInterval(lvl);
                }
            }

            lvl = Level(SkillId.MageGravityWell);
            if (lvl > 0 && _gravityTimer <= 0f)
            {
                if (SpawnGravity(lvl)) _gravityTimer = 8f;
            }

            lvl = Level(SkillId.MageBiocannon);
            ChimeraCompanion pet = _mage.Companion;
            if (lvl > 0 && pet != null && pet.Alive && _cannonTimer <= 0f)
            {
                EnemyHealth target = MageTargeting.Nearest(pet.transform.position, 6f, null,
                    transform.position, _mage.LeashRadius);
                if (target != null)
                {
                    FireBiocannon(lvl, target);
                    _cannonTimer = 6f;
                }
            }
        }

        public float DealDamage(EnemyHealth enemy, float raw, bool fromPet, bool combo = true)
        {
            if (enemy == null || enemy.IsDead || raw <= 0f) return 0f;
            int id = enemy.GetInstanceID();
            float adjusted = _vulnerableUntil.TryGetValue(id, out float until) && until > Time.time ? raw * 1.2f : raw;
            enemy.TakeDamage(adjusted);
            // Additional skill/AoE events must not recursively feed combo counters.
            if (combo && Level(SkillId.MagePredatorResonance) > 0) RegisterResonance(enemy, fromPet);
            return adjusted;
        }

        public void DamageArea(Vector2 center, float radius, float damage, bool fromPet)
        {
            float sqr = radius * radius;
            foreach (EnemyHealth enemy in EnemyHealth.Active)
            {
                if (enemy != null && !enemy.IsDead && ((Vector2)enemy.transform.position - center).sqrMagnitude <= sqr)
                    DealDamage(enemy, damage, fromPet, false);
            }
        }

        private void FireNeedles(int level, EnemyHealth first)
        {
            int count = level >= 5 ? 4 : 2;
            EnemyHealth[] candidates = new List<EnemyHealth>(EnemyHealth.Active).ToArray();
            List<EnemyHealth> valid = new List<EnemyHealth>();
            foreach (EnemyHealth e in candidates)
            {
                if (e != null && !e.IsDead &&
                    ((Vector2)e.transform.position - (Vector2)transform.position).sqrMagnitude <= _mage.AttackRadius * _mage.AttackRadius)
                    valid.Add(e);
            }
            valid.Sort((a,b) => ((Vector2)a.transform.position - (Vector2)transform.position).sqrMagnitude
                .CompareTo(((Vector2)b.transform.position - (Vector2)transform.position).sqrMagnitude));
            for (int i = 0; i < count; i++)
            {
                EnemyHealth target = valid.Count > 0 ? valid[i % valid.Count] : first;
                Vector2 origin = transform.position;
                Vector2 delta = (Vector2)target.transform.position - origin;
                Vector2 forward = delta.sqrMagnitude > .01f ? delta.normalized : Vector2.right;
                MageProjectile.Spawn(origin + forward * .35f, target, forward,
                    _mage.AttackDamage * MageSkillCatalog.NeedleRatio(level), 13f,
                    level >= 5 ? 2 : 1, this, false, level >= 5);
            }
            MageVfx.Play("TinhChamPhanLiet", transform.position, .7f, .25f, evolved: level >= 5);
        }

        private bool SpawnGravity(int level)
        {
            EnemyHealth[] enemies = new List<EnemyHealth>(EnemyHealth.Active).ToArray();
            EnemyHealth best = null;
            int most = -1;
            foreach (EnemyHealth e in enemies)
            {
                if (e == null || e.IsDead || ((Vector2)e.transform.position - (Vector2)transform.position).sqrMagnitude > _mage.AttackRadius * _mage.AttackRadius) continue;
                int count = 0;
                foreach (EnemyHealth other in enemies)
                    if (other != null && !other.IsDead && ((Vector2)other.transform.position - (Vector2)e.transform.position).sqrMagnitude <= 2.2f * 2.2f) count++;
                if (count <= most) continue;
                most = count;
                best = e;
            }
            if (best == null) return false;
            MageGravityField.Create(best.transform.position, _mage.AttackDamage * (.50f + level * .15f), level == 5, this);
            return true;
        }

        public void OnCompanionNormalHit(EnemyHealth enemy)
        {
            _petNormalHits++;
            _petCannonHits++;
            ChimeraCompanion pet = _mage != null ? _mage.Companion : null;
            if (pet == null || !pet.Alive) return;
            int lvl = Level(SkillId.MageMetalClaws);
            if (lvl > 0 && _petNormalHits >= 3)
            {
                _petNormalHits = 0;
                Vector2 facing = enemy != null ? (Vector2)enemy.transform.position - (Vector2)pet.transform.position : Vector2.right;
                if (facing.sqrMagnitude < .01f) facing = Vector2.right;
                facing.Normalize();
                float range = 2.3f, minDot = Mathf.Cos(65f * Mathf.Deg2Rad);
                foreach (EnemyHealth e in EnemyHealth.Active)
                {
                    if (e == null || e.IsDead) continue;
                    Vector2 offset = (Vector2)e.transform.position - (Vector2)pet.transform.position;
                    if (offset.sqrMagnitude > range * range || (offset.sqrMagnitude > .01f && Vector2.Dot(offset.normalized, facing) < minDot)) continue;
                    DealDamage(e, pet.AttackDamage * MageSkillCatalog.MetalClawRatio(lvl), true, false);
                    if (lvl >= 5) _vulnerableUntil[e.GetInstanceID()] = Time.time + 3f;
                }
                MageVfx.Play("TraoLietHopKim", (Vector2)pet.transform.position + facing * 1f, 1.6f,
                    .34f, false, Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg, evolved: lvl >= 5);
            }

            lvl = Level(SkillId.MageCrystalStrike);
            if (lvl > 0 && _petCannonHits >= 5)
            {
                _petCannonHits = 0;
                if (enemy != null && !enemy.IsDead)
                    StartCoroutine(FallingCrystal((Vector2)enemy.transform.position, lvl));
            }
        }

        private IEnumerator FallingCrystal(Vector2 position, int level)
        {
            MageVfx.Play("SongTrungOanhKich", position, 1.4f, .42f, evolved: level >= 5);
            yield return new WaitForSeconds(.4f);
            if (_mage == null) yield break;
            float amount = _mage.AttackDamage * (level == 5 ? 1.15f : .55f + level * .15f);
            DamageArea(position, 1.35f, amount, false);
            MageVfx.Play("SongTrungOanhKich", position, 1.7f, .38f, evolved: level >= 5);
            if (level >= 5)
            {
                // Two distinct nearby monsters; do not land both side crystals on the same one.
                List<EnemyHealth> nearby = new List<EnemyHealth>();
                foreach (EnemyHealth e in EnemyHealth.Active)
                {
                    if (e == null || e.IsDead) continue;
                    float distance = ((Vector2)e.transform.position - position).sqrMagnitude;
                    if (distance >= .5f * .5f && distance <= 3.1f * 3.1f) nearby.Add(e);
                }
                nearby.Sort((a, b) =>
                    ((Vector2)a.transform.position - position).sqrMagnitude.CompareTo(
                    ((Vector2)b.transform.position - position).sqrMagnitude));
                for (int i = 0; i < Mathf.Min(2, nearby.Count); i++)
                {
                    Vector2 otherPos = nearby[i].transform.position;
                    DamageArea(otherPos, .6f, amount * .5f, false);
                    MageVfx.Play("SongTrungOanhKich", otherPos, .75f, .3f, evolved: level >= 5);
                }
            }
        }

        private void FireBiocannon(int level, EnemyHealth target)
        {
            ChimeraCompanion pet = _mage.Companion;
            if (pet == null || !pet.Alive) return;
            Vector2 origin = pet.transform.position;
            Vector2 delta = (Vector2)target.transform.position - origin;
            if (delta.sqrMagnitude < .02f) return;
            Vector2 forward = delta.normalized;
            float maxDistance = 5.6f;
            foreach (RaycastHit2D hit in Physics2D.RaycastAll(origin, forward, maxDistance))
            {
                if (hit.collider == null || hit.collider.isTrigger || hit.collider.GetComponentInParent<EnemyHealth>() != null) continue;
                DienGame.Gameplay.Map.MapObstacle obstacle = hit.collider.GetComponentInParent<DienGame.Gameplay.Map.MapObstacle>();
                if (obstacle != null && obstacle.BlockProjectile) { maxDistance = Mathf.Min(maxDistance, hit.distance); break; }
            }
            EnemyHealth firstHit = null;
            float power = pet.AttackDamage * (.65f + level * .15f);
            foreach (EnemyHealth enemy in EnemyHealth.Active)
            {
                if (enemy == null || enemy.IsDead) continue;
                Vector2 offset = (Vector2)enemy.transform.position - origin;
                float along = Vector2.Dot(offset, forward);
                if (along < 0f || along > maxDistance) continue;
                if ((offset - forward * along).sqrMagnitude > .46f * .46f) continue;
                DealDamage(enemy, power, true, false);
                if (firstHit == null) firstHit = enemy;
            }
            float angle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;
            MageVfx.Play("PhaoSinhThe", origin + forward * maxDistance * .5f,
                Mathf.Max(.6f, maxDistance * .45f), .4f, false, angle, evolved: level >= 5);
            if (level >= 5 && firstHit != null)
            {
                // 2 fork hits. Avoid double-hitting the first target.
                EnemyHealth[] candidates = new List<EnemyHealth>(EnemyHealth.Active).ToArray();
                List<EnemyHealth> extras = new List<EnemyHealth>();
                foreach (EnemyHealth e in candidates)
                    if (e != null && !e.IsDead && e != firstHit && ((Vector2)e.transform.position - (Vector2)firstHit.transform.position).sqrMagnitude <= 4f * 4f) extras.Add(e);
                extras.Sort((a,b) => ((Vector2)a.transform.position - (Vector2)firstHit.transform.position).sqrMagnitude.CompareTo(((Vector2)b.transform.position - (Vector2)firstHit.transform.position).sqrMagnitude));
                for (int i = 0; i < Mathf.Min(2, extras.Count); i++)
                {
                    DealDamage(extras[i], power * .5f, true, false);
                    MageVfx.Play("PhaoSinhThe", extras[i].transform.position, .65f, .28f, evolved: level >= 5);
                }
            }
        }

        private void RegisterResonance(EnemyHealth enemy, bool fromPet)
        {
            if (enemy == null || enemy.IsDead) return;
            int lvl = Level(SkillId.MagePredatorResonance);
            if (lvl == 0) return;
            int id = enemy.GetInstanceID();
            if (!_counters.TryGetValue(id, out ResonanceCounter counter))
            {
                counter = new ResonanceCounter();
                _counters.Add(id, counter);
            }
            if (counter.lastHit + 3f < Time.time) { counter.petHits = 0; counter.playerHits = 0; }
            counter.lastHit = Time.time;
            if (fromPet) counter.petHits++; else counter.playerHits++;
            if (Time.time < counter.nextAllowed) return;
            if (counter.petHits < 1 || counter.playerHits < 1) return;
            if (counter.petHits + counter.playerHits < (lvl == 5 ? 3 : 4)) return;
            counter.petHits = counter.playerHits = 0;
            counter.nextAllowed = Time.time + 2f;
            float damage = (_mage.Companion != null ? _mage.Companion.AttackDamage : _mage.AttackDamage)
                * MageSkillCatalog.ResonanceRatio(lvl);
            Vector2 center = enemy.transform.position;
            DealDamage(enemy, damage, true, false);
            if (lvl >= 5) DamageArea(center, 1.25f, damage * .45f, true);
            MageVfx.Play("CongHuongSanMoi", center, lvl == 5 ? 1.65f : 1.2f, .35f, evolved: lvl >= 5);
        }

        private void TriggerShieldIfNeeded()
        {
            int lvl = Level(SkillId.MageDualShield);
            if (lvl <= 0 || _shieldRemaining > 0f || _shieldCooldown > 0f) return;
            ChimeraCompanion pet = _mage.Companion;
            bool danger = _player.CurrentHealth > 0f && _player.CurrentHealth <= _player.MaxHealth * .35f;
            if (!danger) return;
            _shieldRemaining = 2f;
            _shieldCooldown = 20f;
            float pct = .09f + .03f * lvl;
            _playerShield = _player.MaxHealth * pct;
            _petShield = 0f; // Chimera is immortal; the shield absorbs Player damage only.
            MageVfx.Play("ManChanSongThe", transform.position, 1.35f, 2f, true, 0f, transform, evolved: lvl >= 5);
            // Retain the cooperative shield visual on Chimera, now purely cosmetic.
            if (pet != null && pet.Alive)
                MageVfx.Play("ManChanSongThe", pet.transform.position, 1.2f, 2f, true, 0f, pet.transform, evolved: lvl >= 5);
        }

        public float AbsorbPlayerDamage(float incoming)
        {
            if (_shieldRemaining <= 0f || _playerShield <= 0f) return incoming;
            float taken = Mathf.Min(incoming, _playerShield);
            _playerShield -= taken;
            return incoming - taken;
        }

        public float AbsorbCompanionDamage(float incoming)
        {
            // Compatibility with code still calling this. Chimera never takes damage.
            return 0f;
        }

        private void PulseShield()
        {
            if (_mage == null) return;
            float amount = _mage.AttackDamage * .65f;
            DamageArea(transform.position, 1.8f, amount, false);
            ChimeraCompanion pet = _mage.Companion;
            if (pet != null && pet.Alive) DamageArea(pet.transform.position, 1.8f, amount, true);
            MageVfx.Play("ManChanSongThe", transform.position, 1.9f, evolved: Level(SkillId.MageDualShield) >= 5);
        }

        private void PruneExpired()
        {
            List<int> ids = new List<int>();
            foreach (KeyValuePair<int, float> pair in _vulnerableUntil)
                if (pair.Value <= Time.time) ids.Add(pair.Key);
            foreach (int id in ids) _vulnerableUntil.Remove(id);
            ids.Clear();
            foreach (KeyValuePair<int, ResonanceCounter> pair in _counters)
                if (pair.Value.lastHit + 8f < Time.time) ids.Add(pair.Key);
            foreach (int id in ids) _counters.Remove(id);
        }

        public void OnCompanionDefeated()
        {
            _petNormalHits = 0;
            _petCannonHits = 0;
            _petShield = 0f;
        }
    }
}
