using System.Collections.Generic;
using UnityEngine;

namespace DienGame.Client
{
    [DisallowMultipleComponent]
    public sealed class LobbySafeAreaController : MonoBehaviour
    {
        private readonly Dictionary<RectTransform, Vector2> _basePositions = new();
        private Canvas _canvas;

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

            GetInsets(out _, out _, out float top, out float bottom);

            ApplyOffset("TopBar", new Vector2(0f, -top));
            ApplyOffset("StageInfo", new Vector2(0f, bottom));
            ApplyOffset("BottomNavigation", new Vector2(0f, bottom));
        }

        private void ApplyOffset(string objectName, Vector2 offset)
        {
            Transform found = FindChildRecursive(transform, objectName);
            if (found is not RectTransform rect)
            {
                return;
            }

            if (!_basePositions.TryGetValue(rect, out Vector2 basePosition))
            {
                basePosition = rect.anchoredPosition;
                _basePositions.Add(rect, basePosition);
            }

            rect.anchoredPosition = basePosition + offset;
        }

        private void GetInsets(out float left, out float right, out float top, out float bottom)
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
