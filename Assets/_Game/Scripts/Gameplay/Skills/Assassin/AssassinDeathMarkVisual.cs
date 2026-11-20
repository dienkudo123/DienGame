using DienGame.Gameplay.Enemy;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Assassin
{
    /// <summary>Persistent red X above a marked enemy; updated when each normal hit adds a stack.</summary>
    public sealed class AssassinDeathMarkVisual : MonoBehaviour
    {
        private SpriteRenderer _image;
        private int _stacks;
        private float _time;
        public static void Show(EnemyHealth enemy, int stacks, int level)
        {
            if (enemy == null) return;
            AssassinDeathMarkVisual mark = enemy.GetComponent<AssassinDeathMarkVisual>();
            if (mark == null) mark = enemy.gameObject.AddComponent<AssassinDeathMarkVisual>();
            mark._stacks = stacks;
            mark.EnsureRenderer();
            Sprite[] frames = AssassinVfxBank.Frames(AssassinVfxKind.DeathMark);
            if (frames == null || frames.Length == 0 || mark._image == null) return;
            int frame = level >= 5 ? (stacks == 1 ? 3 : 5) :
                (stacks == 1 ? 2 : stacks == 2 ? 4 : 5);
            mark._image.sprite = frames[Mathf.Min(frame, frames.Length - 1)];
            mark._image.transform.localScale = Vector3.one * (level >= 5 ? .95f : .68f);
        }
        public static void Clear(EnemyHealth enemy)
        {
            if (enemy == null) return;
            AssassinDeathMarkVisual mark = enemy.GetComponent<AssassinDeathMarkVisual>();
            if (mark != null) Destroy(mark);
        }
        private void EnsureRenderer()
        {
            if (_image != null) return;
            GameObject visual = new GameObject("Tu An - X above enemy");
            visual.transform.SetParent(transform, false);
            visual.transform.localPosition = new Vector3(0, .50f, 0);
            _image = visual.AddComponent<SpriteRenderer>();
            SpriteRenderer enemyArt = GetComponentInChildren<SpriteRenderer>();
            _image.sortingLayerID = enemyArt != null ? enemyArt.sortingLayerID : 0;
            _image.sortingOrder = enemyArt != null ? enemyArt.sortingOrder + 20 : 85;
        }
        private void Update()
        {
            if (_image == null) return;
            _time += Time.deltaTime;
            Color color = _image.color;
            color.a = .78f + .22f * Mathf.Abs(Mathf.Sin(_time * 7f));
            _image.color = color;
        }
        private void OnDestroy()
        {
            if (_image != null) Destroy(_image.gameObject);
        }
    }
}
