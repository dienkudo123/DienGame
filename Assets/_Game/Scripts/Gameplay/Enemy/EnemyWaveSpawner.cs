using DienGame.Gameplay.Map;
using DienGame.Gameplay.Player;
using UnityEngine;

namespace DienGame.Gameplay.Enemy
{
    public sealed class EnemyWaveSpawner : MonoBehaviour
    {
        // =========================================
        // REFERENCES
        // =========================================

        [Header("References")]

        [SerializeField]
        private GameObject enemyPrefab;

        [SerializeField]
        private GameObject experienceOrbPrefab;

        [SerializeField, Min(1)]
        private int experiencePerEnemy = 10;

        // Giữ lại để tương thích code cũ.
        // Nếu có RectangularMapBoundary thì MapBoundary
        // sẽ được ưu tiên.
        [SerializeField]
        private Collider2D mapBounds;

        [SerializeField]
        private RectangularMapBoundary mapBoundary;

        // =========================================
        // WAVE
        // =========================================

        [Header("Wave")]

        [SerializeField, Min(1)]
        private int enemiesPerWave = 6;

        [SerializeField, Min(0.1f)]
        private float waveInterval = 5f;

        [SerializeField, Min(0f)]
        private float firstWaveDelay = 1f;

        // =========================================
        // SPAWN
        // =========================================

        [Header("Spawn")]

        [SerializeField, Min(0f)]
        private float safeDistanceFromPlayer = 5f;

        [Tooltip(
            "Khoảng cách spawn tính từ phía trong tường. " +
            "Tăng nếu quái vẫn sinh sát hoặc ngoài tường."
        )]
        [SerializeField, Min(0f)]
        private float boundaryPadding = 1.5f;

        [SerializeField, Min(0f)]
        private float zoneSpread = 1.5f;

        // =========================================
        // RUNTIME
        // =========================================

        private Transform _player;

        private float _spawnTimer;

        private bool _canSpawn = true;

        // =========================================
        // COMPATIBILITY
        // =========================================

        public GameObject EnemyPrefab =>
            enemyPrefab;

        public Collider2D MapBounds =>
            mapBounds;

        // =========================================
        // UNITY
        // =========================================

        private void Start()
        {
            ResolveReferences();

            _spawnTimer =
                firstWaveDelay;

            ValidateSetup();
        }

        private void Update()
        {
            if (!_canSpawn)
            {
                return;
            }

            if (enemyPrefab == null)
            {
                return;
            }

            if (
                mapBoundary == null &&
                mapBounds == null
            )
            {
                return;
            }

            _spawnTimer -=
                Time.deltaTime;

            if (_spawnTimer > 0f)
            {
                return;
            }

            SpawnWave();

            _spawnTimer =
                waveInterval;
        }

        // =========================================
        // REFERENCES
        // =========================================

        private void ResolveReferences()
        {
            if (mapBoundary == null)
            {
                mapBoundary =
                    FindFirstObjectByType<
                        RectangularMapBoundary
                    >();
            }

            PlayerMovement player =
                FindFirstObjectByType<
                    PlayerMovement
                >();

            if (player != null)
            {
                _player =
                    player.transform;
            }
        }

        // =========================================
        // VALIDATION
        // =========================================

        private void ValidateSetup()
        {
            if (enemyPrefab == null)
            {
                Debug.LogError(
                    "EnemyWaveSpawner: " +
                    "Chưa gán Enemy Prefab.",
                    this
                );
            }

            if (experienceOrbPrefab == null)
            {
                Debug.LogError(
                    "EnemyWaveSpawner: " +
                    "Chưa gán Experience Orb Prefab. " +
                    "Enemy sẽ không thể rơi EXP.",
                    this
                );
            }

            if (
                mapBoundary == null &&
                mapBounds == null
            )
            {
                Debug.LogError(
                    "EnemyWaveSpawner: " +
                    "Không tìm thấy RectangularMapBoundary " +
                    "hoặc Map Bounds.",
                    this
                );
            }
        }

        // =========================================
        // WAVE
        // =========================================

        private void SpawnWave()
        {
            Bounds bounds =
                GetSpawnBounds();

            int edge =
                Random.Range(
                    0,
                    4
                );

            float zoneCenter =
                GetZoneCenter(
                    bounds,
                    edge
                );

            for (
                int i = 0;
                i < enemiesPerWave;
                i++
            )
            {
                Vector2 spawnPosition =
                    FindSpawnPosition(
                        bounds,
                        edge,
                        zoneCenter
                    );

                SpawnEnemy(
                    spawnPosition
                );
            }
        }

        // =========================================
        // SPAWN ENEMY
        // =========================================

        private void SpawnEnemy(
            Vector2 spawnPosition)
        {
            GameObject enemyObject =
                Instantiate(
                    enemyPrefab,
                    spawnPosition,
                    Quaternion.identity
                );

            if (enemyObject == null)
            {
                Debug.LogError(
                    "EnemyWaveSpawner: " +
                    "Không thể tạo Enemy.",
                    this
                );

                return;
            }

            SetupExperienceDrop(
                enemyObject
            );
        }

        // =========================================
        // EXPERIENCE DROP
        // =========================================

        private void SetupExperienceDrop(
            GameObject enemyObject)
        {
            if (enemyObject == null)
            {
                return;
            }

            if (experienceOrbPrefab == null)
            {
                return;
            }

            EnemyExperienceDrop experienceDrop =
                enemyObject.GetComponent<
                    EnemyExperienceDrop
                >();

            if (experienceDrop == null)
            {
                experienceDrop =
                    enemyObject.AddComponent<
                        EnemyExperienceDrop
                    >();
            }

            experienceDrop.Initialize(
                experienceOrbPrefab,
                experiencePerEnemy
            );
        }

        // =========================================
        // MAP BOUNDS
        // =========================================

        private Bounds GetSpawnBounds()
        {
            // =====================================
            // MAP MỚI
            // =====================================

            if (mapBoundary != null)
            {
                Vector2 size =
                    mapBoundary.MapSize;

                Vector3 center =
                    mapBoundary
                        .transform
                        .position;

                return new Bounds(
                    center,
                    new Vector3(
                        size.x,
                        size.y,
                        0f
                    )
                );
            }

            // =====================================
            // FALLBACK MAP CŨ
            // =====================================

            if (mapBounds != null)
            {
                return
                    mapBounds.bounds;
            }

            return new Bounds(
                Vector3.zero,
                Vector3.one
            );
        }

        // =========================================
        // ZONE
        // =========================================

        private float GetZoneCenter(
            Bounds bounds,
            int edge)
        {
            float minX =
                bounds.min.x +
                boundaryPadding;

            float maxX =
                bounds.max.x -
                boundaryPadding;

            float minY =
                bounds.min.y +
                boundaryPadding;

            float maxY =
                bounds.max.y -
                boundaryPadding;

            bool horizontalEdge =
                edge == 0 ||
                edge == 2;

            if (horizontalEdge)
            {
                return Random.Range(
                    minX,
                    maxX
                );
            }

            return Random.Range(
                minY,
                maxY
            );
        }

        // =========================================
        // FIND POSITION
        // =========================================

        private Vector2 FindSpawnPosition(
            Bounds bounds,
            int edge,
            float zoneCenter)
        {
            const int MaxAttempts =
                20;

            Vector2 position =
                bounds.center;

            for (
                int i = 0;
                i < MaxAttempts;
                i++
            )
            {
                position =
                    GeneratePosition(
                        bounds,
                        edge,
                        zoneCenter
                    );

                if (
                    IsFarEnoughFromPlayer(
                        position
                    )
                )
                {
                    return position;
                }
            }

            return position;
        }

        // =========================================
        // GENERATE EDGE POSITION
        // =========================================

        private Vector2 GeneratePosition(
            Bounds bounds,
            int edge,
            float zoneCenter)
        {
            float minX =
                bounds.min.x +
                boundaryPadding;

            float maxX =
                bounds.max.x -
                boundaryPadding;

            float minY =
                bounds.min.y +
                boundaryPadding;

            float maxY =
                bounds.max.y -
                boundaryPadding;

            float spread =
                Random.Range(
                    -zoneSpread,
                    zoneSpread
                );

            switch (edge)
            {
                // TOP
                case 0:
                {
                    return new Vector2(
                        Mathf.Clamp(
                            zoneCenter + spread,
                            minX,
                            maxX
                        ),
                        maxY
                    );
                }

                // RIGHT
                case 1:
                {
                    return new Vector2(
                        maxX,
                        Mathf.Clamp(
                            zoneCenter + spread,
                            minY,
                            maxY
                        )
                    );
                }

                // BOTTOM
                case 2:
                {
                    return new Vector2(
                        Mathf.Clamp(
                            zoneCenter + spread,
                            minX,
                            maxX
                        ),
                        minY
                    );
                }

                // LEFT
                default:
                {
                    return new Vector2(
                        minX,
                        Mathf.Clamp(
                            zoneCenter + spread,
                            minY,
                            maxY
                        )
                    );
                }
            }
        }

        // =========================================
        // SAFE DISTANCE
        // =========================================

        private bool IsFarEnoughFromPlayer(
            Vector2 position)
        {
            if (_player == null)
            {
                return true;
            }

            Vector2 playerPosition =
                _player.position;

            float minimumDistanceSquared =
                safeDistanceFromPlayer *
                safeDistanceFromPlayer;

            return (
                position -
                playerPosition
            ).sqrMagnitude >=
                minimumDistanceSquared;
        }

        // =========================================
        // CONTROL
        // =========================================

        public void StopSpawning()
        {
            _canSpawn =
                false;
        }

        public void StartSpawning()
        {
            _canSpawn =
                true;

            _spawnTimer =
                firstWaveDelay;
        }
    }
}