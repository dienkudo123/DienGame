using DienGame.Client;
using UnityEngine;

namespace DienGame.Gameplay.Phase13
{
    /// <summary>
    /// QA-only in-memory counters for one run. DPS is total damage applied to enemies
    /// (including pet, dots, skills); it must NOT be mistaken for damage from a single weapon.
    /// No PlayerPrefs and no runtime gameplay modifiers.
    /// </summary>
    public static class Phase13CombatMetrics
    {
        private static float s_start;
        private static float s_frozenDuration;
        private static float s_enemyDamage;
        private static float s_playerDamage;
        private static float s_healing;
        private static int s_kills;
        private static int s_experience;
        private static bool s_started;
        private static bool s_finished;
        private static string s_class;
        private static int s_stage;
        private static string s_result;

        public static bool IsStarted => s_started;
        public static string ClassName => s_class ?? "-";
        public static int Stage => s_stage;
        public static string Result => s_result ?? "-";
        public static int Kills => s_kills;
        public static int Experience => s_experience;
        public static float TotalEnemyDamage => s_enemyDamage;
        public static float TotalDamageTaken => s_playerDamage;
        public static float TotalHealing => s_healing;
        public static float ElapsedSeconds => !s_started ? 0f :
            (s_finished ? s_frozenDuration : Mathf.Max(0f, Time.time - s_start));
        public static float AverageDps => s_enemyDamage / Mathf.Max(1f, ElapsedSeconds);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void DomainReset() => Clear();

        public static void StartRun()
        {
            Clear();
            s_class = LobbySessionService.CurrentClass.ToString();
            s_stage = LobbySessionService.SelectedStage;
            s_result = "RUNNING";
            s_start = Time.time;
            s_started = true;
        }

        public static void CompleteRun(bool won)
        {
            if (!s_started || s_finished) return;
            s_frozenDuration = Mathf.Max(0f, Time.time - s_start);
            s_result = won ? "WIN" : "LOSE";
            s_finished = true;
        }

        public static void RecordEnemyDamage(float damage)
        {
            if (s_started && !s_finished && damage > 0f) s_enemyDamage += damage;
        }
        public static void RecordPlayerDamage(float damage)
        {
            if (s_started && !s_finished && damage > 0f) s_playerDamage += damage;
        }
        public static void RecordHealing(float amount)
        {
            if (s_started && !s_finished && amount > 0f) s_healing += amount;
        }
        public static void RecordKill()
        {
            if (s_started && !s_finished) s_kills++;
        }
        public static void RecordExperience(int amount)
        {
            if (s_started && !s_finished && amount > 0) s_experience += amount;
        }
        public static void Clear()
        {
            s_start = s_frozenDuration = s_enemyDamage = s_playerDamage = s_healing = 0f;
            s_kills = s_experience = 0;
            s_started = s_finished = false;
            s_class = s_result = null;
            s_stage = 0;
        }
        public static string Summary() =>
            $"[Phase13 QA] {ClassName}, island {Stage}, {Result} | {ElapsedSeconds:F1}s | " +
            $"DPS(total) {AverageDps:F1} | dealt {TotalEnemyDamage:F0} | taken {TotalDamageTaken:F0} | " +
            $"healed {TotalHealing:F0} | kills {Kills} | XP {Experience}";
    }
}
