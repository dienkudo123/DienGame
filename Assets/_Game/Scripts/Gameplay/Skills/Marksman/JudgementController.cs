using System.Collections;
using System.Collections.Generic;
using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Player;
using DienGame.Gameplay.Skills.Visuals;
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

        [Header("VFX Sorting")]
        [SerializeField]
        private string sortingLayerName = "Default";

        [SerializeField]
        private int beamSortingOrder = 116;

        [SerializeField]
        private int cannonSortingOrder = 117;

        [SerializeField]
        private int impactSortingOrder = 118;

        [SerializeField]
        private int shockwaveSortingOrder = 119;

        private PlayerAutoAttack _autoAttack;
        private Material _vfxMaterial;

        private Sprite[] _cannonFrames = System.Array.Empty<Sprite>();
        private Sprite[] _beamFrames = System.Array.Empty<Sprite>();
        private Sprite[] _shockwaveFrames = System.Array.Empty<Sprite>();

        private Sprite _fallbackNodeSprite;
        private Texture2D _fallbackNodeTexture;

        private ContactFilter2D _physicsFilter;
        private readonly List<Collider2D> _physicsResults = new(160);
        private readonly HashSet<EnemyHealth> _damagedEnemies = new();

        private float _cooldownRemaining;
        private bool _isCasting;
        private GameObject _activeVfxRoot;

        private sealed class CannonVisual
        {
            public Transform Transform;
            public JudgementCannonAnimator Animator;
            public JudgementBeamVisual BeamVisual;
            public Transform ImpactTransform;
            public SpriteRenderer ImpactRenderer;
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
            CreateFallbackNodeSprite();
            ReloadVfxFrames();
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

            if (_cannonFrames.Length == 0 ||
                _beamFrames.Length == 0 ||
                _shockwaveFrames.Length == 0)
            {
                ReloadVfxFrames();
            }

            _cooldownRemaining = JudgementSkill.Cooldown;
            _isCasting = true;

            StartCoroutine(CastRoutine());
            return true;
        }

        private IEnumerator CastRoutine()
        {
            float damageRadius = JudgementSkill.GetRadius(skillLevel);
            float orbitRadius = JudgementSkill.GetOrbitRadius(skillLevel);
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

            List<CannonVisual> cannons =
                CreateCannons(
                    root.transform,
                    cannonCount,
                    orbitRadius,
                    damageRadius
                );

            yield return ChargeRoutine(
                cannons,
                orbitRadius,
                damageRadius
            );

            yield return BarrageRoutine(
                cannons,
                orbitRadius,
                damageRadius,
                pulseCount,
                pulseDamage
            );

            DamageEnemiesInRadius(
                transform.position,
                damageRadius,
                finalDamage
            );

            TriggerCameraShake(
                JudgementSkill.FinalShakeDuration,
                JudgementSkill.FinalShakeStrength
            );

            StartCoroutine(PlayScreenFlashRoutine());

            yield return FinishRoutine(
                root.transform,
                cannons,
                orbitRadius,
                damageRadius
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
            float orbitRadius,
            float damageRadius)
        {
            float elapsed = 0f;

            while (elapsed < JudgementSkill.ChargeDuration)
            {
                float deltaTime = Time.deltaTime;
                elapsed += deltaTime;

                float t = Mathf.Clamp01(
                    elapsed / JudgementSkill.ChargeDuration
                );

                float eased = 1f - Mathf.Pow(1f - t, 3f);

                UpdateCannons(
                    cannons,
                    orbitRadius,
                    damageRadius,
                    elapsed * JudgementSkill.RotationSpeed * 0.32f,
                    Mathf.Lerp(0.55f, 1f, eased),
                    Mathf.Lerp(0.15f, 1f, t),
                    deltaTime
                );

                yield return null;
            }
        }

        private IEnumerator BarrageRoutine(
            List<CannonVisual> cannons,
            float orbitRadius,
            float damageRadius,
            int pulseCount,
            float pulseDamage)
        {
            float elapsed = 0f;
            float pulseSpacing =
                JudgementSkill.BarrageDuration /
                Mathf.Max(1, pulseCount);

            int nextPulseIndex = 0;

            while (elapsed < JudgementSkill.BarrageDuration)
            {
                float deltaTime = Time.deltaTime;
                elapsed += deltaTime;

                while (
                    nextPulseIndex < pulseCount &&
                    elapsed >= nextPulseIndex * pulseSpacing)
                {
                    TriggerVolley(cannons);

                    DamageEnemiesInRadius(
                        transform.position,
                        damageRadius,
                        pulseDamage
                    );

                    TriggerCameraShake(
                        JudgementSkill.PulseShakeDuration,
                        JudgementSkill.PulseShakeStrength
                    );

                    nextPulseIndex++;
                }

                float rotation =
                    elapsed * JudgementSkill.RotationSpeed;

                float subtlePulse =
                    1f +
                    Mathf.Sin(elapsed * 12f) * 0.018f;

                UpdateCannons(
                    cannons,
                    orbitRadius,
                    damageRadius,
                    rotation,
                    subtlePulse,
                    1f,
                    deltaTime
                );

                yield return null;
            }
        }

        private IEnumerator FinishRoutine(
            Transform parent,
            List<CannonVisual> cannons,
            float orbitRadius,
            float damageRadius)
        {
            GameObject shockwaveObject = new GameObject(
                "Judgement_FinalShockwave",
                typeof(SpriteRenderer),
                typeof(JudgementShockwaveAnimator)
            );

            shockwaveObject.transform.SetParent(parent, false);
            shockwaveObject.transform.localPosition = Vector3.zero;

            SpriteRenderer shockwaveRenderer =
                shockwaveObject.GetComponent<SpriteRenderer>();

            shockwaveRenderer.sortingLayerName = sortingLayerName;
            shockwaveRenderer.sortingOrder = shockwaveSortingOrder;

            if (_vfxMaterial != null)
            {
                shockwaveRenderer.sharedMaterial = _vfxMaterial;
            }

            JudgementShockwaveAnimator shockwave =
                shockwaveObject.GetComponent<JudgementShockwaveAnimator>();

            shockwave.Initialize(_shockwaveFrames);

            float elapsed = 0f;

            while (elapsed < JudgementSkill.FinishDuration)
            {
                float deltaTime = Time.deltaTime;
                elapsed += deltaTime;

                float t = Mathf.Clamp01(
                    elapsed / JudgementSkill.FinishDuration
                );

                float eased = 1f - Mathf.Pow(1f - t, 3f);

                float diameter = Mathf.Lerp(
                    orbitRadius * 2.0f,
                    JudgementSkill.GetShockwaveDiameter(skillLevel),
                    eased
                );

                shockwave.SetProgress(
                    t,
                    diameter,
                    1f - t * 0.28f
                );

                UpdateCannons(
                    cannons,
                    orbitRadius,
                    damageRadius,
                    JudgementSkill.BarrageDuration *
                    JudgementSkill.RotationSpeed +
                    elapsed * 65f,
                    Mathf.Lerp(1f, 0.58f, t),
                    1f - t,
                    deltaTime
                );

                yield return null;
            }
        }

        private List<CannonVisual> CreateCannons(
            Transform parent,
            int count,
            float orbitRadius,
            float damageRadius)
        {
            List<CannonVisual> cannons = new(count);

            for (int i = 0; i < count; i++)
            {
                float angle = 360f * i / Mathf.Max(1, count);

                GameObject cannonObject =
                    new GameObject($"Judgement_Cannon_{i:00}");

                cannonObject.transform.SetParent(parent, false);

                SpriteRenderer cannonRenderer =
                    cannonObject.AddComponent<SpriteRenderer>();

                cannonRenderer.color = Color.white;
                cannonRenderer.sortingLayerName = sortingLayerName;
                cannonRenderer.sortingOrder = cannonSortingOrder;

                if (_vfxMaterial != null)
                {
                    cannonRenderer.sharedMaterial = _vfxMaterial;
                }

                JudgementCannonAnimator animator =
                    cannonObject.AddComponent<JudgementCannonAnimator>();

                animator.Initialize(
                    _cannonFrames,
                    _fallbackNodeSprite,
                    JudgementSkill.CannonFrameDuration
                );

                GameObject beamObject = new GameObject(
                    $"Judgement_Beam_{i:00}",
                    typeof(SpriteRenderer),
                    typeof(JudgementBeamVisual)
                );

                beamObject.transform.SetParent(parent, false);

                SpriteRenderer beamRenderer =
                    beamObject.GetComponent<SpriteRenderer>();

                beamRenderer.sortingLayerName = sortingLayerName;
                beamRenderer.sortingOrder = beamSortingOrder;

                if (_vfxMaterial != null)
                {
                    beamRenderer.sharedMaterial = _vfxMaterial;
                }

                JudgementBeamVisual beamVisual =
                    beamObject.GetComponent<JudgementBeamVisual>();

                beamVisual.Initialize(_beamFrames);

                GameObject impactObject =
                    new GameObject($"Judgement_Impact_{i:00}");

                impactObject.transform.SetParent(parent, false);

                SpriteRenderer impactRenderer =
                    impactObject.AddComponent<SpriteRenderer>();

                impactRenderer.sprite = _fallbackNodeSprite;
                impactRenderer.enabled = false;
                impactRenderer.sortingLayerName = sortingLayerName;
                impactRenderer.sortingOrder = impactSortingOrder;
                impactRenderer.color = Color.white;

                if (_vfxMaterial != null)
                {
                    impactRenderer.sharedMaterial = _vfxMaterial;
                }

                cannons.Add(
                    new CannonVisual
                    {
                        Transform = cannonObject.transform,
                        Animator = animator,
                        BeamVisual = beamVisual,
                        ImpactTransform = impactObject.transform,
                        ImpactRenderer = impactRenderer,
                        BaseAngle = angle,
                    }
                );
            }

            UpdateCannons(
                cannons,
                orbitRadius,
                damageRadius,
                0f,
                0.55f,
                0.2f,
                0f
            );

            return cannons;
        }

        private static void TriggerVolley(
            List<CannonVisual> cannons)
        {
            foreach (CannonVisual cannon in cannons)
            {
                cannon.Animator?.PlayFire();
            }
        }

        private void UpdateCannons(
            List<CannonVisual> cannons,
            float orbitRadius,
            float damageRadius,
            float rotationDegrees,
            float visualScale,
            float alpha,
            float deltaTime)
        {
            float cannonWidth =
                JudgementSkill.GetCannonWorldWidth(skillLevel);

            float beamHeight =
                JudgementSkill.GetBeamWorldHeight(skillLevel);

            float impactSize =
                JudgementSkill.GetImpactWorldSize(skillLevel);

            for (int i = 0; i < cannons.Count; i++)
            {
                CannonVisual cannon = cannons[i];

                cannon.Animator?.Tick(deltaTime);

                float angle =
                    cannon.BaseAngle +
                    rotationDegrees;

                float radians =
                    angle * Mathf.Deg2Rad;

                Vector3 direction =
                    new Vector3(
                        Mathf.Cos(radians),
                        Mathf.Sin(radians),
                        0f
                    );

                Vector3 cannonPosition =
                    direction * orbitRadius;

                cannon.Transform.localPosition = cannonPosition;

                cannon.Transform.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        angle +
                        JudgementSkill.CannonSpriteAngleOffset
                    );

                cannon.Animator?.SetAlpha(alpha);
                cannon.Animator?.SetWorldWidth(
                    cannonWidth * Mathf.Max(0.05f, visualScale)
                );

                bool beamActive =
                    cannon.Animator != null &&
                    cannon.Animator.IsBeamActive;

                if (!beamActive)
                {
                    cannon.BeamVisual?.Hide();

                    if (cannon.ImpactRenderer != null)
                    {
                        cannon.ImpactRenderer.enabled = false;
                    }

                    continue;
                }

                float normalizedFire =
                    cannon.Animator.NormalizedFireTime;

                float beamTime = Mathf.InverseLerp(
                    0.34f,
                    0.80f,
                    normalizedFire
                );

                float beamPulse = Mathf.Sin(
                    Mathf.Clamp01(beamTime) * Mathf.PI
                );

                Vector3 beamStart =
                    cannonPosition +
                    direction * (cannonWidth * JudgementSkill.BeamStartOffsetFactor);

                Vector3 beamEnd =
                    direction * damageRadius;

                cannon.BeamVisual?.Show(
                    beamStart,
                    beamEnd,
                    beamTime,
                    beamHeight * Mathf.Lerp(0.78f, 1.15f, beamPulse),
                    alpha
                );

                if (cannon.ImpactRenderer == null ||
                    cannon.ImpactTransform == null ||
                    _fallbackNodeSprite == null)
                {
                    continue;
                }

                cannon.ImpactRenderer.enabled = true;
                cannon.ImpactTransform.localPosition = beamEnd;
                cannon.ImpactTransform.localRotation = Quaternion.identity;

                float spriteWidth = Mathf.Max(
                    0.0001f,
                    _fallbackNodeSprite.bounds.size.x
                );

                float impactWorldSize =
                    impactSize *
                    Mathf.Lerp(0.72f, 1.28f, beamPulse);

                float impactScale =
                    impactWorldSize / spriteWidth;

                cannon.ImpactTransform.localScale =
                    Vector3.one * impactScale;

                cannon.ImpactRenderer.color =
                    new Color(
                        1f,
                        Mathf.Lerp(0.48f, 0.92f, beamPulse),
                        Mathf.Lerp(0.08f, 0.58f, beamPulse),
                        alpha * Mathf.Lerp(0.45f, 0.95f, beamPulse)
                    );
            }
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
                shake =
                    camera.gameObject
                        .AddComponent<JudgementCameraShake>();
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

            const float duration = 0.16f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                float flashAlpha =
                    t < 0.25f
                        ? Mathf.Lerp(0f, 0.30f, t / 0.25f)
                        : Mathf.Lerp(
                            0.30f,
                            0f,
                            (t - 0.25f) / 0.75f
                        );

                image.color =
                    new Color(1f, 0.90f, 0.66f, flashAlpha);

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
                button =
                    gameObject
                        .AddComponent<JudgementUltimateButton>();
            }

            Sprite icon =
                useAssignedIcon
                    ? ultimateIcon
                    : null;

            button.Initialize(this, icon);
        }

        private void ReloadVfxFrames()
        {
            JudgementCannonCatalogProvider.Reload();

            _cannonFrames =
                JudgementCannonCatalogProvider.CannonFrames ??
                System.Array.Empty<Sprite>();

            _beamFrames =
                JudgementCannonCatalogProvider.BeamFrames ??
                System.Array.Empty<Sprite>();

            _shockwaveFrames =
                JudgementCannonCatalogProvider.ShockwaveFrames ??
                System.Array.Empty<Sprite>();

            if (_cannonFrames.Length == 0)
            {
                Debug.LogWarning(
                    "JudgementController: chưa bind cannon animation. " +
                    "Cần Judgement_Cannon_Fire_0 ... _7.",
                    this
                );
            }

            if (_beamFrames.Length == 0)
            {
                Debug.LogWarning(
                    "JudgementController: chưa bind beam animation. " +
                    "Cần Judgement_Beam_0 ... _5.",
                    this
                );
            }

            if (_shockwaveFrames.Length == 0)
            {
                Debug.LogWarning(
                    "JudgementController: chưa bind shockwave animation. " +
                    "Cần Judgement_Shockwave_0 ... _7.",
                    this
                );
            }
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

        private void CreateFallbackNodeSprite()
        {
            const int size = 48;

            _fallbackNodeTexture = new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                false
            );

            _fallbackNodeTexture.name = "Judgement_FallbackNodeTexture";
            _fallbackNodeTexture.wrapMode = TextureWrapMode.Clamp;
            _fallbackNodeTexture.filterMode = FilterMode.Bilinear;

            Color32[] pixels = new Color32[size * size];
            Vector2 center =
                new Vector2(
                    (size - 1) * 0.5f,
                    (size - 1) * 0.5f
                );

            float maxRadius = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(
                        new Vector2(x, y),
                        center
                    );

                    float normalized =
                        Mathf.Clamp01(
                            1f -
                            distance /
                            maxRadius
                        );

                    float nodeAlpha = normalized * normalized;

                    pixels[y * size + x] =
                        new Color(1f, 1f, 1f, nodeAlpha);
                }
            }

            _fallbackNodeTexture.SetPixels32(pixels);
            _fallbackNodeTexture.Apply(false, false);

            _fallbackNodeSprite = Sprite.Create(
                _fallbackNodeTexture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                size
            );

            _fallbackNodeSprite.name = "Judgement_FallbackNodeSprite";
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

            if (_fallbackNodeSprite != null)
            {
                Destroy(_fallbackNodeSprite);
            }

            if (_fallbackNodeTexture != null)
            {
                Destroy(_fallbackNodeTexture);
            }
        }
    }
}
