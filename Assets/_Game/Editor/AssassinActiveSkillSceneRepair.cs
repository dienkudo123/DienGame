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
    /// Optional one-click persistent scene repair. Runtime guard already works without it.
    /// Operates ONLY on the Assassin Preview scene and never rebuilds map/UI/assets.
    /// </summary>
    public static class AssassinActiveSkillSceneRepair
    {
        private const string ScenePath =
            "Assets/_Game/Scenes/GameplayTest_AssassinPreview.unity";

        [MenuItem("DienGame/Assassin/6 - Fix Active Skill (Camille Hook)")]
        public static void Repair()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("Exit Play Mode before fixing the Assassin scene.");
                return;
            }
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                Debug.LogError("Missing Assassin scene: " + ScenePath);
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            string priorScene = SceneManager.GetActiveScene().path;
            bool opened = false;
            try
            {
                Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                opened = true;
                AssassinMeleeAttack[] attacks =
                    UnityEngine.Object.FindObjectsByType<AssassinMeleeAttack>(
                        FindObjectsInactive.Include, FindObjectsSortMode.None);
                AssassinMeleeAttack melee = null;
                for (int i = 0; i < attacks.Length; i++)
                {
                    if (attacks[i].gameObject.scene == scene) { melee = attacks[i]; break; }
                }
                if (melee == null)
                    throw new InvalidOperationException("AssassinMeleeAttack missing in Assassin scene.");

                GameObject player = melee.gameObject;
                Disable(player.GetComponent<PlayerAutoAttack>());
                Disable(player.GetComponent<MarksmanSkillController>());
                Disable(player.GetComponent<AirStrikeController>());
                JudgementController judgement = player.GetComponent<JudgementController>();
                Disable(judgement);
                if (judgement != null)
                {
                    SerializedObject serialized = new SerializedObject(judgement);
                    SerializedProperty autoCreate = serialized.FindProperty("autoCreateButton");
                    if (autoCreate != null)
                    {
                        autoCreate.boolValue = false;
                        serialized.ApplyModifiedPropertiesWithoutUndo();
                    }
                }

                GameObject[] roots = scene.GetRootGameObjects();
                int deletedButtons = 0;
                Canvas canvas = null;
                for (int i = 0; i < roots.Length; i++)
                {
                    JudgementUltimateButton[] scripts =
                        roots[i].GetComponentsInChildren<JudgementUltimateButton>(true);
                    for (int j = 0; j < scripts.Length; j++) Disable(scripts[j]);

                    Canvas[] canvases = roots[i].GetComponentsInChildren<Canvas>(true);
                    for (int j = 0; j < canvases.Length; j++)
                        if (canvases[j].name == "GameplayCanvas") canvas = canvases[j];

                    Transform[] children = roots[i].GetComponentsInChildren<Transform>(true);
                    for (int j = children.Length - 1; j >= 0; j--)
                    {
                        Transform t = children[j];
                        if (t != null && t.name == "JudgementUltimateButton" &&
                            t.GetComponentInParent<Canvas>(true) != null)
                        {
                            Undo.DestroyObjectImmediate(t.gameObject);
                            deletedButtons++;
                        }
                    }
                }
                if (canvas == null)
                    throw new InvalidOperationException("GameplayCanvas missing in Assassin scene.");

                AssassinWallDive wallDive = player.GetComponent<AssassinWallDive>();
                if (wallDive == null) wallDive = Undo.AddComponent<AssassinWallDive>(player);
                Undo.RecordObject(wallDive, "Enable Assassin Active Skill");
                wallDive.enabled = true;

                AssassinWallDiveHud hud = canvas.GetComponent<AssassinWallDiveHud>();
                if (hud == null) hud = Undo.AddComponent<AssassinWallDiveHud>(canvas.gameObject);
                Undo.RecordObject(hud, "Bind Assassin Active Button");
                hud.SetWallDive(wallDive);
                hud.enabled = true;

                EditorUtility.SetDirty(player);
                EditorUtility.SetDirty(canvas.gameObject);
                EditorUtility.SetDirty(wallDive);
                EditorUtility.SetDirty(hud);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException("Could not save Assassin scene.");
                Debug.Log("Assassin active skill repaired: Phi Anh Bat Tuong installed; " +
                          "Marksman active disabled; " + deletedButtons +
                          " obsolete UI objects removed. Marksman scene untouched.");
            }
            catch (Exception e)
            {
                Debug.LogError("Assassin active skill repair failed: " + e);
            }
            finally
            {
                if (opened && !string.IsNullOrEmpty(priorScene) &&
                    AssetDatabase.LoadAssetAtPath<SceneAsset>(priorScene) != null)
                    EditorSceneManager.OpenScene(priorScene, OpenSceneMode.Single);
            }
        }

        private static void Disable(Behaviour behaviour)
        {
            if (behaviour == null || !behaviour.enabled) return;
            Undo.RecordObject(behaviour, "Disable Marksman skill on Assassin");
            behaviour.enabled = false;
            EditorUtility.SetDirty(behaviour);
        }
    }
}
