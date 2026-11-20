using UnityEngine;

namespace DienGame.Gameplay.Phase13
{
    /// <summary>
    /// Mobile-only frame profile and bounded cosmetic VFX budget. Physics & damage never
    /// depend on these settings; editor/desktop defaults remain untouched.
    /// </summary>
    public static class Phase13MobileProfile
    {
        private const string FpsKey = "DienGame_Phase13_TargetFps";
        private static float s_hitWindowStart;
        private static int s_hitCount;
        private static float s_deathWindowStart;
        private static int s_deathCount;
        public static int SkippedHitEffects { get; private set; }
        public static int SkippedDeathEffects { get; private set; }
        // Editor preview only; not a player preference or a release setting.
        public static bool SimulateMobileInEditor { get; set; }
        public static bool UseMobileBudget => Application.isMobilePlatform || SimulateMobileInEditor;
        public static int RequestedFps => PlayerPrefs.GetInt(FpsKey, 60) <= 30 ? 30 : 60;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            s_hitWindowStart = s_deathWindowStart = 0f;
            s_hitCount = s_deathCount = SkippedHitEffects = SkippedDeathEffects = 0;
            SimulateMobileInEditor = false;
            if (Application.isMobilePlatform) ApplyFrameRate();
        }

        public static void SetTargetFps(int fps)
        {
            PlayerPrefs.SetInt(FpsKey, fps <= 30 ? 30 : 60);
            PlayerPrefs.Save();
            if (Application.isMobilePlatform) ApplyFrameRate();
        }
        private static void ApplyFrameRate()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = RequestedFps;
        }
        public static bool AllowHitEffect()
        {
            if (!UseMobileBudget) return true;
            float time = Time.unscaledTime;
            if (time - s_hitWindowStart >= 1f)
            {
                s_hitWindowStart = time;
                s_hitCount = 0;
            }
            // Cosmetic-only: low profile skips more particles/lines, not combat events.
            int budget = RequestedFps <= 30 ? 12 : 24;
            if (s_hitCount >= budget)
            {
                SkippedHitEffects++;
                return false;
            }
            s_hitCount++;
            return true;
        }
        public static bool AllowDeathEffect(bool boss)
        {
            if (boss || !UseMobileBudget) return true;
            float time = Time.unscaledTime;
            if (time - s_deathWindowStart >= 1f)
            {
                s_deathWindowStart = time;
                s_deathCount = 0;
            }
            int budget = RequestedFps <= 30 ? 8 : 16;
            if (s_deathCount >= budget)
            {
                SkippedDeathEffects++;
                return false;
            }
            s_deathCount++;
            return true;
        }
    }
}
