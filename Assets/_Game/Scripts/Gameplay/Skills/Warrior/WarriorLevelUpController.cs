using System.Collections.Generic;
using DienGame.Gameplay.Core;
using DienGame.Gameplay.Player;
using DienGame.Gameplay.Skills;
using DienGame.Gameplay.Upgrades;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Warrior
{
    /// <summary>Seven Warrior-only passives; reuses the existing responsive card UI.</summary>
    [RequireComponent(typeof(PlayerSkillManager))]
    [RequireComponent(typeof(PlayerExperience))]
    public sealed class WarriorLevelUpController : MonoBehaviour
    {
        private const string PauseKey = "WarriorLevelUp";
        private PlayerSkillManager _skills;
        private PlayerExperience _exp;
        private LevelUpUI _ui;
        private bool _ready, _choosing;
        private void Awake()
        {
            _skills = GetComponent<PlayerSkillManager>();
            _exp = GetComponent<PlayerExperience>();
        }
        private void OnEnable()
        {
            if (_exp == null) _exp = GetComponent<PlayerExperience>();
            if (_exp != null) _exp.LeveledUp += OnLeveledUp;
        }
        private void Start()
        {
            LocateUI();
            _ready = true;
            ShowNext();
        }
        private void Update()
        {
            if (_ready && !_choosing && AvailablePoints > 0) ShowNext();
        }
        private int AvailablePoints
        {
            get
            {
                if (_exp == null || _skills == null) return 0;
                int spent = 0;
                foreach (SkillId id in WarriorSkillCatalog.Passives) spent += _skills.GetSkillLevel(id);
                return Mathf.Max(0, _exp.CurrentLevel - 1 - spent);
            }
        }
        private void OnLeveledUp(int _) { if (_ready && !_choosing) ShowNext(); }
        private void LocateUI()
        {
            if (_ui != null) return;
            _ui = FindFirstObjectByType<LevelUpUI>();
            if (_ui != null) return;
            foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (canvas.name != "GameplayCanvas") continue;
                _ui = canvas.GetComponent<LevelUpUI>() ?? canvas.gameObject.AddComponent<LevelUpUI>();
                return;
            }
        }
        private void ShowNext()
        {
            if (!_ready || _choosing || AvailablePoints <= 0) return;
            LocateUI();
            if (_ui == null) return;
            List<UpgradeDefinition> all = new List<UpgradeDefinition>();
            foreach (SkillId id in WarriorSkillCatalog.Passives)
            {
                if (!_skills.CanUpgrade(id)) continue;
                int next = _skills.GetSkillLevel(id) + 1;
                string rank = new[] { "", "I", "II", "III", "IV", "V" }[next];
                all.Add(new UpgradeDefinition(id, WarriorSkillCatalog.Name(id) + " " + rank,
                    WarriorSkillCatalog.Description(id, next)));
            }
            if (all.Count == 0) return;
            List<UpgradeDefinition> picks = new List<UpgradeDefinition>();
            while (picks.Count < 3 && all.Count > 0)
            {
                int index = Random.Range(0, all.Count);
                picks.Add(all[index]);
                all.RemoveAt(index);
            }
            _choosing = true;
            GamePauseService.RequestPause(PauseKey);
            _ui.Show(picks, OnSelected);
        }
        private void OnSelected(UpgradeDefinition choice)
        {
            if (_skills != null) _skills.UpgradeSkill(choice.SkillId);
            _ui?.Hide();
            _choosing = false;
            GamePauseService.ReleasePause(PauseKey);
        }
        private void OnDisable()
        {
            if (_exp != null) _exp.LeveledUp -= OnLeveledUp;
            _ui?.Hide();
            GamePauseService.ReleasePause(PauseKey);
        }
        private void OnDestroy() => GamePauseService.ReleasePause(PauseKey);
    }
}
