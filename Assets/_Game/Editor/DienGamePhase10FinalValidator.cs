#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using DienGame.Client;
using DienGame.Gameplay.Equipment;
using DienGame.Gameplay.Player;
using DienGame.Gameplay.Skills;
using DienGame.Gameplay.Skills.Assassin;
using DienGame.Gameplay.Skills.Mage;
using DienGame.Gameplay.Skills.Marksman;
using DienGame.Gameplay.Skills.Warrior;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DienGame.EditorTools
{
    /// <summary>
    /// Editor QA gate for Phase10 FINAL. It doesn't claim to replace Play Mode or device QA.
    /// For builds, stop early if the Warrior scene was never installed and still contains
    /// Assassin combat controllers. It does not write to other scenes or to player saves.
    /// </summary>
    public sealed class DienGamePhase10FinalValidator : IPreprocessBuildWithReport
    {
        private const string ScenePath = "Assets/_Game/Scenes/GameplayTest_Warrior.unity";
        private const string RunTexture = "Assets/_Game/Resources/Warrior/WarriorRun8x4.png";
        private const string SlashTexture = "Assets/_Game/Resources/Warrior/WarriorSlash8x4.png";
        public int callbackOrder => 0;

        [MenuItem("DienGame/Phase10/FINAL - Kiem tra scene + animation + trang bi")]
        public static void ValidateFromMenu()
        {
            List<string> errors = ValidateContent(true);
            if (errors.Count == 0)
                Debug.Log("[DienGame Phase10 FINAL] Đạt kiểm tra cấu trúc. Cần test gameplay/Android thủ công.");
            else
                foreach (string error in errors)
                    Debug.LogError("[DienGame Phase10 FINAL] " + error);
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            List<string> errors = ValidateContent(false);
            if (errors.Count > 0)
                throw new BuildFailedException("[DienGame Phase10 FINAL] Chưa đủ điều kiện build:\n- " +
                    string.Join("\n- ", errors) +
                    "\nCài đặt lại ở DienGame > Phase10 > 01 và chạy FINAL validator.");
        }

        private static List<string> ValidateContent(bool manual)
        {
            List<string> errors = new List<string>();
            CheckAtlas(RunTexture, errors);
            CheckAtlas(SlashTexture, errors);
            ValidateEquipment(errors);
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                errors.Add("Thiếu scene GameplayTest_Warrior.unity.");
                return errors;
            }

            Scene current = SceneManager.GetActiveScene();
            string previousPath = current.path;
            if (current.IsValid() && current.isDirty)
            {
                errors.Add("Scene đang có thay đổi chưa lưu. Lưu scene trước khi kiểm tra Phase10.");
                return errors;
            }

            if (string.IsNullOrEmpty(previousPath))
            {
                errors.Add("Hãy mở một scene đã lưu trước khi chạy kiểm tra Phase10.");
                return errors;
            }

            bool opened = false;
            try
            {
                Scene warrior = current.path == ScenePath ? current :
                    EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                opened = current.path != ScenePath;
                PlayerMovement movement = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();
                if (movement == null) errors.Add("Scene Đấu sĩ chưa có PlayerMovement.");
                else
                {
                    GameObject player = movement.gameObject;
                    if (player.GetComponent<WarriorMeleeAttack>() == null)
                        errors.Add("WarriorMeleeAttack chưa được gắn vào Player. Cài bằng Phase10 > 01.");
                    if (player.GetComponent<WarriorSkillController>() == null)
                        errors.Add("Thiếu WarriorSkillController trên Player.");
                    if (player.GetComponent<WarriorLevelUpController>() == null)
                        errors.Add("Thiếu WarriorLevelUpController trên Player.");
                    if (player.GetComponentInChildren<WarriorVisual>(true) == null)
                        errors.Add("Thiếu WarriorVisual trên nhân vật.");
                    if (player.GetComponent<AssassinMeleeAttack>() != null)
                        errors.Add("Cảnh báo: vẫn còn AssassinMeleeAttack, có nguy cơ chém kép.");
                    if (player.GetComponent<AssassinSkillController>() != null)
                        errors.Add("Đấu sĩ vẫn chứa AssassinSkillController.");
                    PlayerAutoAttack autoAttack = player.GetComponent<PlayerAutoAttack>();
                    if (autoAttack != null && autoAttack.enabled)
                        errors.Add("PlayerAutoAttack cũ vẫn bật trên Đấu sĩ.");
                }
                foreach (GameObject root in warrior.GetRootGameObjects())
                foreach (AssassinMeleeAttack assassin in root.GetComponentsInChildren<AssassinMeleeAttack>(true))
                    if (assassin != null && assassin.isActiveAndEnabled)
                        errors.Add("Warrior scene còn AssassinMeleeAttack đang bật: " + assassin.name);
            }
            catch (Exception ex) { errors.Add("Không mở/đọc được scene Đấu sĩ: " + ex.Message); }
            finally
            {
                if (opened && !string.IsNullOrEmpty(previousPath) &&
                    AssetDatabase.LoadAssetAtPath<SceneAsset>(previousPath) != null)
                    EditorSceneManager.OpenScene(previousPath, OpenSceneMode.Single);
            }
            return errors;
        }

        private static void CheckAtlas(string path, List<string> errors)
        {
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null) { errors.Add("Thiếu atlas " + path); return; }
            if (tex.width % 4 != 0 || tex.height % 8 != 0)
                errors.Add("Atlas không chia hết 4 cột x 8 hàng: " + path);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null || !importer.isReadable)
                errors.Add("Texture phải Read/Write Enabled: " + path);
        }

        private static void ValidateEquipment(List<string> errors)
        {
            int total = 0;
            foreach (PlayableClass cls in new[] { PlayableClass.Marksman, PlayableClass.Assassin,
                PlayableClass.Mage, PlayableClass.Warrior })
            {
                int count = EquipmentCatalog.ForClass(cls).Count;
                int expected = cls == PlayableClass.Marksman ? 5 :
                    cls == PlayableClass.Assassin ? 4 : cls == PlayableClass.Mage ? 6 : 4;
                if (count != expected)
                    errors.Add(cls + ": " + count + "/" + expected + " trang bị.");
                total += count;
            }
            if (total != 19) errors.Add("Tổng trang bị khác 19: " + total);
        }
    }
}
#endif
