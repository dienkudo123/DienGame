using UnityEngine;

namespace DienGame.Gameplay.Skills.Assassin
{
    /// <summary>Short cosmetic afterimage when Tàn Ảnh successfully dodges.</summary>
    public sealed class AssassinDodgeGhost : MonoBehaviour
    {
        private SpriteRenderer _sprite;
        private float _time;

        public static void Spawn(SpriteRenderer source)
        {
            if (source == null || source.sprite == null) return;
            GameObject obj = new GameObject("Assassin Dodge Afterimage");
            obj.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
            obj.transform.localScale = source.transform.lossyScale;
            SpriteRenderer sprite = obj.AddComponent<SpriteRenderer>();
            sprite.sprite = source.sprite;
            sprite.flipX = source.flipX;
            sprite.flipY = source.flipY;
            sprite.sortingLayerID = source.sortingLayerID;
            sprite.sortingOrder = source.sortingOrder - 1;
            sprite.color = new Color(.5f, .28f, 1f, .5f);
            AssassinDodgeGhost ghost = obj.AddComponent<AssassinDodgeGhost>();
            ghost._sprite = sprite;
            ghost._time = .15f;
        }

        private void Update()
        {
            _time -= Time.deltaTime;
            if (_time <= 0f) { Destroy(gameObject); return; }
            Color c = _sprite.color;
            c.a = Mathf.Clamp01(_time / .15f) * .5f;
            _sprite.color = c;
        }
    }
}
