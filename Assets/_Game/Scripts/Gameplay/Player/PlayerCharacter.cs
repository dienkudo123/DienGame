using System.Collections.Generic;
using DienGame.Gameplay.Character;
using UnityEngine;

namespace DienGame.Gameplay.Player
{
    public sealed class PlayerCharacter : CharacterBase
    {
        [Header("Combat")]
        [SerializeField, Min(0.01f)] private float attackInterval = 1f;

        [Header("Skills")]
        [SerializeField] private List<CharacterSkill> skills = new();

        public float AttackInterval => attackInterval;

        /// <summary>Safe once-per-run equipment adjustment; no inspector edits needed.</summary>
        public void MultiplyAttackInterval(float factor)
        {
            attackInterval = Mathf.Max(.08f, attackInterval * Mathf.Clamp(factor, .5f, 2f));
        }
        public bool HasSkill => skills.Count > 0;
        public IReadOnlyList<CharacterSkill> Skills => skills;

        public void AddSkill(CharacterSkill skill)
        {
            if (skill != null && !skills.Contains(skill)) skills.Add(skill);
        }

        public void RemoveSkill(CharacterSkill skill)
        {
            if (skill != null) skills.Remove(skill);
        }

        public bool TryUseSkill(int index)
        {
            if (index < 0 || index >= skills.Count) return false;

            CharacterSkill skill = skills[index];
            if (skill == null || !skill.CanUse(this)) return false;

            skill.Use(this);
            return true;
        }
    }
}
