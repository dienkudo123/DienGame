using UnityEngine;

namespace DienGame.Gameplay.World
{
    /// <summary>
    /// Outline + low-opacity filled area: the warning footprint is precisely the damage area.
    /// Every warning is transient and owned by the returned LineRenderer object.
    /// </summary>
    public static class Stage11Telegraph
    {
        private static Material _lineMaterial;
        private static Sprite _circleFillSprite;
        private static Sprite _squareFillSprite;

        private static Material SharedMaterial
        {
            get
            {
                if (_lineMaterial == null)
                {
                    Shader shader = Shader.Find("Sprites/Default");
                    if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
                    if (shader != null)
                        _lineMaterial = new Material(shader) { name = "DienGame_Telegraph_Only" };
                }
                return _lineMaterial;
            }
        }

        public static LineRenderer Circle(Transform owner, Vector2 center, float radius, Color color)
        {
            LineRenderer lr = Make(owner, "Boss_Telegraph_Circle", color);
            const int points = 64;
            lr.loop = true;
            lr.positionCount = points;
            for (int i = 0; i < points; i++)
            {
                float t = i * 2f * Mathf.PI / points;
                lr.SetPosition(i, new Vector3(center.x + Mathf.Cos(t) * radius,
                    center.y + Mathf.Sin(t) * radius, 0f));
            }
            EnsureSprites();
            AddFill(lr.transform, "WarningArea", _circleFillSprite, center,
                new Vector2(radius, radius), 0f, color);
            return lr;
        }

        public static LineRenderer Strip(Transform owner, Vector2 a, Vector2 b,
            float halfWidth, Color color)
        {
            LineRenderer lr = Make(owner, "Boss_Telegraph_Lane", color);
            Vector2 delta = b - a;
            Vector2 normal = new Vector2(-delta.y, delta.x).normalized * halfWidth;
            lr.positionCount = 4;
            lr.loop = true;
            lr.SetPosition(0, a + normal);
            lr.SetPosition(1, b + normal);
            lr.SetPosition(2, b - normal);
            lr.SetPosition(3, a - normal);
            EnsureSprites();
            AddFill(lr.transform, "WarningLane", _squareFillSprite, (a + b) * .5f,
                new Vector2(delta.magnitude, halfWidth * 2f),
                Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg, color);
            return lr;
        }

        private static LineRenderer Make(Transform owner, string name, Color color)
        {
            GameObject go = new GameObject(name);
            if (owner != null) go.transform.SetParent(owner, true);
            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.widthMultiplier = .085f;
            lr.startColor = color;
            lr.endColor = color;
            lr.sortingOrder = 45;
            if (SharedMaterial != null) lr.sharedMaterial = SharedMaterial;
            return lr;
        }

        private static void AddFill(Transform parent, string name, Sprite sprite,
            Vector2 center, Vector2 size, float rotation, Color color)
        {
            if (sprite == null) return;
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, true);
            go.transform.position = new Vector3(center.x, center.y, 0f);
            go.transform.rotation = Quaternion.Euler(0f, 0f, rotation);
            // Circle sprite bounds are 2x2; the square sprite bounds are 1x1.
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            color.a = .19f;
            renderer.color = color;
            renderer.sortingOrder = 44;
        }

        private static void EnsureSprites()
        {
            if (_squareFillSprite == null)
                _squareFillSprite = Sprite.Create(Texture2D.whiteTexture,
                    new Rect(0f, 0f, 1f, 1f), new Vector2(.5f, .5f), 1f);
            if (_circleFillSprite != null) return;
            const int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color32[] pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = (x + .5f - size * .5f) / (size * .5f);
                float dy = (y + .5f - size * .5f) / (size * .5f);
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                byte a = (byte)Mathf.RoundToInt(255f * Mathf.Clamp01((1.0f - d) * 32f));
                pixels[y * size + x] = new Color32(255, 255, 255, a);
            }
            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            _circleFillSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
                new Vector2(.5f, .5f), size * .5f);
        }
    }
}
