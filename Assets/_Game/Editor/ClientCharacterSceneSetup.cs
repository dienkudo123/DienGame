using System;
using System.Collections.Generic;
using DienGame.Gameplay.Player;
using DienGame.Gameplay.Run;
using DienGame.Gameplay.Skills.Assassin;
using DienGame.Gameplay.Skills.Marksman;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DienGame.EditorTools
{
    /// <summary>
    /// Generates/repairs the separate Assassin scene and registers Lobby/Marksman/Assassin
    /// scenes in Build Settings. Does not edit GameplayTest, Player prefab or script values.
    /// May be run multiple times without duplicating scripts or Build Settings entries.
    /// </summary>
    public static class ClientCharacterSceneSetup
    {
        private const string LobbyPath = "Assets/_Game/Scenes/Lobby.unity";
        private const string MarksmanPath = "Assets/_Game/Scenes/GameplayTest.unity";
        private const string AssassinPath = "Assets/_Game/Scenes/GameplayTest_AssassinPreview.unity";
        private const string AssassinDataPath = "Assets/_Game/Animations/Player/Assassin8Dir/AssassinSpriteData.asset";

        [MenuItem("DienGame/Client/Setup Character Selection (2 classes)")]
        public static void Setup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("DienGame: stop Play Mode before setting up classes.");
                return;
            }

            if (!Exists(LobbyPath) || !Exists(MarksmanPath))
            {
                Debug.LogError("DienGame: missing Lobby.unity or GameplayTest.unity. No project data changed.");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            string previousScenePath = SceneManager.GetActiveScene().path;
            bool openedAssassinScene = false;
            bool success = false;

            try
            {
                AssassinSpriteAnimationData data = AssetDatabase.LoadAssetAtPath<AssassinSpriteAnimationData>(AssassinDataPath);
                if (data == null)
                {
                    // Reuse the previously supplied sprite-slicing logic; no Inspector setup.
                    AssassinPreviewBuilder.BuildSpriteData();
                    data = AssetDatabase.LoadAssetAtPath<AssassinSpriteAnimationData>(AssassinDataPath);
                }

                if (data == null)
                    throw new InvalidOperationException("Cannot generate AssassinSpriteData. Check Run8Dir.png and Slash8Dir.png.");

                if (!Exists(AssassinPath))
                {
                    if (!AssetDatabase.CopyAsset(MarksmanPath, AssassinPath))
                        throw new InvalidOperationException("Cannot copy GameplayTest to Assassin preview. Original scene is unchanged.");
                    AssetDatabase.Refresh();
                }

                Scene assassinScene = EditorSceneManager.OpenScene(AssassinPath, OpenSceneMode.Single);
                openedAssassinScene = true;
                ConfigureAssassinScene(data);
                EditorSceneManager.MarkSceneDirty(assassinScene);
                if (!EditorSceneManager.SaveScene(assassinScene))
                    throw new InvalidOperationException("Could not save the Assassin scene.");

                EnsureBuildSettings();
                success = true;
                Debug.Log("DienGame: Lobby / Marksman / Assassin scenes are ready. Lobby class selection can now load either class.");
            }
            catch (Exception exception)
            {
                Debug.LogError("DienGame: character setup did not finish: " + exception.Message);
            }
            finally
            {
                if (openedAssassinScene && !string.IsNullOrEmpty(previousScenePath) && Exists(previousScenePath))
                    EditorSceneManager.OpenScene(previousScenePath, OpenSceneMode.Single);
            }

            if (success)
                EditorUtility.DisplayDialog("DienGame - Character Selection",
                    "Completed. Lobby now supports Marksman and Assassin.\n\n" +
                    "Assassin is currently a melee-animation preview: its 7 passives and wall-jump ability are NOT implemented. " +
                    "Marksman's skill upgrade UI is disabled only in the Assassin scene.", "OK");
        }

        private static void ConfigureAssassinScene(AssassinSpriteAnimationData data)
        {
            PlayerMovement player = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();
            if (player == null)
                throw new InvalidOperationException("Assassin scene has no active PlayerMovement component.");

            GameObject playerRoot = player.gameObject;
            PlayerAutoAttack marksmanAttack = playerRoot.GetComponent<PlayerAutoAttack>();
            if (marksmanAttack != null) marksmanAttack.enabled = false;

            MarksmanSkillController marksmanSkills = playerRoot.GetComponent<MarksmanSkillController>();
            if (marksmanSkills != null) marksmanSkills.enabled = false;

            AirStrikeController airStrike = playerRoot.GetComponent<AirStrikeController>();
            if (airStrike != null) airStrike.enabled = false;

            JudgementController judgement = playerRoot.GetComponent<JudgementController>();
            if (judgement != null) judgement.enabled = false;

            PlayerAnimator gunAnimator = playerRoot.GetComponentInChildren<PlayerAnimator>(true);
            if (gunAnimator != null) gunAnimator.enabled = false;

            SpriteRenderer visual = gunAnimator != null
                ? gunAnimator.GetComponent<SpriteRenderer>()
                : playerRoot.GetComponentInChildren<SpriteRenderer>(true);
            if (visual == null)
                throw new InvalidOperationException("Assassin scene has no player SpriteRenderer.");

            AssassinMeleeAttack melee = playerRoot.GetComponent<AssassinMeleeAttack>();
            if (melee == null) melee = playerRoot.AddComponent<AssassinMeleeAttack>();
            melee.enabled = true;

            AssassinCrescentVfx crescent = playerRoot.GetComponent<AssassinCrescentVfx>();
            if (crescent == null) crescent = playerRoot.AddComponent<AssassinCrescentVfx>();
            crescent.enabled = true;

            AssassinSpriteAnimator assassinAnimator = visual.GetComponent<AssassinSpriteAnimator>();
            if (assassinAnimator == null) assassinAnimator = visual.gameObject.AddComponent<AssassinSpriteAnimator>();
            assassinAnimator.SetAnimationData(data);
            assassinAnimator.enabled = true;

            // The existing upgrade pool contains only Marksman skills. Do NOT grant gun
            // abilities to Assassin. EXP/level counters still function without this UI.
            LevelUpController levelUp = UnityEngine.Object.FindFirstObjectByType<LevelUpController>();
            if (levelUp != null) levelUp.enabled = false;

            EditorUtility.SetDirty(playerRoot);
            EditorUtility.SetDirty(visual.gameObject);
            if (levelUp != null) EditorUtility.SetDirty(levelUp);
        }

        private static void EnsureBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            EnableOrAdd(scenes, LobbyPath);
            EnableOrAdd(scenes, MarksmanPath);
            EnableOrAdd(scenes, AssassinPath);
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void EnableOrAdd(List<EditorBuildSettingsScene> scenes, string path)
        {
            for (int i = 0; i < scenes.Count; i++)
            {
                if (!string.Equals(scenes[i].path, path, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!scenes[i].enabled)
                    scenes[i] = new EditorBuildSettingsScene(path, true);
                return;
            }

            scenes.Add(new EditorBuildSettingsScene(path, true));
        }

        private static bool Exists(string assetPath) =>
            AssetDatabase.LoadAssetAtPath<SceneAsset>(assetPath) != null;
    }
}
