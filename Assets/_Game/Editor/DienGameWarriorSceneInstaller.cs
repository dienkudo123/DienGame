#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using DienGame.Gameplay.Player;
using DienGame.Gameplay.Run;
using DienGame.Gameplay.Skills;
using DienGame.Gameplay.Skills.Assassin;
using DienGame.Gameplay.Skills.Marksman;
using DienGame.Gameplay.Skills.Warrior;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DienGame.EditorTools
{
    /// <summary>
    /// One-time, non-destructive Warrior scene installer. The new scene is copied from
    /// Assassin (world setup only), stripped of Assassin-specific components, then
    /// connected to Warrior combat/visual/skill controllers. Source scenes are untouched.
    /// Auto-runs when necessary on import if no editor scene is dirty; also has a manual menu.
    /// </summary>
    [InitializeOnLoad]
    public static class WarriorSceneInstaller
    {
        private const string Warrior = "Assets/_Game/Scenes/GameplayTest_Warrior.unity";
        private const string Assassin = "Assets/_Game/Scenes/GameplayTest_AssassinPreview.unity";
        private const string Lobby = "Assets/_Game/Scenes/Lobby.unity";
        private const string Marksman = "Assets/_Game/Scenes/GameplayTest.unity";
        private const string Mage = "Assets/_Game/Scenes/GameplayTest_Mage.unity";
        private const string SpriteData = "Assets/_Game/Animations/Player/Assassin8Dir/AssassinSpriteData.asset";
        private static bool _running;
        static WarriorSceneInstaller() { EditorApplication.delayCall += TryInstallAuto; }
        private static void TryInstallAuto()
        {
            if (_running || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            Scene current = SceneManager.GetActiveScene();
            // Never discard the developer's unsaved editor changes.
            if (current.IsValid() && current.isDirty) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Warrior) == null) return;
            if (EditorPrefs.GetBool("DienGame_WarriorScenePhase10Installed_" + AssetDatabase.AssetPathToGUID(Warrior), false)
                && IsAlreadyConfiguredOnDisk())
            {
                EnsureBuildScenes();
                return;
            }
            InstallInternal(false);
        }
        private static bool IsAlreadyConfiguredOnDisk()
        {
            // EditorPrefs alone is not enough: user may have overwritten an already
            // configured Warrior scene with the source Assassin scene during import.
            string scriptGuid = AssetDatabase.AssetPathToGUID(
                "Assets/_Game/Scripts/Gameplay/Skills/Warrior/WarriorMeleeAttack.cs");
            if (string.IsNullOrEmpty(scriptGuid)) return false;
            try
            {
                return System.IO.File.Exists(Warrior) &&
                    System.IO.File.ReadAllText(Warrior).Contains(scriptGuid);
            }
            catch (Exception) { return false; }
        }

        [MenuItem("DienGame/Phase10/01 - Cai dat Dau si (scene + build settings)")]
        public static void Install() => InstallInternal(true);

        private static void InstallInternal(bool prompt)
        {
            if (_running || EditorApplication.isPlayingOrWillChangePlaymode) return;
            _running = true;
            string previousPath = SceneManager.GetActiveScene().path;
            bool opened = false;
            try
            {
                if (prompt && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Warrior) == null)
                {
                    if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Assassin) == null ||
                        !AssetDatabase.CopyAsset(Assassin, Warrior))
                        throw new InvalidOperationException("Không thể tạo scene Đấu sĩ từ Assassin.");
                    AssetDatabase.ImportAsset(Warrior);
                }
                Scene scene = EditorSceneManager.OpenScene(Warrior, OpenSceneMode.Single);
                opened = true;
                PlayerMovement playerMovement = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();
                if (playerMovement == null) throw new InvalidOperationException("Không thấy nhân vật trong scene.");
                GameObject player = playerMovement.gameObject;
                // Preserve world, camera, collisions, HUD, waves, XP and stage runtime.
                // Remove only scene-level class-specific Assassin components.
                AssassinSpriteAnimationData data = AssetDatabase.LoadAssetAtPath<AssassinSpriteAnimationData>(SpriteData);
                AssassinSpriteAnimator oldAnimator = player.GetComponentInChildren<AssassinSpriteAnimator>(true);
                SpriteRenderer body = oldAnimator != null ? oldAnimator.GetComponent<SpriteRenderer>() :
                    player.GetComponentInChildren<SpriteRenderer>(true);
                if (data == null && oldAnimator != null) data = oldAnimator.AnimationData;
                if (data == null || body == null)
                    throw new InvalidOperationException("Thiếu sprite sheet 8 hướng gốc; không xóa script/scene.");

                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    foreach (MonoBehaviour script in root.GetComponentsInChildren<MonoBehaviour>(true))
                    {
                        if (script == null) continue;
                        string ns = script.GetType().Namespace ?? string.Empty;
                        if (ns == "DienGame.Gameplay.Skills.Assassin")
                            UnityEngine.Object.DestroyImmediate(script);
                    }
                }
                Disable(player.GetComponent<PlayerAutoAttack>());
                Disable(player.GetComponent<MarksmanSkillController>());
                Disable(player.GetComponent<AirStrikeController>());
                Disable(player.GetComponent<JudgementController>());
                Disable(body.GetComponent<PlayerAnimator>());
                Disable(body.GetComponent<Animator>());
                foreach (LevelUpController old in UnityEngine.Object.FindObjectsByType<LevelUpController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    Disable(old);
                foreach (JudgementUltimateButton old in UnityEngine.Object.FindObjectsByType<JudgementUltimateButton>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    Disable(old);
                foreach (GameObject root in scene.GetRootGameObjects())
                    foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                        if (child != null && child.name == "JudgementUltimateButton")
                            UnityEngine.Object.DestroyImmediate(child.gameObject);

                if (player.GetComponent<PlayerSkillManager>() == null) player.AddComponent<PlayerSkillManager>();
                if (player.GetComponent<WarriorSkillController>() == null) player.AddComponent<WarriorSkillController>();
                if (player.GetComponent<WarriorMeleeAttack>() == null) player.AddComponent<WarriorMeleeAttack>();
                if (player.GetComponent<WarriorLevelUpController>() == null) player.AddComponent<WarriorLevelUpController>();
                WarriorVisual visual = body.GetComponent<WarriorVisual>();
                if (visual == null) visual = body.gameObject.AddComponent<WarriorVisual>();
                visual.Configure(data);
                EditorUtility.SetDirty(player);
                EditorUtility.SetDirty(body.gameObject);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException("Không thể lưu scene Warrior.");
                EnsureBuildScenes();
                EditorPrefs.SetBool("DienGame_WarriorScenePhase10Installed_" + AssetDatabase.AssetPathToGUID(Warrior), true);
                Debug.Log("[DienGame Phase10] Đấu sĩ đã có scene độc lập, 8 hướng, 4 kiểu kiếm, 7 kỹ năng. Ba class trước không đổi.");
            }
            catch (Exception ex)
            {
                Debug.LogError("[DienGame Phase10] Chưa cài được Đấu sĩ: " + ex);
            }
            finally
            {
                if (opened && !string.IsNullOrEmpty(previousPath) &&
                    AssetDatabase.LoadAssetAtPath<SceneAsset>(previousPath) != null)
                    EditorSceneManager.OpenScene(previousPath, OpenSceneMode.Single);
                _running = false;
            }
        }
        [MenuItem("DienGame/Phase10/02 - Kiem tra 4 class va scene")]
        public static void Validate()
        {
            int errors = 0;
            foreach (string path in new[] { Lobby, Marksman, Assassin, Mage, Warrior })
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
                { Debug.LogError("[Phase10 QA] Thiếu scene: " + path); errors++; }
            }
            foreach (var c in new[] { DienGame.Client.PlayableClass.Marksman, DienGame.Client.PlayableClass.Assassin,
                DienGame.Client.PlayableClass.Mage, DienGame.Client.PlayableClass.Warrior })
            {
                int count = DienGame.Gameplay.Equipment.EquipmentCatalog.ForClass(c).Count;
                int expected = c == DienGame.Client.PlayableClass.Marksman ? 5 :
                    c == DienGame.Client.PlayableClass.Assassin ? 4 :
                    c == DienGame.Client.PlayableClass.Mage ? 6 : 4;
                if (count != expected) { errors++; Debug.LogError($"[Phase10 QA] {c} {count}/{expected} trang bị"); }
            }
            Debug.Log(errors == 0 ? "[Phase10 QA] Scene và 19 định nghĩa OK; vẫn cần Play Mode/Android test." :
                "[Phase10 QA] " + errors + " lỗi. Xem Console.");
        }
        private static void Disable(Behaviour behaviour)
        { if (behaviour != null) behaviour.enabled = false; }
        private static void EnsureBuildScenes()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (string path in new[] { Lobby, Marksman, Assassin, Mage, Warrior })
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) continue;
                int index = scenes.FindIndex(s => string.Equals(s.path, path, StringComparison.OrdinalIgnoreCase));
                if (index < 0) scenes.Add(new EditorBuildSettingsScene(path, true));
                else if (!scenes[index].enabled) scenes[index] = new EditorBuildSettingsScene(path, true);
            }
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
