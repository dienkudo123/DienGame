using System.Collections.Generic;
using UnityEngine;

namespace DienGame.Gameplay.Equipment
{
    /// <summary>
    /// Non-destructive equipment evolution art. Tier variants are separate PNGs in Resources:
    /// no original sprites or atlases are overwritten. Visual evolution has NO combat effect.
    /// </summary>
    public static class EquipmentEvolutionArt
    {
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();
        private static Sprite Get(string path)
        {
            if (!Cache.TryGetValue(path, out Sprite sprite))
            {
                sprite = Resources.Load<Sprite>(path);
                Cache.Add(path, sprite);
            }
            return sprite;
        }

        public static Sprite IconFor(EquipmentDefinition item, int level = -1)
        {
            if (item == null) return null;
            int stage = EquipmentResonance.EvolutionStage(level < 0 ? EquipmentInventory.Level(item) : level);
            Sprite evolved = stage > 0 ? Get("Equipment/Evolution/Icons/" + item.Id + "_T" + stage) : null;
            return evolved != null ? evolved : EquipmentCatalog.LoadIcon(item);
        }

        public static Sprite ProjectileFor(EquipmentDefinition item)
        {
            if (item == null) return null;
            int stage = EquipmentResonance.EvolutionStage(EquipmentInventory.Level(item));
            Sprite evolved = stage > 0 ? Get("Equipment/Evolution/Projectiles/" + item.Id + "_T" + stage) : null;
            return evolved != null ? evolved : EquipmentCatalog.LoadWorldSprite(item);
        }

        public static Sprite RingForStage(int stage) => stage > 0 && stage <= 3
            ? Get("Equipment/Evolution/Rings/Core_T" + stage) : null;

        public static EquipmentCoreAura AddWorldAura(Transform parent, EquipmentDefinition item,
            SpriteRenderer reference, float worldScale = 1f, Vector3 localOffset = default(Vector3))
        {
            if (item == null || reference == null || parent == null) return null;
            int stage = EquipmentResonance.EvolutionStage(EquipmentInventory.Level(item));
            Sprite ring = RingForStage(stage);
            if (ring == null) return null;
            GameObject go = new GameObject("EvolutionCoreAura_T" + stage);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localOffset;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one * Mathf.Max(.1f, worldScale);
            SpriteRenderer r = go.AddComponent<SpriteRenderer>();
            r.sprite = ring;
            r.sortingLayerID = reference.sortingLayerID;
            r.sortingOrder = reference.sortingOrder - 1;
            r.color = new Color(1f, 1f, 1f, stage == 1 ? .34f : stage == 2 ? .46f : .57f);
            EquipmentCoreAura pulse = go.AddComponent<EquipmentCoreAura>();
            pulse.SetBaseScale(Mathf.Max(.1f, worldScale));
            return pulse;
        }
    }

    /// <summary>Small cosmetic breathing animation. Never modifies colliders, skills or statistics.</summary>
    public sealed class EquipmentCoreAura : MonoBehaviour
    {
        private float _scale = 1f;
        private float _age;
        private SpriteRenderer _visual;
        private float _baseAlpha;
        public void SetBaseScale(float value) { _scale = value; }
        private void Awake()
        {
            _visual = GetComponent<SpriteRenderer>();
            _baseAlpha = _visual != null ? _visual.color.a : 0f;
        }
        private void Update()
        {
            _age += Time.deltaTime;
            // Subtle idle pulse, no flashing and no heap allocation per frame.
            float breath = 1f + .055f * Mathf.Sin(_age * 2.6f);
            transform.localScale = Vector3.one * (_scale * breath);
            if (_visual != null)
            {
                Color c = _visual.color;
                c.a = _baseAlpha * (.91f + .09f * Mathf.Sin(_age * 2.6f));
                _visual.color = c;
            }
        }
    }
}
