using DienGame.Gameplay.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DienGame.Gameplay.Skills.Assassin
{
    /// <summary>
    /// Assassin-only mobile HUD button for hold-drag-release wall dive.
    /// Builds itself at runtime and forwards pointer events directly to AssassinWallDive.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public sealed class AssassinWallDiveHud : MonoBehaviour
    {
        [SerializeField] private AssassinWallDive wallDive;
        private GameObject _buttonObject;
        private Image _background;
        private Image _buttonBorder;
        private Image _cooldownMask;
        private static Sprite s_discSprite;
        private static Sprite s_ringSprite;
        private TMP_Text _label;
        private string _previousLabel;

        public void SetWallDive(AssassinWallDive value) => wallDive = value;

        private void Start()
        {
            if (wallDive == null) wallDive = FindFirstObjectByType<AssassinWallDive>();
            if (wallDive == null)
            {
                Debug.LogWarning("Assassin active skill HUD: WallDive not found.", this);
                enabled = false;
                return;
            }
            BuildButton();
        }

        private void BuildButton()
        {
            _buttonObject = new GameObject("AssassinWallDiveButton", typeof(RectTransform));
            _buttonObject.transform.SetParent(transform, false);
            RectTransform rect = _buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.sizeDelta = new Vector2(144f, 144f);
            rect.anchoredPosition = new Vector2(-60f, 270f);

            _background = _buttonObject.AddComponent<Image>();
            _background.sprite = GetFlatCircleSprite(false);
            _background.type = Image.Type.Simple;
            _background.raycastTarget = true;
            _background.color = new Color(.075f, .09f, .13f, .82f);

            GameObject borderObj = new GameObject("FlatBorder", typeof(RectTransform));
            borderObj.transform.SetParent(_buttonObject.transform, false);
            RectTransform borderRect = borderObj.GetComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = Vector2.zero;
            borderRect.offsetMax = Vector2.zero;
            _buttonBorder = borderObj.AddComponent<Image>();
            _buttonBorder.sprite = GetFlatCircleSprite(true);
            _buttonBorder.type = Image.Type.Simple;
            _buttonBorder.color = new Color(.65f, .71f, .76f, .63f);
            _buttonBorder.raycastTarget = false;

            GameObject cooldown = new GameObject("CooldownMask", typeof(RectTransform));
            cooldown.transform.SetParent(_buttonObject.transform, false);
            RectTransform cooldownRect = cooldown.GetComponent<RectTransform>();
            cooldownRect.anchorMin = Vector2.zero;
            cooldownRect.anchorMax = Vector2.one;
            cooldownRect.offsetMin = Vector2.zero;
            cooldownRect.offsetMax = Vector2.zero;
            _cooldownMask = cooldown.AddComponent<Image>();
            _cooldownMask.sprite = GetFlatCircleSprite(false);
            _cooldownMask.color = new Color(0f, 0f, 0f, .38f);
            _cooldownMask.type = Image.Type.Filled;
            _cooldownMask.fillMethod = Image.FillMethod.Radial360;
            _cooldownMask.fillOrigin = 2;
            _cooldownMask.fillClockwise = false;
            _cooldownMask.fillAmount = 0f;
            _cooldownMask.raycastTarget = false;

            GameObject title = new GameObject("Label", typeof(RectTransform));
            title.transform.SetParent(_buttonObject.transform, false);
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = new Vector2(10f, 10f);
            titleRect.offsetMax = new Vector2(-10f, -10f);
            _label = title.AddComponent<TextMeshProUGUI>();
            _label.fontSize = 25f;
            _label.fontStyle = FontStyles.Normal;
            _label.color = new Color(.92f, .93f, .94f, 1f);
            _label.alignment = TextAlignmentOptions.Center;
            _label.textWrappingMode = TextWrappingModes.Normal;
            _label.raycastTarget = false;

            AssassinWallDivePointer pointer = _buttonObject.AddComponent<AssassinWallDivePointer>();
            pointer.Initialize(this);
        }

        /// <summary>
        /// A monochrome anti-aliased disc and its thin border, created once at
        /// runtime. Avoids imported sprites and the glossy button look.
        /// </summary>
        private static Sprite GetFlatCircleSprite(bool borderOnly)
        {
            if (borderOnly && s_ringSprite != null) return s_ringSprite;
            if (!borderOnly && s_discSprite != null) return s_discSprite;

            const int size = 128;
            const float radius = 60f;
            const float borderInner = 58f;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = borderOnly ? "WallDiveFlatBorder" : "WallDiveFlatDisc",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + .5f - size * .5f;
                    float dy = y + .5f - size * .5f;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float coverage = Mathf.Clamp01(radius + .5f - distance);
                    float alpha = borderOnly
                        ? coverage * Mathf.Clamp01(distance - borderInner + .5f)
                        : coverage;
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(false, true);
            Sprite sprite = Sprite.Create(tex, new Rect(0f, 0f, size, size),
                new Vector2(.5f, .5f), 100f);
            sprite.name = borderOnly ? "WallDiveFlatBorder" : "WallDiveFlatDisc";
            if (borderOnly) s_ringSprite = sprite;
            else s_discSprite = sprite;
            return sprite;
        }

        private void Update()
        {
            if (_buttonObject == null || wallDive == null) return;

            bool visible = !GamePauseService.IsPaused && Time.timeScale > 0f;
            if (_buttonObject.activeSelf != visible) _buttonObject.SetActive(visible);
            if (!visible) return;

            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            string caption;
            Color border;
            switch (wallDive.CurrentPhase)
            {
                case AssassinWallDive.Phase.AimingHook:
                    caption = "MÓC";
                    border = new Color(.58f, .82f, .79f, .85f);
                    break;
                case AssassinWallDive.Phase.Hooking:
                    caption = "MÓC";
                    border = new Color(.58f, .82f, .79f, .85f);
                    break;
                case AssassinWallDive.Phase.Clinging:
                case AssassinWallDive.Phase.AimingDash:
                    caption = "BẬT";
                    border = new Color(.94f, .76f, .47f, .82f);
                    break;
                case AssassinWallDive.Phase.Dashing:
                    caption = "LƯỚT";
                    border = new Color(.94f, .76f, .47f, .62f);
                    break;
                case AssassinWallDive.Phase.Cooldown:
                    caption = Mathf.CeilToInt(wallDive.CooldownRemaining) + "s";
                    border = new Color(.56f, .59f, .63f, .42f);
                    break;
                default:
                    caption = "MÓC";
                    border = new Color(.65f, .71f, .76f, .63f);
                    break;
            }

            if (_background != null)
                _background.color = new Color(.075f, .09f, .13f, .82f);
            if (_buttonBorder != null) _buttonBorder.color = border;

            if (_cooldownMask != null)
            {
                float fill = 0f;
                if (wallDive.CurrentPhase == AssassinWallDive.Phase.Cooldown)
                {
                    float duration = Mathf.Max(.01f, wallDive.CooldownDuration);
                    fill = Mathf.Clamp01(wallDive.CooldownRemaining / duration);
                }
                _cooldownMask.fillAmount = fill;
                _cooldownMask.enabled = fill > .001f;
            }

            if (caption != _previousLabel)
            {
                _previousLabel = caption;
                _label.text = caption;
            }
        }

        private void HandlePressDown(PointerEventData eventData)
        {
            if (wallDive == null) return;
            wallDive.BeginHold(eventData.position);
        }

        private void HandleDrag(PointerEventData eventData)
        {
            if (wallDive == null) return;
            wallDive.UpdateHold(eventData.position);
        }

        private void HandlePressUp(PointerEventData eventData)
        {
            if (wallDive == null) return;
            wallDive.UpdateHold(eventData.position);
            wallDive.ReleaseHold();
        }

        private void HandleCancel()
        {
            if (wallDive == null) return;
            wallDive.CancelHold();
        }

        private void OnDestroy()
        {
            if (_buttonObject != null) Destroy(_buttonObject);
        }

        private sealed class AssassinWallDivePointer : MonoBehaviour,
            IPointerDownHandler, IDragHandler, IPointerUpHandler
        {
            private AssassinWallDiveHud _owner;
            private bool _pressed;

            public void Initialize(AssassinWallDiveHud owner) => _owner = owner;

            public void OnPointerDown(PointerEventData eventData)
            {
                if (_owner == null) return;
                _pressed = true;
                _owner.HandlePressDown(eventData);
            }

            public void OnDrag(PointerEventData eventData)
            {
                if (!_pressed || _owner == null) return;
                _owner.HandleDrag(eventData);
            }

            public void OnPointerUp(PointerEventData eventData)
            {
                if (_owner == null) return;
                if (_pressed)
                    _owner.HandlePressUp(eventData);
                _pressed = false;
            }

            private void OnDisable()
            {
                if (_pressed && _owner != null) _owner.HandleCancel();
                _pressed = false;
            }
        }
    }
}
