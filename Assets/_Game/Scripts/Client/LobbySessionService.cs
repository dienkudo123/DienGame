using DienGame.Gameplay.Core;
using UnityEngine.SceneManagement;

namespace DienGame.Client
{
    /// <summary>
    /// Session bridge giữa Lobby và Gameplay.
    /// Không lưu PlayerPrefs vì đây chỉ là trạng thái của lần chạy hiện tại.
    /// </summary>
    public enum PlayableClass
    {
        Marksman = 0,
        Assassin = 1,
        Mage = 2,
        // Dedicated Phase10 melee class, independent gameplay scene.
        Warrior = 3,
    }

    public static class LobbySessionService
    {
        public const string LobbySceneName = "Lobby";
        public const string GameplaySceneName = "GameplayTest";
        public const string AssassinSceneName = "GameplayTest_AssassinPreview";
        public const string MageSceneName = "GameplayTest_Mage";
        public const string WarriorSceneName = "GameplayTest_Warrior";

        private static PlayableClass _selectedClass = PlayableClass.Marksman;
        public static PlayableClass SelectedClass => _selectedClass;
        public static PlayableClass CurrentClass =>
            SceneManager.GetActiveScene().name == AssassinSceneName ? PlayableClass.Assassin :
            SceneManager.GetActiveScene().name == MageSceneName ? PlayableClass.Mage :
            SceneManager.GetActiveScene().name == WarriorSceneName ? PlayableClass.Warrior :
            _selectedClass;

        public static string SelectedGameplaySceneName =>
            _selectedClass == PlayableClass.Assassin ? AssassinSceneName :
            _selectedClass == PlayableClass.Mage ? MageSceneName :
            _selectedClass == PlayableClass.Warrior ? WarriorSceneName : GameplaySceneName;

        public static bool IsGameplayScene(string sceneName) =>
            sceneName == GameplaySceneName || sceneName == AssassinSceneName ||
            sceneName == MageSceneName || sceneName == WarriorSceneName;

        public static void SelectClass(PlayableClass playerClass)
        {
            if (playerClass != PlayableClass.Marksman && playerClass != PlayableClass.Assassin &&
                playerClass != PlayableClass.Mage && playerClass != PlayableClass.Warrior)
                return;
            _selectedClass = playerClass;
        }

        private static bool _enteredGameplayFromLobby;
        private static bool _restartRequested;
        private static int _selectedStage = 1;

        public static int SelectedStage => _selectedStage;

        // False means the target stage or scene is unavailable; never load a different class as fallback.
        public static bool StartStage(int stage)
        {
            if (!MetaProgressService.IsStageUnlocked(stage) ||
                !UnityEngine.Application.CanStreamedLevelBeLoaded(SelectedGameplaySceneName))
            {
                return false;
            }

            _selectedStage = stage;
            _enteredGameplayFromLobby = true;
            _restartRequested = false;

            GamePauseService.ClearAll();
            SceneManager.LoadScene(SelectedGameplaySceneName);
            return true;
        }

        public static bool ConsumeEnteredFromLobby()
        {
            bool result = _enteredGameplayFromLobby;
            _enteredGameplayFromLobby = false;
            return result;
        }

        // Restart giữ nguyên scene đang chơi nhưng bỏ qua màn hình bắt đầu,
        // vì người chơi đã chọn "CHƠI LẠI" từ một trận đấu đang chạy.
        public static void RestartCurrentRun()
        {
            _enteredGameplayFromLobby = false;
            _restartRequested = true;
            GamePauseService.ClearAll();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public static bool ConsumeAutoStartRequested()
        {
            bool result = _enteredGameplayFromLobby || _restartRequested;
            _enteredGameplayFromLobby = false;
            _restartRequested = false;
            return result;
        }

        public static void ReturnToLobby()
        {
            _enteredGameplayFromLobby = false;
            _restartRequested = false;
            GamePauseService.ClearAll();
            SceneManager.LoadScene(LobbySceneName);
        }
    }
}
