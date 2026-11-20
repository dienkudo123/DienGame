using System;
using DienGame.Gameplay.Skills;
using UnityEngine;

namespace DienGame.Client
{
    /// <summary>
    /// Meta progression local cho sảnh/client.
    /// 5 skill gốc mở sẵn. 3 skill mới mở theo tiến độ màn.
    /// </summary>
    public static class MetaProgressService
    {
        private const string GoldKey = "diengame_meta_gold";
        private const string GemKey = "diengame_meta_gems";
        private const string CheckInStreakKey = "diengame_checkin_streak";
        private const string LastCheckInDateKey = "diengame_checkin_last_date";
        private const string HighestCompletedStageKey = "diengame_highest_completed_stage";

        public const int MaxStageCount = 5;
        public const int ImplementedStageCount = 5;
        // Vertical-slice playtest: all five islands can be entered without progression gates.
        public const bool UnlockAllFiveStagesForTest = true;
        public const int BaseDailyReward = 100;
        public const int TotalMarksmanSkillCount = 8;

        // MVP hiện tại: cho phép test toàn bộ skill ngay trong trận.
        // Sau này khi đủ nhiều màn, đổi thành false để bật lại unlock theo stage.
        public const bool UnlockAllMarksmanSkillsForCurrentMvp = true;

        private static readonly SkillId[] s_marksmanPassiveSkills =
        {
            SkillId.PiercingBullet,
            SkillId.ThunderBullet,
            SkillId.CombatDrone,
            SkillId.AirStrike,
            SkillId.RicochetBullet,
            SkillId.Overdrive,
            SkillId.TacticalShield,
        };

        public static int Gold => PlayerPrefs.GetInt(GoldKey, 0);
        public static int Gems => PlayerPrefs.GetInt(GemKey, 0);
        public static void GrantGold(int amount)
        {
            if (amount<=0)return;
            PlayerPrefs.SetInt(GoldKey,Gold+amount);PlayerPrefs.Save();
        }
        public static bool TrySpendGold(int amount)
        {
            if(amount<0 || Gold<amount)return false;
            PlayerPrefs.SetInt(GoldKey,Gold-amount);PlayerPrefs.Save();return true;
        }
        public static void GrantGems(int count)
        {
            if(count<=0)return;
            PlayerPrefs.SetInt(GemKey,Gems+count);PlayerPrefs.Save();
        }
        public static int CheckInStreak => PlayerPrefs.GetInt(CheckInStreakKey, 0);
        public static int HighestCompletedStage => PlayerPrefs.GetInt(HighestCompletedStageKey, 0);
        public static int CompletedImplementedStages => Mathf.Clamp(HighestCompletedStage, 0, ImplementedStageCount);

        public static bool CanCheckInToday
        {
            get
            {
                string today = DateTime.Now.ToString("yyyyMMdd");
                return PlayerPrefs.GetString(LastCheckInDateKey, string.Empty) != today;
            }
        }

        public static int NextDailyReward
        {
            get
            {
                int nextStreak = GetNextStreak();
                return BaseDailyReward + Mathf.Min(nextStreak - 1, 6) * 25;
            }
        }

        public static int UnlockedMarksmanSkillCount
        {
            get
            {
                int count = IsJudgementUnlocked ? 1 : 0;

                for (int i = 0; i < s_marksmanPassiveSkills.Length; i++)
                {
                    if (IsSkillUnlocked(s_marksmanPassiveSkills[i]))
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public static int ClaimDailyCheckIn()
        {
            if (!CanCheckInToday)
            {
                return 0;
            }

            int streak = GetNextStreak();
            int reward = BaseDailyReward + Mathf.Min(streak - 1, 6) * 25;

            PlayerPrefs.SetInt(CheckInStreakKey, streak);
            PlayerPrefs.SetInt(GoldKey, Gold + reward);
            PlayerPrefs.SetString(LastCheckInDateKey, DateTime.Now.ToString("yyyyMMdd"));
            PlayerPrefs.Save();

            return reward;
        }

        public static bool IsStageImplemented(int stage)
        {
            return stage >= 1 && stage <= ImplementedStageCount;
        }

        public static bool IsStageUnlocked(int stage)
        {
            if (!IsStageImplemented(stage))
            {
                return false;
            }

            if (UnlockAllFiveStagesForTest) return true;
            return stage <= Mathf.Max(1, HighestCompletedStage + 1);
        }

        public static void CompleteStage(int stage)
        {
            if (!IsStageImplemented(stage))
            {
                return;
            }

            if (stage > HighestCompletedStage)
            {
                PlayerPrefs.SetInt(HighestCompletedStageKey, stage);
                PlayerPrefs.Save();
            }
        }

        public static bool IsSkillUnlocked(SkillId skillId)
        {
            int requiredStage = GetRequiredCompletedStage(skillId);

            if (requiredStage < 0)
            {
                return false;
            }

            if (UnlockAllMarksmanSkillsForCurrentMvp)
            {
                return true;
            }

            if (requiredStage == 0)
            {
                return true;
            }

            return HighestCompletedStage >= requiredStage;
        }

        public static int GetRequiredCompletedStage(SkillId skillId)
        {
            return skillId switch
            {
                SkillId.PiercingBullet => 0,
                SkillId.ThunderBullet => 0,
                SkillId.CombatDrone => 0,
                SkillId.AirStrike => 0,

                SkillId.RicochetBullet => 1,
                SkillId.TacticalShield => 3,
                SkillId.Overdrive => 5,

                _ => -1,
            };
        }

        public static string GetSkillUnlockLabel(SkillId skillId)
        {
            if (IsSkillUnlocked(skillId))
            {
                return "ĐÃ MỞ";
            }

            int stage = GetRequiredCompletedStage(skillId);

            return stage > 0
                ? $"MỞ SAU MÀN {stage}"
                : "ĐÃ KHÓA";
        }

        /// <summary>
        /// Ultimate Vòng Vây Tử Thần vẫn mở sẵn.
        /// </summary>
        public static bool IsJudgementUnlocked => true;

        /// <summary>
        /// Chỉ dùng cho Editor/debug để test pool skill mới ngay lập tức.
        /// </summary>
        public static void DebugUnlockAllMarksmanSkills()
        {
            PlayerPrefs.SetInt(HighestCompletedStageKey, MaxStageCount);
            PlayerPrefs.Save();
        }

        public static void DebugResetStageProgress()
        {
            PlayerPrefs.DeleteKey(HighestCompletedStageKey);
            PlayerPrefs.Save();
        }

        private static int GetNextStreak()
        {
            string lastRaw = PlayerPrefs.GetString(LastCheckInDateKey, string.Empty);

            if (!DateTime.TryParseExact(
                    lastRaw,
                    "yyyyMMdd",
                    null,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime lastDate))
            {
                return 1;
            }

            DateTime today = DateTime.Now.Date;
            double days = (today - lastDate.Date).TotalDays;

            return Math.Abs(days - 1d) < 0.001d
                ? Mathf.Max(1, CheckInStreak + 1)
                : 1;
        }
    }
}
