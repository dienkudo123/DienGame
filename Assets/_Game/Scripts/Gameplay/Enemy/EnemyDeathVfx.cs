using System.Collections.Generic;
using UnityEngine;

namespace DienGame.Gameplay.Enemy
{
    public sealed class EnemyDeathVfx : MonoBehaviour
    {
        private static readonly Stack<EnemyDeathVfx> Pool = new();
        private static Material s_sharedMaterial;
        private static Transform s_poolRoot;

        private readonly List<LineRenderer> _lines = new();
        private readonly List<Color> _baseColors = new();
        private float _lifetime;
        private float _duration;
        private bool _inPool;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Pool.Clear();
            s_sharedMaterial = null;
            s_poolRoot = null;
        }

        public static void SpawnHit(
            Vector3 position,
            Color color,
            float radius)
        {
            EnemyDeathVfx vfx = Acquire("Enemy_HitVFX", position);
            vfx.BuildHit(color, Mathf.Max(0.15f, radius));
        }

        public static void SpawnDeath(
            Vector3 position,
            Color color,
            float radius,
            bool boss,
            bool elite)
        {
            EnemyDeathVfx vfx = Acquire(
                boss ? "Boss_DeathVFX" : "Enemy_DeathVFX",
                position
            );

            vfx.BuildDeath(
                color,
                Mathf.Max(0.2f, radius),
                boss,
                elite
            );
        }

        private void Update()
        {
            if (_duration <= 0f)
            {
                return;
            }

            _lifetime -= Time.deltaTime;
            float progress = 1f - Mathf.Clamp01(_lifetime / _duration);
            float scale = Mathf.Lerp(0.72f, 1.55f, progress);
            float fade = 1f - progress;

            for (int i = 0; i < _lines.Count; i++)
            {
                LineRenderer line = _lines[i];
                if (line == null || !line.gameObject.activeSelf)
                {
                    continue;
                }

                line.transform.localScale = Vector3.one * scale;

                Color baseColor = i < _baseColors.Count
                    ? _baseColors[i]
                    : Color.white;

                Color faded = baseColor;
                faded.a *= fade;
                line.startColor = faded;

                Color end = faded;
                end.a *= 0.65f;
                line.endColor = end;
            }

            if (_lifetime <= 0f)
            {
                Release();
            }
        }

        private void BuildHit(Color color, float radius)
        {
            _duration = 0.16f;
            _lifetime = _duration;

            EnsureLineCount(1);
            DisableAllLines();

            ConfigureRing(
                _lines[0],
                radius * 0.5f,
                Mathf.Max(0.025f, radius * 0.055f),
                color,
                0.78f
            );
        }

        private void BuildDeath(
            Color color,
            float radius,
            bool boss,
            bool elite)
        {
            _duration = boss ? 0.72f : elite ? 0.52f : 0.38f;
            _lifetime = _duration;

            int rayCount = boss ? 14 : elite ? 10 : 7;
            int total = rayCount + 2;

            EnsureLineCount(total);
            DisableAllLines();

            ConfigureRing(
                _lines[0],
                radius * 0.55f,
                Mathf.Max(0.035f, radius * 0.055f),
                color,
                0.9f
            );

            ConfigureRing(
                _lines[1],
                radius * 0.28f,
                Mathf.Max(0.02f, radius * 0.03f),
                Color.white,
                0.8f
            );

            for (int i = 0; i < rayCount; i++)
            {
                float angle = i * 360f / rayCount + Random.Range(-8f, 8f);
                float length = radius * Random.Range(0.65f, 1.15f);

                ConfigureRay(
                    _lines[i + 2],
                    angle,
                    length,
                    Mathf.Max(0.025f, radius * 0.05f),
                    color,
                    0.9f
                );
            }
        }

        private void EnsureLineCount(int required)
        {
            while (_lines.Count < required)
            {
                GameObject child = new GameObject("VfxLine");
                child.transform.SetParent(transform, false);

                LineRenderer line = child.AddComponent<LineRenderer>();
                line.useWorldSpace = false;
                line.material = GetSharedMaterial();
                line.sortingOrder = 50;

                _lines.Add(line);
                _baseColors.Add(Color.white);
            }
        }

        private void DisableAllLines()
        {
            for (int i = 0; i < _lines.Count; i++)
            {
                LineRenderer line = _lines[i];
                if (line == null)
                {
                    continue;
                }

                line.gameObject.SetActive(false);
                line.transform.localPosition = Vector3.zero;
                line.transform.localRotation = Quaternion.identity;
                line.transform.localScale = Vector3.one;
            }
        }

        private void ConfigureRing(
            LineRenderer line,
            float radius,
            float width,
            Color color,
            float alpha)
        {
            line.gameObject.SetActive(true);
            line.loop = true;
            line.positionCount = 32;
            line.startWidth = width;
            line.endWidth = width;

            Color c = color;
            c.a = alpha;
            SetBaseColor(line, c);

            for (int i = 0; i < 32; i++)
            {
                float angle = i * Mathf.PI * 2f / 32f;
                line.SetPosition(
                    i,
                    new Vector3(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(angle) * radius * 0.68f,
                        0f
                    )
                );
            }
        }

        private void ConfigureRay(
            LineRenderer line,
            float angle,
            float length,
            float width,
            Color color,
            float alpha)
        {
            line.gameObject.SetActive(true);
            line.loop = false;
            line.positionCount = 2;
            line.startWidth = width;
            line.endWidth = 0f;
            line.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            line.SetPosition(0, Vector3.zero);
            line.SetPosition(1, new Vector3(length, 0f, 0f));

            Color c = color;
            c.a = alpha;
            SetBaseColor(line, c);
        }

        private void SetBaseColor(LineRenderer line, Color color)
        {
            int index = _lines.IndexOf(line);
            if (index >= 0)
            {
                _baseColors[index] = color;
            }

            line.startColor = color;
            Color end = color;
            end.a *= 0.65f;
            line.endColor = end;
        }

        private static EnemyDeathVfx Acquire(string objectName, Vector3 position)
        {
            EnemyDeathVfx vfx = null;

            while (Pool.Count > 0 && vfx == null)
            {
                vfx = Pool.Pop();
            }

            if (vfx == null)
            {
                GameObject go = new GameObject(objectName);
                vfx = go.AddComponent<EnemyDeathVfx>();
            }

            vfx._inPool = false;
            vfx.gameObject.name = objectName;
            vfx.transform.SetParent(null, false);
            vfx.transform.position = position;
            vfx.transform.rotation = Quaternion.identity;
            vfx.transform.localScale = Vector3.one;
            vfx.gameObject.SetActive(true);
            return vfx;
        }

        private void Release()
        {
            if (_inPool)
            {
                return;
            }

            _inPool = true;
            _duration = 0f;
            _lifetime = 0f;
            DisableAllLines();
            gameObject.SetActive(false);
            transform.SetParent(GetPoolRoot(), false);
            Pool.Push(this);
        }

        private static Transform GetPoolRoot()
        {
            if (s_poolRoot != null)
            {
                return s_poolRoot;
            }

            GameObject root = new GameObject("EnemyVfxPool");
            s_poolRoot = root.transform;
            return s_poolRoot;
        }

        private static Material GetSharedMaterial()
        {
            if (s_sharedMaterial != null)
            {
                return s_sharedMaterial;
            }

            Shader shader = Shader.Find("Sprites/Default");

            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Unlit");
            }

            if (shader != null)
            {
                s_sharedMaterial = new Material(shader)
                {
                    name = "EnemyVfx_RuntimeMaterial"
                };
            }

            return s_sharedMaterial;
        }
    }
}
