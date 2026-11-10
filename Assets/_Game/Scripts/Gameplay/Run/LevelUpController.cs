using System.Collections.Generic;
using DienGame.Gameplay.Core;
using DienGame.Gameplay.Player;
using DienGame.Gameplay.Skills;
using DienGame.Gameplay.Upgrades;
using UnityEngine;

namespace DienGame.Gameplay.Run
{
    public sealed class LevelUpController : MonoBehaviour
    {
        private const string PauseReason = "LevelUp";

        private PlayerExperience _experience;
        private PlayerAutoAttack _autoAttack;
        private PlayerSkillManager _skillManager;
        private LevelUpUI _ui;
        private bool _isChoosingUpgrade;

        private void Start()
        {
            FindReferences();

            if (_experience != null)
            {
                _experience.LeveledUp += OnLeveledUp;
            }
        }

        private void FindReferences()
        {
            _experience = FindFirstObjectByType<PlayerExperience>();
            _autoAttack = FindFirstObjectByType<PlayerAutoAttack>();

            if (_autoAttack != null)
            {
                _skillManager = _autoAttack.GetComponent<PlayerSkillManager>();

                if (_skillManager == null)
                {
                    _skillManager = _autoAttack.gameObject.AddComponent<PlayerSkillManager>();
                }
            }

            _ui = FindFirstObjectByType<LevelUpUI>();

            if (_ui == null)
            {
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    _ui = canvas.gameObject.AddComponent<LevelUpUI>();
                }
            }

            ValidateReferences();
        }

        private void ValidateReferences()
        {
            if (_experience == null) Debug.LogError("LevelUpController: Không tìm thấy PlayerExperience.", this);
            if (_autoAttack == null) Debug.LogError("LevelUpController: Không tìm thấy PlayerAutoAttack.", this);
            if (_skillManager == null) Debug.LogError("LevelUpController: Không tìm thấy PlayerSkillManager.", this);
            if (_ui == null) Debug.LogError("LevelUpController: Không tìm thấy LevelUpUI/GameplayCanvas.", this);
        }

        private void OnLeveledUp(int newLevel)
        {
            if (!_isChoosingUpgrade)
            {
                OpenUpgradeSelection();
            }
        }

        private void OpenUpgradeSelection()
        {
            if (_ui == null || _skillManager == null)
            {
                return;
            }

            List<UpgradeDefinition> options = LevelUpUpgradePool.GetRandomOptions(3, _skillManager);

            if (options.Count == 0)
            {
                Debug.Log("Không còn skill Xạ thủ nào có thể nâng cấp.", this);
                return;
            }

            _isChoosingUpgrade = true;
            GamePauseService.RequestPause(PauseReason);
            _ui.Show(options, OnUpgradeSelected);
        }

        private void OnUpgradeSelected(UpgradeDefinition upgrade)
        {
            if (_skillManager == null)
            {
                return;
            }

            int newLevel = _skillManager.UpgradeSkill(upgrade.SkillId);
            Debug.Log($"{upgrade.SkillId} -> Lv{newLevel}", this);

            _ui?.Hide();
            _isChoosingUpgrade = false;
            GamePauseService.ReleasePause(PauseReason);
        }

        private void OnDestroy()
        {
            if (_experience != null)
            {
                _experience.LeveledUp -= OnLeveledUp;
            }

            GamePauseService.ReleasePause(PauseReason);
        }
    }
}
