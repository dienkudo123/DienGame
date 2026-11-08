using DienGame.Gameplay.Skills;

namespace DienGame.Gameplay.Upgrades
{
    public enum UpgradeType
    {
        Skill,
    }

    public readonly struct UpgradeDefinition
    {
        public UpgradeType Type { get; }

        public string Title { get; }

        public string Description { get; }

        public SkillId SkillId { get; }

        public UpgradeDefinition(
            SkillId skillId,
            string title,
            string description)
        {
            Type =
                UpgradeType.Skill;

            SkillId =
                skillId;

            Title =
                title;

            Description =
                description;
        }
    }
}