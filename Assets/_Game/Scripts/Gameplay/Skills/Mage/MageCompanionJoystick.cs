using DienGame.Client;
using DienGame.Gameplay.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DienGame.Gameplay.Skills.Mage
{
    /// <summary>Runtime 2nd joystick: press/drag controls Chimera, release returns to roaming AI.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public sealed class MageCompanionJoystick : MonoBehaviour
    {
        private MageClassController _mage;
        private GameObject _root;
        private RectTransform _thumb;
        private TMP_Text _status;
        private Canvas _canvas;
        private Image _baseImage;
        private const float Radius = 54f;

        private void Start()
        {
            _mage = FindFirstObjectByType<MageClassController>();
            _canvas = GetComponent<Canvas>();
            if (_mage == null) { enabled = false; return; }
            Build();
        }
        private void Update()
        {
            if (_root == null || _mage == null) return;
            bool show = !GamePauseService.IsPaused && Time.timeScale > 0f;
            if (_root.activeSelf != show) _root.SetActive(show);
            if (!show) return;
            ChimeraCompanion pet = _mage.Companion;
            if (pet == null) return;
            _status.text = pet.Alive ? "THÚ  " + Mathf.CeilToInt(pet.HealthFraction * 100f) + "%"
                : "HỒI SINH  " + Mathf.CeilToInt(pet.RespawnRemaining) + "s";
            _baseImage.color = pet.IsManuallyControlled
                ? new Color(1f, 1f, 1f, .98f)
                : new Color(1f, 1f, 1f, .75f);
        }
        private void Build()
        {
            _root = new GameObject("ChimeraJoystick_Right", typeof(RectTransform), typeof(Image));
            _root.transform.SetParent(transform, false);
            RectTransform r = _root.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(1f, 0f);
            r.anchorMax = new Vector2(1f, 0f);
            r.pivot = new Vector2(.5f, .5f);
            r.sizeDelta = new Vector2(172f, 172f);
            r.anchoredPosition = new Vector2(-130f, 145f);
            _baseImage = _root.GetComponent<Image>();
            _baseImage.sprite = Resources.Load<Sprite>("UITheme/JoystickRing") ?? CircleSprite();
            _baseImage.raycastTarget = true;
            _baseImage.color = new Color(.90f, .98f, 1f, .88f);

            GameObject knob = new GameObject("Thumb", typeof(RectTransform), typeof(Image));
            knob.transform.SetParent(_root.transform, false);
            _thumb = knob.GetComponent<RectTransform>();
            _thumb.anchorMin = _thumb.anchorMax = new Vector2(.5f, .5f);
            _thumb.sizeDelta = new Vector2(76f, 76f);
            Image knobImg = knob.GetComponent<Image>();
            knobImg.sprite = CircleSprite();
            knobImg.color = new Color(.55f, .85f, 1f, .82f);
            knobImg.raycastTarget = false;

            GameObject label = new GameObject("PetHP", typeof(RectTransform));
            label.transform.SetParent(_root.transform, false);
            RectTransform l = label.GetComponent<RectTransform>();
            l.anchorMin = l.anchorMax = new Vector2(.5f, 1f);
            l.pivot = new Vector2(.5f, 0f);
            l.sizeDelta = new Vector2(172f, 40f);
            // Anchor the label fully INSIDE the 172px joystick, not above its frame.
            l.anchoredPosition = new Vector2(0f, -48f);
            _status = label.AddComponent<TextMeshProUGUI>();
            _status.alignment = TextAlignmentOptions.Center;
            _status.fontSize = 20f;
            _status.color = new Color(.87f, .83f, .98f, 1f);
            DienGameUITheme.StyleText(_status, 26f, DienGameUITheme.Ivory,
                true, false, TextAlignmentOptions.Center);

            MageJoystickPointer pointer = _root.AddComponent<MageJoystickPointer>();
            pointer.Bind(this);
        }

        public void OnPointerBegin(Vector2 position, Camera camera)
        {
            MovePointer(position, camera, true);
        }
        public void OnPointerDrag(Vector2 position, Camera camera)
        {
            MovePointer(position, camera, true);
        }
        public void OnPointerEnd()
        {
            if (_mage != null && _mage.Companion != null) _mage.Companion.SetManual(Vector2.zero, false);
            if (_thumb != null) _thumb.anchoredPosition = Vector2.zero;
        }
        private void MovePointer(Vector2 screenPoint, Camera camera, bool active)
        {
            if (_root == null || _mage == null || _mage.Companion == null) return;
            RectTransform rect = (RectTransform)_root.transform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, camera, out Vector2 local)) return;
            Vector2 direction = Vector2.ClampMagnitude(local / Radius, 1f);
            _thumb.anchoredPosition = direction * Radius;
            _mage.Companion.SetManual(direction, active);
        }

        private static Sprite s_circle;
        private static Sprite CircleSprite()
        {
            if (s_circle != null) return s_circle;
            const int dim = 96;
            Texture2D texture = new Texture2D(dim, dim, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < dim; y++)
            for (int x = 0; x < dim; x++)
            {
                float r = Vector2.Distance(new Vector2(x, y), new Vector2(47.5f, 47.5f)) / 47f;
                float alpha = Mathf.Clamp01((1.02f - r) * 22f);
                float edge = Mathf.Clamp01(1f - Mathf.Abs(r - .91f) * 40f);
                texture.SetPixel(x, y, new Color(.93f, .9f, 1f, alpha * (.14f + edge * .45f)));
            }
            texture.Apply();
            s_circle = Sprite.Create(texture, new Rect(0,0,dim,dim), new Vector2(.5f,.5f),100f);
            return s_circle;
        }
        private void OnDisable() => OnPointerEnd();
        private void OnDestroy() { if (_root != null) Destroy(_root); }
    }

    public sealed class MageJoystickPointer : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private MageCompanionJoystick _owner;
        private bool _pressed;
        public void Bind(MageCompanionJoystick joystick) => _owner = joystick;
        public void OnPointerDown(PointerEventData data)
        {
            _pressed = true;
            _owner?.OnPointerBegin(data.position, data.pressEventCamera);
        }
        public void OnDrag(PointerEventData data)
        {
            if (_pressed) _owner?.OnPointerDrag(data.position, data.pressEventCamera);
        }
        public void OnPointerUp(PointerEventData data)
        {
            _pressed = false;
            _owner?.OnPointerEnd();
        }
        private void OnDisable()
        {
            if (_pressed) _owner?.OnPointerEnd();
            _pressed = false;
        }
    }
}
