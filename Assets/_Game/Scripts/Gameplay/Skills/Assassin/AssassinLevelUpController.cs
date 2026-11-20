using System.Collections.Generic;
using DienGame.Gameplay.Core;
using DienGame.Gameplay.Player;
using DienGame.Gameplay.Run;
using DienGame.Gameplay.Upgrades;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DienGame.Gameplay.Skills.Assassin
{
    /// <summary>
    /// Assassin-only skill selection. For Editor/Development builds, starts the
    /// Assassin at level 10 with nine actual upgrade choices (levels 2..10).
    /// Marksman progression and its level-up UI logic are not changed.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerExperience))]
    [RequireComponent(typeof(PlayerSkillManager))]
    public sealed class AssassinLevelUpController : MonoBehaviour
    {
        private const string PauseReason = "AssassinLevelUp";

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const int TestStartingLevel = 10;
#endif

        private PlayerExperience _experience;
        private PlayerSkillManager _skills;
        private LevelUpUI _ui;
        private bool _ready;
        private bool _choosing;
        private bool _warnedMissingUI;

        // This hook runs for both Play From Assassin Scene and Lobby -> Assassin.
        // No Inspector setup or editor menu is necessary.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneHook()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // The Marksman scene does not have an AssassinMeleeAttack component.
            AssassinMeleeAttack assassin =
                Object.FindFirstObjectByType<AssassinMeleeAttack>();
            if (assassin == null)
            {
                return;
            }

            GameObject player = assassin.gameObject;

            // The original character-selection scene builder disables the
            // Marksman upgrade flow. Keep the two classes isolated.
            LevelUpController marksmanUpgrade =
                Object.FindFirstObjectByType<LevelUpController>(FindObjectsInactive.Include);
            if (marksmanUpgrade != null)
            {
                marksmanUpgrade.enabled = false;
            }

            if (player.GetComponent<PlayerExperience>() == null)
            {
                player.AddComponent<PlayerExperience>();
            }
            if (player.GetComponent<PlayerSkillManager>() == null)
            {
                player.AddComponent<PlayerSkillManager>();
            }
            if (player.GetComponent<AssassinSkillController>() == null)
            {
                player.AddComponent<AssassinSkillController>();
            }

            AssassinLevelUpController controller =
                player.GetComponent<AssassinLevelUpController>();
            if (controller == null)
            {
                controller = player.AddComponent<AssassinLevelUpController>();
            }
            controller.enabled = true;
        }

        private void Awake()
        {
            _experience = GetComponent<PlayerExperience>();
            _skills = GetComponent<PlayerSkillManager>();
        }

        private void OnEnable()
        {
            if (_experience == null)
            {
                _experience = GetComponent<PlayerExperience>();
            }
            if (_experience != null)
            {
                _experience.LeveledUp -= OnLeveledUp;
                _experience.LeveledUp += OnLeveledUp;
            }
        }

        private void Start()
        {
            FindOrCreateUpgradeUI();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // Grant real EXP up to Lv10 so the existing EXP and HUD systems
            // remain consistent. We do not fake a displayed level number.
            // The event handler deliberately waits until _ready is true.
            if (_experience != null)
            {
                while (_experience.CurrentLevel < TestStartingLevel)
                {
                    int needed = _experience.ExperienceToNextLevel;
                    if (needed <= 0)
                    {
                        Debug.LogError("Assassin Lv10 test: invalid EXP requirement.", this);
                        break;
                    }
                    _experience.AddExperience(needed);
                }
                Debug.Log(
                    $"Assassin TEST: Starting at Lv{_experience.CurrentLevel}; " +
                    $"available upgrades = {GetAvailableSkillPoints()}.", this);
            }
#endif

            _ready = true;
            ShowNextUpgrade();
        }

        private void Update()
        {
            // Level-ups can occur before the Canvas becomes available.
            // Never discard a skill point because of a missing UI reference.
            if (!_ready || _choosing || GetAvailableSkillPoints() <= 0)
            {
                return;
            }

            if (_ui == null)
            {
                FindOrCreateUpgradeUI();
            }
            ShowNextUpgrade();
        }

        private void OnLeveledUp(int newLevel)
        {
            // Available points are derived from the actual player level and
            // already-upgraded Assassin skills, so multi-level EXP pickups
            // cannot lose or duplicate points.
            if (_ready && !_choosing)
            {
                ShowNextUpgrade();
            }
        }

        private int GetAvailableSkillPoints()
        {
            if (_experience == null || _skills == null)
            {
                return 0;
            }

            int spent = 0;
            foreach (SkillId id in AssassinSkillCatalog.PassiveSkills)
            {
                spent += _skills.GetSkillLevel(id);
            }

            // Lv1: zero points; Lv10: nine points total.
            return Mathf.Max(0, (_experience.CurrentLevel - 1) - spent);
        }

        private void FindOrCreateUpgradeUI()
        {
            if (_ui != null)
            {
                return;
            }

            _ui = Object.FindFirstObjectByType<LevelUpUI>();
            if (_ui != null)
            {
                return;
            }

            Canvas candidate = null;
            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas canvas in canvases)
            {
                if (canvas != null && canvas.name == "GameplayCanvas")
                {
                    candidate = canvas;
                    break;
                }
            }
            if (candidate == null)
            {
                foreach (Canvas canvas in canvases)
                {
                    if (canvas != null && canvas.renderMode != RenderMode.WorldSpace)
                    {
                        candidate = canvas;
                        break;
                    }
                }
            }
            if (candidate == null)
            {
                if (!_warnedMissingUI)
                {
                    Debug.LogWarning(
                        "Assassin level-up: waiting for GameplayCanvas; " +
                        "unspent skill points will be preserved.", this);
                    _warnedMissingUI = true;
                }
                return;
            }

            _ui = candidate.GetComponent<LevelUpUI>();
            if (_ui == null)
            {
                _ui = candidate.gameObject.AddComponent<LevelUpUI>();
            }
            _warnedMissingUI = false;
        }

        private void ShowNextUpgrade()
        {
            if (!_ready || _choosing || GetAvailableSkillPoints() == 0)
            {
                return;
            }
            if (_ui == null)
            {
                FindOrCreateUpgradeUI();
                if (_ui == null)
                {
                    return;
                }
            }

            List<UpgradeDefinition> available = new List<UpgradeDefinition>();
            foreach (SkillId id in AssassinSkillCatalog.PassiveSkills)
            {
                if (!_skills.CanUpgrade(id))
                {
                    continue;
                }

                int next = _skills.GetSkillLevel(id) + 1;
                string roman = next == 1 ? "I"
                    : next == 2 ? "II"
                    : next == 3 ? "III"
                    : next == 4 ? "IV" : "V";
                available.Add(new UpgradeDefinition(
                    id,
                    AssassinSkillCatalog.Name(id) + " " + roman,
                    AssassinSkillCatalog.Description(id, next)));
            }

            if (available.Count == 0)
            {
                // All Assassin passives have reached Lv5.
                GamePauseService.ReleasePause(PauseReason);
                return;
            }

            List<UpgradeDefinition> choices = new List<UpgradeDefinition>(3);
            while (choices.Count < 3 && available.Count > 0)
            {
                int index = Random.Range(0, available.Count);
                choices.Add(available[index]);
                available.RemoveAt(index);
            }

            _choosing = true;
            GamePauseService.RequestPause(PauseReason);
            _ui.Show(choices, OnUpgradeChosen);
        }

        private void OnUpgradeChosen(UpgradeDefinition upgrade)
        {
            if (!_choosing || _skills == null)
            {
                return;
            }

            bool isAssassinSkill = false;
            foreach (SkillId id in AssassinSkillCatalog.PassiveSkills)
            {
                if (id == upgrade.SkillId)
                {
                    isAssassinSkill = true;
                    break;
                }
            }
            if (!isAssassinSkill || !_skills.CanUpgrade(upgrade.SkillId))
            {
                Debug.LogWarning("Assassin: rejected invalid skill upgrade.", this);
                return;
            }

            int newSkillLevel = _skills.UpgradeSkill(upgrade.SkillId);
            Debug.Log(
                $"Assassin skill: {upgrade.SkillId} -> Lv{newSkillLevel}; " +
                $"points left: {GetAvailableSkillPoints()}", this);

            _ui.Hide();
            _choosing = false;

            if (GetAvailableSkillPoints() > 0)
            {
                ShowNextUpgrade();
            }
            else
            {
                GamePauseService.ReleasePause(PauseReason);
            }
        }

        private void OnDisable()
        {
            if (_experience != null)
            {
                _experience.LeveledUp -= OnLeveledUp;
            }
            _ui?.Hide();
            _choosing = false;
            GamePauseService.ReleasePause(PauseReason);
        }

        private void OnDestroy()
        {
            GamePauseService.ReleasePause(PauseReason);
        }
    }
}
