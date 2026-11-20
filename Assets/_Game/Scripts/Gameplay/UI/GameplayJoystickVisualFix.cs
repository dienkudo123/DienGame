using DienGame.Client;
using UnityEngine;
using UnityEngine.UI;

namespace DienGame.Gameplay.UI
{
    /// <summary>Replace Unity's default gray joystick rectangles without touching Input System bindings.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public sealed class GameplayJoystickVisualFix : MonoBehaviour
    {
        private bool _applied;
        private static Sprite _knob;
        private static Sprite _ring;

        private void Start() => Apply();
        private void OnEnable() => _applied = false;
        private void LateUpdate()
        {
            if (!_applied) Apply();
        }

        private void Apply()
        {
            Transform found = transform.Find("MovementJoystick");
            if (found == null)
            {
                foreach (RectTransform rt in GetComponentsInChildren<RectTransform>(true))
                    if (rt.name == "MovementJoystick") { found = rt; break; }
            }
            if (found == null) return;
            Image ring = found.GetComponent<Image>();
            if (ring == null) ring = found.gameObject.AddComponent<Image>();
            Transform handle = found.Find("Handle");
            if (handle == null) return;
            Image thumb = handle.GetComponent<Image>();
            if (thumb == null) thumb = handle.gameObject.AddComponent<Image>();
            Sprite ringSprite = Resources.Load<Sprite>("UITheme/JoystickRing");
            if (ringSprite == null) ringSprite = Ring();
            if (ringSprite == null) return;
            ring.sprite = ringSprite;
            ring.type = Image.Type.Simple;
            ring.preserveAspect = true;
            ring.color = new Color(.88f, .98f, 1f, .84f);
            ring.raycastTarget = true;
            thumb.sprite = Knob();
            thumb.type = Image.Type.Simple;
            thumb.preserveAspect = true;
            thumb.color = new Color(.42f, .87f, .97f, .86f);
            thumb.raycastTarget = true;
            _applied = true;
        }

        // A fallback is generated at runtime if Resources were omitted from a build.
        // Prevents the gray square seen when Unity renders an Image without a sprite.
        private static Sprite Ring()
        {
            if (_ring != null) return _ring;
            const int n = 128;
            Texture2D texture = new Texture2D(n, n, TextureFormat.RGBA32, false);
            texture.name = "DienGameJoystickRingFallback";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float r = Vector2.Distance(new Vector2(x + .5f, y + .5f),
                    new Vector2(n * .5f, n * .5f)) / (n * .5f);
                float band = Mathf.Clamp01((r - .66f) * 40f) * Mathf.Clamp01((.98f - r) * 40f);
                float inner = (1f - Mathf.Clamp01(r / .68f)) * .10f;
                pixels[y * n + x] = new Color(.22f, .71f, .87f, band * .84f + inner);
            }
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            _ring = Sprite.Create(texture, new Rect(0, 0, n, n), new Vector2(.5f, .5f), 100f);
            return _ring;
        }

        private static Sprite Knob()
        {
            if (_knob != null) return _knob;
            const int n = 128;
            Texture2D texture = new Texture2D(n, n, TextureFormat.RGBA32, false);
            texture.name = "DienGameJoystickKnob";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[n*n];
            float radius = (n-2)*.5f;
            for (int y=0;y<n;y++)
            for (int x=0;x<n;x++)
            {
                float d = Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(n*.5f,n*.5f));
                float rim = Mathf.Clamp01((radius-d)*.7f);
                float glow = Mathf.Clamp01((d - radius*.72f)/(radius*.25f));
                pixels[y*n+x] = new Color(.12f+.30f*glow, .34f+.34f*glow, .49f+.36f*glow,
                    rim * (.83f + .17f*glow));
            }
            texture.SetPixels(pixels);
            texture.Apply(false,true);
            _knob = Sprite.Create(texture,new Rect(0,0,n,n),new Vector2(.5f,.5f),100f);
            return _knob;
        }
    }
}
