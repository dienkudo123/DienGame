using DienGame.Gameplay.Core;
using DienGame.Gameplay.Player;
using DienGame.Gameplay.Run;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DienGame.Gameplay.UI
{
    [DisallowMultipleComponent]
    public sealed class GameShellUI : MonoBehaviour
    {
        private const string StartPauseReason = "StartMenu";
        private const string ManualPauseReason = "ManualPause";

        private GameObject _startPanel;
        private GameObject _pausePanel;
        private Button _pauseButton;
        private TMP_Text _statsText;
        private bool _built;

        private void Awake()
        {
            BuildIfNeeded();
        }

        private void Start()
        {
            GamePauseService.RequestPause(StartPauseReason);
            RefreshStats();
        }

        private void OnDestroy()
        {
            GamePauseService.ReleasePause(StartPauseReason);
            GamePauseService.ReleasePause(ManualPauseReason);
        }

        private void BuildIfNeeded()
        {
            if (_built)
            {
                return;
            }

            _built = true;
            BuildPauseButton();
            BuildStartPanel();
            BuildPausePanel();
        }

        private void BuildPauseButton()
        {
            GameObject buttonObject = CreateUIObject("PauseButton", transform);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(92f, 92f);
            rect.anchoredPosition = new Vector2(-24f, -24f);

            Image bg = buttonObject.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.05f, 0.72f);

            _pauseButton = buttonObject.AddComponent<Button>();
            _pauseButton.targetGraphic = bg;
            _pauseButton.onClick.AddListener(OpenPause);

            TMP_Text label = CreateLabel(buttonObject.transform, "Ⅱ", 38f);
            label.raycastTarget = false;
        }

        private void BuildStartPanel()
        {
            _startPanel = CreateFullscreenPanel("StartPanel", new Color(0.025f, 0.025f, 0.035f, 0.93f));

            GameObject card = CreateUIObject("StartCard", _startPanel.transform);
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(760f, 720f);

            TMP_Text title = CreateLabel(card.transform, "DIENGAME", 82f);
            SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(700f, 120f), new Vector2(0.5f, 1f));
            title.fontStyle = FontStyles.Bold;

            TMP_Text subtitle = CreateLabel(card.transform, "XẠ THỦ  •  SINH TỒN 10 PHÚT", 28f);
            SetRect(subtitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -185f), new Vector2(700f, 80f), new Vector2(0.5f, 1f));
            subtitle.color = new Color(0.78f, 0.78f, 0.82f, 1f);

            _statsText = CreateLabel(card.transform, string.Empty, 24f);
            SetRect(_statsText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(680f, 120f), new Vector2(0.5f, 0.5f));
            _statsText.color = new Color(0.92f, 0.80f, 0.45f, 1f);

            CreateActionButton(card.transform, "CHƠI", new Vector2(0f, -155f), StartGame);
        }

        private void BuildPausePanel()
        {
            _pausePanel = CreateFullscreenPanel("PausePanel", new Color(0f, 0f, 0f, 0.76f));

            TMP_Text title = CreateLabel(_pausePanel.transform, "TẠM DỪNG", 58f);
            SetRect(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 220f), new Vector2(700f, 100f), new Vector2(0.5f, 0.5f));
            title.fontStyle = FontStyles.Bold;

            CreateActionButton(_pausePanel.transform, "TIẾP TỤC", new Vector2(0f, 40f), ResumeGame);
            CreateActionButton(_pausePanel.transform, "CHƠI LẠI", new Vector2(0f, -100f), RestartGame);

            _pausePanel.SetActive(false);
        }

        private void StartGame()
        {
            _startPanel.SetActive(false);
            GamePauseService.ReleasePause(StartPauseReason);
        }

        private void OpenPause()
        {
            if (_pausePanel.activeSelf)
            {
                return;
            }

            _pausePanel.SetActive(true);
            GamePauseService.RequestPause(ManualPauseReason);
        }

        private void ResumeGame()
        {
            _pausePanel.SetActive(false);
            GamePauseService.ReleasePause(ManualPauseReason);
        }

        private static void RestartGame()
        {
            GamePauseService.ClearAll();
            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.name);
        }

        public void RefreshStats()
        {
            if (_statsText == null)
            {
                return;
            }

            _statsText.text =
                $"Lượt chơi: {GameProgressService.RunsPlayed}    " +
                $"Thắng: {GameProgressService.Wins}    " +
                $"Kỷ lục: Lv.{GameProgressService.BestLevel}";
        }

        private GameObject CreateFullscreenPanel(string name, Color color)
        {
            GameObject panel = CreateUIObject(name, transform);
            RectTransform rect = panel.GetComponent<RectTransform>();
            Stretch(rect);

            Image image = panel.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = true;
            return panel;
        }

        private static void CreateActionButton(Transform parent, string text, Vector2 position, UnityEngine.Events.UnityAction action)
        {
            GameObject obj = CreateUIObject(text + "Button", parent);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(500f, 110f);
            rect.anchoredPosition = position;

            Image image = obj.AddComponent<Image>();
            image.color = new Color(0.16f, 0.12f, 0.07f, 0.98f);

            Outline outline = obj.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.60f, 0.12f, 0.85f);
            outline.effectDistance = new Vector2(2f, -2f);

            Button button = obj.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);

            TMP_Text label = CreateLabel(obj.transform, text, 34f);
            label.fontStyle = FontStyles.Bold;
            label.raycastTarget = false;
        }

        private static TMP_Text CreateLabel(Transform parent, string text, float fontSize)
        {
            GameObject obj = CreateUIObject("Text", parent);
            RectTransform rect = obj.GetComponent<RectTransform>();
            Stretch(rect);

            TextMeshProUGUI label = obj.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            return label;
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject obj = new(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, Vector2 pivot)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }
    }
}
