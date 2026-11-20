using System;
using DienGame.Gameplay.Player;
using DienGame.Gameplay.Skills.Assassin;
using DienGame.Gameplay.Skills.Marksman;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DienGame.EditorTools
{
    /// <summary>
    /// Fixes leftover Marksman ultimate UI in the separate Assassin scene.
    /// Does not modify GameplayTest (Marksman), Lobby, combat balance, or any prefabs.
    /// Safe to run more than once.
    /// </summary>
    public static class AssassinHudClassIsolationFix
    {
        private const string AssassinScenePath =
            "Assets/_Game/Scenes/GameplayTest_AssassinPreview.unity";

        private const string GameplayCanvasName = "GameplayCanvas";
        private const string MarksmanUltimateButtonName = "JudgementUltimateButton";

        [MenuItem("DienGame/Assassin/5 - Fix HUD (Remove Marksman Ultimate)")]
        public static void Fix()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("Assassin HUD fix: Exit Play Mode first.");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(AssassinScenePath) == null)
            {
                Debug.LogError(
                    "Assassin HUD fix: Missing GameplayTest_AssassinPreview.unity. " +
                    "Set up the Assassin scene first.");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            string previousScenePath = SceneManager.GetActiveScene().path;
            bool opened = false;

            try
            {
                Scene assassinScene = EditorSceneManager.OpenScene(
                    AssassinScenePath, OpenSceneMode.Single);
                opened = true;

                AssassinMeleeAttack assassinAttack =
                    UnityEngine.Object.FindFirstObjectByType<AssassinMeleeAttack>(
                        FindObjectsInactive.Include);

                if (assassinAttack == null || assassinAttack.gameObject.scene != assassinScene)
                {
                    throw new InvalidOperationException(
                        "AssassinMeleeAttack not found in Assassin scene. " +
                        "Do not apply this repair to the Marksman scene.");
                }

                GameObject player = assassinAttack.gameObject;

                // The shared Player and joystick remain untouched. Only Marksman
                // combat behaviour is disabled in the Assassin scene.
                DisableIfPresent(player.GetComponent<PlayerAutoAttack>());
                DisableIfPresent(player.GetComponent<MarksmanSkillController>());
                DisableIfPresent(player.GetComponent<AirStrikeController>());

                JudgementController judgement = player.GetComponent<JudgementController>();
                if (judgement != null)
                {
                    DisableIfPresent(judgement);

                    // Defence in depth: do not create the Marksman ultimate button
                    // even if another setup later accidentally enables the component.
                    SerializedObject serialized = new SerializedObject(judgement);
                    SerializedProperty autoCreate = serialized.FindProperty("autoCreateButton");
                    if (autoCreate != null && autoCreate.boolValue)
                    {
                        autoCreate.boolValue = false;
                        serialized.ApplyModifiedProperties();
                    }
                }

                // Some versions attach this component directly to the player.
                JudgementUltimateButton[] oldButtonScripts =
                    player.GetComponentsInChildren<JudgementUltimateButton>(true);
                for (int i = 0; i < oldButtonScripts.Length; i++)
                {
                    DisableIfPresent(oldButtonScripts[i]);
                }

                Canvas gameplayCanvas = null;
                GameObject[] roots = assassinScene.GetRootGameObjects();
                for (int i = 0; i < roots.Length && gameplayCanvas == null; i++)
                {
                    Canvas[] canvases = roots[i].GetComponentsInChildren<Canvas>(true);
                    for (int j = 0; j < canvases.Length; j++)
                    {
                        if (canvases[j].name == GameplayCanvasName)
                        {
                            gameplayCanvas = canvases[j];
                            break;
                        }
                    }
                }

                // Remove only the old UI object if it was serialized in this scene.
                // A dynamically created runtime button is prevented by the disabled
                // JudgementController and autoCreateButton = false above.
                int removedButtons = 0;
                for (int i = 0; i < roots.Length; i++)
                {
                    Transform[] all = roots[i].GetComponentsInChildren<Transform>(true);
                    for (int j = all.Length - 1; j >= 0; j--)
                    {
                        Transform target = all[j];
                        if (target == null || target.name != MarksmanUltimateButtonName)
                        {
                            continue;
                        }

                        if (target.GetComponentInParent<Canvas>(true) == null)
                        {
                            continue;
                        }

                        Undo.DestroyObjectImmediate(target.gameObject);
                        removedButtons++;
                    }
                }

                // Keep the Assassin active skill UI working. Do not generate an
                // active ability here: it must already be installed in step 4.
                AssassinWallDive ability = player.GetComponent<AssassinWallDive>();
                if (ability != null && gameplayCanvas != null)
                {
                    AssassinWallDiveHud hud =
                        gameplayCanvas.GetComponent<AssassinWallDiveHud>();
                    if (hud == null)
                    {
                        hud = Undo.AddComponent<AssassinWallDiveHud>(
                            gameplayCanvas.gameObject);
                    }

                    Undo.RecordObject(hud, "Repair Assassin active skill HUD");
                    hud.SetWallDive(ability);
                    hud.enabled = true;
                    EditorUtility.SetDirty(hud);
                }

                EditorSceneManager.MarkSceneDirty(assassinScene);
                if (!EditorSceneManager.SaveScene(assassinScene))
                {
                    throw new InvalidOperationException("Could not save Assassin scene.");
                }

                Debug.Log(
                    "Assassin HUD fix: Marksman auto-attack / ultimate disabled " +
                    "ONLY in Assassin scene. Old saved ultimate buttons removed: " +
                    removedButtons + ". Marksman scene was not edited.");

                if (ability == null)
                {
                    Debug.LogWarning(
                        "Assassin HUD fix: Wall Dive is not installed yet. " +
                        "Run DienGame > Assassin > 4 - Install Wall Dive + Class Skill Guard " +
                        "after installing the 7 passive skills.");
                }
                else if (gameplayCanvas == null)
                {
                    Debug.LogWarning(
                        "Assassin HUD fix: GameplayCanvas not found, " +
                        "so Wall Dive HUD could not be checked.");
                }
            }
            catch (Exception exception)
            {
                Debug.LogError("Assassin HUD fix failed: " + exception);
            }
            finally
            {
                if (opened && !string.IsNullOrEmpty(previousScenePath) &&
                    AssetDatabase.LoadAssetAtPath<SceneAsset>(previousScenePath) != null)
                {
                    EditorSceneManager.OpenScene(previousScenePath, OpenSceneMode.Single);
                }
            }
        }

        private static void DisableIfPresent(Behaviour component)
        {
            if (component == null || !component.enabled)
            {
                return;
            }

            Undo.RecordObject(component, "Disable Marksman behaviour in Assassin scene");
            component.enabled = false;
            EditorUtility.SetDirty(component);
        }
    }
}
