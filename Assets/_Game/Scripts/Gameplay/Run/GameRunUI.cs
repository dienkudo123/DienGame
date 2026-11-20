using DienGame.Client;
using DienGame.Gameplay.Enemy;
using TMPro;
using UnityEngine;
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

        private bool _hasTimerDisplay;
        private GameRunController.RunPhase _lastTimerPhase;
        private int _lastDisplayedSeconds;

        private bool _hasBossFill;
        private float _lastBossFill;
        private bool _hasBossText;
        private int _lastShownBossHealth;
        private int _lastShownBossMaxHealth;

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
            _hasTimerDisplay = false;
            _hasBossFill = false;
            _hasBossText = false;

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

            GameRunController.RunPhase phase = _controller.Phase;

            if (phase == GameRunController.RunPhase.Survival)
            {
                int totalSeconds =
                    Mathf.CeilToInt(_controller.RemainingSurvivalTime);

                if (_hasTimerDisplay &&
                    _lastTimerPhase == phase &&
                    _lastDisplayedSeconds == totalSeconds)
                {
                    return;
                }

                int minutes = totalSeconds / 60;
                int seconds = totalSeconds % 60;

                _timerText.text = $"{minutes:00}:{seconds:00}";
                _lastDisplayedSeconds = totalSeconds;
                _lastTimerPhase = phase;
                _hasTimerDisplay = true;
                return;
            }

            if (_hasTimerDisplay && _lastTimerPhase == phase)
            {
                return;
            }

            _timerText.text =
                phase == GameRunController.RunPhase.Boss
                    ? "BOSS"
                    : string.Empty;
            _lastTimerPhase = phase;
            _hasTimerDisplay = true;
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

            // Calling SetActive every frame is unnecessary when state is unchanged.
            if (_bossBarRoot.activeSelf != show)
            {
                _bossBarRoot.SetActive(show);
            }

            if (!show)
            {
                return;
            }

            if (_bossHealthFill != null)
            {
                float value = _bossHealth.HealthNormalized;
                if (!_hasBossFill || _lastBossFill != value)
                {
                    _bossHealthFill.fillAmount = value;
                    _lastBossFill = value;
                    _hasBossFill = true;
                }
            }

            if (_bossHealthText != null)
            {
                int shownHealth = Mathf.CeilToInt(_bossHealth.CurrentHealth);
                int shownMaxHealth = Mathf.CeilToInt(_bossHealth.MaxHealth);

                if (!_hasBossText ||
                    _lastShownBossHealth != shownHealth ||
                    _lastShownBossMaxHealth != shownMaxHealth)
                {
                    _bossHealthText.text =
                        $"BOSS  {shownHealth} / {shownMaxHealth}";
                    _lastShownBossHealth = shownHealth;
                    _lastShownBossMaxHealth = shownMaxHealth;
                    _hasBossText = true;
                }
            }
        }

        private void OnBossSpawned(EnemyHealth bossHealth)
        {
            _bossHealth = bossHealth;
            _hasBossFill = false;
            _hasBossText = false;

            if (_bossBarRoot != null && !_bossBarRoot.activeSelf)
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
            LobbySessionService.RestartCurrentRun();
        }

        private static void ReturnToLobby()
        {
            LobbySessionService.ReturnToLobby();
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
            DienGameUITheme.StyleText(_timerText, 42f, DienGameUITheme.Gold,
                true, false, TextAlignmentOptions.Center);
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

            background.color = new Color(.04f, .13f, .18f, .95f);
            DienGameUITheme.Skin(background, "Panel");

            GameObject fillObject =
                CreateUIObject("Fill", _bossBarRoot.transform);

            RectTransform fillRect =
                fillObject.GetComponent<RectTransform>();

            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(12f, 11f);
            fillRect.offsetMax = new Vector2(-12f, -11f);

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
            DienGameUITheme.StyleText(_bossHealthText, 32f, DienGameUITheme.Ivory,
                true, false, TextAlignmentOptions.Center);

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

            background.color = DienGameUITheme.Veil;
            background.raycastTarget = true;
            GameObject resultFrame = DienGameUITheme.Panel(_resultPanel.transform,
                "ResultFrame", "Panel");
            DienGameUITheme.Rect((RectTransform)resultFrame.transform,
                .075f, .345f, .925f, .67f);

            GameObject resultTextObject =
                CreateUIObject("ResultText", resultFrame.transform);

            RectTransform resultTextRect =
                resultTextObject.GetComponent<RectTransform>();

            DienGameUITheme.Rect(resultTextRect, .06f, .63f, .94f, .90f);

            _resultText =
                resultTextObject.AddComponent<TextMeshProUGUI>();

            _resultText.fontSize = 64f;
            _resultText.fontStyle = FontStyles.Bold;
            DienGameUITheme.StyleText(_resultText, 54f, DienGameUITheme.Gold,
                true, true, TextAlignmentOptions.Center);

            GameObject restartObject =
                CreateUIObject("RestartButton", resultFrame.transform);

            RectTransform restartRect =
                restartObject.GetComponent<RectTransform>();

            DienGameUITheme.Rect(restartRect, .18f, .37f, .82f, .54f);

            Image restartImage =
                restartObject.AddComponent<Image>();

            restartImage.color = Color.white;
            DienGameUITheme.Skin(restartImage, "ButtonGold");

            Button restartButton =
                restartObject.AddComponent<Button>();

            restartButton.targetGraphic = restartImage;
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
            DienGameUITheme.StyleText(restartText, 38f, new Color(.2f,.13f,.07f),
                true, false, TextAlignmentOptions.Center);

            GameObject lobbyObject =
                CreateUIObject("LobbyButton", resultFrame.transform);

            RectTransform lobbyRect =
                lobbyObject.GetComponent<RectTransform>();

            DienGameUITheme.Rect(lobbyRect, .18f, .13f, .82f, .30f);

            Image lobbyImage = lobbyObject.AddComponent<Image>();
            lobbyImage.color = Color.white;
            DienGameUITheme.Skin(lobbyImage, "ButtonGhost");

            Button lobbyButton = lobbyObject.AddComponent<Button>();
            lobbyButton.targetGraphic = lobbyImage;
            lobbyButton.onClick.AddListener(ReturnToLobby);

            GameObject lobbyTextObject =
                CreateUIObject("Text", lobbyObject.transform);

            RectTransform lobbyTextRect =
                lobbyTextObject.GetComponent<RectTransform>();

            StretchFullScreen(lobbyTextRect);

            TextMeshProUGUI lobbyText =
                lobbyTextObject.AddComponent<TextMeshProUGUI>();

            lobbyText.text = "VỀ SẢNH";
            lobbyText.fontSize = 34f;
            lobbyText.fontStyle = FontStyles.Bold;
            lobbyText.alignment = TextAlignmentOptions.Center;
            lobbyText.color = new Color(1f, 0.72f, 0.28f, 1f);
            DienGameUITheme.StyleText(lobbyText, 34f, DienGameUITheme.Ivory,
                true, false, TextAlignmentOptions.Center);

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
