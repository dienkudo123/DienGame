#if UNITY_EDITOR
using System.Text;
using DienGame.Gameplay.Skills.Mage;
using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    public static class DienGameFireHawkFinalQa
    {
        [MenuItem("DienGame/Phase09/FireHawk FINAL/1 - Verify 4 atlases")]
        private static void CheckAtlases()
        {
            string[] names = {
                "Equipment/FireHawkFinal/Flight/FireHawk_Fly4x4",
                "Equipment/FireHawkFinal/Cast/FireHawk_CastLv1_4x5",
                "Equipment/FireHawkFinal/Cast/FireHawk_CastLv5_4x5",
                "Equipment/FireHawkFinal/Vfx/FireHawk_TwinFlame8x1"
            };
            int[] widths = {1024, 1280, 1280, 2048};
            int[] heights = {1024, 1024, 1024, 256};
            bool okay = true;
            StringBuilder report = new StringBuilder("FireHawk FINAL assets:");
            for (int i = 0; i < names.Length; i++)
            {
                Texture2D image = Resources.Load<Texture2D>(names[i]);
                bool passed = image != null && image.width == widths[i] && image.height == heights[i];
                okay &= passed;
                report.Append("\n").Append(passed ? "OK: " : "ERROR: ").Append(names[i]);
            }
            Debug.Log(report.ToString());
            EditorUtility.DisplayDialog("FireHawk FINAL", okay ? "4/4 atlases found. Test Play Mode next." :
                "Missing or wrong-sized atlas. See Console.", "OK");
        }

        [MenuItem("DienGame/Phase09/FireHawk FINAL/2 - Preview skill Lv1 in Play Mode")]
        private static void PreviewLv1() { Preview(false); }

        [MenuItem("DienGame/Phase09/FireHawk FINAL/3 - Preview skill Lv5 in Play Mode")]
        private static void PreviewLv5() { Preview(true); }

        private static void Preview(bool level5)
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Enter Mage scene in Play Mode before previewing Phoenix skill.");
                return;
            }
            ChimeraCompanion pet = Object.FindFirstObjectByType<ChimeraCompanion>();
            if (pet == null || !pet.DebugPreviewFireHawkSignature(level5))
            {
                Debug.LogWarning("Select the FireHawk/Phoenix equipment, then restart Mage scene.");
                return;
            }
            Debug.Log("FireHawk signature preview triggered: " + (level5 ? "Lv5" : "Lv1") +
                ". This is VISUAL only, does not alter skill levels or save data.");
        }
    }
}
#endif
