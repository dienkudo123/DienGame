using UnityEngine;

namespace DienGame.Gameplay.Skills.Mage
{
    /// <summary>Short, painted orange flame burst; separate visual style at Lv1/Lv5.</summary>
    [DisallowMultipleComponent]
    public sealed class FireHawkImpactAnimation : MonoBehaviour
    {
        private float _time;
        private bool _level5;
        private SpriteRenderer _visual;

        public static void Spawn(Vector2 center, bool level5)
        {
            GameObject go = new GameObject("FireHawk_FlameImpact_" + (level5 ? "Lv5" : "Lv1"));
            go.transform.position = center;
            FireHawkImpactAnimation effect = go.AddComponent<FireHawkImpactAnimation>();
            effect._level5 = level5;
            effect._visual = go.AddComponent<SpriteRenderer>();
            effect._visual.sortingOrder = 48;
            effect._visual.sprite = FireHawkFinalArt.Flame(0);
            go.transform.localScale = Vector3.one * (level5 ? .7f : .46f);
        }

        private void Update()
        {
            _time += Time.deltaTime;
            float t = Mathf.Clamp01(_time / (_level5 ? .46f : .34f));
            if (_visual != null)
            {
                _visual.sprite = FireHawkFinalArt.Flame(Mathf.Min(7, Mathf.FloorToInt(t * 8f)));
                _visual.color = new Color(1f, 1f, 1f, 1f - t);
            }
            float size = (_level5 ? .7f : .46f) * (1f + .72f * t);
            transform.localScale = Vector3.one * size;
            transform.Rotate(0f, 0f, Time.deltaTime * (_level5 ? 98f : 65f));
            if (t >= 1f) Destroy(gameObject);
        }
    }
}
