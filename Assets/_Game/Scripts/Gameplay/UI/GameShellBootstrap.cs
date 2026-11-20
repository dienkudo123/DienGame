using System.Collections;
using DienGame.Client;
using DienGame.Gameplay.Run;
using DienGame.Gameplay.Web;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace DienGame.Gameplay.UI
{
    /// <summary>
    /// Dựng GameShellUI an toàn kể cả khi GameRunController/Canvas được tạo muộn sau khi scene load.
    /// Không cần gắn component thủ công trong Inspector.
    /// </summary>
    public static class GameShellBootstrap
    {
        private const int MaxRetryFrames = 180;
        private static bool _retryScheduled;

        // RuntimeInitializeOnLoadMethod(AfterSceneLoad) chỉ chạy ở lần khởi động đầu.
        // SceneManager.sceneLoaded bảo đảm khởi tạo lại Pause UI sau mỗi lần restart.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneLoaded()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!LobbySessionService.IsGameplayScene(scene.name))
            {
                return;
            }

            if (!TryBootstrap())
            {
                ScheduleRetry();
            }
        }

        private static bool TryBootstrap()
        {
            if (Object.FindFirstObjectByType<GameRunController>() == null)
            {
                return false;
            }

            Canvas canvas = FindGameplayCanvas();
            if (canvas == null)
            {
                return false;
            }

            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            if (canvas.GetComponent<GameShellUI>() == null)
            {
                canvas.gameObject.AddComponent<GameShellUI>();
            }

            if (canvas.GetComponent<GameplayJoystickVisualFix>() == null)
            {
                canvas.gameObject.AddComponent<GameplayJoystickVisualFix>();
            }

            if (canvas.GetComponent<MobileSafeAreaController>() == null)
            {
                canvas.gameObject.AddComponent<MobileSafeAreaController>();
            }

            DienGameWebMobileBridge.Configure();
            return true;
        }

        private static void ScheduleRetry()
        {
            if (_retryScheduled)
            {
                return;
            }

            _retryScheduled = true;

            GameObject retryObject = new("GameShellBootstrapRetry");
            Object.DontDestroyOnLoad(retryObject);
            retryObject.hideFlags = HideFlags.HideInHierarchy;
            retryObject.AddComponent<RetryRunner>();
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

        private sealed class RetryRunner : MonoBehaviour
        {
            private IEnumerator Start()
            {
                for (int frame = 0; frame < MaxRetryFrames; frame++)
                {
                    if (!LobbySessionService.IsGameplayScene(SceneManager.GetActiveScene().name))
                    {
                        break;
                    }

                    if (TryBootstrap())
                    {
                        break;
                    }

                    yield return null;
                }

                _retryScheduled = false;
                Destroy(gameObject);
            }
        }
    }
}
