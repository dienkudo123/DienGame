using UnityEngine;

namespace DienGame.Gameplay.Skills.Assassin
{
    /// <summary>
    /// Code-generated VFX for Anh Phan Than. No PNGs / Inspector setup required.
    /// Summon: violet ground ripple. Lv5 finisher: two bright curved slashes.
    /// VFX are cosmetic and have no collision or damage.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AssassinCloneSkillVfx : MonoBehaviour
    {
        private enum EffectKind { Spawn, Finisher }

        private const int RingSegments = 40;
        private const int SlashSegments = 28;

        private EffectKind _kind;
        private float _elapsed;
        private float _duration;
        private float _range;
        private LineRenderer _main;
        private LineRenderer _secondary;
        private Material _material;

        public static void PlaySpawn(Vector2 point, SpriteRenderer sprite)
        {
            Create(point, Vector2.right, sprite, EffectKind.Spawn);
        }

        public static void PlayFinisher(Vector2 point, Vector2 facing,
            SpriteRenderer sprite)
        {
            Create(point, facing, sprite, EffectKind.Finisher);
        }

        private static void Create(Vector2 point, Vector2 facing,
            SpriteRenderer sprite, EffectKind kind)
        {
            GameObject obj = new GameObject(kind == EffectKind.Spawn
                ? "VFX - Anh Phan Than Summon"
                : "VFX - Anh Phan Than Lv5 Finisher");
            obj.transform.position = new Vector3(point.x, point.y, 0f);
            if (kind == EffectKind.Finisher && facing.sqrMagnitude > 0.0001f)
            {
                float angle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
                obj.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            AssassinCloneSkillVfx effect = obj.AddComponent<AssassinCloneSkillVfx>();
            effect.Setup(kind, sprite);
        }

        private void Setup(EffectKind kind, SpriteRenderer source)
        {
            _kind = kind;
            _duration = kind == EffectKind.Spawn ? 0.28f : 0.25f;
            _elapsed = 0f;
            _range = kind == EffectKind.Spawn ? 0.62f : 1.48f;

            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null)
            {
                Debug.LogWarning("AssassinCloneSkillVfx: no 2D VFX shader available.");
                Destroy(gameObject);
                return;
            }

            _material = new Material(shader) { name = "AssassinClone_RuntimeVFX" };
            int sortLayer = source != null ? source.sortingLayerID : 0;
            int sortOrder = source != null ? source.sortingOrder + 8 : 30;

            _main = CreateLine("Primary", 0.14f, sortLayer, sortOrder + 1);
            _secondary = CreateLine("Secondary", 0.065f, sortLayer, sortOrder);

            if (kind == EffectKind.Spawn)
            {
                BuildRing(_main, 0.53f, 0.25f);
                BuildRing(_secondary, 0.38f, 0.17f);
            }
            else
            {
                BuildSlash(_main, _range, 0f);
                BuildSlash(_secondary, _range * 0.83f, -0.16f);
            }
        }

        private LineRenderer CreateLine(string objectName, float width,
            int sortingLayerId, int sortingOrder)
        {
            GameObject child = new GameObject(objectName);
            child.transform.SetParent(transform, false);
            LineRenderer line = child.AddComponent<LineRenderer>();
            line.sharedMaterial = _material;
            line.useWorldSpace = false;
            line.loop = false;
            line.numCapVertices = 4;
            line.numCornerVertices = 4;
            line.widthMultiplier = width;
            line.sortingLayerID = sortingLayerId;
            line.sortingOrder = sortingOrder;
            return line;
        }

        private static void BuildRing(LineRenderer line, float xRadius,
            float yRadius)
        {
            if (line == null) return;
            line.positionCount = RingSegments + 1;
            for (int i = 0; i <= RingSegments; i++)
            {
                float phase = (i / (float)RingSegments) * Mathf.PI * 2f;
                line.SetPosition(i, new Vector3(Mathf.Cos(phase) * xRadius,
                    Mathf.Sin(phase) * yRadius, 0f));
            }
        }

        private static void BuildSlash(LineRenderer line, float radius,
            float sidewaysOffset)
        {
            if (line == null) return;
            line.positionCount = SlashSegments + 1;
            for (int i = 0; i <= SlashSegments; i++)
            {
                float degrees = Mathf.Lerp(-88f, 88f,
                    i / (float)SlashSegments);
                float phase = degrees * Mathf.Deg2Rad;
                float adjustedRadius = radius +
                    0.045f * Mathf.Sin(i * Mathf.PI / SlashSegments);
                line.SetPosition(i, new Vector3(
                    Mathf.Cos(phase) * adjustedRadius,
                    Mathf.Sin(phase) * adjustedRadius + sidewaysOffset,
                    0f));
            }
        }

        private void Update()
        {
            if (_main == null || _secondary == null)
            {
                Destroy(gameObject);
                return;
            }
            if (Time.deltaTime <= 0f) return;

            _elapsed += Time.deltaTime;
            if (_elapsed >= _duration)
            {
                Destroy(gameObject);
                return;
            }

            float t = Mathf.Clamp01(_elapsed / _duration);
            float alpha = Mathf.Sin(Mathf.PI * t);
            if (_kind == EffectKind.Spawn)
            {
                transform.localScale = Vector3.one * Mathf.Lerp(0.50f, 1.55f, t);
                SetColors(_main, new Color(0.72f, 0.40f, 1f, alpha * 0.92f));
                SetColors(_secondary, new Color(1f, 0.83f, 1f, alpha * 0.78f));
            }
            else
            {
                transform.localScale = Vector3.one * Mathf.Lerp(0.78f, 1.12f, t);
                SetColors(_main, new Color(0.84f, 0.42f, 1f, alpha));
                SetColors(_secondary, new Color(1f, 0.95f, 1f, alpha * 0.90f));
            }
        }

        private static void SetColors(LineRenderer line, Color color)
        {
            line.startColor = color;
            line.endColor = color;
        }

        private void OnDestroy()
        {
            if (_material != null) Destroy(_material);
        }
    }
}
