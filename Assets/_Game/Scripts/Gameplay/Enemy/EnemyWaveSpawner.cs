using System.Collections.Generic;
using DienGame.Gameplay.Balance;
using DienGame.Gameplay.Map;
using DienGame.Gameplay.Player;
using UnityEngine;

namespace DienGame.Gameplay.Enemy
{
    public sealed class EnemyWaveSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject experienceOrbPrefab;
        [SerializeField] private Collider2D mapBounds;
        [SerializeField] private RectangularMapBoundary mapBoundary;

        [Header("Spawn Position")]
        [SerializeField, Min(0f)] private float safeDistanceFromPlayer = 5f;
        [SerializeField, Min(0f)] private float boundaryPadding = 1.5f;
        [SerializeField, Min(0f)] private float zoneSpread = 1.5f;

        private readonly HashSet<EnemyHealth> _aliveEnemies = new();

        private Transform _player;
        private EnemyFactory _enemyFactory;
        private float _spawnTimer;
        private float _elapsedRunTime;
        private float _eliteCooldown;
        private bool _canSpawn = true;

        public GameObject EnemyPrefab => enemyPrefab;
        public Collider2D MapBounds => mapBounds;
        public int AliveEnemyCount => _aliveEnemies.Count;
        public float RunProgress => GetDifficultyProgress();

        private void Start()
        {
            ResolveReferences();
            _enemyFactory = new EnemyFactory(enemyPrefab, experienceOrbPrefab);
            _spawnTimer = GameBalance.FirstWaveDelay;
            ValidateSetup();
        }

        private void Update()
        {
            if (!_canSpawn || enemyPrefab == null)
            {
                return;
            }

            if (mapBoundary == null && mapBounds == null)
            {
                return;
            }

            _elapsedRunTime += Time.deltaTime;
            _spawnTimer -= Time.deltaTime;
            _eliteCooldown = Mathf.Max(0f, _eliteCooldown - Time.deltaTime);

            if (_spawnTimer > 0f)
            {
                return;
            }

            SpawnWave();
            _spawnTimer = GetCurrentWaveInterval();
        }

        private void ResolveReferences()
        {
            if (mapBoundary == null)
            {
                mapBoundary = FindFirstObjectByType<RectangularMapBoundary>();
            }

            PlayerMovement player = FindFirstObjectByType<PlayerMovement>();

            if (player != null)
            {
                _player = player.transform;
            }
        }

        private void ValidateSetup()
        {
            if (enemyPrefab == null)
            {
                Debug.LogError("EnemyWaveSpawner: Chưa gán Enemy Prefab.", this);
            }

            if (experienceOrbPrefab == null)
            {
                Debug.LogWarning(
                    "EnemyWaveSpawner: Chưa gán Experience Orb Prefab.",
                    this
                );
            }

            if (mapBoundary == null && mapBounds == null)
            {
                Debug.LogError(
                    "EnemyWaveSpawner: Không tìm thấy RectangularMapBoundary hoặc Map Bounds.",
                    this
                );
            }

            if (EnemyCatalogProvider.Catalog == null)
            {
                Debug.LogError(
                    "EnemyWaveSpawner: EnemyCatalog chưa được tạo. " +
                    "Chọn DienGame > Enemies > Rebuild Enemy Catalog.",
                    this
                );
            }
        }

        private void SpawnWave()
        {
            int availableSlots = GameBalance.MaxAliveEnemies - _aliveEnemies.Count;

            if (availableSlots <= 0)
            {
                return;
            }

            Bounds bounds = GetSpawnBounds();
            int edge = Random.Range(0, 4);
            float zoneCenter = GetZoneCenter(bounds, edge);

            int spawnCount = Mathf.Min(
                GetCurrentEnemiesPerWave(),
                availableSlots
            );

            for (int i = 0; i < spawnCount; i++)
            {
                Vector2 spawnPosition = FindSpawnPosition(
                    bounds,
                    edge,
                    zoneCenter
                );

                SpawnEnemy(
                    spawnPosition,
                    RollArchetype(),
                    RollElite()
                );
            }
        }

        private void SpawnEnemy(
            Vector2 spawnPosition,
            EnemyArchetype archetype,
            bool isElite)
        {
            EnsureFactory();

            GameObject enemyObject = _enemyFactory?.Spawn(
                spawnPosition,
                archetype,
                isElite,
                GetDifficultyProgress()
            );

            if (enemyObject != null)
            {
                TrackEnemy(enemyObject);
            }
        }

        public EnemyHealth SpawnBoss(Vector2 spawnPosition)
        {
            EnsureFactory();

            GameObject boss = _enemyFactory?.Spawn(
                spawnPosition,
                EnemyArchetype.Boss,
                false,
                0f
            );

            if (boss == null)
            {
                return null;
            }

            boss.name = "Boss_Judgement";
            EnemyHealth health = boss.GetComponent<EnemyHealth>();

            if (health != null)
            {
                TrackEnemy(boss);
            }

            return health;
        }

        private void EnsureFactory()
        {
            if (_enemyFactory == null)
            {
                _enemyFactory = new EnemyFactory(
                    enemyPrefab,
                    experienceOrbPrefab
                );
            }
        }

        private EnemyArchetype RollArchetype()
        {
            float progress = GetDifficultyProgress();
            float roll = Random.value;

            if (progress < 0.20f)
            {
                return roll < 0.72f
                    ? EnemyArchetype.Grunt
                    : EnemyArchetype.Swarm;
            }

            if (progress < 0.40f)
            {
                if (roll < 0.45f) return EnemyArchetype.Grunt;
                if (roll < 0.75f) return EnemyArchetype.Runner;
                return EnemyArchetype.Swarm;
            }

            if (progress < 0.60f)
            {
                if (roll < 0.35f) return EnemyArchetype.Grunt;
                if (roll < 0.60f) return EnemyArchetype.Runner;
                if (roll < 0.75f) return EnemyArchetype.Tank;
                if (roll < 0.88f) return EnemyArchetype.Ranged;
                return EnemyArchetype.Swarm;
            }

            if (progress < 0.80f)
            {
                if (roll < 0.27f) return EnemyArchetype.Grunt;
                if (roll < 0.51f) return EnemyArchetype.Runner;
                if (roll < 0.71f) return EnemyArchetype.Tank;
                if (roll < 0.87f) return EnemyArchetype.Ranged;
                return EnemyArchetype.Swarm;
            }

            if (roll < 0.20f) return EnemyArchetype.Grunt;
            if (roll < 0.40f) return EnemyArchetype.Runner;
            if (roll < 0.66f) return EnemyArchetype.Tank;
            if (roll < 0.86f) return EnemyArchetype.Ranged;
            return EnemyArchetype.Swarm;
        }

        private bool RollElite()
        {
            float progress = GetDifficultyProgress();

            if (progress < GameBalance.EliteStartProgress ||
                _eliteCooldown > 0f)
            {
                return false;
            }

            float eliteProgress = Mathf.InverseLerp(
                GameBalance.EliteStartProgress,
                1f,
                progress
            );

            float chance = Mathf.Lerp(
                0.02f,
                GameBalance.EliteMaxSpawnChance,
                eliteProgress
            );

            if (Random.value > chance)
            {
                return false;
            }

            _eliteCooldown = GameBalance.EliteMinimumInterval;
            return true;
        }

        private void TrackEnemy(GameObject enemyObject)
        {
            EnemyHealth health = enemyObject.GetComponent<EnemyHealth>();

            if (health == null)
            {
                return;
            }

            _aliveEnemies.Add(health);
            health.Died -= OnTrackedEnemyDied;
            health.Died += OnTrackedEnemyDied;
        }

        private void OnTrackedEnemyDied(EnemyHealth enemy)
        {
            if (enemy == null)
            {
                return;
            }

            enemy.Died -= OnTrackedEnemyDied;
            _aliveEnemies.Remove(enemy);
        }

        private float GetDifficultyProgress()
        {
            return Mathf.Clamp01(
                _elapsedRunTime / GameBalance.DifficultyRampDuration
            );
        }

        private int GetCurrentEnemiesPerWave()
        {
            return Mathf.RoundToInt(
                Mathf.Lerp(
                    GameBalance.BaseEnemiesPerWave,
                    GameBalance.MaxEnemiesPerWave,
                    GetDifficultyProgress()
                )
            );
        }

        private float GetCurrentWaveInterval()
        {
            return Mathf.Lerp(
                GameBalance.BaseWaveInterval,
                GameBalance.MinWaveInterval,
                GetDifficultyProgress()
            );
        }

        private Bounds GetSpawnBounds()
        {
            if (mapBoundary != null)
            {
                Vector2 size = mapBoundary.MapSize;
                Vector3 center = mapBoundary.transform.position;

                return new Bounds(
                    center,
                    new Vector3(size.x, size.y, 0f)
                );
            }

            if (mapBounds != null)
            {
                return mapBounds.bounds;
            }

            return new Bounds(Vector3.zero, Vector3.one);
        }

        private float GetZoneCenter(Bounds bounds, int edge)
        {
            float minX = bounds.min.x + boundaryPadding;
            float maxX = bounds.max.x - boundaryPadding;
            float minY = bounds.min.y + boundaryPadding;
            float maxY = bounds.max.y - boundaryPadding;

            return edge == 0 || edge == 2
                ? Random.Range(minX, maxX)
                : Random.Range(minY, maxY);
        }

        private Vector2 FindSpawnPosition(
            Bounds bounds,
            int edge,
            float zoneCenter)
        {
            const int MaxAttempts = 20;
            Vector2 position = bounds.center;

            for (int i = 0; i < MaxAttempts; i++)
            {
                position = GeneratePosition(bounds, edge, zoneCenter);

                if (IsFarEnoughFromPlayer(position))
                {
                    return position;
                }
            }

            return position;
        }

        private Vector2 GeneratePosition(
            Bounds bounds,
            int edge,
            float zoneCenter)
        {
            float minX = bounds.min.x + boundaryPadding;
            float maxX = bounds.max.x - boundaryPadding;
            float minY = bounds.min.y + boundaryPadding;
            float maxY = bounds.max.y - boundaryPadding;
            float spread = Random.Range(-zoneSpread, zoneSpread);

            return edge switch
            {
                0 => new Vector2(
                    Mathf.Clamp(zoneCenter + spread, minX, maxX),
                    maxY
                ),
                1 => new Vector2(
                    maxX,
                    Mathf.Clamp(zoneCenter + spread, minY, maxY)
                ),
                2 => new Vector2(
                    Mathf.Clamp(zoneCenter + spread, minX, maxX),
                    minY
                ),
                _ => new Vector2(
                    minX,
                    Mathf.Clamp(zoneCenter + spread, minY, maxY)
                ),
            };
        }

        private bool IsFarEnoughFromPlayer(Vector2 position)
        {
            if (_player == null)
            {
                return true;
            }

            float minDistanceSquared =
                safeDistanceFromPlayer * safeDistanceFromPlayer;

            return (
                position - (Vector2)_player.position
            ).sqrMagnitude >= minDistanceSquared;
        }

        public void StopSpawning()
        {
            _canSpawn = false;
        }

        public void StartSpawning()
        {
            _canSpawn = true;
            _spawnTimer = GameBalance.FirstWaveDelay;
        }
    }
}
