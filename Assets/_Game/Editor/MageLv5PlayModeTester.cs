#if UNITY_EDITOR
using DienGame.Gameplay.Skills;
using DienGame.Gameplay.Skills.Mage;
using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    /// <summary>
    /// Optional QA utility. Does not serialize level changes; scene reload restores normal progression.
    /// </summary>
    public static class MageLv5PlayModeTester
    {
        private const string MenuPath = "DienGame/Mage/4 - Test Unlock All Lv5 (Play Mode Only)";

        [MenuItem(MenuPath)]
        private static void UpgradeMageSkillsForTesting()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Enter Play Mode in the Mage scene before running the Lv5 skill test.");
                return;
            }
            MageClassController mage = Object.FindFirstObjectByType<MageClassController>();
            if (mage == null)
            {
                Debug.LogWarning("No MageClassController found in the active scene.");
                return;
            }
            PlayerSkillManager manager = mage.GetComponent<PlayerSkillManager>();
            if (manager == null)
            {
                Debug.LogWarning("Mage PlayerSkillManager not found.");
                return;
            }
            foreach (SkillId skill in MageSkillCatalog.Passives)
            {
                while (manager.CanUpgrade(skill) && manager.GetSkillLevel(skill) < 5)
                    manager.UpgradeSkill(skill);
            }
            Debug.Log("Mage Lv5 QA: all seven passives are Lv5 for this Play Mode session.");
        }

        [MenuItem(MenuPath, true)]
        private static bool Validate()
        {
            return Application.isPlaying;
        }
    }
}
#endif
