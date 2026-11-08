using System.Collections;
using System.Collections.Generic;
using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Player;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Marksman
{
    [RequireComponent(typeof(PlayerSkillManager))]
    [RequireComponent(typeof(PlayerAutoAttack))]
    public sealed class AirStrikeController : MonoBehaviour
    {
        // =========================================
        // ASSETS
        // =========================================

        [Header("Air Strike - Warning")]
        [SerializeField]
        private Sprite warningSprite;

        [Header("Air Strike - Bomb Frames")]
        [SerializeField]
        private Sprite[] bombFrames;

        [Header("Air Strike - Explosion Frames")]
        [SerializeField]
        private Sprite[] explosionFrames;

        [Header("Air Strike MAX - Fire Frames")]
        [SerializeField]
        private Sprite[] fireFieldFrames;

        // =========================================
        // ANIMATION
        // =========================================

        [Header("Animation")]

        [SerializeField, Min(1f)]
        private float explosionFPS = 16f;

        [SerializeField, Min(1f)]
        private float fireFPS = 8f;

        // =========================================
        // VISUAL SIZE
        // =========================================

        [Header("Visual Size")]

        // Kích thước vòng cảnh báo.
        // Chỉ ảnh hưởng hình ảnh, KHÔNG ảnh hưởng vùng damage.
        [SerializeField, Min(0.05f)]
        private float warningVisualMultiplier = 0.55f;

        // Chiều cao quả bom trong world.
        [SerializeField, Min(0.1f)]
        private float bombWorldHeight = 1.5f;

        // Kích thước hình ảnh vụ nổ.
        [SerializeField, Min(0.1f)]
        private float explosionVisualMultiplier = 1.20f;

        // Kích thước hình ảnh bãi lửa.
        [SerializeField, Min(0.1f)]
        private float fireVisualMultiplier = 1.10f;

        // =========================================
        // SORTING
        // =========================================

        [Header("Sorting")]

        [SerializeField]
        private string sortingLayerName = "Default";

        [SerializeField]
        private int warningSortingOrder = 90;

        [SerializeField]
        private int fireSortingOrder = 91;

        [SerializeField]
        private int bombSortingOrder = 100;

        [SerializeField]
        private int explosionSortingOrder = 110;

        // =========================================
        // REFERENCES
        // =========================================

        private PlayerSkillManager _skillManager;

        private PlayerAutoAttack _autoAttack;

        private Material _vfxMaterial;

        // =========================================
        // RUNTIME
        // =========================================

        private float _airStrikeTimer;

        // =========================================
        // UNITY
        // =========================================

        private void Awake()
        {
            _skillManager =
                GetComponent<PlayerSkillManager>();

            _autoAttack =
                GetComponent<PlayerAutoAttack>();

            CreateVfxMaterial();
        }

        private void OnEnable()
        {
            if (_skillManager != null)
            {
                _skillManager.SkillLevelChanged +=
                    OnSkillLevelChanged;
            }
        }

        private void OnDisable()
        {
            if (_skillManager != null)
            {
                _skillManager.SkillLevelChanged -=
                    OnSkillLevelChanged;
            }
        }

        private void Start()
        {
            _airStrikeTimer =
                AirStrikeSkill.Cooldown;
        }

        private void Update()
        {
            if (Time.timeScale <= 0f)
            {
                return;
            }

            UpdateAirStrike();
        }

        // =========================================
        // UPDATE
        // =========================================

        private void UpdateAirStrike()
        {
            if (
                _skillManager == null ||
                _autoAttack == null
            )
            {
                return;
            }

            int level =
                _skillManager.GetSkillLevel(
                    SkillId.AirStrike
                );

            if (level <= 0)
            {
                _airStrikeTimer =
                    AirStrikeSkill.Cooldown;

                return;
            }

            _airStrikeTimer -=
                Time.deltaTime;

            if (_airStrikeTimer > 0f)
            {
                return;
            }

            bool started =
                TriggerAirStrike(level);

            if (started)
            {
                _airStrikeTimer =
                    AirStrikeSkill.Cooldown;
            }
            else
            {
                _airStrikeTimer = 0.25f;
            }
        }

        // =========================================
        // START AIR STRIKE
        // =========================================

        private bool TriggerAirStrike(
            int level)
        {
            int bombCount =
                AirStrikeSkill.GetBombCount(
                    level
                );

            List<Vector3> targets =
                GetRandomTargetPositions(
                    bombCount
                );

            if (targets.Count == 0)
            {
                return false;
            }

            for (
                int i = 0;
                i < bombCount;
                i++
            )
            {
                Vector3 targetPosition =
                    targets[
                        i % targets.Count
                    ];

                float delay =
                    i *
                    AirStrikeSkill.BombStagger;

                StartCoroutine(
                    DropBombRoutine(
                        targetPosition,
                        level,
                        delay
                    )
                );
            }

            return true;
        }

        // =========================================
        // RANDOM TARGET
        // =========================================

        private List<Vector3>
            GetRandomTargetPositions(
                int requestedCount)
        {
            EnemyHealth[] allEnemies =
                FindObjectsByType<EnemyHealth>(
                    FindObjectsSortMode.None
                );

            List<EnemyHealth> candidates =
                new();

            float maxDistanceSquared =
                AirStrikeSkill.TargetRange *
                AirStrikeSkill.TargetRange;

            foreach (
                EnemyHealth enemy
                in allEnemies
            )
            {
                if (
                    enemy == null ||
                    enemy.IsDead
                )
                {
                    continue;
                }

                float distanceSquared =
                    (
                        enemy.transform.position -
                        transform.position
                    ).sqrMagnitude;

                if (
                    distanceSquared >
                    maxDistanceSquared
                )
                {
                    continue;
                }

                candidates.Add(
                    enemy
                );
            }

            List<Vector3> results =
                new();

            if (candidates.Count == 0)
            {
                return results;
            }

            List<EnemyHealth> original =
                new(candidates);

            while (
                results.Count <
                requestedCount
            )
            {
                if (candidates.Count == 0)
                {
                    candidates.AddRange(
                        original
                    );
                }

                int randomIndex =
                    Random.Range(
                        0,
                        candidates.Count
                    );

                EnemyHealth selected =
                    candidates[randomIndex];

                if (
                    selected != null &&
                    !selected.IsDead
                )
                {
                    results.Add(
                        selected.transform.position
                    );
                }

                candidates.RemoveAt(
                    randomIndex
                );
            }

            return results;
        }

        // =========================================
        // BOMB SEQUENCE
        // =========================================

        private IEnumerator DropBombRoutine(
            Vector3 targetPosition,
            int level,
            float startDelay)
        {
            if (startDelay > 0f)
            {
                yield return
                    new WaitForSeconds(
                        startDelay
                    );
            }

            // =====================================
            // WARNING
            // =====================================

            GameObject warningObject =
                null;

            Vector3 warningBaseScale =
                Vector3.one;

            if (warningSprite != null)
            {
                warningObject =
                    CreateSpriteObject(
                        "AirStrike_Warning",
                        warningSprite,
                        targetPosition,
                        warningSortingOrder
                    );

                SpriteRenderer warningRenderer =
                    warningObject
                        .GetComponent<SpriteRenderer>();

                float radius =
                    AirStrikeSkill
                        .GetExplosionRadius(
                            level
                        );

                // =================================
                // VÒNG CẢNH BÁO NHỎ HƠN
                // =================================

                SetWorldWidth(
                    warningRenderer,
                    radius *
                    2f *
                    warningVisualMultiplier
                );

                warningBaseScale =
                    warningObject
                        .transform
                        .localScale;
            }

            float warningTimer = 0f;

            while (
                warningTimer <
                AirStrikeSkill.WarningDuration
            )
            {
                warningTimer +=
                    Time.deltaTime;

                if (warningObject != null)
                {
                    float pulse =
                        1f +
                        Mathf.Sin(
                            warningTimer *
                            14f
                        ) *
                        0.08f;

                    warningObject
                        .transform
                        .localScale =
                        warningBaseScale *
                        pulse;

                    warningObject
                        .transform
                        .Rotate(
                            0f,
                            0f,
                            70f *
                            Time.deltaTime
                        );
                }

                yield return null;
            }

            if (warningObject != null)
            {
                Destroy(
                    warningObject
                );
            }

            // =====================================
            // BOMB
            // =====================================

            Vector3 bombStartPosition =
                targetPosition +
                Vector3.up *
                AirStrikeSkill
                    .BombStartHeight;

            GameObject bombObject =
                null;

            SpriteRenderer bombRenderer =
                null;

            if (
                bombFrames != null &&
                bombFrames.Length > 0 &&
                bombFrames[0] != null
            )
            {
                bombObject =
                    CreateSpriteObject(
                        "AirStrike_Bomb",
                        bombFrames[0],
                        bombStartPosition,
                        bombSortingOrder
                    );

                bombRenderer =
                    bombObject
                        .GetComponent<SpriteRenderer>();
            }

            float fallTimer = 0f;

            while (
                fallTimer <
                AirStrikeSkill.BombFallDuration
            )
            {
                fallTimer +=
                    Time.deltaTime;

                float progress =
                    Mathf.Clamp01(
                        fallTimer /
                        AirStrikeSkill.BombFallDuration
                    );

                if (bombObject != null)
                {
                    bombObject
                        .transform
                        .position =
                        Vector3.Lerp(
                            bombStartPosition,
                            targetPosition,
                            progress
                        );

                    UpdateBombFrame(
                        bombRenderer,
                        progress
                    );

                    float currentHeight =
                        Mathf.Lerp(
                            bombWorldHeight *
                            0.65f,
                            bombWorldHeight,
                            progress
                        );

                    SetWorldHeight(
                        bombRenderer,
                        currentHeight
                    );
                }

                yield return null;
            }

            if (bombObject != null)
            {
                Destroy(
                    bombObject
                );
            }

            Explode(
                targetPosition,
                level
            );
        }

        // =========================================
        // BOMB ANIMATION
        // =========================================

        private void UpdateBombFrame(
            SpriteRenderer renderer,
            float progress)
        {
            if (
                renderer == null ||
                bombFrames == null ||
                bombFrames.Length == 0
            )
            {
                return;
            }

            int frameIndex =
                Mathf.FloorToInt(
                    progress *
                    bombFrames.Length
                );

            frameIndex =
                Mathf.Clamp(
                    frameIndex,
                    0,
                    bombFrames.Length - 1
                );

            Sprite frame =
                bombFrames[
                    frameIndex
                ];

            if (frame != null)
            {
                renderer.sprite =
                    frame;
            }
        }

        // =========================================
        // EXPLOSION
        // =========================================

        private void Explode(
            Vector3 position,
            int level)
        {
            float radius =
                AirStrikeSkill
                    .GetExplosionRadius(
                        level
                    );

            float damage =
                _autoAttack.AttackDamage *
                AirStrikeSkill
                    .GetDamageMultiplier(
                        level
                    );

            DamageEnemiesInRadius(
                position,
                radius,
                damage
            );

            if (
                explosionFrames != null &&
                explosionFrames.Length > 0
            )
            {
                StartCoroutine(
                    PlayExplosionRoutine(
                        position,
                        radius
                    )
                );
            }

            if (
                AirStrikeSkill.IsMaxForm(
                    level
                )
            )
            {
                StartCoroutine(
                    FireFieldRoutine(
                        position
                    )
                );
            }
        }

        // =========================================
        // EXPLOSION VFX
        // =========================================

        private IEnumerator
            PlayExplosionRoutine(
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

            GameObject explosionObject =
                CreateSpriteObject(
                    "AirStrike_Explosion",
                    firstFrame,
                    position,
                    explosionSortingOrder
                );

            SpriteRenderer renderer =
                explosionObject
                    .GetComponent<SpriteRenderer>();

            float visualDiameter =
                radius *
                2f *
                explosionVisualMultiplier;

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
                        visualDiameter
                    );
                }

                yield return
                    new WaitForSeconds(
                        frameDuration
                    );
            }

            Destroy(
                explosionObject
            );
        }

        // =========================================
        // FIRE FIELD
        // =========================================

        private IEnumerator FireFieldRoutine(
            Vector3 position)
        {
            float radius =
                AirStrikeSkill
                    .GetFireRadius(
                        5
                    );

            float fireDamage =
                _autoAttack.AttackDamage *
                AirStrikeSkill
                    .FireDamageMultiplier;

            GameObject fireObject =
                null;

            SpriteRenderer renderer =
                null;

            Sprite firstFrame =
                GetFirstValidSprite(
                    fireFieldFrames
                );

            if (firstFrame != null)
            {
                fireObject =
                    CreateSpriteObject(
                        "AirStrike_FireField_MAX",
                        firstFrame,
                        position,
                        fireSortingOrder
                    );

                renderer =
                    fireObject
                        .GetComponent<SpriteRenderer>();

                SetWorldWidth(
                    renderer,
                    radius *
                    2f *
                    fireVisualMultiplier
                );
            }

            float lifeTimer = 0f;

            float tickTimer =
                AirStrikeSkill
                    .FireTickInterval;

            float animationTimer = 0f;

            int frameIndex = 0;

            while (
                lifeTimer <
                AirStrikeSkill.FireDuration
            )
            {
                float deltaTime =
                    Time.deltaTime;

                lifeTimer +=
                    deltaTime;

                tickTimer -=
                    deltaTime;

                animationTimer +=
                    deltaTime;

                if (tickTimer <= 0f)
                {
                    tickTimer +=
                        AirStrikeSkill
                            .FireTickInterval;

                    DamageEnemiesInRadius(
                        position,
                        radius,
                        fireDamage
                    );
                }

                if (
                    renderer != null &&
                    fireFieldFrames != null &&
                    fireFieldFrames.Length > 0
                )
                {
                    float frameDuration =
                        1f /
                        Mathf.Max(
                            1f,
                            fireFPS
                        );

                    if (
                        animationTimer >=
                        frameDuration
                    )
                    {
                        animationTimer -=
                            frameDuration;

                        frameIndex++;

                        if (
                            frameIndex >=
                            fireFieldFrames.Length
                        )
                        {
                            frameIndex = 0;
                        }

                        Sprite frame =
                            fireFieldFrames[
                                frameIndex
                            ];

                        if (frame != null)
                        {
                            renderer.sprite =
                                frame;

                            SetWorldWidth(
                                renderer,
                                radius *
                                2f *
                                fireVisualMultiplier
                            );
                        }
                    }
                }

                yield return null;
            }

            if (fireObject != null)
            {
                Destroy(
                    fireObject
                );
            }
        }

        // =========================================
        // DAMAGE
        // =========================================

        private static void DamageEnemiesInRadius(
            Vector3 position,
            float radius,
            float damage)
        {
            Collider2D[] hits =
                Physics2D.OverlapCircleAll(
                    position,
                    radius
                );

            HashSet<EnemyHealth> damagedEnemies =
                new();

            foreach (
                Collider2D hit
                in hits
            )
            {
                EnemyHealth enemy =
                    hit.GetComponentInParent<EnemyHealth>();

                if (
                    enemy == null ||
                    enemy.IsDead
                )
                {
                    continue;
                }

                if (
                    damagedEnemies.Contains(
                        enemy
                    )
                )
                {
                    continue;
                }

                damagedEnemies.Add(
                    enemy
                );

                enemy.TakeDamage(
                    damage
                );
            }
        }

        // =========================================
        // LEVEL CHANGE
        // =========================================

        private void OnSkillLevelChanged(
            SkillId skillId,
            int level)
        {
            if (
                skillId !=
                SkillId.AirStrike
            )
            {
                return;
            }

            _airStrikeTimer = 0f;
        }

        // =========================================
        // CREATE SPRITE
        // =========================================

        private GameObject CreateSpriteObject(
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

            return obj;
        }

        // =========================================
        // WORLD SIZE
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

            float spriteWidth =
                renderer.sprite
                    .bounds
                    .size
                    .x;

            if (spriteWidth <= 0.001f)
            {
                return;
            }

            float scale =
                desiredWidth /
                spriteWidth;

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

            float spriteHeight =
                renderer.sprite
                    .bounds
                    .size
                    .y;

            if (spriteHeight <= 0.001f)
            {
                return;
            }

            float scale =
                desiredHeight /
                spriteHeight;

            renderer.transform.localScale =
                new Vector3(
                    scale,
                    scale,
                    1f
                );
        }

        // =========================================
        // HELPERS
        // =========================================

        private static Sprite GetFirstValidSprite(
            Sprite[] frames)
        {
            if (frames == null)
            {
                return null;
            }

            foreach (
                Sprite frame
                in frames
            )
            {
                if (frame != null)
                {
                    return frame;
                }
            }

            return null;
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

            if (shader != null)
            {
                _vfxMaterial =
                    new Material(
                        shader
                    );
            }
        }

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