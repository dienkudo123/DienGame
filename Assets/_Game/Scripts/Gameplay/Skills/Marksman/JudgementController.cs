using System.Collections;
using System.Collections.Generic;
using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Player;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Marksman
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerAutoAttack))]
    public sealed class JudgementController : MonoBehaviour
    {
        // =========================================
        // LEVEL
        // =========================================

        [Header("Skill Level")]

        [SerializeField, Range(1, 5)]
        private int skillLevel = 1;

        // =========================================
        // REUSE AIR STRIKE ASSETS
        // =========================================

        [Header("Reuse Air Strike - Bomb Frames")]

        [SerializeField]
        private Sprite[] bombFrames;

        [Header("Reuse Air Strike - Explosion Frames")]

        [SerializeField]
        private Sprite[] explosionFrames;

        // =========================================
        // MAX ASSET
        // =========================================

        [Header("Judgement MAX - Plane Shadow")]

        [SerializeField]
        private Sprite planeShadowSprite;

        // =========================================
        // BUTTON
        // =========================================

        [Header("Ultimate Button")]

        [SerializeField]
        private Sprite ultimateIcon;

        [SerializeField]
        private bool autoCreateButton = true;

        // =========================================
        // VISUAL
        // =========================================

        [Header("Visual")]

        [SerializeField, Min(0.1f)]
        private float baseBombWorldHeight = 1.5f;

        [SerializeField, Min(0.1f)]
        private float planeShadowWorldWidth = 7f;

        [SerializeField, Min(1f)]
        private float explosionFPS = 16f;

        // =========================================
        // SORTING
        // =========================================

        [Header("Sorting")]

        [SerializeField]
        private string sortingLayerName = "Default";

        [SerializeField]
        private int planeShadowSortingOrder = 5;

        [SerializeField]
        private int bombSortingOrder = 100;

        [SerializeField]
        private int explosionSortingOrder = 120;

        // =========================================
        // REFERENCES
        // =========================================

        private PlayerAutoAttack _autoAttack;

        private Transform _cachedTransform;

        private Material _vfxMaterial;

        // =========================================
        // PHYSICS
        // =========================================

        private ContactFilter2D _physicsFilter;

        private readonly List<Collider2D> _physicsResults =
            new(128);

        private readonly List<EnemyHealth> _targetCandidates =
            new(128);

        private readonly HashSet<EnemyHealth> _enemySet =
            new();

        private readonly HashSet<EnemyHealth> _damagedEnemies =
            new();

        // =========================================
        // RUNTIME
        // =========================================

        private float _cooldownRemaining;

        private bool _isCasting;

        // =========================================
        // PUBLIC
        // =========================================

        public int SkillLevel =>
            skillLevel;

        public float CooldownRemaining =>
            Mathf.Max(
                0f,
                _cooldownRemaining
            );

        public float CooldownNormalized
        {
            get
            {
                if (
                    JudgementSkill.Cooldown <=
                    0.001f
                )
                {
                    return 0f;
                }

                return Mathf.Clamp01(
                    _cooldownRemaining /
                    JudgementSkill.Cooldown
                );
            }
        }

        public bool IsReady =>
            !_isCasting &&
            _cooldownRemaining <= 0f;

        // =========================================
        // UNITY
        // =========================================

        private void Awake()
        {
            _cachedTransform =
                transform;

            _autoAttack =
                GetComponent<PlayerAutoAttack>();

            // Unity 6:
            // NoFilter() là instance method.
            _physicsFilter =
                new ContactFilter2D();

            _physicsFilter.NoFilter();

            CreateVfxMaterial();
        }

        private void Start()
        {
            // Vào game Ultimate sẵn sàng dùng.
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

            UpdateCooldown();
        }

        // =========================================
        // LEVEL
        // =========================================

        public void SetSkillLevel(int level)
        {
            skillLevel =
                Mathf.Clamp(
                    level,
                    1,
                    5
                );
        }

        public void UpgradeSkill()
        {
            skillLevel =
                Mathf.Clamp(
                    skillLevel + 1,
                    1,
                    5
                );
        }

        // =========================================
        // COOLDOWN
        // =========================================

        private void UpdateCooldown()
        {
            if (_cooldownRemaining <= 0f)
            {
                return;
            }

            _cooldownRemaining -=
                Time.deltaTime;

            if (_cooldownRemaining < 0f)
            {
                _cooldownRemaining = 0f;
            }
        }

        // =========================================
        // ACTIVATE
        // =========================================

        public bool TryActivate()
        {
            if (!IsReady)
            {
                return false;
            }

            if (
                !TryGetRandomTargetPosition(
                    out Vector3 targetPosition
                )
            )
            {
                // Không có quái:
                // không mất cooldown.
                return false;
            }

            _cooldownRemaining =
                JudgementSkill.Cooldown;

            _isCasting = true;

            StartCoroutine(
                CastRoutine(
                    targetPosition
                )
            );

            return true;
        }

        // =========================================
        // TARGET
        // =========================================

        private bool TryGetRandomTargetPosition(
            out Vector3 targetPosition)
        {
            targetPosition =
                Vector3.zero;

            _physicsResults.Clear();
            _targetCandidates.Clear();
            _enemySet.Clear();

            Physics2D.OverlapCircle(
                _cachedTransform.position,
                JudgementSkill.TargetRange,
                _physicsFilter,
                _physicsResults
            );

            foreach (
                Collider2D hit
                in _physicsResults
            )
            {
                if (hit == null)
                {
                    continue;
                }

                EnemyHealth enemy =
                    hit.GetComponentInParent<EnemyHealth>();

                if (
                    enemy == null ||
                    enemy.IsDead
                )
                {
                    continue;
                }

                // Một enemy có nhiều collider
                // vẫn chỉ được thêm một lần.
                if (!_enemySet.Add(enemy))
                {
                    continue;
                }

                _targetCandidates.Add(
                    enemy
                );
            }

            if (_targetCandidates.Count == 0)
            {
                return false;
            }

            EnemyHealth selected =
                _targetCandidates[
                    Random.Range(
                        0,
                        _targetCandidates.Count
                    )
                ];

            if (
                selected == null ||
                selected.IsDead
            )
            {
                return false;
            }

            // Khóa vị trí lúc bấm skill.
            // Sau đó bom không đuổi theo enemy.
            targetPosition =
                selected.transform.position;

            return true;
        }

        // =========================================
        // CAST
        // =========================================

        private IEnumerator CastRoutine(
            Vector3 targetPosition)
        {
            // =====================================
            // LV5 MAX - PLANE SHADOW
            // =====================================

            if (
                JudgementSkill.IsMaxLevel(
                    skillLevel
                )
            )
            {
                yield return
                    PlayPlaneShadowRoutine(
                        targetPosition
                    );
            }

            // =====================================
            // BOMB
            // =====================================

            yield return
                DropBombRoutine(
                    targetPosition
                );

            // =====================================
            // EXPLOSION
            // =====================================

            Explode(
                targetPosition
            );

            _isCasting = false;
        }

        // =========================================
        // MAX PLANE SHADOW
        // =========================================

        private IEnumerator PlayPlaneShadowRoutine(
            Vector3 targetPosition)
        {
            if (planeShadowSprite == null)
            {
                yield break;
            }

            Vector3 direction =
                Random.value < 0.5f
                    ? Vector3.right
                    : Vector3.left;

            float halfDistance =
                JudgementSkill
                    .MaxPlaneTravelDistance *
                0.5f;

            Vector3 startPosition =
                targetPosition -
                direction *
                halfDistance;

            Vector3 endPosition =
                targetPosition +
                direction *
                halfDistance;

            SpriteRenderer renderer =
                CreateVfxRenderer(
                    "Judgement_MAX_PlaneShadow",
                    planeShadowSprite,
                    startPosition,
                    planeShadowSortingOrder
                );

            SetWorldWidth(
                renderer,
                planeShadowWorldWidth
            );

            // Bóng máy bay mờ.
            renderer.color =
                new Color(
                    0f,
                    0f,
                    0f,
                    0.45f
                );

            Transform planeTransform =
                renderer.transform;

            renderer.flipX =
                direction.x < 0f;

            float duration =
                Mathf.Max(
                    0.05f,
                    JudgementSkill.MaxPlaneDuration
                );

            float timer = 0f;

            while (timer < duration)
            {
                timer +=
                    Time.deltaTime;

                float progress =
                    Mathf.Clamp01(
                        timer /
                        duration
                    );

                planeTransform.position =
                    Vector3.Lerp(
                        startPosition,
                        endPosition,
                        progress
                    );

                yield return null;
            }

            Destroy(
                renderer.gameObject
            );
        }

        // =========================================
        // BOMB
        // =========================================

        private IEnumerator DropBombRoutine(
            Vector3 targetPosition)
        {
            Sprite firstFrame =
                GetFirstValidSprite(
                    bombFrames
                );

            if (firstFrame == null)
            {
                yield return
                    WaitForDuration(
                        JudgementSkill
                            .BombFallDuration
                    );

                yield break;
            }

            Vector3 startPosition =
                targetPosition +
                Vector3.up *
                JudgementSkill
                    .BombStartHeight;

            SpriteRenderer renderer =
                CreateVfxRenderer(
                    "Judgement_Bomb",
                    firstFrame,
                    startPosition,
                    bombSortingOrder
                );

            Transform bombTransform =
                renderer.transform;

            float duration =
                Mathf.Max(
                    0.05f,
                    JudgementSkill.BombFallDuration
                );

            float timer = 0f;

            float bombMultiplier =
                JudgementSkill
                    .GetBombVisualMultiplier(
                        skillLevel
                    );

            while (timer < duration)
            {
                timer +=
                    Time.deltaTime;

                float progress =
                    Mathf.Clamp01(
                        timer /
                        duration
                    );

                // Bom tăng tốc dần.
                float fallProgress =
                    progress *
                    progress;

                bombTransform.position =
                    Vector3.Lerp(
                        startPosition,
                        targetPosition,
                        fallProgress
                    );

                UpdateProgressFrame(
                    renderer,
                    bombFrames,
                    progress
                );

                float currentHeight =
                    Mathf.Lerp(
                        baseBombWorldHeight *
                        0.7f,
                        baseBombWorldHeight,
                        progress
                    );

                currentHeight *=
                    bombMultiplier;

                SetWorldHeight(
                    renderer,
                    currentHeight
                );

                yield return null;
            }

            Destroy(
                renderer.gameObject
            );
        }

        // =========================================
        // EXPLOSION
        // =========================================

        private void Explode(
            Vector3 position)
        {
            float radius =
                JudgementSkill
                    .GetExplosionRadius(
                        skillLevel
                    );

            float damage =
                _autoAttack.AttackDamage *
                JudgementSkill
                    .GetDamageMultiplier(
                        skillLevel
                    );

            DamageEnemiesInRadius(
                position,
                radius,
                damage
            );

            Sprite firstFrame =
                GetFirstValidSprite(
                    explosionFrames
                );

            if (firstFrame != null)
            {
                StartCoroutine(
                    PlayExplosionRoutine(
                        position,
                        radius
                    )
                );
            }

            // Chỉ Lv5 MAX mới rung màn hình.
            if (
                JudgementSkill.IsMaxLevel(
                    skillLevel
                )
            )
            {
                TriggerCameraShake();
            }
        }

        // =========================================
        // EXPLOSION VISUAL
        // =========================================

        private IEnumerator PlayExplosionRoutine(
            Vector3 position,
            float radius)
        {
            Sprite firstFrame =
                GetFirstValidSprite(
                    explosionFrames
                );

            if (firstFrame == null)
            {
                yield break;
            }

            SpriteRenderer renderer =
                CreateVfxRenderer(
                    "Judgement_Explosion",
                    firstFrame,
                    position,
                    explosionSortingOrder
                );

            float visualMultiplier =
                JudgementSkill
                    .GetExplosionVisualMultiplier(
                        skillLevel
                    );

            float visualWidth =
                radius *
                2f *
                visualMultiplier;

            float frameDuration =
                1f /
                Mathf.Max(
                    1f,
                    explosionFPS
                );

            for (
                int i = 0;
                i < explosionFrames.Length;
                i++
            )
            {
                Sprite frame =
                    explosionFrames[i];

                if (frame != null)
                {
                    renderer.sprite =
                        frame;

                    SetWorldWidth(
                        renderer,
                        visualWidth
                    );
                }

                yield return
                    WaitForDuration(
                        frameDuration
                    );
            }

            Destroy(
                renderer.gameObject
            );
        }

        // =========================================
        // DAMAGE
        // =========================================

        private void DamageEnemiesInRadius(
            Vector3 position,
            float radius,
            float damage)
        {
            _physicsResults.Clear();
            _damagedEnemies.Clear();

            Physics2D.OverlapCircle(
                position,
                radius,
                _physicsFilter,
                _physicsResults
            );

            foreach (
                Collider2D hit
                in _physicsResults
            )
            {
                if (hit == null)
                {
                    continue;
                }

                EnemyHealth enemy =
                    hit.GetComponentInParent<EnemyHealth>();

                if (
                    enemy == null ||
                    enemy.IsDead
                )
                {
                    continue;
                }

                // Một enemy có nhiều collider
                // vẫn chỉ ăn damage 1 lần.
                if (!_damagedEnemies.Add(enemy))
                {
                    continue;
                }

                enemy.TakeDamage(
                    damage
                );
            }
        }

        // =========================================
        // CAMERA SHAKE
        // =========================================

        private static void TriggerCameraShake()
        {
            Camera camera =
                Camera.main;

            if (camera == null)
            {
                return;
            }

            JudgementCameraShake shake =
                camera.GetComponent<
                    JudgementCameraShake
                >();

            if (shake == null)
            {
                shake =
                    camera.gameObject
                        .AddComponent<
                            JudgementCameraShake
                        >();
            }

            shake.Shake(
                JudgementSkill.MaxShakeDuration,
                JudgementSkill.MaxShakeStrength
            );
        }

        // =========================================
        // UI
        // =========================================

        private void SetupButton()
        {
            JudgementUltimateButton button =
                GetComponent<
                    JudgementUltimateButton
                >();

            if (button == null)
            {
                button =
                    gameObject.AddComponent<
                        JudgementUltimateButton
                    >();
            }

            button.Initialize(
                this,
                ultimateIcon
            );
        }

        // =========================================
        // CREATE VFX
        // =========================================

        private SpriteRenderer CreateVfxRenderer(
            string objectName,
            Sprite sprite,
            Vector3 position,
            int sortingOrder)
        {
            GameObject obj =
                new GameObject(
                    objectName
                );

            obj.transform.position =
                position;

            SpriteRenderer renderer =
                obj.AddComponent<SpriteRenderer>();

            renderer.sprite =
                sprite;

            renderer.color =
                Color.white;

            renderer.sortingLayerName =
                sortingLayerName;

            renderer.sortingOrder =
                sortingOrder;

            if (_vfxMaterial != null)
            {
                renderer.sharedMaterial =
                    _vfxMaterial;
            }

            return renderer;
        }

        // =========================================
        // FRAME
        // =========================================

        private static void UpdateProgressFrame(
            SpriteRenderer renderer,
            Sprite[] frames,
            float progress)
        {
            if (
                renderer == null ||
                frames == null ||
                frames.Length == 0
            )
            {
                return;
            }

            int index =
                Mathf.FloorToInt(
                    progress *
                    frames.Length
                );

            index =
                Mathf.Clamp(
                    index,
                    0,
                    frames.Length - 1
                );

            Sprite frame =
                frames[index];

            if (frame != null)
            {
                renderer.sprite =
                    frame;
            }
        }

        // =========================================
        // SIZE
        // =========================================

        private static void SetWorldWidth(
            SpriteRenderer renderer,
            float desiredWidth)
        {
            if (
                renderer == null ||
                renderer.sprite == null
            )
            {
                return;
            }

            float width =
                renderer.sprite
                    .bounds
                    .size
                    .x;

            if (width <= 0.001f)
            {
                return;
            }

            float scale =
                desiredWidth /
                width;

            renderer.transform.localScale =
                new Vector3(
                    scale,
                    scale,
                    1f
                );
        }

        private static void SetWorldHeight(
            SpriteRenderer renderer,
            float desiredHeight)
        {
            if (
                renderer == null ||
                renderer.sprite == null
            )
            {
                return;
            }

            float height =
                renderer.sprite
                    .bounds
                    .size
                    .y;

            if (height <= 0.001f)
            {
                return;
            }

            float scale =
                desiredHeight /
                height;

            renderer.transform.localScale =
                new Vector3(
                    scale,
                    scale,
                    1f
                );
        }

        // =========================================
        // SPRITE
        // =========================================

        private static Sprite GetFirstValidSprite(
            Sprite[] frames)
        {
            if (frames == null)
            {
                return null;
            }

            for (
                int i = 0;
                i < frames.Length;
                i++
            )
            {
                if (frames[i] != null)
                {
                    return frames[i];
                }
            }

            return null;
        }

        // =========================================
        // WAIT
        // =========================================

        private static IEnumerator WaitForDuration(
            float duration)
        {
            float timer = 0f;

            while (timer < duration)
            {
                timer +=
                    Time.deltaTime;

                yield return null;
            }
        }

        // =========================================
        // MATERIAL
        // =========================================

        private void CreateVfxMaterial()
        {
            Shader shader =
                Shader.Find(
                    "Universal Render Pipeline/2D/Sprite-Unlit-Default"
                );

            if (shader == null)
            {
                shader =
                    Shader.Find(
                        "Sprites/Default"
                    );
            }

            if (shader == null)
            {
                return;
            }

            _vfxMaterial =
                new Material(
                    shader
                );
        }

        // =========================================
        // CLEANUP
        // =========================================

        private void OnDestroy()
        {
            if (_vfxMaterial != null)
            {
                Destroy(
                    _vfxMaterial
                );
            }
        }
    }
}