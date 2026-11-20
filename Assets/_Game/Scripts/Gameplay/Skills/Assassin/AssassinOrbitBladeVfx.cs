using UnityEngine;

namespace DienGame.Gameplay.Skills.Assassin
{
    /// <summary>Each circular drone spins itself AND orbits its owner. Art is one PNG for each tier.</summary>
    [DisallowMultipleComponent]
    public sealed class AssassinOrbitBladeVfx : MonoBehaviour
    {
        public const float OrbitSpeedRadians = 3.4f;
        private const float SelfSpinDegrees = 540f;
        private const int MaxCount = 3;
        private readonly SpriteRenderer[] _renderers = new SpriteRenderer[MaxCount];
        private bool _ready;
        private bool _visible;
        private int _count = 2;
        private bool _lv5;
        private float _phase, _radius = 1.15f, _time;

        public static Vector2 GetOrbitOffset(int index, int count, float radius, float elapsed)
        {
            float phase = index * (Mathf.PI * 2f / Mathf.Max(1, count)) + elapsed * OrbitSpeedRadians;
            return new Vector2(Mathf.Cos(phase), Mathf.Sin(phase)) * radius;
        }
        public void SetSkillLevel(int level)
        {
            int count = level >= 5 ? 3 : 2;
            bool lv5 = level >= 5;
            if (_ready && count == _count && lv5 == _lv5) return;
            _count = count;
            _lv5 = lv5;
            if (_visible) EnsureReady();
            Refresh();
        }
        public void SetVisible(bool value)
        {
            _visible = value;
            if (value) EnsureReady();
            Refresh();
        }
        public void Draw(float elapsed, float radius)
        {
            _phase = elapsed;
            _radius = Mathf.Max(.5f, radius);
        }
        private void EnsureReady()
        {
            Sprite[] frames = AssassinVfxBank.Frames(_lv5 ? AssassinVfxKind.OrbitLv5 : AssassinVfxKind.OrbitNormal);
            if (frames == null || frames.Length == 0) return;
            if (!_ready)
            {
                SpriteRenderer owner = GetComponentInChildren<SpriteRenderer>();
                for (int i = 0; i < MaxCount; i++)
                {
                    GameObject child = new GameObject("Orbiting blade " + (i + 1));
                    child.transform.SetParent(transform, false);
                    child.transform.localScale = Vector3.one * 1.05f;
                    SpriteRenderer sr = child.AddComponent<SpriteRenderer>();
                    sr.sortingLayerID = owner != null ? owner.sortingLayerID : 0;
                    sr.sortingOrder = owner != null ? owner.sortingOrder + 20 + i : 80 + i;
                    _renderers[i] = sr;
                }
                _ready = true;
            }
            for (int i = 0; i < MaxCount; i++) _renderers[i].sprite = frames[0];
        }
        private void LateUpdate()
        {
            if (!_visible || !_ready || Time.deltaTime <= 0f) return;
            _time += Time.deltaTime;
            for (int i = 0; i < _count; i++)
            {
                SpriteRenderer sr = _renderers[i];
                if (sr == null) continue;
                sr.transform.position = transform.position +
                    (Vector3)GetOrbitOffset(i, _count, _radius, _phase);
                sr.transform.rotation = Quaternion.Euler(0, 0, -_time * SelfSpinDegrees + i * (360f / _count));
            }
        }
        private void Refresh()
        {
            if (!_ready) return;
            for (int i = 0; i < MaxCount; i++)
                if (_renderers[i] != null) _renderers[i].enabled = _visible && i < _count;
        }
        private void OnDisable() { _visible = false; Refresh(); }
    }
}
