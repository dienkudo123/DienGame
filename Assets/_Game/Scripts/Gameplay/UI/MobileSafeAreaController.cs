using System.Collections.Generic;
using UnityEngine;

namespace DienGame.Gameplay.UI
{
    /// <summary>
    /// Tự đẩy các HUD quan trọng khỏi notch / home indicator.
    /// Không cần chỉnh scene hay kéo reference trong Inspector.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MobileSafeAreaController : MonoBehaviour
    {
        private enum EdgeMode
        {
            TopLeft,
            TopCenter,
            TopRight,
            BottomLeft,
            BottomRight,
        }

        private readonly Dictionary<RectTransform, Vector2> _basePositions = new();
        private Canvas _canvas;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        private void LateUpdate()
        {
            if (_canvas == null)
            {
                return;
            }

            Vector2Int screenSize = new(Screen.width, Screen.height);
            Rect safe = Screen.safeArea;

            bool changed =
                safe != _lastSafeArea ||
                screenSize != _lastScreenSize;

            // Runtime UI như nút ULT có thể xuất hiện sau Awake, nên vẫn scan nhẹ.
            ApplyToNamed("GameplayHUDRoot", EdgeMode.TopLeft);
            ApplyToNamed("MovementJoystick", EdgeMode.BottomLeft);
            ApplyToNamed("JudgementUltimateButton", EdgeMode.BottomRight);
            ApplyToNamed("RunTimer", EdgeMode.TopCenter);
            ApplyToNamed("BossHealthBar", EdgeMode.TopCenter);
            ApplyToNamed("PauseButton", EdgeMode.TopRight);

            if (changed)
            {
                _lastSafeArea = safe;
                _lastScreenSize = screenSize;
            }
        }

        private void ApplyToNamed(string objectName, EdgeMode mode)
        {
            Transform found = transform.Find(objectName);

            if (found == null)
            {
                found = FindChildRecursive(transform, objectName);
            }

            if (found is not RectTransform rect)
            {
                return;
            }

            if (!_basePositions.TryGetValue(rect, out Vector2 basePosition))
            {
                basePosition = rect.anchoredPosition;
                _basePositions.Add(rect, basePosition);
            }

            GetInsets(out float left, out float right, out float top, out float bottom);

            Vector2 offset = mode switch
            {
                EdgeMode.TopLeft => new Vector2(left, -top),
                EdgeMode.TopCenter => new Vector2(0f, -top),
                EdgeMode.TopRight => new Vector2(-right, -top),
                EdgeMode.BottomLeft => new Vector2(left, bottom),
                EdgeMode.BottomRight => new Vector2(-right, bottom),
                _ => Vector2.zero,
            };

            rect.anchoredPosition = basePosition + offset;
        }

        private void GetInsets(
            out float left,
            out float right,
            out float top,
            out float bottom)
        {
            Rect safe = Screen.safeArea;
            float scale = Mathf.Max(0.001f, _canvas.scaleFactor);

            left = safe.xMin / scale;
            right = (Screen.width - safe.xMax) / scale;
            bottom = safe.yMin / scale;
            top = (Screen.height - safe.yMax) / scale;
        }

        private static Transform FindChildRecursive(Transform root, string objectName)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);

                if (child.name == objectName)
                {
                    return child;
                }

                Transform nested = FindChildRecursive(child, objectName);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }
    }
}
