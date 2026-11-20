using DienGame.Gameplay.Skills;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Marksman
{
    /// <summary>
    /// Các bù trừ chỉ số của build Xạ thủ.
    /// Tất cả giá trị được tính theo level hiện tại, KHÔNG nhân chồng mỗi lần nâng cấp.
    /// </summary>
    public static class MarksmanStatBalance
    {
        public static float GetHandDamageMultiplier(PlayerSkillManager skillManager)
        {
            if (skillManager == null)
            {
                return 1f;
            }

            float percent = 0f;

            percent += GetPiercingDamagePercent(skillManager.GetSkillLevel(SkillId.PiercingBullet));
            percent += GetAirStrikeDamagePercent(skillManager.GetSkillLevel(SkillId.AirStrike));
            percent += GetRicochetDamagePercent(skillManager.GetSkillLevel(SkillId.RicochetBullet));

            return Mathf.Max(0.10f, 1f + percent / 100f);
        }

        public static float GetAttackSpeedMultiplier(PlayerSkillManager skillManager)
        {
            if (skillManager == null)
            {
                return 1f;
            }

            float percent = 0f;

            percent += GetPiercingAttackSpeedPercent(skillManager.GetSkillLevel(SkillId.PiercingBullet));
            percent += GetThunderAttackSpeedPercent(skillManager.GetSkillLevel(SkillId.ThunderBullet));

            return Mathf.Max(0.25f, 1f + percent / 100f);
        }

        public static float GetMoveSpeedMultiplier(PlayerSkillManager skillManager)
        {
            if (skillManager == null)
            {
                return 1f;
            }

            float percent = 0f;

            percent += GetThunderMoveSpeedPercent(skillManager.GetSkillLevel(SkillId.ThunderBullet));
            percent += GetRicochetMoveSpeedPercent(skillManager.GetSkillLevel(SkillId.RicochetBullet));

            return Mathf.Max(0.25f, 1f + percent / 100f);
        }

        public static float GetMaxHealthMultiplier(PlayerSkillManager skillManager)
        {
            if (skillManager == null)
            {
                return 1f;
            }

            float percent = GetAirStrikeMaxHealthPercent(
                skillManager.GetSkillLevel(SkillId.AirStrike)
            );

            return Mathf.Max(0.25f, 1f + percent / 100f);
        }

        public static string GetTradeoffDescription(SkillId skillId, int level)
        {
            level = Mathf.Clamp(level, 0, PlayerSkillManager.MaxSkillLevel);

            return skillId switch
            {
                SkillId.PiercingBullet =>
                    $"Chỉ số nhân vật: Sát thương tay +{GetPiercingDamagePercent(level):0}% • Tốc độ đánh {GetSignedPercent(GetPiercingAttackSpeedPercent(level))}.",

                SkillId.ThunderBullet =>
                    $"Chỉ số nhân vật: Tốc độ đánh +{GetThunderAttackSpeedPercent(level):0}% • Tốc độ di chuyển {GetSignedPercent(GetThunderMoveSpeedPercent(level))}.",

                SkillId.AirStrike =>
                    $"Chỉ số nhân vật: Sát thương tay +{GetAirStrikeDamagePercent(level):0}% • Máu tối đa {GetSignedPercent(GetAirStrikeMaxHealthPercent(level))}.",

                SkillId.RicochetBullet =>
                    $"Chỉ số nhân vật: Tốc chạy +{GetRicochetMoveSpeedPercent(level):0}% • Sát thương tay {GetSignedPercent(GetRicochetDamagePercent(level))}.",

                _ => string.Empty,
            };
        }

        private static string GetSignedPercent(float value)
        {
            return value >= 0f
                ? $"+{value:0}%"
                : $"{value:0}%";
        }

        public static float GetPiercingDamagePercent(int level)
        {
            return level switch
            {
                <= 0 => 0f,
                1 => 5f,
                2 => 10f,
                3 => 15f,
                4 => 20f,
                _ => 25f,
            };
        }

        public static float GetPiercingAttackSpeedPercent(int level)
        {
            return level switch
            {
                <= 0 => 0f,
                1 => -2f,
                2 => -4f,
                3 => -6f,
                4 => -8f,
                _ => -10f,
            };
        }

        public static float GetThunderAttackSpeedPercent(int level)
        {
            return level switch
            {
                <= 0 => 0f,
                1 => 4f,
                2 => 8f,
                3 => 12f,
                4 => 16f,
                _ => 20f,
            };
        }

        public static float GetThunderMoveSpeedPercent(int level)
        {
            return level switch
            {
                <= 0 => 0f,
                1 => -2f,
                2 => -4f,
                3 => -6f,
                4 => -8f,
                _ => -10f,
            };
        }

        public static float GetAirStrikeDamagePercent(int level)
        {
            return level switch
            {
                <= 0 => 0f,
                1 => 5f,
                2 => 10f,
                3 => 15f,
                4 => 20f,
                _ => 25f,
            };
        }

        public static float GetAirStrikeMaxHealthPercent(int level)
        {
            return level switch
            {
                <= 0 => 0f,
                1 => -2f,
                2 => -4f,
                3 => -6f,
                4 => -8f,
                _ => -10f,
            };
        }

        public static float GetRicochetMoveSpeedPercent(int level)
        {
            return level switch
            {
                <= 0 => 0f,
                1 => 3f,
                2 => 6f,
                3 => 9f,
                4 => 12f,
                _ => 15f,
            };
        }

        public static float GetRicochetDamagePercent(int level)
        {
            return level switch
            {
                <= 0 => 0f,
                1 => -4f,
                2 => -8f,
                3 => -12f,
                4 => -16f,
                _ => -20f,
            };
        }
    }
}
