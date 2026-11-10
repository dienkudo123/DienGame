using System;
using DienGame.Gameplay.Balance;
using UnityEngine;

namespace DienGame.Gameplay.Player
{
    public sealed class PlayerExperience : MonoBehaviour
    {
        [Header("Level")]
        [SerializeField, Min(1)] private int startingLevel = 1;

        private int _currentLevel;
        private int _currentExperience;

        public int CurrentLevel => _currentLevel;
        public int CurrentExperience => _currentExperience;

        public int ExperienceToNextLevel =>
            CalculateExperienceToNextLevel(_currentLevel);

        public event Action<int> LeveledUp;

        private void Awake()
        {
            _currentLevel = Mathf.Max(1, startingLevel);
            _currentExperience = 0;
        }

        public void AddExperience(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _currentExperience += amount;
            CheckLevelUp();
        }

        private void CheckLevelUp()
        {
            while (_currentExperience >= ExperienceToNextLevel)
            {
                int requiredExperience = ExperienceToNextLevel;

                _currentExperience -= requiredExperience;
                _currentLevel++;

                LeveledUp?.Invoke(_currentLevel);

                Debug.Log(
                    $"LEVEL UP -> {_currentLevel}",
                    this
                );
            }
        }

        private static int CalculateExperienceToNextLevel(int level)
        {
            int levelOffset = Mathf.Max(0, level - 1);

            return
                GameBalance.BaseExperienceToLevel +
                levelOffset * GameBalance.ExperienceIncreasePerLevel;
        }
    }
}
