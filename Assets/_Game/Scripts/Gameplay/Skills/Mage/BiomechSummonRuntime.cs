using System.Collections.Generic;
using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Equipment;
using DienGame.Gameplay.Map;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Mage
{
    /// <summary>
    /// Optional module for newly added BIO summons only. Chimera and all prior pets
    /// bypass this class, retaining their original animations and skill effects.
    /// Each biomech form has one limited, non-stacking combat identity.
    /// </summary>
    public sealed class BiomechSummonRuntime
    {
        private enum Form { IceFox, FireHawk, PoisonLizard, VoidOrb, Wolf }

        private readonly Form _form;
        private readonly string _artName;
        private float _nextProcAt;
        private int _hitCounter;

        // Small role differences, constrained to the same overall balance budget.
        // Chimera never enters this class and keeps its original combat AI.
        public float MoveFactor => _form == Form.Wolf ? 1.07f :
            _form == Form.FireHawk ? 1.08f : _form == Form.PoisonLizard ? .94f :
            _form == Form.VoidOrb ? .88f : 1.00f;
        public float AttackIntervalFactor => _form == Form.Wolf ? .95f :
            _form == Form.FireHawk ? 1.08f : _form == Form.VoidOrb ? 1.10f :
            _form == Form.IceFox ? 1.04f : 1.00f;
        public float RangeBonus => _form == Form.VoidOrb ? .30f :
            _form == Form.IceFox ? .10f : _form == Form.FireHawk ? .10f : .0f;
        public float SearchBonus => _form == Form.Wolf ? .30f :
            _form == Form.VoidOrb ? .40f : .15f;

        private BiomechSummonRuntime(Form form, string artName)
        {
            _form = form;
            _artName = artName;
        }

        public static BiomechSummonRuntime Create(EquipmentDefinition item)
        {
            if (item == null) return null;
            switch (item.Id)
            {
                case "mage_bio_icefox": return new BiomechSummonRuntime(Form.IceFox, "IceFox");
                case "mage_bio_firehawk": return new BiomechSummonRuntime(Form.FireHawk, "FireHawk");
                case "mage_bio_lizard": return new BiomechSummonRuntime(Form.PoisonLizard, "PoisonLizard");
                case "mage_bio_void": return new BiomechSummonRuntime(Form.VoidOrb, "VoidOrb");
                case "mage_bio_wolf": return new BiomechSummonRuntime(Form.Wolf, "Wolf");
                default: return null;
            }
        }

        public Sprite GetFrame(int facing, bool attacking, int frame)
        {
            return BiomechSummonArt.GetFrame(_artName, facing, attacking, frame);
        }

        public void PlayNormalAttackVfx(Vector2 position, float angle)
        {
            string effect;
            switch (_form)
            {
                case Form.IceFox: effect = "TinhChamPhanLiet"; break;
                case Form.FireHawk: effect = "SongTrungOanhKich"; break;
                case Form.PoisonLizard: effect = "PhaoSinhThe"; break;
                case Form.VoidOrb: effect = "TrongTruongDiGioi"; break;
                default: effect = "TraoLietHopKim"; break;
            }
            MageVfx.Play(effect, position, .30f, .25f, false, angle);
        }

        public void OnNormalAttackHit(EnemyHealth primary, float attackDamage, MageSkillController skills)
        {
            if (primary == null || attackDamage <= 0f) return;
            _hitCounter++;
            int threshold = _form == Form.VoidOrb || _form == Form.Wolf ? 5 : 4;
            if (_hitCounter < threshold || Time.time < _nextProcAt) return;
            _hitCounter = 0;
            _nextProcAt = Time.time + (_form == Form.Wolf ? 5.0f : 4.5f);

            switch (_form)
            {
                case Form.IceFox:
                {
                    EnemyHealth target = FindNeighbor(primary, 1.65f);
                    if (target != null)
                        EnemyEquipmentStatus.ApplyHit(target, EquipmentEffect.Slow, attackDamage);
                    MageVfx.Play("TinhChamPhanLiet", primary.transform.position, .65f, .26f);
                    break;
                }
                case Form.FireHawk:
                {
                    EnemyHealth target = FindNeighbor(primary, 1.55f);
                    if (target != null)
                    {
                        EnemyEquipmentStatus.ApplyHit(target, EquipmentEffect.Burn, attackDamage * .7f);
                        // Small extra AoE budget, no bonus to primary target.
                        DealExtra(target, attackDamage * .10f, skills);
                    }
                    MageVfx.Play("SongTrungOanhKich", primary.transform.position, .68f, .28f);
                    break;
                }
                case Form.PoisonLizard:
                {
                    EnemyHealth target = FindNeighbor(primary, 1.8f);
                    if (target != null)
                        EnemyEquipmentStatus.ApplyHit(target, EquipmentEffect.Poison, attackDamage * .7f);
                    MageVfx.Play("PhaoSinhThe", primary.transform.position, .52f, .27f);
                    break;
                }
                case Form.VoidOrb:
                {
                    EnemyHealth target = FindNeighbor(primary, 1.45f);
                    if (target != null) DealExtra(target, attackDamage * .18f, skills);
                    MageVfx.Play("TrongTruongDiGioi", primary.transform.position, .62f, .25f);
                    break;
                }
                case Form.Wolf:
                {
                    // Bonus single-target strike does not recursively build skill combos.
                    DealExtra(primary, attackDamage * .25f, skills);
                    MageVfx.Play("TraoLietHopKim", primary.transform.position, .75f, .30f);
                    break;
                }
            }
        }

        private static void DealExtra(EnemyHealth target, float damage, MageSkillController skills)
        {
            if (target == null || target.IsDead || damage <= 0f) return;
            if (skills != null) skills.DealDamage(target, damage, true, false);
            else target.TakeDamage(damage);
        }

        private static EnemyHealth FindNeighbor(EnemyHealth origin, float radius)
        {
            if (origin == null) return null;
            Vector2 center = origin.transform.position;
            float best = radius * radius;
            EnemyHealth picked = null;
            foreach (EnemyHealth other in EnemyHealth.Active)
            {
                if (other == null || other == origin || other.IsDead) continue;
                Vector2 delta = (Vector2)other.transform.position - center;
                float sqr = delta.sqrMagnitude;
                if (sqr >= best || LineBlocked(center, delta)) continue;
                best = sqr;
                picked = other;
            }
            return picked;
        }

        private static bool LineBlocked(Vector2 center, Vector2 delta)
        {
            if (delta.sqrMagnitude <= .0001f) return false;
            foreach (RaycastHit2D hit in Physics2D.RaycastAll(center, delta.normalized, delta.magnitude))
            {
                if (hit.collider == null || hit.collider.isTrigger) continue;
                MapObstacle obstacle = hit.collider.GetComponentInParent<MapObstacle>();
                if (obstacle != null && obstacle.BlockProjectile) return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Cache grid frames once per equip form. New assets live under a distinct folder
    /// so none of the original Mage/Chimera sprites are touched or replaced.
    /// </summary>
    public static class BiomechSummonArt
    {
        private static readonly Dictionary<string, Sprite[]> Frames = new Dictionary<string, Sprite[]>();
        private static readonly HashSet<string> Missing = new HashSet<string>();

        public static Sprite GetFrame(string form, int facing, bool attacking, int frame)
        {
            if (string.IsNullOrEmpty(form)) return null;
            int columns = attacking ? 5 : 4;
            string category = attacking ? "Attack" : "Walk";
            string suffix = attacking ? "Attack4x5" : "Walk4x4";
            string path = "Equipment/Biomech/" + category + "/Bio_" + form + "_" + suffix;
            if (!Frames.TryGetValue(path, out Sprite[] atlas))
            {
                Texture2D tex = Resources.Load<Texture2D>(path);
                if (tex == null)
                {
                    if (Missing.Add(path)) Debug.LogError("DienGame: thiếu atlas thú cơ sinh: Resources/" + path);
                    return null;
                }
                int rows = 4;
                if (tex.width % columns != 0 || tex.height % rows != 0)
                {
                    Debug.LogError("DienGame: atlas sai lưới 4 hướng: " + path);
                    return null;
                }
                int cw = tex.width / columns;
                int ch = tex.height / rows;
                float ppu = form == "Wolf" ? 220f : 185f;
                atlas = new Sprite[columns * rows];
                for (int y = 0; y < rows; y++)
                for (int x = 0; x < columns; x++)
                {
                    Rect rect = new Rect(x * cw, (rows - y - 1) * ch, cw, ch);
                    atlas[y * columns + x] = Sprite.Create(tex, rect,
                        new Vector2(.5f, .5f), ppu, 0, SpriteMeshType.FullRect);
                }
                Frames.Add(path, atlas);
            }
            int index = Mathf.Clamp(facing, 0, 3) * columns + Mathf.Clamp(frame, 0, columns - 1);
            return atlas[index];
        }
    }
}
