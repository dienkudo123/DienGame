using System;
using UnityEngine;

namespace DienGame.Gameplay.Player
{
    public sealed class PlayerExperience : MonoBehaviour
    {
        [Header("Level")]

        [SerializeField, Min(1)]
        private int startingLevel = 1;

        [SerializeField, Min(1)]
        private int baseExperienceToLevel = 30;

        [SerializeField, Min(0)]
        private int experienceIncreasePerLevel = 20;

        private int _currentLevel;
        private int _currentExperience;

        public int CurrentLevel =>
            _currentLevel;

        public int CurrentExperience =>
            _currentExperience;

        public int ExperienceToNextLevel =>
            CalculateExperienceToNextLevel(
                _currentLevel
            );

        public event Action<int> LeveledUp;

        private void Awake()
        {
            _currentLevel =
                Mathf.Max(
                    1,
                    startingLevel
                );

            _currentExperience = 0;
        }

        public void AddExperience(
            int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _currentExperience +=
                amount;

            CheckLevelUp();
        }

        private void CheckLevelUp()
        {
            while (
                _currentExperience >=
                ExperienceToNextLevel
            )
            {
                int requiredExperience =
                    ExperienceToNextLevel;

                _currentExperience -=
                    requiredExperience;

                _currentLevel++;

                LeveledUp?.Invoke(
                    _currentLevel
                );

                Debug.Log(
                    $"LEVEL UP -> {_currentLevel}",
                    this
                );
            }
        }

        private int CalculateExperienceToNextLevel(
            int level)
        {
            int levelOffset =
                Mathf.Max(
                    0,
                    level - 1
                );

            return
                baseExperienceToLevel +
                levelOffset *
                experienceIncreasePerLevel;
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            startingLevel =
                Mathf.Max(
                    1,
                    startingLevel
                );

            baseExperienceToLevel =
                Mathf.Max(
                    1,
                    baseExperienceToLevel
                );

            experienceIncreasePerLevel =
                Mathf.Max(
                    0,
                    experienceIncreasePerLevel
                );
        }

#endif
    }
}