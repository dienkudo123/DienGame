using UnityEngine;

namespace DienGame.Gameplay.Skills.Visuals
{
    public sealed class SkillIconCatalog : ScriptableObject
    {
        [Header("Marksman Passive Skills")]
        [SerializeField] private Sprite piercingBullet;
        [SerializeField] private Sprite thunderBullet;
        [SerializeField] private Sprite combatDrone;
        [SerializeField] private Sprite airStrike;

        [Header("Marksman Ultimate")]
        [SerializeField] private Sprite judgement;

        public Sprite JudgementIcon => judgement;

        public Sprite GetIcon(SkillId skillId)
        {
            return skillId switch
            {
                SkillId.PiercingBullet => piercingBullet,
                SkillId.ThunderBullet => thunderBullet,
                SkillId.CombatDrone => combatDrone,
                SkillId.AirStrike => airStrike,
                _ => null,
            };
        }

        public bool SetIcons(
            Sprite piercing,
            Sprite thunder,
            Sprite drone,
            Sprite airStrikeIcon,
            Sprite judgementIcon)
        {
            bool changed =
                piercingBullet != piercing ||
                thunderBullet != thunder ||
                combatDrone != drone ||
                airStrike != airStrikeIcon ||
                judgement != judgementIcon;

            piercingBullet = piercing;
            thunderBullet = thunder;
            combatDrone = drone;
            airStrike = airStrikeIcon;
            judgement = judgementIcon;

            return changed;
        }
    }
}
