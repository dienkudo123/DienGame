using System.Collections.Generic;
using DienGame.Gameplay.Core;
using DienGame.Gameplay.Player;
using DienGame.Gameplay.Run;
using DienGame.Gameplay.Skills;
using DienGame.Gameplay.Upgrades;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Mage
{
    /// <summary>Reuses existing LevelUpUI, PlayerExperience and PlayerSkillManager, Mage-only skills.</summary>
    [RequireComponent(typeof(PlayerExperience))]
    [RequireComponent(typeof(PlayerSkillManager))]
    public sealed class MageLevelUpController : MonoBehaviour
    {
        private const string PauseReason = "MageLevelUp";
        private PlayerExperience _experience;
        private PlayerSkillManager _skills;
        private LevelUpUI _ui;
        private bool _ready;
        private bool _choosing;

        private void Awake()
        {
            _experience = GetComponent<PlayerExperience>();
            _skills = GetComponent<PlayerSkillManager>();
        }
        private void OnEnable()
        {
            if (_experience == null) _experience = GetComponent<PlayerExperience>();
            if (_experience != null) _experience.LeveledUp += OnLevel;
        }
        private void Start()
        {
            LocateUI();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // Match the existing Assassin test workflow: 9 real skill choices at Lv10.
            if (_experience != null)
            {
                while (_experience.CurrentLevel < 10)
                    _experience.AddExperience(_experience.ExperienceToNextLevel);
            }
#endif
            _ready = true;
            OpenNext();
        }
        private void Update()
        {
            if (_ready && !_choosing && Points > 0) OpenNext();
        }
        private int Points
        {
            get
            {
                if (_experience == null || _skills == null) return 0;
                int spent = 0;
                foreach (SkillId id in MageSkillCatalog.Passives) spent += _skills.GetSkillLevel(id);
                return Mathf.Max(0, _experience.CurrentLevel - 1 - spent);
            }
        }
        private void OnLevel(int level)
        {
            if (_ready && !_choosing) OpenNext();
        }
        private void LocateUI()
        {
            if (_ui != null) return;
            _ui = FindFirstObjectByType<LevelUpUI>();
            if (_ui != null) return;
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas c in canvases)
            {
                if (c != null && c.name == "GameplayCanvas")
                {
                    _ui = c.GetComponent<LevelUpUI>();
                    if (_ui == null) _ui = c.gameObject.AddComponent<LevelUpUI>();
                    return;
                }
            }
            if (canvases.Length > 0)
            {
                _ui = canvases[0].GetComponent<LevelUpUI>();
                if (_ui == null) _ui = canvases[0].gameObject.AddComponent<LevelUpUI>();
            }
        }
        private void OpenNext()
        {
            if (!_ready || _choosing || Points <= 0) return;
            LocateUI();
            if (_ui == null) return;
            List<UpgradeDefinition> options = new List<UpgradeDefinition>();
            foreach (SkillId id in MageSkillCatalog.Passives)
            {
                // One exclusive skill for the selected pet; shared upgrades stay visible.
                if (!MageSkillCatalog.AvailableForSelectedPet(id) || !_skills.CanUpgrade(id)) continue;
                int next = _skills.GetSkillLevel(id) + 1;
                string roman = new[] { "", "I", "II", "III", "IV", "V" }[next];
                options.Add(new UpgradeDefinition(id, MageSkillCatalog.Name(id) + " " + roman,
                    MageSkillCatalog.Description(id, next)));
            }
            if (options.Count == 0) return;
            List<UpgradeDefinition> choices = new List<UpgradeDefinition>();
            while (choices.Count < 3 && options.Count > 0)
            {
                int index = Random.Range(0, options.Count);
                choices.Add(options[index]);
                options.RemoveAt(index);
            }
            _choosing = true;
            GamePauseService.RequestPause(PauseReason);
            _ui.Show(choices, Upgrade);
        }
        private void Upgrade(UpgradeDefinition choice)
        {
            if (!_choosing || !_skills.CanUpgrade(choice.SkillId)) return;
            bool valid = false;
            foreach (SkillId id in MageSkillCatalog.Passives) if (id == choice.SkillId) valid = true;
            if (!valid) return;
            _skills.UpgradeSkill(choice.SkillId);
            _ui.Hide();
            _choosing = false;
            if (Points > 0) OpenNext();
            else GamePauseService.ReleasePause(PauseReason);
        }
        private void OnDisable()
        {
            if (_experience != null) _experience.LeveledUp -= OnLevel;
            if (_ui != null) _ui.Hide();
            _choosing = false;
            GamePauseService.ReleasePause(PauseReason);
        }
    }
}
