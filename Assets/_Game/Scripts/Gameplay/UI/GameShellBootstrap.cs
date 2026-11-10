using DienGame.Gameplay.Run;
using DienGame.Gameplay.Web;
using UnityEngine;
using UnityEngine.UI;

namespace DienGame.Gameplay.UI
{
    public static class GameShellBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (Object.FindFirstObjectByType<GameRunController>() == null)
            {
                return;
            }

            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            Canvas canvas = FindGameplayCanvas();
            if (canvas == null)
            {
                return;
            }

            if (canvas.GetComponent<GameShellUI>() == null)
            {
                canvas.gameObject.AddComponent<GameShellUI>();
            }

            if (canvas.GetComponent<MobileSafeAreaController>() == null)
            {
                canvas.gameObject.AddComponent<MobileSafeAreaController>();
            }

            DienGameWebMobileBridge.Configure();
        }

        private static Canvas FindGameplayCanvas()
        {
            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);

            foreach (Canvas canvas in canvases)
            {
                if (canvas != null && canvas.name == "GameplayCanvas")
                {
                    return canvas;
                }
            }

            foreach (Canvas canvas in canvases)
            {
                if (canvas != null && canvas.renderMode != RenderMode.WorldSpace)
                {
                    return canvas;
                }
            }

            return null;
        }
    }
}
