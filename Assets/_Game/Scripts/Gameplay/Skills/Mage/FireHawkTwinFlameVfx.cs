using UnityEngine;

namespace DienGame.Gameplay.Skills.Mage
{
    /// <summary>
    /// Only visual objects (no extra colliders, no damage). Two illustrated fire bolts
    /// follow the targets while the impact and actual damage are timed by the signature.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FireHawkTwinFlameVfx : MonoBehaviour
    {
        private Vector2 _origin;
        private Vector2 _target;
        private float _duration;
        private float _elapsed;
        private bool _evolved;
        private SpriteRenderer _renderer;

        public static void Launch(Vector2 origin, Vector2 target, bool evolved, int side)
        {
            Vector2 delta = target - origin;
            Vector2 forward = delta.sqrMagnitude > .0001f ? delta.normalized : Vector2.right;
            Vector2 across = new Vector2(-forward.y, forward.x);
            Vector2 start = origin + forward * .22f + across * (side == 0 ? -.14f : .14f);
            GameObject obj = new GameObject("FireHawk_SongTrung_OanhKich_Flame");
            obj.transform.position = start;
            FireHawkTwinFlameVfx fx = obj.AddComponent<FireHawkTwinFlameVfx>();
            fx._origin = start;
            fx._target = target;
            fx._duration = evolved ? .28f : .33f;
            fx._evolved = evolved;
            fx._renderer = obj.AddComponent<SpriteRenderer>();
            fx._renderer.sortingOrder = 46;
            fx._renderer.sprite = FireHawkFinalArt.Flame(0);
            float angle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;
            obj.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            obj.transform.localScale = Vector3.one * (evolved ? .52f : .37f);
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / Mathf.Max(.01f, _duration));
            transform.position = Vector2.Lerp(_origin, _target, t * t * (3f - 2f * t));
            if (_renderer != null)
            {
                _renderer.sprite = FireHawkFinalArt.Flame(Mathf.Min(7, Mathf.FloorToInt(t * 8f)));
                Color color = Color.white;
                color.a = Mathf.Clamp01((1f - t) * 1.7f);
                _renderer.color = color;
            }
            float pulse = 1f + Mathf.Sin(t * 10f) * .12f;
            transform.localScale = Vector3.one * (_evolved ? .52f : .37f) * pulse;
            if (t >= 1f) Destroy(gameObject);
        }
    }
}
