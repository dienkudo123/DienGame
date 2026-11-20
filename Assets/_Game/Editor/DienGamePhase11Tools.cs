#if UNITY_EDITOR
using System.Collections.Generic;
using DienGame.Gameplay.World;
using DienGame.Gameplay.Run;
using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    /// <summary>Read-only content checks and a Play Mode boss shortcut for Phase 11.</summary>
    public static class DienGamePhase11Tools
    {
        [MenuItem("DienGame/Phase11/01 - Kiem tra 5 dao + quai + Boss")]
        public static void Validate()
        {
            var errors = new List<string>();
            string[] scenePaths =
            {
                "Assets/_Game/Scenes/Lobby.unity",
                "Assets/_Game/Scenes/GameplayTest.unity",
                "Assets/_Game/Scenes/GameplayTest_AssassinPreview.unity",
                "Assets/_Game/Scenes/GameplayTest_Mage.unity",
                "Assets/_Game/Scenes/GameplayTest_Warrior.unity"
            };
            foreach (string path in scenePaths)
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
                    errors.Add("Scene khong ton tai: " + path);

            for (int stage = 1; stage <= WorldStageThemes.TotalStages; stage++)
            {
                WorldStageTheme theme = WorldStageThemes.Get(stage);
                Stage11EncounterProfile profile = Stage11EncounterProfiles.Get(stage);
                if (theme.Stage != profile.Stage) errors.Add("Sai encounter profile dao " + stage);
                if (stage <= 1) continue;
                if (string.IsNullOrEmpty(theme.Ground) ||
                    Resources.Load<Sprite>(theme.Ground) == null)
                    errors.Add("Khong load duoc ground dao " + stage + " : " + theme.Ground);
                foreach (string prop in theme.Props)
                    if (Resources.Load<Sprite>("World/Props/" + prop) == null)
                        errors.Add("Thieu prop dao " + stage + " : " + prop);
                if (profile.HazardWarning < 1.2f || profile.HazardInterval < 20f ||
                    profile.HazardHealthFraction > .08f ||
                    profile.BossSpecialInterval < 15f || profile.BossHealthMultiplier > 1.4f)
                    errors.Add("Phase11 balance out of bounds: dao " + stage);
            }

            if (errors.Count == 0)
            {
                Debug.Log("[DienGame Phase11] 5 island profiles, stage art and QA constraints present. " +
                          "Can Unity Play Mode and Android QA for actual gameplay.");
                EditorUtility.DisplayDialog("Phase11 Content QA", "Da kiem tra du lieu 5 dao, scene va tai nguyen. " +
                    "Hay choi thu Unity Play Mode truoc khi phat hanh.", "OK");
            }
            else
            {
                foreach (string error in errors) Debug.LogError("[Phase11] " + error);
                EditorUtility.DisplayDialog("Phase11 Content QA", "Co " + errors.Count +
                    " loi. Xem Unity Console.", "OK");
            }
        }

        [MenuItem("DienGame/Phase11/02 - Chuyen den Boss ngay (Play Mode)")]
        public static void EnterBossNow()
        {
            GameRunController run = Object.FindFirstObjectByType<GameRunController>();
            if (run == null) { Debug.LogWarning("Phase11: hay vao man choi truoc."); return; }
            run.DebugSkipToBossPhase();
            Debug.Log("Phase11: da yeu cau bat dau pha Boss. Kiem tra HUD va vach canh bao.");
        }

        [MenuItem("DienGame/Phase11/02 - Chuyen den Boss ngay (Play Mode)", true)]
        private static bool CanEnterBossNow() => EditorApplication.isPlaying;
    }
}
#endif
