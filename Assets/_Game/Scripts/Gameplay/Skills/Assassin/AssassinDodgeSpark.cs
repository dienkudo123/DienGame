using UnityEngine;

namespace DienGame.Gameplay.Skills.Assassin
{
    /// <summary>Small cyan speed streak on a successful dodge, WITHOUT a player ghost/afterimage.</summary>
    public sealed class AssassinDodgeSpark : MonoBehaviour
    {
        private static Material s_material;
        private readonly LineRenderer[] _lines = new LineRenderer[3];
        private float _remaining;
        private bool _lv5;
        public static void Spawn(Transform owner, bool levelFive)
        {
            if (owner == null) return;
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) return;
            if (s_material == null) s_material = new Material(shader) { name = "AssassinDodge_Cyan" };
            GameObject obj = new GameObject("Tan Anh - Speed Streak");
            obj.transform.SetParent(owner, false);
            obj.transform.localPosition = Vector3.zero;
            AssassinDodgeSpark fx = obj.AddComponent<AssassinDodgeSpark>();
            fx._remaining = levelFive ? .25f : .15f;
            fx._lv5 = levelFive;
            for (int i = 0; i < fx._lines.Length; i++)
            {
                GameObject stroke = new GameObject("Cyan Streak " + i);
                stroke.transform.SetParent(obj.transform, false);
                LineRenderer line = stroke.AddComponent<LineRenderer>();
                line.sharedMaterial = s_material;
                line.useWorldSpace = false;
                line.positionCount = 2;
                line.widthMultiplier = levelFive ? .08f : .05f;
                line.sortingOrder = 95;
                float y = (i - 1) * .22f;
                line.SetPosition(0, new Vector3(-.65f, y, 0));
                line.SetPosition(1, new Vector3(.0f + i * .11f, y + .08f, 0));
                fx._lines[i] = line;
            }
        }
        private void Update()
        {
            _remaining -= Time.deltaTime;
            if (_remaining <= 0f) { Destroy(gameObject); return; }
            float a = Mathf.Clamp01(_remaining / (_lv5 ? .25f : .15f));
            Color start = new Color(.6f, .96f, 1f, a);
            Color end = new Color(.11f, .67f, 1f, a * .15f);
            for (int i = 0; i < _lines.Length; i++)
            {
                if (_lines[i] == null) continue;
                _lines[i].startColor = start;
                _lines[i].endColor = end;
            }
        }
    }
}
