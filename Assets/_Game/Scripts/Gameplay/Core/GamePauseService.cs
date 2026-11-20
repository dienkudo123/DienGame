using System.Collections.Generic;
using UnityEngine;

namespace DienGame.Gameplay.Core
{
    /// <summary>
    /// Quản lý pause tập trung theo "reason" để Level Up, Pause Menu,
    /// Start Menu và Win/Lose không giẫm Time.timeScale của nhau.
    /// </summary>
    public static class GamePauseService
    {
        private static readonly HashSet<string> PauseReasons = new();

        public static bool IsPaused => PauseReasons.Count > 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetState()
        {
            PauseReasons.Clear();
            Time.timeScale = 1f;
        }

        public static void RequestPause(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return;
            }

            PauseReasons.Add(reason);
            Apply();
        }

        public static void ReleasePause(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return;
            }

            PauseReasons.Remove(reason);
            Apply();
        }

        public static bool HasReason(string reason)
        {
            return !string.IsNullOrWhiteSpace(reason) && PauseReasons.Contains(reason);
        }

        public static void ClearAll()
        {
            PauseReasons.Clear();
            Apply();
        }

        private static void Apply()
        {
            Time.timeScale = IsPaused ? 0f : 1f;
        }
    }
}
