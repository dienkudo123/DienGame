using System.Collections.Generic;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Marksman
{
    public sealed class RuntimeLineVfx : MonoBehaviour
    {
        private static readonly Stack<RuntimeLineVfx> Pool = new();
        private static Material s_sharedMaterial;
        private static Transform s_poolRoot;

        private LineRenderer _line;
        private float _remaining;
        private float _duration;
        private Color _startColor;
        private Color _endColor;
        private bool _inPool;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Pool.Clear();
            s_sharedMaterial = null;
            s_poolRoot = null;
        }

        private void Awake()
        {
            _line = gameObject.AddComponent<LineRenderer>();
            _line.positionCount = 2;
            _line.useWorldSpace = true;
            _line.material = GetSharedMaterial();
        }

        private void Update()
        {
            _remaining -= Time.deltaTime;

            if (_duration > 0f)
            {
                float alpha = Mathf.Clamp01(_remaining / _duration);

                Color start = _startColor;
                start.a *= alpha;
                Color end = _endColor;
                end.a *= alpha;

                _line.startColor = start;
                _line.endColor = end;
            }

            if (_remaining <= 0f)
            {
                Release();
            }
        }

        public static void Show(
            Vector3 from,
            Vector3 to,
            float duration,
            float width,
            int sortingOrder = 30)
        {
            RuntimeLineVfx vfx = Acquire();
            vfx.Configure(from, to, duration, width, sortingOrder);
        }

        private void Configure(
            Vector3 from,
            Vector3 to,
            float duration,
            float width,
            int sortingOrder)
        {
            _duration = Mathf.Max(0.02f, duration);
            _remaining = _duration;

            _line.SetPosition(0, from);
            _line.SetPosition(1, to);
            _line.startWidth = width;
            _line.endWidth = width * 0.35f;
            _line.sortingOrder = sortingOrder;

            _startColor = Color.white;
            _endColor = new Color(1f, 1f, 1f, 0.12f);
            _line.startColor = _startColor;
            _line.endColor = _endColor;
        }

        private static RuntimeLineVfx Acquire()
        {
            RuntimeLineVfx vfx = null;

            while (Pool.Count > 0 && vfx == null)
            {
                vfx = Pool.Pop();
            }

            if (vfx == null)
            {
                GameObject go = new GameObject("DroneTracer");
                vfx = go.AddComponent<RuntimeLineVfx>();
            }

            vfx._inPool = false;
            vfx.transform.SetParent(null, false);
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
            _remaining = 0f;
            _duration = 0f;
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

            GameObject root = new GameObject("RuntimeLineVfxPool");
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
                    name = "RuntimeLineVfx_SharedMaterial"
                };
            }

            return s_sharedMaterial;
        }
    }
}
