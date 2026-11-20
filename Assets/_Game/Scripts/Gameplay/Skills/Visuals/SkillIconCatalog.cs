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
        [SerializeField] private Sprite ricochetBullet;
        [SerializeField] private Sprite overdrive;
        [SerializeField] private Sprite tacticalShield;

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
                SkillId.RicochetBullet => ricochetBullet,
                SkillId.Overdrive => overdrive,
                SkillId.TacticalShield => tacticalShield,
                _ => null,
            };
        }

        public bool SetIcons(
            Sprite piercing,
            Sprite thunder,
            Sprite drone,
            Sprite airStrikeIcon,
            Sprite ricochetIcon,
            Sprite overdriveIcon,
            Sprite shieldIcon,
            Sprite judgementIcon)
        {
            bool changed =
                piercingBullet != piercing ||
                thunderBullet != thunder ||
                combatDrone != drone ||
                airStrike != airStrikeIcon ||
                ricochetBullet != ricochetIcon ||
                overdrive != overdriveIcon ||
                tacticalShield != shieldIcon ||
                judgement != judgementIcon;

            piercingBullet = piercing;
            thunderBullet = thunder;
            combatDrone = drone;
            airStrike = airStrikeIcon;
            ricochetBullet = ricochetIcon;
            overdrive = overdriveIcon;
            tacticalShield = shieldIcon;
            judgement = judgementIcon;

            return changed;
        }
    }
}
