using System;
using DienGame.Gameplay.Player;
using DienGame.Gameplay.Skills.Assassin;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DienGame.EditorTools
{
    public static class AssassinWallDiveSceneSetup
    {
        private const string AssassinPath = "Assets/_Game/Scenes/GameplayTest_AssassinPreview.unity";

        [MenuItem("DienGame/Assassin/4 - Install Wall Dive + Class Skill Guard")]
        public static void Install()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("Stop Play Mode before installing Wall Dive.");
                return;
            }
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(AssassinPath) == null)
            {
                Debug.LogError("Assassin scene not found. Run Client Character Selection Setup first.");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string previousPath = SceneManager.GetActiveScene().path;
            bool sceneOpened = false;
            try
            {
                Scene scene = EditorSceneManager.OpenScene(AssassinPath, OpenSceneMode.Single);
                sceneOpened = true;
                AssassinMeleeAttack melee = UnityEngine.Object.FindFirstObjectByType<AssassinMeleeAttack>();
                if (melee == null)
                    throw new InvalidOperationException("AssassinMeleeAttack missing. Run DienGame > Client > Setup Character Selection first.");
                GameObject player = melee.gameObject;
                if (player.GetComponent<PlayerMovement>() == null)
                    throw new InvalidOperationException("PlayerMovement missing from Assassin Player.");
                if (player.GetComponent<AssassinSkillController>() == null ||
                    player.GetComponent<AssassinLevelUpController>() == null)
                    throw new InvalidOperationException("Install 7 Passive Skills (menu 3) before installing Wall Dive.");

                Canvas gameplayCanvas = null;
                Canvas[] canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
                for (int i = 0; i < canvases.Length; i++)
                {
                    if (canvases[i].name == "GameplayCanvas")
                    {
                        gameplayCanvas = canvases[i];
                        break;
                    }
                }
                if (gameplayCanvas == null)
                    throw new InvalidOperationException("GameplayCanvas missing from Assassin scene.");

                AssassinWallDive ability = player.GetComponent<AssassinWallDive>();
                if (ability == null) ability = player.AddComponent<AssassinWallDive>();
                ability.enabled = true;
                AssassinWallDiveHud hud = gameplayCanvas.GetComponent<AssassinWallDiveHud>();
                if (hud == null) hud = gameplayCanvas.gameObject.AddComponent<AssassinWallDiveHud>();
                hud.SetWallDive(ability);
                hud.enabled = true;

                EditorUtility.SetDirty(player);
                EditorUtility.SetDirty(gameplayCanvas.gameObject);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException("Unity could not save the Assassin scene.");
                Debug.Log("DienGame: Wall Dive installed ONLY in Assassin scene. No Marksman scene modified. Skill class isolation is enforced in PlayerSkillManager.");
            }
            catch (Exception ex)
            {
                Debug.LogError("Assassin Wall Dive install failed: " + ex.Message);
            }
            finally
            {
                if (sceneOpened && !string.IsNullOrEmpty(previousPath) &&
                    AssetDatabase.LoadAssetAtPath<SceneAsset>(previousPath) != null)
                    EditorSceneManager.OpenScene(previousPath, OpenSceneMode.Single);
            }
        }
    }
}
