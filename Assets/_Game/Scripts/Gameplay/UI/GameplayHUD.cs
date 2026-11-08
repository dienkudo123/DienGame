using DienGame.Gameplay.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DienGame.Gameplay.UI
{
    public sealed class GameplayHUD : MonoBehaviour
    {
        [SerializeField]
        private Image healthFill;

        [SerializeField]
        private TMP_Text healthText;

        [SerializeField]
        private Image experienceFill;

        [SerializeField]
        private TMP_Text experienceText;

        [SerializeField]
        private TMP_Text levelText;

        private PlayerHealth _health;
        private PlayerExperience _experience;

        private void Start()
        {
            _health =
                FindFirstObjectByType<PlayerHealth>();

            _experience =
                FindFirstObjectByType<PlayerExperience>();

            Refresh();
        }

        private void Update()
        {
            Refresh();
        }

        private void Refresh()
        {
            RefreshHealth();
            RefreshExperience();
        }

        private void RefreshHealth()
        {
            if (_health == null)
            {
                return;
            }

            float value =
                _health.MaxHealth > 0f
                    ? _health.CurrentHealth /
                      _health.MaxHealth
                    : 0f;

            if (healthFill != null)
            {
                healthFill.fillAmount =
                    Mathf.Clamp01(value);
            }

            if (healthText != null)
            {
                healthText.text =
                    $"{Mathf.CeilToInt(_health.CurrentHealth)}" +
                    $" / {Mathf.CeilToInt(_health.MaxHealth)}";
            }
        }

        private void RefreshExperience()
        {
            if (_experience == null)
            {
                return;
            }

            int required =
                _experience.ExperienceToNextLevel;

            float value =
                required > 0
                    ? (float)_experience.CurrentExperience /
                      required
                    : 0f;

            if (experienceFill != null)
            {
                experienceFill.fillAmount =
                    Mathf.Clamp01(value);
            }

            if (experienceText != null)
            {
                experienceText.text =
                    $"{_experience.CurrentExperience}" +
                    $" / {required}";
            }

            if (levelText != null)
            {
                // Chỉ hiện số, không có "Lv."
                levelText.text =
                    _experience.CurrentLevel.ToString();
            }
        }

        public void Configure(
            Image newHealthFill,
            TMP_Text newHealthText,
            Image newExperienceFill,
            TMP_Text newExperienceText,
            TMP_Text newLevelText)
        {
            healthFill =
                newHealthFill;

            healthText =
                newHealthText;

            experienceFill =
                newExperienceFill;

            experienceText =
                newExperienceText;

            levelText =
                newLevelText;
        }
    }
}