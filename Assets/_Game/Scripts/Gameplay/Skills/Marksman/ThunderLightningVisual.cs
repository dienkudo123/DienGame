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
        private float _widthScale = 1f;

        // Static fallback anchors. These are also continuously refreshed while
        // the linked enemies are alive, so the lightning stays attached to them.
        private Vector3 _from;
        private Vector3 _to;

        private Transform _fromTarget;
        private Transform _toTarget;
        private Collider2D _fromCollider;
        private Collider2D _toCollider;
        private SpriteRenderer _fromTargetRenderer;
        private SpriteRenderer _toTargetRenderer;

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

        // LateUpdate is intentional: enemies normally move during Update/FixedUpdate.
        // Rebuilding the beam here makes both endpoints follow their final position
        // for the current rendered frame instead of leaving the lightning behind.
        private void LateUpdate()
        {
            RefreshTrackedAnchors();
            UpdateTransform();
            UpdateAnimation();
            UpdateLifetime();
        }

        public static ThunderLightningVisual Spawn(
            Sprite[] frames,
            float framesPerSecond,
            Transform fromTarget,
            Transform toTarget,
            float widthScale,
            float lifetime,
            int sortingOrder,
            bool maxForm)
        {
            ThunderLightningVisual visual = Acquire();
            visual.gameObject.name = maxForm
                ? "ThunderLightning_MAX"
                : "ThunderLightning";

            visual.InitializeTracked(
                frames,
                framesPerSecond,
                fromTarget,
                toTarget,
                widthScale,
                lifetime,
                sortingOrder
            );

            return visual;
        }

        // Kept for any static VFX call sites that may still need a fixed line.
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

            visual.InitializeStatic(
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

        public void InitializeTracked(
            Sprite[] frames,
            float framesPerSecond,
            Transform fromTarget,
            Transform toTarget,
            float widthScale,
            float lifetime,
            int sortingOrder)
        {
            _fromTarget = fromTarget;
            _toTarget = toTarget;

            CacheTargetComponents();

            _from = ResolveAnchor(
                _fromTarget,
                _fromCollider,
                _fromTargetRenderer,
                fromTarget != null ? fromTarget.position : transform.position
            );

            _to = ResolveAnchor(
                _toTarget,
                _toCollider,
                _toTargetRenderer,
                toTarget != null ? toTarget.position : _from
            );

            InitializeCommon(
                frames,
                framesPerSecond,
                widthScale,
                lifetime,
                sortingOrder
            );
        }

        public void InitializeStatic(
            Sprite[] frames,
            float framesPerSecond,
            Vector3 from,
            Vector3 to,
            float widthScale,
            float lifetime,
            int sortingOrder)
        {
            ClearTrackedTargets();
            _from = from;
            _to = to;

            InitializeCommon(
                frames,
                framesPerSecond,
                widthScale,
                lifetime,
                sortingOrder
            );
        }

        private void InitializeCommon(
            Sprite[] frames,
            float framesPerSecond,
            float widthScale,
            float lifetime,
            int sortingOrder)
        {
            _frames = frames;
            _framesPerSecond = Mathf.Max(1f, framesPerSecond);
            _widthScale = Mathf.Max(0.01f, widthScale);
            _lifeTimer = Mathf.Max(0.02f, lifetime);
            _frameIndex = 0;
            _frameTimer = 0f;
            _spriteRenderer.sortingOrder = sortingOrder;

            if (_frames == null || _frames.Length == 0 || _frames[0] == null)
            {
                Release();
                return;
            }

            _spriteRenderer.sprite = _frames[0];
            UpdateTransform();
        }

        private void CacheTargetComponents()
        {
            _fromCollider = GetTargetComponent<Collider2D>(_fromTarget);
            _toCollider = GetTargetComponent<Collider2D>(_toTarget);

            _fromTargetRenderer = GetTargetComponent<SpriteRenderer>(_fromTarget);
            _toTargetRenderer = GetTargetComponent<SpriteRenderer>(_toTarget);
        }

        private static T GetTargetComponent<T>(Transform target)
            where T : Component
        {
            if (target == null)
            {
                return null;
            }

            T component = target.GetComponent<T>();
            if (component != null)
            {
                return component;
            }

            return target.GetComponentInChildren<T>();
        }

        private void RefreshTrackedAnchors()
        {
            if (_fromTarget != null && _fromTarget.gameObject.activeInHierarchy)
            {
                _from = ResolveAnchor(
                    _fromTarget,
                    _fromCollider,
                    _fromTargetRenderer,
                    _from
                );
            }

            if (_toTarget != null && _toTarget.gameObject.activeInHierarchy)
            {
                _to = ResolveAnchor(
                    _toTarget,
                    _toCollider,
                    _toTargetRenderer,
                    _to
                );
            }
        }

        private static Vector3 ResolveAnchor(
            Transform target,
            Collider2D targetCollider,
            SpriteRenderer targetRenderer,
            Vector3 fallback)
        {
            if (target == null)
            {
                return fallback;
            }

            if (
                targetCollider != null &&
                targetCollider.enabled &&
                targetCollider.gameObject.activeInHierarchy
            )
            {
                Vector3 center = targetCollider.bounds.center;
                center.z = target.position.z;
                return center;
            }

            if (
                targetRenderer != null &&
                targetRenderer.enabled &&
                targetRenderer.gameObject.activeInHierarchy
            )
            {
                Vector3 center = targetRenderer.bounds.center;
                center.z = target.position.z;
                return center;
            }

            return target.position;
        }

        private void UpdateTransform()
        {
            Vector3 direction = _to - _from;
            _distance = direction.magnitude;

            if (_distance <= 0.001f)
            {
                // Keep the object alive for its tiny lifetime instead of returning
                // it to the pool immediately. This avoids flicker when two enemies
                // briefly overlap while moving.
                transform.position = _from;
                transform.localScale = Vector3.zero;
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
            float thicknessScale = 0.1f * _widthScale;

            transform.localScale = new Vector3(
                lengthScale,
                thicknessScale,
                1f
            );
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
            _widthScale = 1f;
            _spriteRenderer.sprite = null;

            ClearTrackedTargets();

            transform.localScale = Vector3.one;
            transform.rotation = Quaternion.identity;

            gameObject.SetActive(false);
            transform.SetParent(GetPoolRoot(), false);
            Pool.Push(this);
        }

        private void ClearTrackedTargets()
        {
            _fromTarget = null;
            _toTarget = null;
            _fromCollider = null;
            _toCollider = null;
            _fromTargetRenderer = null;
            _toTargetRenderer = null;
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
