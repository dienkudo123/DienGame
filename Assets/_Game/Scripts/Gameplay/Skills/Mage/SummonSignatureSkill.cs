using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Equipment;
using DienGame.Gameplay.Map;
using DienGame.Gameplay.Skills;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Mage
{
    /// <summary>
    /// Every summon owns exactly one signature. Level 1 uses the normal Mage VFX atlas;
    /// level 5 uses the existing *_Lv5 atlas. Both atlases are eight frames (4x2).
    /// The signatures are limited procs, not extra passive spam on every basic hit.
    /// Normal hits and chips retain their own separate budgets.
    /// </summary>
    public sealed class SummonSignatureSkill
    {
        private readonly SkillId _skillId;
        private readonly string _effect;
        private readonly EquipmentEffect _status;
        private readonly int _hitsNeeded;
        private readonly float _cooldown;
        private int _hits;
        private float _readyAt;
        public bool LastTriggered { get; private set; }

        private SummonSignatureSkill(SkillId id, string effect, EquipmentEffect status,
            int hitsNeeded, float cooldown)
        {
            _skillId = id;
            _effect = effect;
            _status = status;
            _hitsNeeded = hitsNeeded;
            _cooldown = cooldown;
        }

        public static SkillId SkillFor(EquipmentDefinition item)
        {
            if (item == null) return SkillId.None;
            switch (item.Id)
            {
                case "mage_chimera": return SkillId.MageMetalClaws;
                case "mage_bio_icefox": return SkillId.MageCrystalNeedles;
                case "mage_bio_firehawk": return SkillId.MageCrystalStrike;
                case "mage_bio_lizard": return SkillId.MageBiocannon;
                case "mage_bio_void": return SkillId.MageGravityWell;
                default: return SkillId.None;
            }
        }

        public static string EffectName(EquipmentDefinition item)
        {
            if (item == null) return "";
            switch (item.Id)
            {
                case "mage_chimera": return "TraoLietHopKim";
                case "mage_bio_icefox": return "TinhChamPhanLiet";
                case "mage_bio_firehawk": return "SongTrungOanhKich";
                case "mage_bio_lizard": return "PhaoSinhThe";
                case "mage_bio_void": return "TrongTruongDiGioi";
                default: return "";
            }
        }

        public static SummonSignatureSkill Create(EquipmentDefinition item)
        {
            SkillId id = SkillFor(item);
            switch (id)
            {
                case SkillId.MageMetalClaws:
                    return new SummonSignatureSkill(id, "TraoLietHopKim", EquipmentEffect.None, 3, 5f);
                case SkillId.MageCrystalNeedles:
                    return new SummonSignatureSkill(id, "TinhChamPhanLiet", EquipmentEffect.Slow, 4, 6f);
                case SkillId.MageCrystalStrike:
                    return new SummonSignatureSkill(id, "SongTrungOanhKich", EquipmentEffect.Burn, 4, 6.5f);
                case SkillId.MageBiocannon:
                    return new SummonSignatureSkill(id, "PhaoSinhThe", EquipmentEffect.Poison, 4, 6.5f);
                case SkillId.MageGravityWell:
                    return new SummonSignatureSkill(id, "TrongTruongDiGioi", EquipmentEffect.Arcane, 5, 7.5f);
                default: return null;
            }
        }

        public void OnCompanionHit(EnemyHealth primary, float damage,
            MageSkillController skills, Vector2 petPosition)
        {
            LastTriggered = false;
            if (primary == null || damage <= 0f) return;
            _hits++;
            if (_hits < _hitsNeeded || Time.time < _readyAt || primary.IsDead) return;
            _hits = 0;
            _readyAt = Time.time + _cooldown;
            LastTriggered = true;

            // Pet-specific upgrade levels come from the Mage skill choice.
            // Unlearned signatures have their Lv1 baseline, with no free Lv5.
            int level = skills != null ? Mathf.Max(1, skills.Level(_skillId)) : 1;
            bool evolved = level >= 5;
            float factor = Mathf.Lerp(1f, 1.40f, (level - 1) / 4f);
            Vector2 center = primary.transform.position;
            Vector2 direction = center - petPosition;
            float angle = direction.sqrMagnitude < .001f ? 0f :
                Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Signature-specific gameplay. Extra hits never recursively trigger combo/chips.
            switch (_skillId)
            {
                case SkillId.MageMetalClaws:
                    Deal(primary, damage * .30f * factor, skills);
                    HitExtra(center, primary, .95f, evolved ? 2 : 1, damage * .15f * factor,
                        EquipmentEffect.None, skills);
                    break;
                case SkillId.MageCrystalNeedles:
                    EnemyEquipmentStatus.ApplyHit(primary, EquipmentEffect.Slow, damage);
                    HitExtra(center, primary, 1.85f, evolved ? 2 : 1, damage * .08f * factor,
                        EquipmentEffect.Slow, skills);
                    break;
                case SkillId.MageCrystalStrike:
                    EnemyEquipmentStatus.ApplyHit(primary, EquipmentEffect.Burn, damage);
                    HitExtra(center, primary, 1.75f, evolved ? 2 : 1, damage * .18f * factor,
                        EquipmentEffect.Burn, skills);
                    break;
                case SkillId.MageBiocannon:
                    EnemyEquipmentStatus.ApplyHit(primary, EquipmentEffect.Poison, damage);
                    HitExtra(center, primary, 2.0f, evolved ? 2 : 1, damage * .09f * factor,
                        EquipmentEffect.Poison, skills);
                    break;
                case SkillId.MageGravityWell:
                    Deal(primary, damage * .20f * factor, skills);
                    HitExtra(center, primary, 1.65f, evolved ? 2 : 1, damage * .14f * factor,
                        EquipmentEffect.None, skills);
                    break;
            }
            MageVfx.Play(_effect, center, evolved ? .88f : .67f,
                evolved ? .48f : .38f, false, angle, evolved: evolved);
        }

        private static void Deal(EnemyHealth target, float damage, MageSkillController owner)
        {
            if (target == null || target.IsDead || damage <= 0f) return;
            if (owner != null) owner.DealDamage(target, damage, true, false);
            else target.TakeDamage(damage);
        }

        private static void HitExtra(Vector2 center, EnemyHealth primary, float radius,
            int count, float damage, EquipmentEffect status, MageSkillController owner)
        {
            // At most two extra targets, with line of sight through actual collision.
            EnemyHealth firstExtra = null;
            for (int i = 0; i < count; i++)
            {
                EnemyHealth picked = null;
                float closest = radius * radius;
                foreach (EnemyHealth e in EnemyHealth.Active)
                {
                    if (e == null || e.IsDead || e == primary) continue;
                    Vector2 delta = (Vector2)e.transform.position - center;
                    float sq = delta.sqrMagnitude;
                    if (sq > closest || Blocked(center, delta)) continue;
                    // For Lv5, avoid hitting the same one twice by exhausting the
                    // closest candidate on the first step (handled below via exclusion).
                    if (i == 1 && e == firstExtra) continue;
                    picked = e;
                    closest = sq;
                }
                if (picked == null) break;
                if (i == 0) firstExtra = picked;
                if (status != EquipmentEffect.None)
                    EnemyEquipmentStatus.ApplyHit(picked, status, damage);
                Deal(picked, damage, owner);
            }
        }

        private static bool Blocked(Vector2 origin, Vector2 delta)
        {
            if (delta.sqrMagnitude < .0001f) return false;
            foreach (RaycastHit2D hit in Physics2D.RaycastAll(origin, delta.normalized, delta.magnitude))
            {
                if (hit.collider == null || hit.collider.isTrigger) continue;
                MapObstacle obstacle = hit.collider.GetComponentInParent<MapObstacle>();
                if (obstacle != null && obstacle.BlockProjectile) return true;
            }
            return false;
        }
    }
}
