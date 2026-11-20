using DienGame.Gameplay.Skills.Visuals;
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

        [Tooltip(
            "Kích thước ULT so với joystick. " +
            "0.5 = bằng một nửa joystick."
        )]
        [SerializeField, Range(0.2f, 1f)]
        private float joystickSizeMultiplier = 0.5f;

        [Tooltip(
            "Nếu không tìm thấy MovementJoystick " +
            "thì dùng kích thước này."
        )]
        [SerializeField, Min(40f)]
        private float fallbackButtonSize = 120f;

        [Tooltip(
            "Nếu không tìm thấy MovementJoystick " +
            "thì dùng vị trí này."
        )]
        [SerializeField]
        private Vector2 fallbackAnchoredPosition =
            new(-180f, 180f);

        [Header("Icon")]

        [Tooltip(
            "Phóng icon nhẹ bên trong mask tròn. " +
            "Tăng nếu ảnh còn lộ viền vuông."
        )]
        [SerializeField, Range(1f, 1.3f)]
        private float iconZoom = 1.06f;

        // =========================================
        // CONTROLLER
        // =========================================

        private JudgementController _controller;

        private Sprite _icon;

        // =========================================
        // UI
        // =========================================

        private GameObject _buttonObject;

        private Button _button;

        private Image _buttonRaycastImage;

        private Image _iconImage;

        private Image _cooldownOverlay;

        private Text _fallbackLabel;

        private Text _cooldownText;

        // =========================================
        // CIRCLE MASK
        // =========================================

        private Texture2D _circleTexture;

        private Sprite _circleSprite;

        // =========================================
        // STATE
        // =========================================

        private bool _initialized;

        private float _iconRetryTimer;

        private const float IconRetryInterval =
            0.5f;

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
                icon != null
                    ? icon
                    : SkillIconCatalogProvider
                        .JudgementIcon;

            if (_initialized)
            {
                RefreshIcon();
                return;
            }

            _initialized = true;

            CreateCircleSprite();

            BuildUI();
        }

        // =========================================
        // UPDATE
        // =========================================

        private void Update()
        {
            RetryLoadIcon();

            if (
                _controller == null ||
                _button == null
            )
            {
                return;
            }

            UpdateCooldown();
        }

        // =========================================
        // BUILD UI
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

            _buttonObject =
                new GameObject(
                    "JudgementUltimateButton",
                    typeof(RectTransform),
                    typeof(Image),
                    typeof(Button)
                );

            _buttonObject
                .transform
                .SetParent(
                    canvas.transform,
                    false
                );

            RectTransform buttonRect =
                _buttonObject
                    .GetComponent<RectTransform>();

            ConfigureLayout(
                buttonRect
            );

            // -------------------------------------
            // ROOT IMAGE
            // Chỉ dùng để nhận raycast.
            // Không còn nền tròn cũ.
            // -------------------------------------

            _buttonRaycastImage =
                _buttonObject
                    .GetComponent<Image>();

            _buttonRaycastImage.sprite =
                _circleSprite;

            _buttonRaycastImage.color =
                new Color(
                    1f,
                    1f,
                    1f,
                    0.001f
                );

            _buttonRaycastImage
                .preserveAspect =
                true;

            _buttonRaycastImage
                .raycastTarget =
                true;

            // -------------------------------------
            // BUTTON
            // -------------------------------------

            _button =
                _buttonObject
                    .GetComponent<Button>();

            _button.targetGraphic =
                _buttonRaycastImage;

            _button.transition =
                Selectable.Transition.None;

            Navigation navigation =
                _button.navigation;

            navigation.mode =
                Navigation.Mode.None;

            _button.navigation =
                navigation;

            _button
                .onClick
                .AddListener(
                    OnPressed
                );

            CreateCircularIcon();

            CreateCooldownOverlay();

            CreateTexts();

            RefreshIcon();
        }

        // =========================================
        // LAYOUT
        // =========================================

        private void ConfigureLayout(
            RectTransform buttonRect)
        {
            float buttonSize =
                fallbackButtonSize;

            Vector2 position =
                fallbackAnchoredPosition;

            RectTransform joystick =
                FindMovementJoystick();

            if (joystick != null)
            {
                float joystickSize =
                    Mathf.Min(
                        Mathf.Abs(
                            joystick
                                .rect
                                .width
                        ),
                        Mathf.Abs(
                            joystick
                                .rect
                                .height
                        )
                    );

                if (joystickSize <= 1f)
                {
                    joystickSize =
                        Mathf.Min(
                            Mathf.Abs(
                                joystick
                                    .sizeDelta
                                    .x
                            ),
                            Mathf.Abs(
                                joystick
                                    .sizeDelta
                                    .y
                            )
                        );
                }

                if (joystickSize > 1f)
                {
                    buttonSize =
                        joystickSize *
                        joystickSizeMultiplier;
                }

                // Joystick:
                // anchor bottom-left
                //
                // Ultimate:
                // anchor bottom-right
                //
                // Giữ vị trí đối xứng.
                position =
                    new Vector2(
                        -Mathf.Abs(
                            joystick
                                .anchoredPosition
                                .x
                        ),
                        joystick
                            .anchoredPosition
                            .y
                    );
            }

            buttonRect.anchorMin =
                new Vector2(
                    1f,
                    0f
                );

            buttonRect.anchorMax =
                new Vector2(
                    1f,
                    0f
                );

            buttonRect.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );

            buttonRect.sizeDelta =
                new Vector2(
                    buttonSize,
                    buttonSize
                );

            buttonRect.anchoredPosition =
                position;
        }

        private static RectTransform
            FindMovementJoystick()
        {
            GameObject joystickObject =
                GameObject.Find(
                    "MovementJoystick"
                );

            if (joystickObject != null)
            {
                RectTransform rect =
                    joystickObject
                        .GetComponent<
                            RectTransform
                        >();

                if (rect != null)
                {
                    return rect;
                }
            }

            RectTransform[] rects =
                FindObjectsByType<
                    RectTransform
                >(
                    FindObjectsSortMode.None
                );

            foreach (
                RectTransform rect
                in rects
            )
            {
                if (
                    rect != null &&
                    rect.name ==
                    "MovementJoystick"
                )
                {
                    return rect;
                }
            }

            return null;
        }

        // =========================================
        // CIRCULAR ICON
        // =========================================

        private void CreateCircularIcon()
        {
            // -------------------------------------
            // MASK OBJECT
            // -------------------------------------

            GameObject maskObject =
                new GameObject(
                    "IconCircleMask",
                    typeof(RectTransform),
                    typeof(Image),
                    typeof(Mask)
                );

            maskObject
                .transform
                .SetParent(
                    _buttonObject.transform,
                    false
                );

            RectTransform maskRect =
                maskObject
                    .GetComponent<
                        RectTransform
                    >();

            Stretch(
                maskRect
            );

            Image maskImage =
                maskObject
                    .GetComponent<Image>();

            maskImage.sprite =
                _circleSprite;

            maskImage.color =
                Color.white;

            maskImage.preserveAspect =
                true;

            maskImage.raycastTarget =
                false;

            Mask mask =
                maskObject
                    .GetComponent<Mask>();

            mask.showMaskGraphic =
                false;

            // -------------------------------------
            // ICON
            // -------------------------------------

            GameObject iconObject =
                new GameObject(
                    "UltimateIcon",
                    typeof(RectTransform),
                    typeof(Image)
                );

            iconObject
                .transform
                .SetParent(
                    maskObject.transform,
                    false
                );

            RectTransform iconRect =
                iconObject
                    .GetComponent<
                        RectTransform
                    >();

            Stretch(
                iconRect
            );

            iconRect.localScale =
                Vector3.one *
                iconZoom;

            _iconImage =
                iconObject
                    .GetComponent<Image>();

            _iconImage.color =
                Color.white;

            _iconImage.preserveAspect =
                true;

            _iconImage.raycastTarget =
                false;
        }

        // =========================================
        // COOLDOWN
        // =========================================

        private void CreateCooldownOverlay()
        {
            GameObject cooldownObject =
                new GameObject(
                    "CooldownOverlay",
                    typeof(RectTransform),
                    typeof(Image)
                );

            cooldownObject
                .transform
                .SetParent(
                    _buttonObject.transform,
                    false
                );

            RectTransform cooldownRect =
                cooldownObject
                    .GetComponent<
                        RectTransform
                    >();

            Stretch(
                cooldownRect
            );

            _cooldownOverlay =
                cooldownObject
                    .GetComponent<Image>();

            _cooldownOverlay.sprite =
                _circleSprite;

            _cooldownOverlay.color =
                new Color(
                    0f,
                    0f,
                    0f,
                    0.68f
                );

            _cooldownOverlay.type =
                Image.Type.Filled;

            _cooldownOverlay.fillMethod =
                Image.FillMethod.Radial360;

            _cooldownOverlay.fillOrigin =
                (int)
                    Image.Origin360.Top;

            _cooldownOverlay.fillClockwise =
                false;

            _cooldownOverlay.fillAmount =
                0f;

            _cooldownOverlay
                .raycastTarget =
                false;
        }

        private void UpdateCooldown()
        {
            bool ready =
                _controller.IsReady;

            _button.interactable =
                ready;

            if (_cooldownOverlay != null)
            {
                _cooldownOverlay.fillAmount =
                    Mathf.Clamp01(
                        _controller
                            .CooldownNormalized
                    );

                _cooldownOverlay.enabled =
                    !ready;
            }

            if (_cooldownText != null)
            {
                _cooldownText.text =
                    ready
                        ? string.Empty
                        : Mathf
                            .CeilToInt(
                                _controller
                                    .CooldownRemaining
                            )
                            .ToString();
            }

            if (_iconImage != null)
            {
                _iconImage.color =
                    ready
                        ? Color.white
                        : new Color(
                            0.65f,
                            0.65f,
                            0.65f,
                            1f
                        );
            }
        }

        // =========================================
        // TEXT
        // =========================================

        private void CreateTexts()
        {
            // -------------------------------------
            // FALLBACK "ULT"
            // Chỉ hiện nếu không tìm thấy icon.
            // -------------------------------------

            GameObject fallbackObject =
                new GameObject(
                    "FallbackULTText",
                    typeof(RectTransform),
                    typeof(Text)
                );

            fallbackObject
                .transform
                .SetParent(
                    _buttonObject.transform,
                    false
                );

            Stretch(
                fallbackObject
                    .GetComponent<
                        RectTransform
                    >()
            );

            _fallbackLabel =
                fallbackObject
                    .GetComponent<Text>();

            _fallbackLabel.font =
                Resources
                    .GetBuiltinResource<Font>(
                        "LegacyRuntime.ttf"
                    );

            _fallbackLabel.text =
                "ULT";

            _fallbackLabel.alignment =
                TextAnchor.MiddleCenter;

            _fallbackLabel.fontSize =
                22;

            _fallbackLabel.fontStyle =
                FontStyle.Bold;

            _fallbackLabel.color =
                new Color(
                    1f,
                    0.72f,
                    0.18f,
                    1f
                );

            _fallbackLabel
                .raycastTarget =
                false;

            // -------------------------------------
            // COOLDOWN NUMBER
            // -------------------------------------

            GameObject cooldownTextObject =
                new GameObject(
                    "CooldownText",
                    typeof(RectTransform),
                    typeof(Text),
                    typeof(Outline)
                );

            cooldownTextObject
                .transform
                .SetParent(
                    _buttonObject.transform,
                    false
                );

            Stretch(
                cooldownTextObject
                    .GetComponent<
                        RectTransform
                    >()
            );

            _cooldownText =
                cooldownTextObject
                    .GetComponent<Text>();

            _cooldownText.font =
                Resources
                    .GetBuiltinResource<Font>(
                        "LegacyRuntime.ttf"
                    );

            _cooldownText.alignment =
                TextAnchor.MiddleCenter;

            _cooldownText.fontSize =
                26;

            _cooldownText.fontStyle =
                FontStyle.Bold;

            _cooldownText.color =
                Color.white;

            _cooldownText
                .raycastTarget =
                false;

            Outline outline =
                cooldownTextObject
                    .GetComponent<Outline>();

            outline.effectColor =
                new Color(
                    0f,
                    0f,
                    0f,
                    0.85f
                );

            outline.effectDistance =
                new Vector2(
                    2f,
                    -2f
                );
        }

        // =========================================
        // ICON CATALOG
        // =========================================

        private void RetryLoadIcon()
        {
            if (_icon != null)
            {
                return;
            }

            _iconRetryTimer -=
                Time.unscaledDeltaTime;

            if (_iconRetryTimer > 0f)
            {
                return;
            }

            _iconRetryTimer =
                IconRetryInterval;

            Sprite loadedIcon =
                SkillIconCatalogProvider
                    .JudgementIcon;

            if (loadedIcon == null)
            {
                return;
            }

            _icon =
                loadedIcon;

            RefreshIcon();
        }

        private void RefreshIcon()
        {
            if (
                _iconImage == null ||
                _fallbackLabel == null
            )
            {
                return;
            }

            if (_icon == null)
            {
                _icon =
                    SkillIconCatalogProvider
                        .JudgementIcon;
            }

            bool hasIcon =
                _icon != null;

            _iconImage.sprite =
                _icon;

            _iconImage.enabled =
                hasIcon;

            _fallbackLabel.enabled =
                !hasIcon;
        }

        // =========================================
        // INPUT
        // =========================================

        private void OnPressed()
        {
            _controller
                ?.TryActivate();
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
                if (
                    canvas != null &&
                    canvas.name ==
                    "GameplayCanvas"
                )
                {
                    return canvas;
                }
            }

            foreach (
                Canvas canvas
                in canvases
            )
            {
                if (
                    canvas != null &&
                    canvas.renderMode !=
                    RenderMode.WorldSpace
                )
                {
                    return canvas;
                }
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

            canvas.sortingOrder =
                500;

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
        // CIRCLE SPRITE
        // =========================================

        private void CreateCircleSprite()
        {
            const int size =
                128;

            _circleTexture =
                new Texture2D(
                    size,
                    size,
                    TextureFormat.RGBA32,
                    false
                )
                {
                    name =
                        "UltimateCircleMask",

                    wrapMode =
                        TextureWrapMode.Clamp,

                    filterMode =
                        FilterMode.Bilinear,
                };

            Color32[] pixels =
                new Color32[
                    size * size
                ];

            Vector2 center =
                new Vector2(
                    (size - 1) * 0.5f,
                    (size - 1) * 0.5f
                );

            float radius =
                size * 0.49f;

            for (
                int y = 0;
                y < size;
                y++
            )
            {
                for (
                    int x = 0;
                    x < size;
                    x++
                )
                {
                    float distance =
                        Vector2.Distance(
                            new Vector2(
                                x,
                                y
                            ),
                            center
                        );

                    // Mép anti-alias nhẹ.
                    float alpha =
                        Mathf.Clamp01(
                            radius -
                            distance
                        );

                    pixels[
                        y * size +
                        x
                    ] =
                        new Color(
                            1f,
                            1f,
                            1f,
                            alpha
                        );
                }
            }

            _circleTexture
                .SetPixels32(
                    pixels
                );

            _circleTexture
                .Apply(
                    false,
                    true
                );

            _circleSprite =
                Sprite.Create(
                    _circleTexture,
                    new Rect(
                        0f,
                        0f,
                        size,
                        size
                    ),
                    new Vector2(
                        0.5f,
                        0.5f
                    ),
                    100f
                );
        }

        // =========================================
        // RECT UTILITY
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
            if (_circleSprite != null)
            {
                Destroy(
                    _circleSprite
                );
            }

            if (_circleTexture != null)
            {
                Destroy(
                    _circleTexture
                );
            }
        }
    }
}