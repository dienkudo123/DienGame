using System;
using System.Collections.Generic;
using DienGame.Gameplay.Player;
using DienGame.Gameplay.Run;
using DienGame.Gameplay.Skills;
using DienGame.Gameplay.Skills.Mage;
using DienGame.Gameplay.Skills.Marksman;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DienGame.EditorTools
{
    /// <summary>
    /// ONE-CLICK setup. Creates a separate Mage gameplay scene by copying the
    /// original Marksman scene, keeps Player/Enemy/EXP/Camera systems unchanged.
    /// Safe on repeated runs. Does not rewrite Marksman/Assassin scenes.
    /// </summary>
    public static class MageSceneInstaller
    {
        private const string Lobby = "Assets/_Game/Scenes/Lobby.unity";
        private const string Marksman = "Assets/_Game/Scenes/GameplayTest.unity";
        private const string Assassin = "Assets/_Game/Scenes/GameplayTest_AssassinPreview.unity";
        private const string MageScene = "Assets/_Game/Scenes/GameplayTest_Mage.unity";

        [MenuItem("DienGame/Mage/1 - Install Mage + Chimera (All)")]
        public static void Install()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("Stop Play Mode before installing Mage.");
                return;
            }
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Marksman) == null)
            {
                Debug.LogError("Mage installer: source scene is missing: " + Marksman);
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string previous = SceneManager.GetActiveScene().path;
            bool opened = false;
            try
            {
                ImportMageArt();
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(MageScene) == null)
                {
                    if (!AssetDatabase.CopyAsset(Marksman, MageScene))
                        throw new InvalidOperationException("Failed to copy Marksman scene.");
                    AssetDatabase.Refresh();
                }
                Scene scene = EditorSceneManager.OpenScene(MageScene, OpenSceneMode.Single);
                opened = true;
                PlayerMovement movement = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();
                if (movement == null) throw new InvalidOperationException("PlayerMovement not found.");
                GameObject player = movement.gameObject;

                Disable(player.GetComponent<PlayerAutoAttack>());
                Disable(player.GetComponent<MarksmanSkillController>());
                Disable(player.GetComponent<AirStrikeController>());
                JudgementController judgement = player.GetComponent<JudgementController>();
                Disable(judgement);
                if (judgement != null)
                {
                    SerializedObject serialized = new SerializedObject(judgement);
                    SerializedProperty autoCreate = serialized.FindProperty("autoCreateButton");
                    if (autoCreate != null) { autoCreate.boolValue = false; serialized.ApplyModifiedProperties(); }
                }
                foreach (JudgementUltimateButton button in UnityEngine.Object.FindObjectsByType<JudgementUltimateButton>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    Disable(button);

                PlayerAnimator[] gunAnimators = player.GetComponentsInChildren<PlayerAnimator>(true);
                SpriteRenderer renderer = null;
                foreach (PlayerAnimator old in gunAnimators)
                {
                    renderer = old.GetComponent<SpriteRenderer>();
                    Disable(old);
                    if (renderer != null) break;
                }
                if (renderer == null) renderer = player.GetComponentInChildren<SpriteRenderer>(true);
                if (renderer == null) throw new InvalidOperationException("Player SpriteRenderer missing.");
                Animator unityAnimator = renderer.GetComponent<Animator>();
                Disable(unityAnimator);
                if (renderer.GetComponent<MageSpriteAnimator>() == null)
                    renderer.gameObject.AddComponent<MageSpriteAnimator>();

                // Reuse these common components in their existing scene object.
                if (player.GetComponent<PlayerSkillManager>() == null) player.AddComponent<PlayerSkillManager>();
                if (player.GetComponent<PlayerExperience>() == null) player.AddComponent<PlayerExperience>();
                if (player.GetComponent<MageSkillController>() == null) player.AddComponent<MageSkillController>();
                if (player.GetComponent<MageClassController>() == null) player.AddComponent<MageClassController>();
                if (player.GetComponent<MageLevelUpController>() == null) player.AddComponent<MageLevelUpController>();
                // No boundary ring: the removed visual must not be installed into Mage scene.

                foreach (LevelUpController marksmanLevel in UnityEngine.Object.FindObjectsByType<LevelUpController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    Disable(marksmanLevel);

                Canvas canvas = null;
                foreach (Canvas c in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    if (c != null && c.name == "GameplayCanvas") { canvas = c; break; }
                if (canvas == null) throw new InvalidOperationException("GameplayCanvas not found.");
                if (canvas.GetComponent<MageCompanionJoystick>() == null)
                    canvas.gameObject.AddComponent<MageCompanionJoystick>();

                // Remove only legacy visual buttons copied into Mage scene.
                foreach (GameObject rootObject in scene.GetRootGameObjects())
                {
                    foreach (Transform oldVisual in rootObject.GetComponentsInChildren<Transform>(true))
                    {
                        if (oldVisual != null && oldVisual.name == "JudgementUltimateButton")
                            UnityEngine.Object.DestroyImmediate(oldVisual.gameObject);
                    }
                }
                EditorUtility.SetDirty(player);
                EditorUtility.SetDirty(renderer.gameObject);
                EditorUtility.SetDirty(canvas.gameObject);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save Mage scene.");
                EnsureBuildScene();
                Debug.Log("Mage + Chimera installed. Class select in Lobby -> PHÁP SƯ. GameplayTest and Assassin are not modified.");
            }
            catch (Exception ex)
            {
                Debug.LogError("Mage scene installer failed: " + ex);
            }
            finally
            {
                if (opened && !string.IsNullOrEmpty(previous) && AssetDatabase.LoadAssetAtPath<SceneAsset>(previous) != null)
                    EditorSceneManager.OpenScene(previous, OpenSceneMode.Single);
            }
        }

        private static void ImportMageArt()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/_Game/Resources/Mage" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                if (importer.isReadable && importer.alphaIsTransparency && !importer.mipmapEnabled) continue;
                importer.isReadable = true;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }
        }

        private static void Disable(Behaviour script)
        {
            if (script != null) script.enabled = false;
        }

        private static void EnsureBuildScene()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            Add(scenes, Lobby);
            Add(scenes, Marksman);
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Assassin) != null) Add(scenes, Assassin);
            Add(scenes, MageScene);
            EditorBuildSettings.scenes = scenes.ToArray();
        }
        private static void Add(List<EditorBuildSettingsScene> scenes, string path)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) return;
            for (int i = 0; i < scenes.Count; i++)
            {
                if (string.Equals(scenes[i].path, path, StringComparison.OrdinalIgnoreCase))
                {
                    if (!scenes[i].enabled) scenes[i] = new EditorBuildSettingsScene(path, true);
                    return;
                }
            }
            scenes.Add(new EditorBuildSettingsScene(path, true));
        }
    }
}
