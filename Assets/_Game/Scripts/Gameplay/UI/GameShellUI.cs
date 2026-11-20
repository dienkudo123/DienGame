using DienGame.Client;
using DienGame.Gameplay.Core;
using DienGame.Gameplay.Run;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DienGame.Gameplay.UI
{
    /// <summary>Responsive, code-built start/pause UI. Keeps all existing game actions intact.</summary>
    [DisallowMultipleComponent]
    public sealed class GameShellUI : MonoBehaviour
    {
        private const string StartPauseReason = "StartMenu";
        private const string ManualPauseReason = "ManualPause";
        private GameObject _startPanel;
        private GameObject _pausePanel;
        private GameObject _audioSettingsPanel;
        private Button _pauseButton;
        private TMP_Text _statsText;
        private bool _built;
        private CanvasGroup _movementCanvasGroup;
        private float _movementOriginalAlpha = 1f;
        private bool _movementOriginalBlocks = true;
        private bool _movementOriginalInteractable = true;
        private bool _movementWasHidden;

        private void Awake() => BuildIfNeeded();

        private void Start()
        {
            bool autoStart = LobbySessionService.ConsumeAutoStartRequested();
            if (autoStart)
            {
                if (_startPanel != null) _startPanel.SetActive(false);
                if (_pauseButton != null) _pauseButton.gameObject.SetActive(true);
                GamePauseService.ReleasePause(StartPauseReason);
                SetMovementVisible(true);
            }
            else
            {
                if (_pauseButton != null) _pauseButton.gameObject.SetActive(false);
                GamePauseService.RequestPause(StartPauseReason);
                SetMovementVisible(false);
            }
            RefreshStats();
        }

        private void OnDestroy()
        {
            SetMovementVisible(true);
            GamePauseService.ReleasePause(StartPauseReason);
            GamePauseService.ReleasePause(ManualPauseReason);
        }

        private void LateUpdate()
        {
            if ((_pausePanel != null && _pausePanel.activeSelf) ||
                (_startPanel != null && _startPanel.activeSelf))
            {
                // Joystick may be created after the pause UI by another bootstrap.
                if (!_movementWasHidden) SetMovementVisible(false);
                return;
            }
            BringPauseButtonToFront();
        }

        private void BringPauseButtonToFront()
        {
            if (_pauseButton != null && _pauseButton.gameObject.activeSelf)
                _pauseButton.transform.SetAsLastSibling();
        }

        private void BuildIfNeeded()
        {
            if (_built) return;
            _built = true;
            BuildPauseButton();
            BuildStartPanel();
            BuildPausePanel();
            BringPauseButtonToFront();
        }

        private void BuildPauseButton()
        {
            GameObject obj = DienGameUITheme.Panel(transform, "PauseButton", "IconPlate");
            RectTransform rt = (RectTransform)obj.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.sizeDelta = new Vector2(92f, 92f);
            rt.anchoredPosition = new Vector2(-28f, -26f);
            Image bg = obj.GetComponent<Image>();
            bg.raycastTarget = true;
            _pauseButton = obj.AddComponent<Button>();
            _pauseButton.targetGraphic = bg;
            _pauseButton.onClick.AddListener(OpenPause);
            DienGameUITheme.Text(obj.transform, "PauseGlyph", "II", 40f, DienGameUITheme.Ivory,
                .12f, .12f, .88f, .88f, TextAlignmentOptions.Center, true);
        }

        private GameObject MakeVeil(string name)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(transform, false);
            DienGameUITheme.Rect((RectTransform)obj.transform, 0f, 0f, 1f, 1f);
            Image veil = obj.GetComponent<Image>();
            veil.color = DienGameUITheme.Veil;
            veil.raycastTarget = true;
            return obj;
        }

        private void BuildStartPanel()
        {
            _startPanel = MakeVeil("StartPanel");
            GameObject frame = DienGameUITheme.Panel(_startPanel.transform, "StartFrame", "Panel");
            DienGameUITheme.Rect((RectTransform)frame.transform, .07f, .315f, .93f, .695f);
            GameObject header = DienGameUITheme.Panel(frame.transform, "Header", "Header");
            DienGameUITheme.Rect((RectTransform)header.transform, .04f, .75f, .96f, .948f);
            DienGameUITheme.Text(header.transform, "Title", "DIENGAME", 62f,
                DienGameUITheme.Gold, .07f, .14f, .93f, .86f, TextAlignmentOptions.Center, true);
            string cls = LobbySessionService.CurrentClass == PlayableClass.Assassin ? "SÁT THỦ" :
                LobbySessionService.CurrentClass == PlayableClass.Mage ? "PHÁP SƯ" : "XẠ THỦ";
            DienGameUITheme.Text(frame.transform, "Class", cls + " · SINH TỒN 10 PHÚT", 31f,
                DienGameUITheme.Ivory, .07f, .61f, .93f, .733f, TextAlignmentOptions.Center, true);
            _statsText = DienGameUITheme.Text(frame.transform, "Stats", "", 26f,
                DienGameUITheme.Muted, .08f, .44f, .92f, .57f);
            DienGameUITheme.Button(frame.transform, "StartGame", "BẮT ĐẦU", StartGame,
                "ButtonGold", .18f, .18f, .82f, .345f, 39f);
            DienGameUITheme.Text(frame.transform, "StartHint", "DI CHUYỂN 360° · NÂNG KỸ NĂNG KHI LÊN CẤP",
                23f, DienGameUITheme.Muted, .08f, .045f, .92f, .14f);
        }

        private void BuildPausePanel()
        {
            _pausePanel = MakeVeil("PausePanel");
            GameObject frame = DienGameUITheme.Panel(_pausePanel.transform, "PauseFrame", "Panel");
            DienGameUITheme.Rect((RectTransform)frame.transform, .07f, .26f, .93f, .74f);
            GameObject header = DienGameUITheme.Panel(frame.transform, "Header", "Header");
            DienGameUITheme.Rect((RectTransform)header.transform, .06f, .805f, .94f, .955f);
            DienGameUITheme.Text(header.transform, "Title", "TẠM DỪNG", 58f,
                DienGameUITheme.Gold, .08f, .12f, .92f, .88f,
                TextAlignmentOptions.Center, true);
            DienGameUITheme.Text(frame.transform, "Subtitle", "TRẬN ĐẤU ĐANG TẠM DỪNG",
                27f, DienGameUITheme.Muted, .09f, .716f, .91f, .797f);
            DienGameUITheme.Button(frame.transform, "Resume", "TIẾP TỤC", ResumeGame,
                "ButtonGold", .135f, .555f, .865f, .688f, 37f);
            DienGameUITheme.Button(frame.transform, "AudioSettings", "CÀI ĐẶT ÂM THANH", OpenPauseAudioSettings,
                "ButtonTeal", .135f, .410f, .865f, .543f, 33f);
            DienGameUITheme.Button(frame.transform, "Restart", "CHƠI LẠI", RestartGame,
                "ButtonGhost", .135f, .265f, .865f, .398f, 37f);
            DienGameUITheme.Button(frame.transform, "Lobby", "VỀ SẢNH", ReturnToLobby,
                "ButtonGhost", .135f, .120f, .865f, .253f, 37f);
            DienGameUITheme.Text(frame.transform, "Footnote", "CÀI ĐẶT TỰ LƯU KHI THAY ĐỔI",
                23f, DienGameUITheme.Muted, .10f, .025f, .90f, .105f);

            // Child overlay: keeps ManualPause held while adjusting sliders with Time.timeScale = 0.
            _audioSettingsPanel = DienGameAudioSettingsPanel.Create(_pausePanel.transform, ClosePauseAudioSettings);
            _audioSettingsPanel.SetActive(false);
            _pausePanel.SetActive(false);
        }

        private void OpenPauseAudioSettings()
        {
            if (_audioSettingsPanel == null) return;
            _audioSettingsPanel.SetActive(true);
            _audioSettingsPanel.transform.SetAsLastSibling();
        }

        private void ClosePauseAudioSettings()
        {
            if (_audioSettingsPanel != null) _audioSettingsPanel.SetActive(false);
            // Do not release ManualPause: player goes back to the pause menu.
        }

        private void StartGame()
        {
            if (_startPanel != null) _startPanel.SetActive(false);
            if (_pauseButton != null) _pauseButton.gameObject.SetActive(true);
            GamePauseService.ReleasePause(StartPauseReason);
            SetMovementVisible(true);
            BringPauseButtonToFront();
        }
        private void OpenPause()
        {
            if (_pausePanel == null || _pausePanel.activeSelf) return;
            if (_pauseButton != null) _pauseButton.gameObject.SetActive(false);
            _pausePanel.SetActive(true);
            if (_audioSettingsPanel != null) _audioSettingsPanel.SetActive(false);
            _pausePanel.transform.SetAsLastSibling();
            SetMovementVisible(false);
            GamePauseService.RequestPause(ManualPauseReason);
        }
        private void ResumeGame()
        {
            if (_audioSettingsPanel != null) _audioSettingsPanel.SetActive(false);
            if (_pausePanel != null) _pausePanel.SetActive(false);
            GamePauseService.ReleasePause(ManualPauseReason);
            SetMovementVisible(true);
            if (_pauseButton != null) _pauseButton.gameObject.SetActive(true);
            BringPauseButtonToFront();
        }
        // Hide only the gamepad artwork while the menu is open. Keep the stick object,
        // OnScreenStick bindings and gameplay logic intact for the next resume.
        private void SetMovementVisible(bool visible)
        {
            if (visible && !_movementWasHidden) return;
            if (_movementCanvasGroup == null)
            {
                RectTransform[] elements = GetComponentsInChildren<RectTransform>(true);
                foreach (RectTransform element in elements)
                {
                    if (element == null || element.name != "MovementJoystick") continue;
                    _movementCanvasGroup = element.GetComponent<CanvasGroup>();
                    if (_movementCanvasGroup == null)
                        _movementCanvasGroup = element.gameObject.AddComponent<CanvasGroup>();
                    break;
                }
            }
            if (_movementCanvasGroup == null) return;
            if (!visible)
            {
                if (!_movementWasHidden)
                {
                    _movementOriginalAlpha = _movementCanvasGroup.alpha;
                    _movementOriginalBlocks = _movementCanvasGroup.blocksRaycasts;
                    _movementOriginalInteractable = _movementCanvasGroup.interactable;
                    _movementWasHidden = true;
                }
                _movementCanvasGroup.alpha = 0f;
                _movementCanvasGroup.blocksRaycasts = false;
                _movementCanvasGroup.interactable = false;
            }
            else
            {
                _movementCanvasGroup.alpha = _movementOriginalAlpha;
                _movementCanvasGroup.blocksRaycasts = _movementOriginalBlocks;
                _movementCanvasGroup.interactable = _movementOriginalInteractable;
                _movementWasHidden = false;
            }
        }

        private static void RestartGame() => LobbySessionService.RestartCurrentRun();
        private static void ReturnToLobby() => LobbySessionService.ReturnToLobby();

        public void RefreshStats()
        {
            if (_statsText == null) return;
            _statsText.text = $"Lượt: {GameProgressService.RunsPlayed}   ·   " +
                $"Thắng: {GameProgressService.Wins}   ·   Kỷ lục: Lv.{GameProgressService.BestLevel}";
        }
    }
}
