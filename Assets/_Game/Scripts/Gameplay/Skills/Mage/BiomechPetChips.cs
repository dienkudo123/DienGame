using System;
using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Equipment;
using DienGame.Gameplay.Map;
using DienGame.Gameplay.Player;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Mage
{
    /// <summary>
    /// Optional tactical chip for the five new biomechanical summons. The original
    /// Chimera never reads this system, so its skills and artwork remain untouched.
    /// Save version and equipment IDs from phase 08 are deliberately preserved.
    /// Gameplay labels are inspired by Quy Tang's biotech / field-X aesthetic,
    /// not declarations of canonical story items.
    /// </summary>
    public enum BiomechPetChipMode
    {
        Empty = 0,
        PredatorPulse = 1,
        ScatterPulse = 2,
        Symbiosis = 3
    }

    public static class BiomechPetChips
    {
        private const string Key = "diengame_phase09_pet_chip_v1_";
        public const int UnlockLevel = 3;

        public static bool Supports(EquipmentDefinition item)
        {
            if (item == null) return false;
            switch (item.Id)
            {
                case "mage_bio_icefox":
                case "mage_bio_firehawk":
                case "mage_bio_lizard":
                case "mage_bio_void":
                case "mage_bio_wolf":
                    return true;
                default: return false;
            }
        }

        public static bool Available(EquipmentDefinition item)
        {
            return Supports(item) && EquipmentInventory.IsUnlocked(item)
                && EquipmentInventory.Level(item) >= UnlockLevel;
        }

        public static BiomechPetChipMode Current(EquipmentDefinition item)
        {
            if (!Available(item)) return BiomechPetChipMode.Empty;
            int value = EquipmentQaSandbox.Active
                ? EquipmentQaSandbox.ReadOption("chip", item.Id)
                : PlayerPrefs.GetInt(Key + item.Id, 0);
            return value >= 0 && value <= 3 ? (BiomechPetChipMode)value : BiomechPetChipMode.Empty;
        }

        public static bool TryEquip(EquipmentDefinition item, BiomechPetChipMode mode)
        {
            if (!Available(item) || !Enum.IsDefined(typeof(BiomechPetChipMode), mode)) return false;
            if (EquipmentQaSandbox.Active)
                EquipmentQaSandbox.WriteOption("chip", item.Id, (int)mode);
            else
            {
                PlayerPrefs.SetInt(Key + item.Id, (int)mode);
                PlayerPrefs.Save();
            }
            return true;
        }

        public static string Name(BiomechPetChipMode mode)
        {
            switch (mode)
            {
                case BiomechPetChipMode.PredatorPulse: return "XUNG SĂN MỒI";
                case BiomechPetChipMode.ScatterPulse: return "QUÉT XUNG";
                case BiomechPetChipMode.Symbiosis: return "CỘNG SINH";
                default: return "KHÔNG LẮP CHIP";
            }
        }

        public static string Description(BiomechPetChipMode mode)
        {
            switch (mode)
            {
                case BiomechPetChipMode.PredatorPulse:
                    return "6 đòn: xung lực đơn mục tiêu +22%. Đòn thường -4%, hồi 8 giây.";
                case BiomechPetChipMode.ScatterPulse:
                    return "7 đòn: quét 2 quái gần, mỗi quái +16%. Đòn thường -5%, hồi 8 giây.";
                case BiomechPetChipMode.Symbiosis:
                    return "8 đòn: hồi 1,5% máu tối đa cho chủ. Đòn thường -6%, hồi 12 giây.";
                default: return "Không có hiệu ứng bổ sung, giữ nguyên sát thương và nhịp đánh.";
            }
        }

        public static string Icon(BiomechPetChipMode mode)
        {
            switch (mode)
            {
                case BiomechPetChipMode.PredatorPulse: return "Equipment/Statuses/Critical";
                case BiomechPetChipMode.ScatterPulse: return "Equipment/Statuses/Pierce";
                case BiomechPetChipMode.Symbiosis: return "Equipment/Statuses/Shield";
                default: return "Equipment/Statuses/Summon";
            }
        }

        public static float NormalDamageFactor(BiomechPetChipMode mode)
        {
            switch (mode)
            {
                case BiomechPetChipMode.PredatorPulse: return .96f;
                case BiomechPetChipMode.ScatterPulse: return .95f;
                case BiomechPetChipMode.Symbiosis: return .94f;
                default: return 1f;
            }
        }
    }

    /// <summary>
    /// Per-companion runtime state: counter/cooldown cannot leak across runs or pets.
    /// No extra attacks feed Mage combo counters, and no skill-level/crowd-control
    /// power is multiplied by the reinforcement or calibration levels.
    /// </summary>
    public sealed class BiomechPetChipRuntime
    {
        private readonly BiomechPetChipMode _mode;
        private int _hits;
        private float _nextAllowed;

        private BiomechPetChipRuntime(BiomechPetChipMode mode) { _mode = mode; }

        public float NormalDamageFactor => BiomechPetChips.NormalDamageFactor(_mode);

        public static BiomechPetChipRuntime Create(EquipmentDefinition item)
        {
            if (!BiomechPetChips.Available(item)) return null;
            BiomechPetChipMode mode = BiomechPetChips.Current(item);
            return mode == BiomechPetChipMode.Empty ? null : new BiomechPetChipRuntime(mode);
        }

        public void OnNormalHit(EnemyHealth primary, float baseDamage,
            MageClassController owner, MageSkillController skills)
        {
            if (primary == null || baseDamage <= 0f || owner == null) return;
            _hits++;
            int threshold = _mode == BiomechPetChipMode.PredatorPulse ? 6 :
                _mode == BiomechPetChipMode.ScatterPulse ? 7 : 8;
            if (_hits < threshold || Time.time < _nextAllowed) return;
            // Do not waste a charged chip if the target is already defeated, or
            // if the owner's health is full. The charged proc remains ready.
            if (_mode == BiomechPetChipMode.PredatorPulse && primary.IsDead) return;
            if (_mode == BiomechPetChipMode.Symbiosis)
            {
                PlayerHealth health = owner.GetComponent<PlayerHealth>();
                if (health == null || health.IsDead || health.CurrentHealth >= health.MaxHealth) return;
            }
            if (_mode == BiomechPetChipMode.ScatterPulse && !HasScatterTarget(primary))
                return; // Hold charged proc until there is a valid target.
            _hits = 0;
            _nextAllowed = Time.time + (_mode == BiomechPetChipMode.Symbiosis ? 12f : 8f);

            switch (_mode)
            {
                case BiomechPetChipMode.PredatorPulse:
                {
                    if (primary.IsDead) break;
                    Deal(primary, baseDamage * .22f, skills);
                    MageVfx.Play("TraoLietHopKim", primary.transform.position, .57f, .25f);
                    break;
                }
                case BiomechPetChipMode.ScatterPulse:
                {
                    // Cap to two extra enemies. Never hit through projectile-blocking walls.
                    int applied = 0;
                    Vector2 center = primary.transform.position;
                    foreach (EnemyHealth other in EnemyHealth.Active)
                    {
                        if (other == null || other == primary || other.IsDead) continue;
                        Vector2 delta = (Vector2)other.transform.position - center;
                        if (delta.sqrMagnitude > 1.6f * 1.6f || IsBlocked(center, delta)) continue;
                        Deal(other, baseDamage * .16f, skills);
                        if (++applied >= 2) break;
                    }
                    if (applied > 0) MageVfx.Play("SongTrungOanhKich", center, .54f, .25f);
                    break;
                }
                case BiomechPetChipMode.Symbiosis:
                {
                    PlayerHealth health = owner.GetComponent<PlayerHealth>();
                    if (health == null || health.IsDead || health.CurrentHealth >= health.MaxHealth) break;
                    health.Heal(health.MaxHealth * .015f);
                    MageVfx.Play("ManChanSongThe", owner.transform.position, .58f, .30f);
                    break;
                }
            }
        }

        private static bool HasScatterTarget(EnemyHealth primary)
        {
            if (primary == null) return false;
            Vector2 center = primary.transform.position;
            foreach (EnemyHealth other in EnemyHealth.Active)
            {
                if (other == null || other == primary || other.IsDead) continue;
                Vector2 delta = (Vector2)other.transform.position - center;
                if (delta.sqrMagnitude <= 1.6f * 1.6f && !IsBlocked(center, delta)) return true;
            }
            return false;
        }

        private static bool IsBlocked(Vector2 origin, Vector2 delta)
        {
            if (delta.sqrMagnitude < .001f) return false;
            foreach (RaycastHit2D hit in Physics2D.RaycastAll(origin, delta.normalized, delta.magnitude))
            {
                if (hit.collider == null || hit.collider.isTrigger) continue;
                MapObstacle obstacle = hit.collider.GetComponentInParent<MapObstacle>();
                if (obstacle != null && obstacle.BlockProjectile) return true;
            }
            return false;
        }

        private static void Deal(EnemyHealth target, float damage, MageSkillController skills)
        {
            if (target == null || target.IsDead || damage <= 0f) return;
            if (skills != null) skills.DealDamage(target, damage, true, false);
            else target.TakeDamage(damage);
        }
    }
}
