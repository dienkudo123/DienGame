using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DienGame.Client
{
    public static class LobbyBootstrap
    {
        // Scene Lobby không có EventSystem serialized; tạo lại mỗi lần load scene.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneLoaded()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != LobbySessionService.LobbySceneName)
            {
                return;
            }

            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            EnsureEventSystem();
            Canvas canvas = EnsureCanvas();

            if (canvas.GetComponent<LobbyUI>() == null)
            {
                canvas.gameObject.AddComponent<LobbyUI>();
            }

            if (canvas.GetComponent<LobbySafeAreaController>() == null)
            {
                canvas.gameObject.AddComponent<LobbySafeAreaController>();
            }
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();

            InputSystemUIInputModule inputModule =
                eventSystemObject.AddComponent<InputSystemUIInputModule>();

            inputModule.AssignDefaultActions();
        }

        private static Canvas EnsureCanvas()
        {
            Canvas existing = Object.FindFirstObjectByType<Canvas>();
            if (existing != null)
            {
                return existing;
            }

            GameObject canvasObject = new("LobbyCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }
    }
}
