using System.Collections.Generic;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Marksman
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ThunderLightningVisual : MonoBehaviour
    {
        private static readonly Stack<ThunderLightningVisual> Pool = new();
        private static Material s_sharedMaterial;
        private static Transform s_poolRoot;

        private SpriteRenderer _spriteRenderer;
        private Sprite[] _frames;
        private int _frameIndex;
        private float _frameTimer;
        private float _framesPerSecond;
        private float _lifeTimer;
        private float _distance;
        private Vector3 _from;
        private Vector3 _to;
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
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.enabled = true;
            _spriteRenderer.color = Color.white;
            _spriteRenderer.sharedMaterial = GetSharedMaterial();
        }

        private void Update()
        {
            UpdateAnimation();
            UpdateLifetime();
        }

        public static ThunderLightningVisual Spawn(
            Sprite[] frames,
            float framesPerSecond,
            Vector3 from,
            Vector3 to,
            float widthScale,
            float lifetime,
            int sortingOrder,
            bool maxForm)
        {
            ThunderLightningVisual visual = Acquire();
            visual.gameObject.name = maxForm
                ? "ThunderLightning_MAX"
                : "ThunderLightning";

            visual.Initialize(
                frames,
                framesPerSecond,
                from,
                to,
                widthScale,
                lifetime,
                sortingOrder
            );

            return visual;
        }

        public void Initialize(
            Sprite[] frames,
            float framesPerSecond,
            Vector3 from,
            Vector3 to,
            float widthScale,
            float lifetime,
            int sortingOrder)
        {
            _frames = frames;
            _framesPerSecond = Mathf.Max(1f, framesPerSecond);
            _from = from;
            _to = to;
            _lifeTimer = Mathf.Max(0.02f, lifetime);
            _frameIndex = 0;
            _frameTimer = 0f;
            _spriteRenderer.sortingOrder = sortingOrder;
            _ = widthScale;

            if (_frames == null || _frames.Length == 0 || _frames[0] == null)
            {
                Release();
                return;
            }

            _spriteRenderer.sprite = _frames[0];
            SetupTransform();
        }

        private void SetupTransform()
        {
            Vector3 direction = _to - _from;
            _distance = direction.magnitude;

            if (_distance <= 0.001f)
            {
                Release();
                return;
            }

            transform.position = (_from + _to) * 0.5f;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            UpdateScale();
        }

        private void UpdateScale()
        {
            Sprite sprite = _spriteRenderer.sprite;
            if (sprite == null)
            {
                return;
            }

            float spriteWidth = sprite.bounds.size.x;
            if (spriteWidth <= 0.001f)
            {
                return;
            }

            float lengthScale = _distance / spriteWidth;
            transform.localScale = new Vector3(lengthScale, 0.1f, 1f);
        }

        private void UpdateAnimation()
        {
            if (_frames == null || _frames.Length <= 1)
            {
                return;
            }

            float frameDuration = 1f / _framesPerSecond;
            _frameTimer += Time.deltaTime;

            while (_frameTimer >= frameDuration)
            {
                _frameTimer -= frameDuration;
                _frameIndex = (_frameIndex + 1) % _frames.Length;

                Sprite nextSprite = _frames[_frameIndex];
                if (nextSprite == null)
                {
                    continue;
                }

                _spriteRenderer.sprite = nextSprite;
                UpdateScale();
            }
        }

        private void UpdateLifetime()
        {
            _lifeTimer -= Time.deltaTime;
            if (_lifeTimer <= 0f)
            {
                Release();
            }
        }

        private static ThunderLightningVisual Acquire()
        {
            ThunderLightningVisual visual = null;

            while (Pool.Count > 0 && visual == null)
            {
                visual = Pool.Pop();
            }

            if (visual == null)
            {
                GameObject go = new GameObject("ThunderLightning");
                go.AddComponent<SpriteRenderer>();
                visual = go.AddComponent<ThunderLightningVisual>();
            }

            visual._inPool = false;
            visual.transform.SetParent(null, false);
            visual.gameObject.SetActive(true);
            return visual;
        }

        private void Release()
        {
            if (_inPool)
            {
                return;
            }

            _inPool = true;
            _frames = null;
            _frameIndex = 0;
            _frameTimer = 0f;
            _lifeTimer = 0f;
            _distance = 0f;
            _spriteRenderer.sprite = null;
            transform.localScale = Vector3.one;
            transform.rotation = Quaternion.identity;

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

            GameObject root = new GameObject("ThunderLightningPool");
            s_poolRoot = root.transform;
            return s_poolRoot;
        }

        private static Material GetSharedMaterial()
        {
            if (s_sharedMaterial != null)
            {
                return s_sharedMaterial;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            if (shader != null)
            {
                s_sharedMaterial = new Material(shader)
                {
                    name = "ThunderLightning_SharedMaterial"
                };
            }

            return s_sharedMaterial;
        }
    }
}
