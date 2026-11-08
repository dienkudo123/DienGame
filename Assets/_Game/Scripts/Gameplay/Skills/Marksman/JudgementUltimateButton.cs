using UnityEngine;
using UnityEngine.UI;

namespace DienGame.Gameplay.Skills.Marksman
{
    [DisallowMultipleComponent]
    public sealed class JudgementUltimateButton : MonoBehaviour
    {
        // =========================================
        // LAYOUT
        // =========================================

        [Header("Layout")]

        [SerializeField]
        private Vector2 buttonSize =
            new(120f, 120f);

        [SerializeField]
        private Vector2 anchoredPosition =
            new(-100f, 140f);

        // =========================================
        // RUNTIME
        // =========================================

        private JudgementController _controller;

        private Sprite _icon;

        private GameObject _buttonObject;

        private Button _button;

        private Image _iconImage;

        private Image _cooldownOverlay;

        private bool _initialized;

        // =========================================
        // INITIALIZE
        // =========================================

        public void Initialize(
            JudgementController controller,
            Sprite icon)
        {
            _controller =
                controller;

            _icon =
                icon;

            if (_initialized)
            {
                if (_iconImage != null)
                {
                    _iconImage.sprite =
                        _icon;
                }

                return;
            }

            _initialized = true;

            BuildUI();
        }

        // =========================================
        // UPDATE
        // =========================================

        private void Update()
        {
            if (
                _controller == null ||
                _button == null
            )
            {
                return;
            }

            bool ready =
                _controller.IsReady;

            _button.interactable =
                ready;

            if (_cooldownOverlay != null)
            {
                _cooldownOverlay.fillAmount =
                    _controller
                        .CooldownNormalized;

                _cooldownOverlay.enabled =
                    !ready;
            }
        }

        // =========================================
        // BUILD
        // =========================================

        private void BuildUI()
        {
            Canvas canvas =
                FindCanvas();

            if (canvas == null)
            {
                canvas =
                    CreateCanvas();
            }

            // =====================================
            // ROOT
            // =====================================

            _buttonObject =
                new GameObject(
                    "JudgementUltimateButton",
                    typeof(RectTransform),
                    typeof(Image),
                    typeof(Button)
                );

            _buttonObject.transform.SetParent(
                canvas.transform,
                false
            );

            RectTransform rect =
                _buttonObject.GetComponent<
                    RectTransform
                >();

            rect.anchorMin =
                new Vector2(
                    1f,
                    0f
                );

            rect.anchorMax =
                new Vector2(
                    1f,
                    0f
                );

            rect.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );

            rect.sizeDelta =
                buttonSize;

            rect.anchoredPosition =
                anchoredPosition;

            // =====================================
            // ICON
            // =====================================

            _iconImage =
                _buttonObject
                    .GetComponent<Image>();

            _iconImage.sprite =
                _icon;

            _iconImage.preserveAspect =
                true;

            if (_icon == null)
            {
                _iconImage.color =
                    new Color(
                        0.20f,
                        0.20f,
                        0.20f,
                        0.95f
                    );
            }

            // =====================================
            // BUTTON
            // =====================================

            _button =
                _buttonObject.GetComponent<
                    Button
                >();

            _button.targetGraphic =
                _iconImage;

            _button.onClick.AddListener(
                OnPressed
            );

            // =====================================
            // COOLDOWN
            // =====================================

            GameObject cooldownObject =
                new GameObject(
                    "Cooldown",
                    typeof(RectTransform),
                    typeof(Image)
                );

            cooldownObject.transform.SetParent(
                _buttonObject.transform,
                false
            );

            RectTransform cooldownRect =
                cooldownObject.GetComponent<
                    RectTransform
                >();

            Stretch(
                cooldownRect
            );

            _cooldownOverlay =
                cooldownObject.GetComponent<
                    Image
                >();

            _cooldownOverlay.color =
                new Color(
                    0f,
                    0f,
                    0f,
                    0.65f
                );

            _cooldownOverlay.type =
                Image.Type.Filled;

            _cooldownOverlay.fillMethod =
                Image.FillMethod.Radial360;

            _cooldownOverlay.fillOrigin =
                (int)Image.Origin360.Top;

            _cooldownOverlay.fillClockwise =
                false;

            _cooldownOverlay.raycastTarget =
                false;

            _cooldownOverlay.fillAmount =
                0f;
        }

        // =========================================
        // CLICK
        // =========================================

        private void OnPressed()
        {
            if (_controller == null)
            {
                return;
            }

            _controller.TryActivate();
        }

        // =========================================
        // CANVAS
        // =========================================

        private static Canvas FindCanvas()
        {
            Canvas[] canvases =
                FindObjectsByType<Canvas>(
                    FindObjectsSortMode.None
                );

            foreach (
                Canvas canvas
                in canvases
            )
            {
                if (canvas == null)
                {
                    continue;
                }

                if (
                    canvas.renderMode ==
                    RenderMode.WorldSpace
                )
                {
                    continue;
                }

                return canvas;
            }

            return null;
        }

        private static Canvas CreateCanvas()
        {
            GameObject obj =
                new GameObject(
                    "RuntimeHUD",
                    typeof(Canvas),
                    typeof(CanvasScaler),
                    typeof(GraphicRaycaster)
                );

            Canvas canvas =
                obj.GetComponent<Canvas>();

            canvas.renderMode =
                RenderMode.ScreenSpaceOverlay;

            canvas.sortingOrder = 500;

            CanvasScaler scaler =
                obj.GetComponent<
                    CanvasScaler
                >();

            scaler.uiScaleMode =
                CanvasScaler
                    .ScaleMode
                    .ScaleWithScreenSize;

            scaler.referenceResolution =
                new Vector2(
                    1080f,
                    1920f
                );

            scaler.matchWidthOrHeight =
                0.5f;

            return canvas;
        }

        // =========================================
        // RECT
        // =========================================

        private static void Stretch(
            RectTransform rect)
        {
            rect.anchorMin =
                Vector2.zero;

            rect.anchorMax =
                Vector2.one;

            rect.offsetMin =
                Vector2.zero;

            rect.offsetMax =
                Vector2.zero;
        }

        // =========================================
        // CLEANUP
        // =========================================

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(
                    OnPressed
                );
            }

            if (_buttonObject != null)
            {
                Destroy(
                    _buttonObject
                );
            }
        }
    }
}