#if UNITY_EDITOR

using DienGame.Client;
using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    public static class MetaProgressDebugMenu
    {
        [MenuItem("DienGame/Client/Debug/Unlock All Marksman Skills")]
        public static void UnlockAllMarksmanSkills()
        {
            MetaProgressService.DebugUnlockAllMarksmanSkills();
            Debug.Log("DienGame Meta: đã mở khóa toàn bộ 8 skill Xạ thủ để test.");
        }

        [MenuItem("DienGame/Client/Debug/Reset Stage Progress")]
        public static void ResetStageProgress()
        {
            MetaProgressService.DebugResetStageProgress();
            Debug.Log("DienGame Meta: đã reset tiến độ màn/skill unlock.");
        }
    }
}

#endif
