using UnityEngine;

namespace DienGame.Client
{
    /// <summary>
    /// Keeps only overlay CONTENT within the device safe area. The dimmed backdrop
    /// stays full-screen, so notches never expose the game behind a modal.
    /// Attach to a stretched RectTransform; no scene/Inspector wiring needed.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class DienGameSafeAreaRoot : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect _lastSafe;
        private Vector2Int _lastScreen;

        private void Awake() => _rect = (RectTransform)transform;

        private void OnEnable() => Apply(force: true);

        private void LateUpdate() => Apply(force: false);

        private void Apply(bool force)
        {
            if (_rect == null) _rect = (RectTransform)transform;
            if (Screen.width <= 0 || Screen.height <= 0) return;
            Rect safe = Screen.safeArea;
            Vector2Int size = new Vector2Int(Screen.width, Screen.height);
            if (!force && safe == _lastSafe && size == _lastScreen) return;
            _lastSafe = safe;
            _lastScreen = size;

            Vector2 a = new Vector2(Mathf.Clamp01(safe.xMin / size.x),
                Mathf.Clamp01(safe.yMin / size.y));
            Vector2 b = new Vector2(Mathf.Clamp01(safe.xMax / size.x),
                Mathf.Clamp01(safe.yMax / size.y));
            // In editor and some WebGL embeds Screen.safeArea may return invalid values.
            if (b.x <= a.x || b.y <= a.y) { a = Vector2.zero; b = Vector2.one; }
            _rect.anchorMin = a;
            _rect.anchorMax = b;
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}
