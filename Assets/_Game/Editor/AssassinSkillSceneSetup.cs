using System;
using DienGame.Gameplay.Player;
using DienGame.Gameplay.Run;
using DienGame.Gameplay.Skills;
using DienGame.Gameplay.Skills.Assassin;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DienGame.EditorTools
{
    public static class AssassinSkillSceneSetup
    {
        private const string AssassinPath = "Assets/_Game/Scenes/GameplayTest_AssassinPreview.unity";

        [MenuItem("DienGame/Assassin/3 - Install 7 Passive Skills")]
        public static void Install()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("Stop Play Mode before installing Assassin skills.");
                return;
            }
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(AssassinPath) == null)
            {
                Debug.LogError("Assassin scene missing. Run DienGame > Client > Setup Character Selection (2 classes) first.");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string previousPath = SceneManager.GetActiveScene().path;
            bool opened = false;
            try
            {
                Scene scene = EditorSceneManager.OpenScene(AssassinPath, OpenSceneMode.Single);
                opened = true;
                PlayerMovement movement = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();
                if (movement == null) throw new InvalidOperationException("PlayerMovement missing in Assassin scene.");
                GameObject player = movement.gameObject;
                AssassinMeleeAttack melee = player.GetComponent<AssassinMeleeAttack>();
                if (melee == null) throw new InvalidOperationException("AssassinMeleeAttack missing. Run Client Setup first.");
                if (player.GetComponent<PlayerSkillManager>() == null)
                    player.AddComponent<PlayerSkillManager>();
                AssassinSkillController skills = player.GetComponent<AssassinSkillController>();
                if (skills == null) skills = player.AddComponent<AssassinSkillController>();
                skills.enabled = true;
                AssassinLevelUpController upgrade = player.GetComponent<AssassinLevelUpController>();
                if (upgrade == null) upgrade = player.AddComponent<AssassinLevelUpController>();
                upgrade.enabled = true;

                LevelUpController marksmanLevelUp = UnityEngine.Object.FindFirstObjectByType<LevelUpController>(FindObjectsInactive.Include);
                if (marksmanLevelUp != null) marksmanLevelUp.enabled = false;

                EditorUtility.SetDirty(player);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("Assassin 7 passive skills installed in Assassin scene ONLY. No Marksman scene or gameplay scripts changed.");
            }
            catch (Exception e)
            {
                Debug.LogError("Assassin skill install failed: " + e.Message);
            }
            finally
            {
                if (opened && !string.IsNullOrEmpty(previousPath) && AssetDatabase.LoadAssetAtPath<SceneAsset>(previousPath) != null)
                    EditorSceneManager.OpenScene(previousPath, OpenSceneMode.Single);
            }
        }
    }
}
