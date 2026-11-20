using UnityEngine;

namespace DienGame.Gameplay.Core
{
    /// <summary>
    /// Meta progression cực nhẹ cho MVP. Chỉ lưu thống kê local bằng PlayerPrefs.
    /// Không chứa stat pay-to-win hay upgrade vĩnh viễn ở pass này.
    /// </summary>
    public static class GameProgressService
    {
        private const string RunsKey = "diengame_runs";
        private const string WinsKey = "diengame_wins";
        private const string BestLevelKey = "diengame_best_level";

        public static int RunsPlayed => PlayerPrefs.GetInt(RunsKey, 0);
        public static int Wins => PlayerPrefs.GetInt(WinsKey, 0);
        public static int BestLevel => PlayerPrefs.GetInt(BestLevelKey, 1);

        public static void RecordRun(bool won, int levelReached)
        {
            PlayerPrefs.SetInt(RunsKey, RunsPlayed + 1);

            if (won)
            {
                PlayerPrefs.SetInt(WinsKey, Wins + 1);
            }

            if (levelReached > BestLevel)
            {
                PlayerPrefs.SetInt(BestLevelKey, Mathf.Max(1, levelReached));
            }

            PlayerPrefs.Save();
        }
    }
}
