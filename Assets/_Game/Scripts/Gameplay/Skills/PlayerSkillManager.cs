using System;
using System.Collections.Generic;
using UnityEngine;

namespace DienGame.Gameplay.Skills
{
    public sealed class PlayerSkillManager : MonoBehaviour
    {
        public const int MaxSkillLevel = 5;

        private readonly Dictionary<SkillId, int> _skillLevels = new();

        public event Action<SkillId, int> SkillLevelChanged;

        public int GetSkillLevel(SkillId skillId)
        {
            return _skillLevels.TryGetValue(
                skillId,
                out int level
            )
                ? level
                : 0;
        }

        public bool HasSkill(SkillId skillId)
        {
            return GetSkillLevel(skillId) > 0;
        }

        public bool IsMaxLevel(SkillId skillId)
        {
            return GetSkillLevel(skillId) >= MaxSkillLevel;
        }

        public bool CanUpgrade(SkillId skillId)
        {
            return
                skillId != SkillId.None &&
                !IsMaxLevel(skillId);
        }

        public int UpgradeSkill(SkillId skillId)
        {
            if (!CanUpgrade(skillId))
            {
                return GetSkillLevel(skillId);
            }

            int newLevel =
                GetSkillLevel(skillId) + 1;

            _skillLevels[skillId] =
                newLevel;

            Debug.Log(
                $"SKILL UPGRADE: {skillId} -> Lv{newLevel}",
                this
            );

            SkillLevelChanged?.Invoke(
                skillId,
                newLevel
            );

            return newLevel;
        }
    }
}