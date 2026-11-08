using System;
using System.Collections.Generic;
using DienGame.Gameplay.Skills;
using DienGame.Gameplay.Skills.Marksman;
using UnityEngine;

namespace DienGame.Gameplay.Upgrades
{
    public static class LevelUpUpgradePool
    {
        public static List<UpgradeDefinition>
            GetRandomOptions(
                int count,
                PlayerSkillManager skillManager)
        {
            List<UpgradeDefinition> available =
                new();

            AddSkill(
                available,
                skillManager,
                SkillId.PiercingBullet,
                "Đạn Xuyên Phá",
                PiercingBulletSkill.GetDescription
            );

            AddSkill(
                available,
                skillManager,
                SkillId.ThunderBullet,
                "Lôi Đạn",
                ThunderBulletSkill.GetDescription
            );

            AddSkill(
                available,
                skillManager,
                SkillId.CombatDrone,
                "Drone Chiến Đấu",
                CombatDroneSkill.GetDescription
            );

            AddSkill(
                available,
                skillManager,
                SkillId.AirStrike,
                "Không Kích",
                AirStrikeSkill.GetDescription
            );

            List<UpgradeDefinition> result =
                new();

            int optionCount =
                Mathf.Min(
                    count,
                    available.Count
                );

            while (
                result.Count <
                optionCount
            )
            {
                int randomIndex =
                    UnityEngine.Random.Range(
                        0,
                        available.Count
                    );

                result.Add(
                    available[randomIndex]
                );

                available.RemoveAt(
                    randomIndex
                );
            }

            return result;
        }

        private static void AddSkill(
            List<UpgradeDefinition> list,
            PlayerSkillManager skillManager,
            SkillId skillId,
            string skillName,
            Func<int, string> descriptionFactory)
        {
            if (
                skillManager == null ||
                !skillManager.CanUpgrade(
                    skillId
                )
            )
            {
                return;
            }

            int currentLevel =
                skillManager.GetSkillLevel(
                    skillId
                );

            int nextLevel =
                currentLevel + 1;

            string title =
                $"{skillName} {ToRoman(nextLevel)}";

            string description =
                descriptionFactory(
                    nextLevel
                );

            list.Add(
                new UpgradeDefinition(
                    skillId,
                    title,
                    description
                )
            );
        }

        private static string ToRoman(
            int level)
        {
            return level switch
            {
                1 => "I",
                2 => "II",
                3 => "III",
                4 => "IV",
                5 => "V",
                _ => level.ToString(),
            };
        }
    }
}