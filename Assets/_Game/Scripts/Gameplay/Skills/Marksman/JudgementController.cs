using System.Collections;
using System.Collections.Generic;
using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Player;
using UnityEngine;
using UnityEngine.UI;

namespace DienGame.Gameplay.Skills.Marksman
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerAutoAttack))]
    public sealed class JudgementController : MonoBehaviour
    {
        [Header("Skill Level")]
        [SerializeField, Range(1, 5)]
        private int skillLevel = 1;

        [Header("Ultimate Button")]
        [SerializeField]
        private Sprite ultimateIcon;

        [SerializeField]
        private bool useAssignedIcon = false;

        [SerializeField]
        private bool autoCreateButton = true;

        [Header("VFX")]
        [SerializeField]
        private string sortingLayerName = "Default";

        [SerializeField]
        private int ringSortingOrder = 115;

        [SerializeField]
        private int beamSortingOrder = 116;

        [SerializeField]
        private int nodeSortingOrder = 117;

        private PlayerAutoAttack _autoAttack;
        private Material _vfxMaterial;
        private Sprite _nodeSprite;
        private Texture2D _nodeTexture;

        private ContactFilter2D _physicsFilter;
        private readonly List<Collider2D> _physicsResults = new(160);
        private readonly HashSet<EnemyHealth> _damagedEnemies = new();

        private float _cooldownRemaining;
        private bool _isCasting;
        private GameObject _activeVfxRoot;

        private sealed class CannonVisual
        {
            public Transform NodeTransform;
            public SpriteRenderer NodeRenderer;
            public LineRenderer BeamRenderer;
            public float BaseAngle;
        }

        public int SkillLevel => skillLevel;

        public float CooldownRemaining => Mathf.Max(0f, _cooldownRemaining);

        public float CooldownNormalized =>
            JudgementSkill.Cooldown <= 0.001f
                ? 0f
                : Mathf.Clamp01(_cooldownRemaining / JudgementSkill.Cooldown);

        public bool IsReady => !_isCasting && _cooldownRemaining <= 0f;

        private void Awake()
        {
            _autoAttack = GetComponent<PlayerAutoAttack>();

            _physicsFilter = new ContactFilter2D();
            _physicsFilter.NoFilter();

            CreateVfxMaterial();
            CreateNodeSprite();
        }

        private void Start()
        {
            _cooldownRemaining = 0f;

            if (autoCreateButton)
            {
                SetupButton();
            }
        }

        private void Update()
        {
            if (Time.timeScale <= 0f)
            {
                return;
            }

            if (_cooldownRemaining > 0f)
            {
                _cooldownRemaining -= Time.deltaTime;

                if (_cooldownRemaining < 0f)
                {
                    _cooldownRemaining = 0f;
                }
            }
        }

        public void SetSkillLevel(int level)
        {
            skillLevel = Mathf.Clamp(level, 1, 5);
        }

        public void UpgradeSkill()
        {
            SetSkillLevel(skillLevel + 1);
        }

        public bool TryActivate()
        {
            if (!IsReady || _autoAttack == null)
            {
                return false;
            }

            _cooldownRemaining = JudgementSkill.Cooldown;
            _isCasting = true;

            StartCoroutine(CastRoutine());
            return true;
        }

        private IEnumerator CastRoutine()
        {
            float radius = JudgementSkill.GetRadius(skillLevel);
            int cannonCount = JudgementSkill.GetCannonCount(skillLevel);
            int pulseCount = JudgementSkill.GetPulseCount(skillLevel);

            float totalDamage =
                _autoAttack.AttackDamage *
                JudgementSkill.GetTotalDamageMultiplier(skillLevel);

            float pulseDamage =
                totalDamage *
                JudgementSkill.PulseDamageFraction /
                Mathf.Max(1, pulseCount);

            float finalDamage =
                totalDamage *
                JudgementSkill.FinalDamageFraction;

            GameObject root = new GameObject("Judgement_DeathRing_VFX");
            _activeVfxRoot = root;
            root.transform.SetParent(transform, false);
            root.transform.localPosition = Vector3.zero;

            Color primaryColor =
                JudgementSkill.IsMaxLevel(skillLevel)
                    ? new Color(1f, 0.18f, 0.12f, 1f)
                    : new Color(1f, 0.58f, 0.08f, 1f);

            Color secondaryColor =
                JudgementSkill.IsMaxLevel(skillLevel)
                    ? new Color(1f, 0.82f, 0.28f, 1f)
                    : new Color(1f, 0.92f, 0.55f, 1f);

            LineRenderer innerRing =
                CreateRing(
                    root.transform,
                    "Judgement_InnerRing",
                    JudgementSkill.OrbitRadius,
                    0.055f,
                    secondaryColor,
                    ringSortingOrder
                );

            LineRenderer outerRing =
                CreateRing(
                    root.transform,
                    "Judgement_OuterRing",
                    radius,
                    0.04f,
                    primaryColor,
                    ringSortingOrder
                );

            List<CannonVisual> cannons =
                CreateCannons(
                    root.transform,
                    cannonCount,
                    radius,
                    primaryColor,
                    secondaryColor
                );

            yield return ChargeRoutine(
                cannons,
                innerRing,
                outerRing,
                radius,
                primaryColor,
                secondaryColor
            );

            yield return BarrageRoutine(
                cannons,
                innerRing,
                outerRing,
                radius,
                pulseCount,
                pulseDamage,
                primaryColor,
                secondaryColor
            );

            DamageEnemiesInRadius(
                transform.position,
                radius,
                finalDamage
            );

            TriggerCameraShake(
                JudgementSkill.FinalShakeDuration,
                JudgementSkill.FinalShakeStrength
            );

            StartCoroutine(PlayScreenFlashRoutine());

            yield return FinishRoutine(
                cannons,
                innerRing,
                outerRing,
                radius,
                primaryColor,
                secondaryColor
            );

            if (root != null)
            {
                Destroy(root);
            }

            if (_activeVfxRoot == root)
            {
                _activeVfxRoot = null;
            }

            _isCasting = false;
        }

        private IEnumerator ChargeRoutine(
            List<CannonVisual> cannons,
            LineRenderer innerRing,
            LineRenderer outerRing,
            float radius,
            Color primaryColor,
            Color secondaryColor)
        {
            float elapsed = 0f;

            while (elapsed < JudgementSkill.ChargeDuration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(
                    elapsed / JudgementSkill.ChargeDuration
                );

                float eased = 1f - Mathf.Pow(1f - t, 3f);

                SetRingRadius(
                    innerRing,
                    Mathf.Lerp(0.25f, JudgementSkill.OrbitRadius, eased)
                );

                SetRingRadius(
                    outerRing,
                    Mathf.Lerp(radius * 0.72f, radius, eased)
                );

                SetLineAlpha(innerRing, secondaryColor, Mathf.Lerp(0.1f, 0.95f, t));
                SetLineAlpha(outerRing, primaryColor, Mathf.Lerp(0.05f, 0.7f, t));

                UpdateCannons(
                    cannons,
                    radius,
                    elapsed * JudgementSkill.RotationSpeed * 0.35f,
                    Mathf.Lerp(0.15f, 1f, eased),
                    0f,
                    primaryColor,
                    secondaryColor
                );

                yield return null;
            }
        }

        private IEnumerator BarrageRoutine(
            List<CannonVisual> cannons,
            LineRenderer innerRing,
            LineRenderer outerRing,
            float radius,
            int pulseCount,
            float pulseDamage,
            Color primaryColor,
            Color secondaryColor)
        {
            float elapsed = 0f;
            float pulseSpacing =
                JudgementSkill.BarrageDuration /
                Mathf.Max(1, pulseCount);

            int nextPulseIndex = 0;
            float pulseGlow = 0f;

            while (elapsed < JudgementSkill.BarrageDuration)
            {
                elapsed += Time.deltaTime;

                while (
                    nextPulseIndex < pulseCount &&
                    elapsed >= nextPulseIndex * pulseSpacing)
                {
                    DamageEnemiesInRadius(
                        transform.position,
                        radius,
                        pulseDamage
                    );

                    TriggerCameraShake(
                        JudgementSkill.PulseShakeDuration,
                        JudgementSkill.PulseShakeStrength
                    );

                    pulseGlow = 1f;
                    nextPulseIndex++;
                }

                pulseGlow = Mathf.MoveTowards(
                    pulseGlow,
                    0f,
                    Time.deltaTime * 5.5f
                );

                float rotation =
                    elapsed *
                    JudgementSkill.RotationSpeed;

                float pulseScale =
                    1f +
                    Mathf.Sin(elapsed * 13f) * 0.035f;

                UpdateCannons(
                    cannons,
                    radius,
                    rotation,
                    pulseScale,
                    pulseGlow,
                    primaryColor,
                    secondaryColor
                );

                SetRingRadius(
                    innerRing,
                    JudgementSkill.OrbitRadius +
                    Mathf.Sin(elapsed * 8f) * 0.05f
                );

                SetRingRadius(
                    outerRing,
                    radius +
                    Mathf.Sin(elapsed * 5f) * 0.08f
                );

                SetLineAlpha(
                    innerRing,
                    secondaryColor,
                    0.72f + pulseGlow * 0.28f
                );

                SetLineAlpha(
                    outerRing,
                    primaryColor,
                    0.42f + pulseGlow * 0.38f
                );

                yield return null;
            }
        }

        private IEnumerator FinishRoutine(
            List<CannonVisual> cannons,
            LineRenderer innerRing,
            LineRenderer outerRing,
            float radius,
            Color primaryColor,
            Color secondaryColor)
        {
            LineRenderer shockwave =
                CreateRing(
                    _activeVfxRoot.transform,
                    "Judgement_FinalShockwave",
                    JudgementSkill.OrbitRadius,
                    0.32f,
                    secondaryColor,
                    beamSortingOrder + 2
                );

            float elapsed = 0f;

            while (elapsed < JudgementSkill.FinishDuration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(
                    elapsed / JudgementSkill.FinishDuration
                );

                float eased = 1f - Mathf.Pow(1f - t, 3f);

                SetRingRadius(
                    shockwave,
                    Mathf.Lerp(
                        JudgementSkill.OrbitRadius,
                        radius * 1.08f,
                        eased
                    )
                );

                float width = Mathf.Lerp(0.32f, 0.025f, t);
                shockwave.startWidth = width;
                shockwave.endWidth = width;
                SetLineAlpha(shockwave, secondaryColor, 1f - t);

                UpdateCannons(
                    cannons,
                    radius,
                    JudgementSkill.BarrageDuration *
                    JudgementSkill.RotationSpeed +
                    elapsed * 70f,
                    Mathf.Lerp(1.15f, 0.15f, t),
                    1f - t,
                    primaryColor,
                    secondaryColor
                );

                SetLineAlpha(innerRing, secondaryColor, 1f - t);
                SetLineAlpha(outerRing, primaryColor, 1f - t);

                yield return null;
            }
        }

        private List<CannonVisual> CreateCannons(
            Transform parent,
            int count,
            float radius,
            Color primaryColor,
            Color secondaryColor)
        {
            List<CannonVisual> cannons = new(count);

            for (int i = 0; i < count; i++)
            {
                float angle = 360f * i / Mathf.Max(1, count);

                GameObject nodeObject = new GameObject($"Cannon_{i:00}");
                nodeObject.transform.SetParent(parent, false);

                SpriteRenderer nodeRenderer =
                    nodeObject.AddComponent<SpriteRenderer>();

                nodeRenderer.sprite = _nodeSprite;
                nodeRenderer.color = secondaryColor;
                nodeRenderer.sortingLayerName = sortingLayerName;
                nodeRenderer.sortingOrder = nodeSortingOrder;

                if (_vfxMaterial != null)
                {
                    nodeRenderer.sharedMaterial = _vfxMaterial;
                }

                GameObject beamObject = new GameObject($"Beam_{i:00}");
                beamObject.transform.SetParent(parent, false);

                LineRenderer beam = beamObject.AddComponent<LineRenderer>();
                ConfigureLineRenderer(
                    beam,
                    JudgementSkill.GetBeamWidth(skillLevel),
                    primaryColor,
                    beamSortingOrder
                );

                beam.positionCount = 2;

                cannons.Add(
                    new CannonVisual
                    {
                        NodeTransform = nodeObject.transform,
                        NodeRenderer = nodeRenderer,
                        BeamRenderer = beam,
                        BaseAngle = angle,
                    }
                );
            }

            UpdateCannons(
                cannons,
                radius,
                0f,
                0.15f,
                0f,
                primaryColor,
                secondaryColor
            );

            return cannons;
        }

        private void UpdateCannons(
            List<CannonVisual> cannons,
            float radius,
            float rotationDegrees,
            float visualScale,
            float pulseGlow,
            Color primaryColor,
            Color secondaryColor)
        {
            float orbitRadius = JudgementSkill.OrbitRadius;
            float nodeSize = JudgementSkill.GetNodeSize(skillLevel);
            float beamWidth = JudgementSkill.GetBeamWidth(skillLevel);

            for (int i = 0; i < cannons.Count; i++)
            {
                CannonVisual cannon = cannons[i];

                float angle = cannon.BaseAngle + rotationDegrees;
                float radians = angle * Mathf.Deg2Rad;

                Vector3 direction = new Vector3(
                    Mathf.Cos(radians),
                    Mathf.Sin(radians),
                    0f
                );

                Vector3 nodePosition = direction * orbitRadius;
                Vector3 beamEnd = direction * radius;

                cannon.NodeTransform.localPosition = nodePosition;

                float scale =
                    nodeSize *
                    Mathf.Max(0.05f, visualScale) *
                    (1f + pulseGlow * 0.22f);

                cannon.NodeTransform.localScale =
                    new Vector3(scale, scale, 1f);

                cannon.NodeRenderer.color =
                    Color.Lerp(
                        secondaryColor,
                        Color.white,
                        pulseGlow * 0.75f
                    );

                LineRenderer beam = cannon.BeamRenderer;
                beam.SetPosition(0, nodePosition);
                beam.SetPosition(1, beamEnd);

                float currentWidth =
                    beamWidth *
                    (1f + pulseGlow * 1.2f);

                beam.startWidth = currentWidth;
                beam.endWidth = currentWidth * 0.28f;

                Color beamColor =
                    Color.Lerp(
                        primaryColor,
                        Color.white,
                        pulseGlow * 0.8f
                    );

                beam.startColor = beamColor;
                beam.endColor = new Color(
                    beamColor.r,
                    beamColor.g,
                    beamColor.b,
                    0.12f + pulseGlow * 0.28f
                );
            }
        }

        private LineRenderer CreateRing(
            Transform parent,
            string objectName,
            float radius,
            float width,
            Color color,
            int sortingOrder)
        {
            GameObject ringObject = new GameObject(objectName);
            ringObject.transform.SetParent(parent, false);

            LineRenderer ring = ringObject.AddComponent<LineRenderer>();
            ConfigureLineRenderer(ring, width, color, sortingOrder);

            ring.loop = true;
            ring.positionCount = 72;

            SetRingRadius(ring, radius);
            return ring;
        }

        private void ConfigureLineRenderer(
            LineRenderer line,
            float width,
            Color color,
            int sortingOrder)
        {
            line.useWorldSpace = false;
            line.textureMode = LineTextureMode.Stretch;
            line.alignment = LineAlignment.View;
            line.numCapVertices = 4;
            line.numCornerVertices = 4;

            line.startWidth = width;
            line.endWidth = width;
            line.startColor = color;
            line.endColor = color;

            line.sortingLayerName = sortingLayerName;
            line.sortingOrder = sortingOrder;

            if (_vfxMaterial != null)
            {
                line.sharedMaterial = _vfxMaterial;
            }
        }

        private static void SetRingRadius(
            LineRenderer ring,
            float radius)
        {
            if (ring == null)
            {
                return;
            }

            int count = ring.positionCount;

            for (int i = 0; i < count; i++)
            {
                float angle =
                    i * Mathf.PI * 2f / count;

                ring.SetPosition(
                    i,
                    new Vector3(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(angle) * radius,
                        0f
                    )
                );
            }
        }

        private static void SetLineAlpha(
            LineRenderer line,
            Color baseColor,
            float alpha)
        {
            if (line == null)
            {
                return;
            }

            alpha = Mathf.Clamp01(alpha);

            Color color = new Color(
                baseColor.r,
                baseColor.g,
                baseColor.b,
                alpha
            );

            line.startColor = color;
            line.endColor = color;
        }

        private void DamageEnemiesInRadius(
            Vector3 center,
            float radius,
            float damage)
        {
            if (damage <= 0f)
            {
                return;
            }

            _physicsResults.Clear();
            _damagedEnemies.Clear();

            Physics2D.OverlapCircle(
                center,
                radius,
                _physicsFilter,
                _physicsResults
            );

            foreach (Collider2D hit in _physicsResults)
            {
                if (hit == null)
                {
                    continue;
                }

                EnemyHealth enemy =
                    hit.GetComponentInParent<EnemyHealth>();

                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                if (!_damagedEnemies.Add(enemy))
                {
                    continue;
                }

                enemy.TakeDamage(damage);
            }
        }

        private static void TriggerCameraShake(
            float duration,
            float strength)
        {
            Camera camera = Camera.main;

            if (camera == null)
            {
                return;
            }

            JudgementCameraShake shake =
                camera.GetComponent<JudgementCameraShake>();

            if (shake == null)
            {
                shake = camera.gameObject.AddComponent<JudgementCameraShake>();
            }

            shake.Shake(duration, strength);
        }

        private IEnumerator PlayScreenFlashRoutine()
        {
            GameObject canvasObject = new GameObject(
                "Judgement_FinalFlashCanvas",
                typeof(Canvas),
                typeof(CanvasScaler)
            );

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 3000;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);

            GameObject flashObject = new GameObject(
                "Flash",
                typeof(RectTransform),
                typeof(Image)
            );

            flashObject.transform.SetParent(canvasObject.transform, false);

            RectTransform rect = flashObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = flashObject.GetComponent<Image>();
            image.raycastTarget = false;

            float duration = 0.16f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                float alpha =
                    t < 0.25f
                        ? Mathf.Lerp(0f, 0.36f, t / 0.25f)
                        : Mathf.Lerp(0.36f, 0f, (t - 0.25f) / 0.75f);

                image.color = new Color(1f, 0.90f, 0.66f, alpha);
                yield return null;
            }

            Destroy(canvasObject);
        }

        private void SetupButton()
        {
            JudgementUltimateButton button =
                GetComponent<JudgementUltimateButton>();

            if (button == null)
            {
                button = gameObject.AddComponent<JudgementUltimateButton>();
            }

            Sprite icon =
                useAssignedIcon
                    ? ultimateIcon
                    : null;

            button.Initialize(this, icon);
        }

        private void CreateVfxMaterial()
        {
            Shader shader = Shader.Find("Sprites/Default");

            if (shader == null)
            {
                shader = Shader.Find(
                    "Universal Render Pipeline/2D/Sprite-Unlit-Default"
                );
            }

            if (shader != null)
            {
                _vfxMaterial = new Material(shader);
            }
        }

        private void CreateNodeSprite()
        {
            const int size = 48;

            _nodeTexture = new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                false
            );

            _nodeTexture.name = "Judgement_RuntimeNodeTexture";
            _nodeTexture.wrapMode = TextureWrapMode.Clamp;
            _nodeTexture.filterMode = FilterMode.Bilinear;

            Color32[] pixels = new Color32[size * size];
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float maxRadius = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(
                        new Vector2(x, y),
                        center
                    );

                    float normalized = Mathf.Clamp01(1f - distance / maxRadius);
                    float alpha = normalized * normalized;

                    pixels[y * size + x] =
                        new Color(1f, 1f, 1f, alpha);
                }
            }

            _nodeTexture.SetPixels32(pixels);
            _nodeTexture.Apply(false, false);

            _nodeSprite = Sprite.Create(
                _nodeTexture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                size
            );

            _nodeSprite.name = "Judgement_RuntimeNodeSprite";
        }

        private void OnDisable()
        {
            if (_activeVfxRoot != null)
            {
                Destroy(_activeVfxRoot);
                _activeVfxRoot = null;
            }

            _isCasting = false;
        }

        private void OnDestroy()
        {
            if (_vfxMaterial != null)
            {
                Destroy(_vfxMaterial);
            }

            if (_nodeSprite != null)
            {
                Destroy(_nodeSprite);
            }

            if (_nodeTexture != null)
            {
                Destroy(_nodeTexture);
            }
        }
    }
}
