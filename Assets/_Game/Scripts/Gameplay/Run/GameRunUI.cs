using DienGame.Gameplay.Enemy;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DienGame.Gameplay.Run
{
    [RequireComponent(typeof(Canvas))]
    public sealed class GameRunUI : MonoBehaviour
    {
        private GameRunController _controller;
        private EnemyHealth _bossHealth;

        private GameObject _root;
        private TMP_Text _timerText;
        private GameObject _bossBarRoot;
        private Image _bossHealthFill;
        private TMP_Text _bossHealthText;
        private GameObject _resultPanel;
        private TMP_Text _resultText;

        private void Awake()
        {
            BuildUI();
        }

        private void Update()
        {
            if (_controller == null)
            {
                return;
            }

            RefreshTimer();
            RefreshBossBar();
        }

        public void Bind(GameRunController controller)
        {
            if (_controller == controller)
            {
                return;
            }

            Unsubscribe();

            _controller = controller;

            if (_controller == null)
            {
                return;
            }

            _controller.BossSpawned += OnBossSpawned;
            _controller.RunEnded += OnRunEnded;

            if (_controller.BossHealth != null)
            {
                OnBossSpawned(_controller.BossHealth);
            }
        }

        private void RefreshTimer()
        {
            if (_timerText == null)
            {
                return;
            }

            if (_controller.Phase == GameRunController.RunPhase.Survival)
            {
                int totalSeconds =
                    Mathf.CeilToInt(_controller.RemainingSurvivalTime);

                int minutes = totalSeconds / 60;
                int seconds = totalSeconds % 60;

                _timerText.text = $"{minutes:00}:{seconds:00}";
                return;
            }

            if (_controller.Phase == GameRunController.RunPhase.Boss)
            {
                _timerText.text = "BOSS";
                return;
            }

            _timerText.text = string.Empty;
        }

        private void RefreshBossBar()
        {
            if (_bossBarRoot == null)
            {
                return;
            }

            bool show =
                _controller != null &&
                _controller.Phase == GameRunController.RunPhase.Boss &&
                _bossHealth != null;

            _bossBarRoot.SetActive(show);

            if (!show)
            {
                return;
            }

            if (_bossHealthFill != null)
            {
                _bossHealthFill.fillAmount =
                    _bossHealth.HealthNormalized;
            }

            if (_bossHealthText != null)
            {
                _bossHealthText.text =
                    $"BOSS  {Mathf.CeilToInt(_bossHealth.CurrentHealth)} / " +
                    $"{Mathf.CeilToInt(_bossHealth.MaxHealth)}";
            }
        }

        private void OnBossSpawned(EnemyHealth bossHealth)
        {
            _bossHealth = bossHealth;

            if (_bossBarRoot != null)
            {
                _bossBarRoot.SetActive(true);
            }
        }

        private void OnRunEnded(GameRunController.RunPhase phase)
        {
            if (_resultPanel == null || _resultText == null)
            {
                return;
            }

            _resultText.text =
                phase == GameRunController.RunPhase.Won
                    ? "CHIẾN THẮNG"
                    : "GAME OVER";

            _resultPanel.SetActive(true);
        }

        private void RestartRun()
        {
            Time.timeScale = 1f;

            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.name);
        }

        private void BuildUI()
        {
            _root = CreateUIObject("RunUIRoot", transform);

            RectTransform rootRect =
                _root.GetComponent<RectTransform>();

            StretchFullScreen(rootRect);

            CreateTimer();
            CreateBossBar();
            CreateResultPanel();
        }

        private void CreateTimer()
        {
            GameObject timerObject =
                CreateUIObject("RunTimer", _root.transform);

            RectTransform rect =
                timerObject.GetComponent<RectTransform>();

            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(300f, 80f);
            rect.anchoredPosition = new Vector2(0f, -20f);

            _timerText =
                timerObject.AddComponent<TextMeshProUGUI>();

            _timerText.text = "10:00";
            _timerText.fontSize = 44f;
            _timerText.fontStyle = FontStyles.Bold;
            _timerText.alignment = TextAlignmentOptions.Center;
            _timerText.raycastTarget = false;
        }

        private void CreateBossBar()
        {
            _bossBarRoot =
                CreateUIObject("BossHealthBar", _root.transform);

            RectTransform rect =
                _bossBarRoot.GetComponent<RectTransform>();

            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(760f, 54f);
            rect.anchoredPosition = new Vector2(0f, -88f);

            Image background =
                _bossBarRoot.AddComponent<Image>();

            background.color =
                new Color(0.08f, 0.08f, 0.08f, 0.92f);

            GameObject fillObject =
                CreateUIObject("Fill", _bossBarRoot.transform);

            RectTransform fillRect =
                fillObject.GetComponent<RectTransform>();

            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(5f, 5f);
            fillRect.offsetMax = new Vector2(-5f, -5f);

            _bossHealthFill = fillObject.AddComponent<Image>();
            _bossHealthFill.type = Image.Type.Filled;
            _bossHealthFill.fillMethod = Image.FillMethod.Horizontal;
            _bossHealthFill.fillOrigin = 0;
            _bossHealthFill.fillAmount = 1f;
            _bossHealthFill.color = new Color(0.85f, 0.16f, 0.12f, 1f);

            GameObject textObject =
                CreateUIObject("Text", _bossBarRoot.transform);

            RectTransform textRect =
                textObject.GetComponent<RectTransform>();

            StretchFullScreen(textRect);

            _bossHealthText =
                textObject.AddComponent<TextMeshProUGUI>();

            _bossHealthText.fontSize = 26f;
            _bossHealthText.fontStyle = FontStyles.Bold;
            _bossHealthText.alignment = TextAlignmentOptions.Center;
            _bossHealthText.raycastTarget = false;

            _bossBarRoot.SetActive(false);
        }

        private void CreateResultPanel()
        {
            _resultPanel =
                CreateUIObject("RunResultPanel", _root.transform);

            RectTransform panelRect =
                _resultPanel.GetComponent<RectTransform>();

            StretchFullScreen(panelRect);

            Image background =
                _resultPanel.AddComponent<Image>();

            background.color =
                new Color(0f, 0f, 0f, 0.72f);

            GameObject resultTextObject =
                CreateUIObject("ResultText", _resultPanel.transform);

            RectTransform resultTextRect =
                resultTextObject.GetComponent<RectTransform>();

            resultTextRect.anchorMin = new Vector2(0.5f, 0.5f);
            resultTextRect.anchorMax = new Vector2(0.5f, 0.5f);
            resultTextRect.pivot = new Vector2(0.5f, 0.5f);
            resultTextRect.sizeDelta = new Vector2(800f, 150f);
            resultTextRect.anchoredPosition = new Vector2(0f, 90f);

            _resultText =
                resultTextObject.AddComponent<TextMeshProUGUI>();

            _resultText.fontSize = 64f;
            _resultText.fontStyle = FontStyles.Bold;
            _resultText.alignment = TextAlignmentOptions.Center;

            GameObject restartObject =
                CreateUIObject("RestartButton", _resultPanel.transform);

            RectTransform restartRect =
                restartObject.GetComponent<RectTransform>();

            restartRect.anchorMin = new Vector2(0.5f, 0.5f);
            restartRect.anchorMax = new Vector2(0.5f, 0.5f);
            restartRect.pivot = new Vector2(0.5f, 0.5f);
            restartRect.sizeDelta = new Vector2(420f, 110f);
            restartRect.anchoredPosition = new Vector2(0f, -100f);

            Image restartImage =
                restartObject.AddComponent<Image>();

            restartImage.color =
                new Color(0.18f, 0.18f, 0.18f, 0.98f);

            Button restartButton =
                restartObject.AddComponent<Button>();

            restartButton.onClick.AddListener(RestartRun);

            GameObject restartTextObject =
                CreateUIObject("Text", restartObject.transform);

            RectTransform restartTextRect =
                restartTextObject.GetComponent<RectTransform>();

            StretchFullScreen(restartTextRect);

            TextMeshProUGUI restartText =
                restartTextObject.AddComponent<TextMeshProUGUI>();

            restartText.text = "CHƠI LẠI";
            restartText.fontSize = 38f;
            restartText.fontStyle = FontStyles.Bold;
            restartText.alignment = TextAlignmentOptions.Center;
            restartText.raycastTarget = false;

            _resultPanel.SetActive(false);
        }

        private static GameObject CreateUIObject(
            string objectName,
            Transform parent)
        {
            var gameObject =
                new GameObject(
                    objectName,
                    typeof(RectTransform)
                );

            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        private static void StretchFullScreen(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private void Unsubscribe()
        {
            if (_controller == null)
            {
                return;
            }

            _controller.BossSpawned -= OnBossSpawned;
            _controller.RunEnded -= OnRunEnded;
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }
    }
}
