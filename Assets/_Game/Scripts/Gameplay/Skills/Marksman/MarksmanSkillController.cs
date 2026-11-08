using System.Collections.Generic;
using DienGame.Gameplay.Combat;
using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Player;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Marksman
{
    [RequireComponent(typeof(PlayerSkillManager))]
    public sealed class MarksmanSkillController : MonoBehaviour
    {
        // =========================================
        // THUNDER NORMAL
        // =========================================

        [Header("Thunder - Normal Yellow")]

        [SerializeField]
        private Sprite[] thunderNormalFrames;

        [Header("Thunder - MAX Purple")]

        [SerializeField]
        private Sprite[] thunderMaxFrames;

        [Header("Thunder Visual Settings")]

        [SerializeField, Min(1f)]
        private float thunderAnimationFPS = 12f;

        [SerializeField, Min(0.01f)]
        private float thunderVisualDuration = 0.15f;

        [SerializeField, Min(0.01f)]
        private float thunderWidthScale = 1f;

        [SerializeField]
        private int thunderSortingOrder = 40;

        // =========================================
        // THUNDER DRAGON MAX
        // =========================================

        [Header("Thunder Dragon MAX")]

        [SerializeField]
        private Sprite[] thunderDragonFrames;

        [SerializeField, Min(1f)]
        private float thunderDragonAnimationFPS = 10f;

        [SerializeField, Min(0.01f)]
        private float thunderDragonVisualScale = 1f;

        [SerializeField, Range(0.05f, 1f)]
        private float thunderDragonHitboxScale = 0.45f;

        [SerializeField]
        private int thunderDragonSortingOrder = 35;

        // =========================================
        // DRONE
        // =========================================

        [Header("Drone Normal Animation")]

        [SerializeField]
        private Sprite[] normalDroneFrames;

        [Header("Drone MAX Animation")]

        [SerializeField]
        private Sprite[] maxDroneFrames;

        [Header("Drone Animation")]

        [SerializeField, Min(1f)]
        private float droneAnimationFPS = 8f;

        [SerializeField]
        private int droneSortingOrder = 20;

        // =========================================
        // REFERENCES
        // =========================================

        private PlayerSkillManager
            _skillManager;

        private PlayerAutoAttack
            _autoAttack;

        // =========================================
        // PIERCING
        // =========================================

        private float _piercingBulletTimer;

        // =========================================
        // THUNDER
        // =========================================

        private int _normalShotCounter;

        private float _dragonTimer;

        // =========================================
        // DRONE
        // =========================================

        private readonly List<DroneRuntime>
            _drones = new();

        private float _droneOrbitAngle;

        private int _lastDroneLevel = -1;

        // =========================================
        // UNITY
        // =========================================

        private void Awake()
        {
            _skillManager =
                GetComponent<PlayerSkillManager>();

            _autoAttack =
                GetComponent<PlayerAutoAttack>();
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
            _piercingBulletTimer =
                PiercingBulletSkill.Cooldown;

            _dragonTimer =
                ThunderBulletSkill.DragonCooldown;

            RefreshDroneSystem();
        }

        private void Update()
        {
            UpdatePiercingBullet();

            UpdateThunderDragon();

            UpdateDrones();
        }

        // =========================================
        // PIERCING BULLET
        // =========================================

        private void UpdatePiercingBullet()
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
                    SkillId.PiercingBullet
                );

            if (level <= 0)
            {
                _piercingBulletTimer =
                    PiercingBulletSkill.Cooldown;

                return;
            }

            _piercingBulletTimer -=
                Time.deltaTime;

            if (_piercingBulletTimer > 0f)
            {
                return;
            }

            _piercingBulletTimer =
                PiercingBulletSkill.Cooldown;

            FirePiercingBullet(
                level
            );
        }

        private void FirePiercingBullet(
            int level)
        {
            EnemyHealth target =
                FindNearestEnemy(
                    transform.position,
                    PiercingBulletSkill.TargetRange,
                    null
                );

            if (target == null)
            {
                return;
            }

            GameObject projectilePrefab =
                _autoAttack.ProjectilePrefab;

            if (projectilePrefab == null)
            {
                return;
            }

            Vector2 direction =
                (Vector2)target.transform.position -
                (Vector2)transform.position;

            if (
                direction.sqrMagnitude <=
                0.0001f
            )
            {
                return;
            }

            direction.Normalize();

            Vector2 spawnPosition =
                (Vector2)transform.position +
                direction *
                _autoAttack.ProjectileSpawnDistance;

            GameObject projectileObject =
                Instantiate(
                    projectilePrefab,
                    spawnPosition,
                    Quaternion.identity
                );

            if (
                !projectileObject.TryGetComponent(
                    out Projectile projectile
                )
            )
            {
                Destroy(projectileObject);

                return;
            }

            float damage =
                _autoAttack.AttackDamage *
                PiercingBulletSkill
                    .GetDamageMultiplier(
                        level
                    );

            projectile.InitializePiercing(
                direction,
                PiercingBulletSkill.ProjectileSpeed,
                damage,
                level
            );
        }

        // =========================================
        // THUNDER SHOT COUNTER
        // =========================================

        public int RegisterNormalShotAndGetThunderLevel()
        {
            if (_skillManager == null)
            {
                return 0;
            }

            int level =
                _skillManager.GetSkillLevel(
                    SkillId.ThunderBullet
                );

            if (level <= 0)
            {
                _normalShotCounter = 0;

                return 0;
            }

            _normalShotCounter++;

            int requiredShots =
                ThunderBulletSkill
                    .GetShotsPerTrigger(
                        level
                    );

            if (
                _normalShotCounter <
                requiredShots
            )
            {
                return 0;
            }

            _normalShotCounter = 0;

            // Projectile sẽ giữ level này.
            // Sét chỉ thực sự xảy ra khi nó HIT enemy.
            return level;
        }

        // =========================================
        // THUNDER HIT
        // =========================================

        public void TriggerThunderFromHit(
            EnemyHealth firstEnemy,
            float baseDamage,
            int thunderLevel)
        {
            if (
                firstEnemy == null ||
                firstEnemy.IsDead ||
                thunderLevel <= 0
            )
            {
                return;
            }

            int maxTargets =
                ThunderBulletSkill
                    .GetChainTargets(
                        thunderLevel
                    );

            float chainRange =
                ThunderBulletSkill
                    .GetChainRange(
                        thunderLevel
                    );

            float lightningDamage =
                baseDamage *
                ThunderBulletSkill
                    .GetDamageMultiplier(
                        thunderLevel
                    );

            var hitEnemies =
                new HashSet<EnemyHealth>();

            EnemyHealth currentEnemy =
                firstEnemy;

            for (
                int targetIndex = 0;
                targetIndex < maxTargets;
                targetIndex++
            )
            {
                if (
                    currentEnemy == null ||
                    currentEnemy.IsDead
                )
                {
                    break;
                }

                // Sét gây damage lên enemy hiện tại.
                currentEnemy.TakeDamage(
                    lightningDamage
                );

                hitEnemies.Add(
                    currentEnemy
                );

                // Đã đủ số mục tiêu.
                if (
                    targetIndex >=
                    maxTargets - 1
                )
                {
                    break;
                }

                EnemyHealth nextEnemy =
                    FindNearestEnemy(
                        currentEnemy.transform.position,
                        chainRange,
                        hitEnemies
                    );

                if (nextEnemy == null)
                {
                    break;
                }

                // Visual nối:
                // Enemy 1 -> Enemy 2
                // Enemy 2 -> Enemy 3...
                SpawnThunderLightning(
                    currentEnemy.transform.position,
                    nextEnemy.transform.position,
                    thunderLevel
                );

                currentEnemy =
                    nextEnemy;
            }
        }

        private void SpawnThunderLightning(
            Vector3 from,
            Vector3 to,
            int level)
        {
            bool maxForm =
                ThunderBulletSkill
                    .IsMaxForm(
                        level
                    );

            Sprite[] frames =
                maxForm
                    ? thunderMaxFrames
                    : thunderNormalFrames;

            if (
                frames == null ||
                frames.Length == 0
            )
            {
                return;
            }

            GameObject lightningObject =
                new GameObject(
                    maxForm
                        ? "ThunderLightning_MAX"
                        : "ThunderLightning"
                );

            ThunderLightningVisual visual =
                lightningObject.AddComponent<
                    ThunderLightningVisual
                >();

            visual.Initialize(
                frames,
                thunderAnimationFPS,
                from,
                to,
                thunderWidthScale,
                thunderVisualDuration,
                thunderSortingOrder
            );
        }

        // =========================================
        // MAX THUNDER DRAGON
        // =========================================

        private void UpdateThunderDragon()
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
                    SkillId.ThunderBullet
                );

            if (
                !ThunderBulletSkill
                    .IsMaxForm(
                        level
                    )
            )
            {
                _dragonTimer =
                    ThunderBulletSkill
                        .DragonCooldown;

                return;
            }

            _dragonTimer -=
                Time.deltaTime;

            if (_dragonTimer > 0f)
            {
                return;
            }

            _dragonTimer =
                ThunderBulletSkill
                    .DragonCooldown;

            SpawnThunderDragon();
        }

        private void SpawnThunderDragon()
        {
            if (
                thunderDragonFrames == null ||
                thunderDragonFrames.Length == 0
            )
            {
                Debug.LogWarning(
                    "MarksmanSkillController: Chưa gán Thunder Dragon Frames.",
                    this
                );

                return;
            }

            float range =
                ThunderBulletSkill
                    .DragonFlightRange;

            Vector2 randomDirection =
                Random.insideUnitCircle;

            if (
                randomDirection.sqrMagnitude <=
                0.001f
            )
            {
                randomDirection =
                    Vector2.right;
            }

            randomDirection.Normalize();

            Vector2 perpendicular =
                new Vector2(
                    -randomDirection.y,
                    randomDirection.x
                );

            // Đường bay không bắt buộc đi qua chính giữa Player.
            float randomOffset =
                Random.Range(
                    -range * 0.5f,
                    range * 0.5f
                );

            Vector2 flightCenter =
                (Vector2)transform.position +
                perpendicular *
                randomOffset;

            Vector2 startPosition =
                flightCenter -
                randomDirection *
                range;

            Vector2 endPosition =
                flightCenter +
                randomDirection *
                range;

            float damage =
                _autoAttack.AttackDamage *
                ThunderBulletSkill
                    .DragonDamageMultiplier;

            GameObject dragonObject =
                new GameObject(
                    "ThunderDragon_MAX"
                );

            ThunderDragonVisual dragon =
                dragonObject.AddComponent<
                    ThunderDragonVisual
                >();

            dragon.Initialize(
                thunderDragonFrames,
                thunderDragonAnimationFPS,
                startPosition,
                endPosition,
                ThunderBulletSkill
                    .DragonMoveSpeed,
                damage,
                thunderDragonVisualScale,
                thunderDragonHitboxScale,
                thunderDragonSortingOrder
            );
        }

        // =========================================
        // SKILL LEVEL CHANGE
        // =========================================

        private void OnSkillLevelChanged(
            SkillId skillId,
            int level)
        {
            if (
                skillId ==
                SkillId.PiercingBullet
            )
            {
                _piercingBulletTimer = 0f;
            }

            if (
                skillId ==
                SkillId.ThunderBullet
            )
            {
                _normalShotCounter = 0;

                _dragonTimer =
                    ThunderBulletSkill
                        .DragonCooldown;
            }

            if (
                skillId ==
                SkillId.CombatDrone
            )
            {
                RefreshDroneSystem();
            }
        }

        // =========================================
        // DRONE
        // =========================================

        private void RefreshDroneSystem()
        {
            if (_skillManager == null)
            {
                return;
            }

            int level =
                _skillManager.GetSkillLevel(
                    SkillId.CombatDrone
                );

            if (
                level ==
                _lastDroneLevel
            )
            {
                return;
            }

            _lastDroneLevel =
                level;

            RebuildDrones(level);
        }

        private void RebuildDrones(
            int level)
        {
            DestroyAllDrones();

            int droneCount =
                CombatDroneSkill
                    .GetDroneCount(
                        level
                    );

            if (droneCount <= 0)
            {
                return;
            }

            bool maxForm =
                CombatDroneSkill
                    .IsMaxForm(
                        level
                    );

            for (
                int i = 0;
                i < droneCount;
                i++
            )
            {
                CreateDrone(
                    level,
                    maxForm
                );
            }
        }

        private void CreateDrone(
            int level,
            bool maxForm)
        {
            GameObject droneObject =
                new GameObject(
                    maxForm
                        ? "EliteCombatDrone"
                        : "CombatDrone"
                );

            droneObject.transform.SetParent(
                transform,
                false
            );

            SpriteRenderer renderer =
                droneObject.AddComponent<
                    SpriteRenderer
                >();

            renderer.sortingOrder =
                droneSortingOrder;

            CombatDroneVisual visual =
                droneObject.AddComponent<
                    CombatDroneVisual
                >();

            Sprite[] frames =
                maxForm
                    ? maxDroneFrames
                    : normalDroneFrames;

            visual.Initialize(
                frames,
                droneAnimationFPS,
                CombatDroneSkill
                    .GetVisualScale(
                        level
                    )
            );

            DroneRuntime runtime =
                new DroneRuntime
                {
                    Transform =
                        droneObject.transform,

                    Visual =
                        visual,

                    AttackTimer =
                        Random.Range(
                            0f,
                            CombatDroneSkill
                                .GetAttackInterval(
                                    level
                                )
                        )
                };

            _drones.Add(runtime);
        }

        private void DestroyAllDrones()
        {
            foreach (
                DroneRuntime drone
                in _drones
            )
            {
                if (
                    drone != null &&
                    drone.Transform != null
                )
                {
                    Destroy(
                        drone.Transform.gameObject
                    );
                }
            }

            _drones.Clear();
        }

        private void UpdateDrones()
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
                    SkillId.CombatDrone
                );

            if (level <= 0)
            {
                return;
            }

            if (
                level !=
                _lastDroneLevel
            )
            {
                RefreshDroneSystem();
            }

            if (_drones.Count == 0)
            {
                return;
            }

            _droneOrbitAngle +=
                CombatDroneSkill
                    .GetOrbitSpeed(
                        level
                    ) *
                Time.deltaTime;

            for (
                int i = 0;
                i < _drones.Count;
                i++
            )
            {
                DroneRuntime drone =
                    _drones[i];

                if (
                    drone == null ||
                    drone.Transform == null
                )
                {
                    continue;
                }

                UpdateDronePosition(
                    drone,
                    i,
                    level
                );

                UpdateDroneAttack(
                    drone,
                    level
                );
            }
        }

        private void UpdateDronePosition(
            DroneRuntime drone,
            int index,
            int level)
        {
            float spacing =
                360f /
                Mathf.Max(
                    1,
                    _drones.Count
                );

            float angle =
                _droneOrbitAngle +
                spacing *
                index;

            float radians =
                angle *
                Mathf.Deg2Rad;

            float radius =
                CombatDroneSkill
                    .GetOrbitRadius(
                        level
                    );

            drone.Transform.localPosition =
                new Vector3(
                    Mathf.Cos(radians) *
                    radius,

                    Mathf.Sin(radians) *
                    radius,

                    0f
                );
        }

        private void UpdateDroneAttack(
            DroneRuntime drone,
            int level)
        {
            drone.AttackTimer -=
                Time.deltaTime;

            if (drone.AttackTimer > 0f)
            {
                return;
            }

            EnemyHealth target =
                FindNearestEnemy(
                    drone.Transform.position,
                    CombatDroneSkill
                        .GetAttackRange(
                            level
                        ),
                    null
                );

            if (target == null)
            {
                return;
            }

            if (drone.Visual != null)
            {
                drone.Visual.AimAt(
                    target.transform.position
                );
            }

            float damage =
                _autoAttack.AttackDamage *
                CombatDroneSkill
                    .GetDamageMultiplier(
                        level
                    );

            FireDroneWeapon(
                drone,
                target,
                damage,
                level
            );

            drone.AttackTimer =
                CombatDroneSkill
                    .GetAttackInterval(
                        level
                    );
        }

        private void FireDroneWeapon(
            DroneRuntime drone,
            EnemyHealth target,
            float damage,
            int level)
        {
            Vector3 muzzlePosition =
                drone.Visual != null
                    ? drone.Visual.MuzzlePosition
                    : drone.Transform.position;

            bool maxForm =
                CombatDroneSkill
                    .IsMaxForm(
                        level
                    );

            if (!maxForm)
            {
                DrawTracer(
                    muzzlePosition,
                    target.transform.position,
                    0.07f,
                    0.045f
                );

                target.TakeDamage(
                    damage
                );

                return;
            }

            Vector3 direction =
                (
                    target.transform.position -
                    muzzlePosition
                ).normalized;

            Vector3 perpendicular =
                new Vector3(
                    -direction.y,
                    direction.x,
                    0f
                );

            const float barrelSpacing =
                0.07f;

            DrawTracer(
                muzzlePosition +
                perpendicular *
                barrelSpacing,
                target.transform.position,
                0.10f,
                0.05f
            );

            DrawTracer(
                muzzlePosition -
                perpendicular *
                barrelSpacing,
                target.transform.position,
                0.10f,
                0.05f
            );

            target.TakeDamage(
                damage
            );
        }

        // =========================================
        // FIND NEAREST ENEMY
        // =========================================

        private static EnemyHealth FindNearestEnemy(
            Vector3 origin,
            float maxRange,
            HashSet<EnemyHealth> excluded)
        {
            EnemyHealth[] enemies =
                FindObjectsByType<
                    EnemyHealth
                >(
                    FindObjectsSortMode.None
                );

            EnemyHealth nearestEnemy =
                null;

            float nearestDistanceSquared =
                maxRange *
                maxRange;

            foreach (
                EnemyHealth enemy
                in enemies
            )
            {
                if (
                    enemy == null ||
                    enemy.IsDead
                )
                {
                    continue;
                }

                if (
                    excluded != null &&
                    excluded.Contains(
                        enemy
                    )
                )
                {
                    continue;
                }

                float distanceSquared =
                    (
                        enemy.transform.position -
                        origin
                    ).sqrMagnitude;

                if (
                    distanceSquared >
                    nearestDistanceSquared
                )
                {
                    continue;
                }

                nearestDistanceSquared =
                    distanceSquared;

                nearestEnemy =
                    enemy;
            }

            return nearestEnemy;
        }

        // =========================================
        // DRONE TRACER
        // =========================================

        private static void DrawTracer(
            Vector3 from,
            Vector3 to,
            float duration,
            float width)
        {
            GameObject tracer =
                new GameObject(
                    "DroneTracer"
                );

            LineRenderer line =
                tracer.AddComponent<
                    LineRenderer
                >();

            line.positionCount = 2;

            line.useWorldSpace = true;

            line.SetPosition(
                0,
                from
            );

            line.SetPosition(
                1,
                to
            );

            line.startWidth =
                width;

            line.endWidth =
                width *
                0.35f;

            Shader shader =
                Shader.Find(
                    "Sprites/Default"
                );

            if (shader != null)
            {
                line.material =
                    new Material(
                        shader
                    );
            }

            line.sortingOrder = 30;

            Destroy(
                tracer,
                duration
            );
        }

        private void OnDestroy()
        {
            DestroyAllDrones();
        }

        private sealed class DroneRuntime
        {
            public Transform Transform;

            public CombatDroneVisual Visual;

            public float AttackTimer;
        }
    }
}