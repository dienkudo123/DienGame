using DienGame.Client;
using DienGame.Gameplay.Equipment;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Assassin
{
    /// <summary>Stationary melee slash VFX. Damage and attack timing remain in AssassinMeleeAttack.</summary>
    public sealed class AssassinCrescentVfx : MonoBehaviour
    {
        private AssassinSkillController _skills;
        private SpriteRenderer _source;
        private bool _forceLv5;
        private void Awake()
        {
            _skills = GetComponent<AssassinSkillController>();
            AssassinSpriteAnimator animator = GetComponentInChildren<AssassinSpriteAnimator>(true);
            _source = animator != null ? animator.GetComponent<SpriteRenderer>() : null;
        }
        public void SetLevelFive(bool value) { _forceLv5 = value; }
        public void ShowSlash(Vector2 facing)
        {
            if (facing.sqrMagnitude < .0001f) return;
            if (_skills == null) _skills = GetComponent<AssassinSkillController>();
            bool lv5 = _forceLv5 || (_skills != null &&
                _skills.Level(DienGame.Gameplay.Skills.SkillId.AssassinBloodSlash) >= 5);
            Vector2 point = (Vector2)transform.position + facing.normalized * .75f;
            AssassinVfxBank.Play(lv5 ? AssassinVfxKind.MeleeLv5 : AssassinVfxKind.MeleeNormal,
                point, facing, _source, 2.2f, .15f);
            EquipmentDefinition item = EquipmentInventory.Selected(PlayableClass.Assassin);
            if (item == null) return;
            Sprite skin = EquipmentEvolutionArt.IconFor(item);
            if (skin == null) return;
            GameObject go = new GameObject("Equipment_Slash_" + item.Id);
            go.transform.position = point;
            float rotation = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg - 45f;
            go.transform.rotation = Quaternion.Euler(0f, 0f, rotation);
            go.transform.localScale = Vector3.one * .28f;
            SpriteRenderer r = go.AddComponent<SpriteRenderer>();
            r.sprite = skin;
            r.color = EquipmentResonance.VisualTint(item);
            r.sortingOrder = _source != null ? _source.sortingOrder + 12 : 82;
            EquipmentEvolutionArt.AddWorldAura(go.transform, item, r, 1.0f);
            go.AddComponent<EquipmentSlashFade>();
        }
    }

    /// <summary>Short decorative flash; damage remains owned by AssassinMeleeAttack.</summary>
    public sealed class EquipmentSlashFade : MonoBehaviour
    {
        private SpriteRenderer _sprite;
        private float _age;
        private void Awake() => _sprite = GetComponent<SpriteRenderer>();
        private void Update()
        {
            _age += Time.deltaTime;
            if (_age >= .16f) { Destroy(gameObject); return; }
            if (_sprite != null)
            {
                Color color = _sprite.color;
                color.a = 1f - _age / .16f;
                _sprite.color = color;
            }
        }
    }
}
