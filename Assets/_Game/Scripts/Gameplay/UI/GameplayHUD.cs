using DienGame.Client;
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

        // Cache the last values written to UI. We still read gameplay state every
        // frame so HP changes, buffs and EXP pickups remain visible immediately.
        private bool _hasHealthFill;
        private float _lastHealthFill;
        private bool _hasHealthText;
        private int _lastShownHealth;
        private int _lastShownMaxHealth;

        private bool _hasExperienceFill;
        private float _lastExperienceFill;
        private bool _hasExperienceText;
        private int _lastShownExperience;
        private int _lastShownRequiredExperience;
        private bool _hasLevelText;
        private int _lastShownLevel;

        private void Start()
        {
            _health =
                FindFirstObjectByType<PlayerHealth>();

            _experience =
                FindFirstObjectByType<PlayerExperience>();

            // Keep the original circular HP/EXP art and its fill behavior.
            // Only improve typography/contrast and autosizing on narrow screens.
            PolishText();
            Refresh();
        }

        private void PolishText()
        {
            DienGameUITheme.StyleText(healthText, 32f, DienGameUITheme.Ivory,
                true, false, TextAlignmentOptions.Center);
            DienGameUITheme.StyleText(experienceText, 29f, DienGameUITheme.Ivory,
                true, false, TextAlignmentOptions.Center);
            DienGameUITheme.StyleText(levelText, 42f, DienGameUITheme.Gold,
                true, false, TextAlignmentOptions.Center);
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

            float currentHealth = _health.CurrentHealth;
            float maxHealth = _health.MaxHealth;
            float ratio = maxHealth > 0f
                ? currentHealth / maxHealth
                : 0f;

            if (healthFill != null)
            {
                float value = Mathf.Clamp01(ratio);
                if (!_hasHealthFill || _lastHealthFill != value)
                {
                    healthFill.fillAmount = value;
                    _lastHealthFill = value;
                    _hasHealthFill = true;
                }
            }

            if (healthText != null)
            {
                int shownHealth = Mathf.CeilToInt(currentHealth);
                int shownMaxHealth = Mathf.CeilToInt(maxHealth);

                if (!_hasHealthText ||
                    _lastShownHealth != shownHealth ||
                    _lastShownMaxHealth != shownMaxHealth)
                {
                    healthText.text = $"{shownHealth} / {shownMaxHealth}";
                    _lastShownHealth = shownHealth;
                    _lastShownMaxHealth = shownMaxHealth;
                    _hasHealthText = true;
                }
            }
        }

        private void RefreshExperience()
        {
            if (_experience == null)
            {
                return;
            }

            int required = _experience.ExperienceToNextLevel;
            int currentExperience = _experience.CurrentExperience;
            int currentLevel = _experience.CurrentLevel;
            float ratio = required > 0
                ? (float)currentExperience / required
                : 0f;

            if (experienceFill != null)
            {
                float value = Mathf.Clamp01(ratio);
                if (!_hasExperienceFill || _lastExperienceFill != value)
                {
                    experienceFill.fillAmount = value;
                    _lastExperienceFill = value;
                    _hasExperienceFill = true;
                }
            }

            if (experienceText != null &&
                (!_hasExperienceText ||
                 _lastShownExperience != currentExperience ||
                 _lastShownRequiredExperience != required))
            {
                experienceText.text = $"{currentExperience} / {required}";
                _lastShownExperience = currentExperience;
                _lastShownRequiredExperience = required;
                _hasExperienceText = true;
            }

            if (levelText != null &&
                (!_hasLevelText || _lastShownLevel != currentLevel))
            {
                // Chỉ hiện số, không có "Lv."
                levelText.text = currentLevel.ToString();
                _lastShownLevel = currentLevel;
                _hasLevelText = true;
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

            PolishText();
            // New UI references must receive the current values on the next frame.
            _hasHealthFill = false;
            _hasHealthText = false;
            _hasExperienceFill = false;
            _hasExperienceText = false;
            _hasLevelText = false;
        }
    }
}