using System;
using System.Collections.Generic;
using DienGame.Gameplay.Skills.Assassin;
using DienGame.Gameplay.Skills.Mage;
using DienGame.Gameplay.Skills.Warrior;
using UnityEngine;

namespace DienGame.Gameplay.Skills
{
    /// <summary>
    /// Class isolation is enforced here, not only in level-up UI. An Assassin scene
    /// contains AssassinMeleeAttack; Marksman does not. There are no shared skills.
    /// </summary>
    public sealed class PlayerSkillManager : MonoBehaviour
    {
        public const int MaxSkillLevel = 5;
        private readonly Dictionary<SkillId, int> _skillLevels = new Dictionary<SkillId, int>();
        public event Action<SkillId, int> SkillLevelChanged;

        public int GetSkillLevel(SkillId skillId) =>
            _skillLevels.TryGetValue(skillId, out int level) ? level : 0;

        public bool HasSkill(SkillId skillId) => GetSkillLevel(skillId) > 0;
        public bool IsMaxLevel(SkillId skillId) => GetSkillLevel(skillId) >= MaxSkillLevel;

        public bool CanUpgrade(SkillId skillId)
        {
            if (skillId == SkillId.None || IsMaxLevel(skillId)) return false;
            // The class comes from this Player's combat component, not a Lobby static;
            // restart and standalone gameplay preview therefore remain deterministic.
            if (GetComponent<MageClassController>() != null) return IsMageSkill(skillId);
            if (GetComponent<WarriorMeleeAttack>() != null) return IsWarriorSkill(skillId);
            bool isAssassin = GetComponent<AssassinMeleeAttack>() != null;
            return isAssassin ? IsAssassinSkill(skillId) : IsMarksmanSkill(skillId);
        }

        private static bool IsWarriorSkill(SkillId id)
        {
            switch (id)
            {
                case SkillId.WarriorDrive:
                case SkillId.WarriorBulwark:
                case SkillId.WarriorShockwave:
                case SkillId.WarriorHeat:
                case SkillId.WarriorBreaker:
                case SkillId.WarriorTempo:
                case SkillId.WarriorRegeneration:
                    return true;
                default: return false;
            }
        }

        private static bool IsMageSkill(SkillId id)
        {
            switch (id)
            {
                case SkillId.MageCrystalNeedles:
                case SkillId.MageGravityWell:
                case SkillId.MageMetalClaws:
                case SkillId.MageBiocannon:
                case SkillId.MageCrystalStrike:
                case SkillId.MagePredatorResonance:
                case SkillId.MageDualShield:
                    return true;
                default: return false;
            }
        }

        private static bool IsAssassinSkill(SkillId id)
        {
            switch (id)
            {
                case SkillId.AssassinBloodSlash:
                case SkillId.AssassinAfterimage:
                case SkillId.AssassinFlyingDagger:
                case SkillId.AssassinShadowClone:
                case SkillId.AssassinDeathMark:
                case SkillId.AssassinOrbitBlades:
                case SkillId.AssassinExecution:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsMarksmanSkill(SkillId id)
        {
            switch (id)
            {
                case SkillId.PiercingBullet:
                case SkillId.ThunderBullet:
                case SkillId.CombatDrone:
                case SkillId.AirStrike:
                case SkillId.RicochetBullet:
                case SkillId.Overdrive:
                case SkillId.TacticalShield:
                    return true;
                default:
                    return false;
            }
        }

        public int UpgradeSkill(SkillId skillId)
        {
            if (!CanUpgrade(skillId))
            {
                if (skillId != SkillId.None)
                    Debug.LogWarning($"This character cannot upgrade {skillId}.", this);
                return GetSkillLevel(skillId);
            }

            int newLevel = GetSkillLevel(skillId) + 1;
            _skillLevels[skillId] = newLevel;
            Debug.Log($"SKILL UPGRADE: {skillId} -> Lv{newLevel}", this);
            SkillLevelChanged?.Invoke(skillId, newLevel);
            return newLevel;
        }
    }
}
