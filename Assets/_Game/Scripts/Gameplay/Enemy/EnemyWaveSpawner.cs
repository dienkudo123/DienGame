using System.Collections.Generic;
using DienGame.Gameplay.Balance;
using DienGame.Gameplay.Map;
using DienGame.Gameplay.Player;
using DienGame.Gameplay.World;
using DienGame.Client;
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
        private int _waveIndex;
        private bool _canSpawn = true;
        private Stage11EncounterProfile _stageProfile;
        private float _openingShotDamage = GameBalance.PlayerDamage;
        public int WaveNumber => _waveIndex;
        public EnemyArchetype LastWaveArchetype { get; private set; } = EnemyArchetype.Grunt;

        public GameObject EnemyPrefab => enemyPrefab;
        public Collider2D MapBounds => mapBounds;
        public int AliveEnemyCount => _aliveEnemies.Count;
        public float RunProgress => GetDifficultyProgress();

        private void Start()
        {
            ResolveReferences();
            int stage = WorldStageRuntime.Current != null
                ? WorldStageRuntime.Current.Theme.Stage : LobbySessionService.SelectedStage;
            _stageProfile = Stage11EncounterProfiles.Get(stage);
            PlayerAutoAttack marksman = FindFirstObjectByType<PlayerAutoAttack>();
            if (marksman != null && marksman.isActiveAndEnabled)
                _openingShotDamage = Mathf.Max(1f, marksman.AttackDamage);
            _enemyFactory = new EnemyFactory(enemyPrefab, experienceOrbPrefab, stage, _openingShotDamage);
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

            // Exactly ONE species per wave. If the nearest edge is blocked,
            // TryFindSpawnPosition also samples walkable positions around the player.
            EnemyArchetype waveType = RollArchetype();
            LastWaveArchetype = waveType;
            Physics2D.SyncTransforms();
            int spawned = 0;
            for (int i = 0; i < spawnCount; i++)
            {
                if (!TryFindSpawnPosition(bounds, edge, zoneCenter, out Vector2 spawnPosition))
                    continue; // Never spawn inside map geometry.
                SpawnEnemy(spawnPosition, waveType, RollElite());
                spawned++;
            }

            // Don't advance to a different monster species on a fully blocked wave.
            // Keep the pressure cap intact and retry after the next interval.
            if (spawned > 0) _waveIndex++;
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
            if (float.IsNaN(spawnPosition.x) || float.IsNaN(spawnPosition.y)) return null;
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

            boss.name = _stageProfile.Stage <= 1
                ? "Boss_Judgement" : "Boss_Stage" + _stageProfile.Stage;
            EnemyHealth health = boss.GetComponent<EnemyHealth>();

            if (health != null)
            {
                // Four telegraphed attacks are now inside BossCombatController.
                // Do NOT add the old independent Stage11BossModifier, which could overlap casts.
                health.SetMaxHealth(GameBalance.BossMaxHealth * _stageProfile.BossHealthMultiplier);
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
                    experienceOrbPrefab, _stageProfile.Stage, _openingShotDamage
                );
            }
        }

        private EnemyArchetype RollArchetype()
        {
            // The first waves teach target acquisition and dodging before adding ranged/tank enemies.
            if (_waveIndex < 2)
                return EnemyArchetype.Grunt;
            float progress = GetDifficultyProgress();
            if (_waveIndex < 4)
                return Random.value < 0.68f ? EnemyArchetype.Grunt : EnemyArchetype.Swarm;
            if (_stageProfile.Stage > 1)
                return _stageProfile.RollArchetype(progress);
            // Stage 1: heavy/ranged pressure arrives gradually, never mixed within one wave.
            float roll = Random.value;
            if (progress < 0.30f)
                return roll < .45f ? EnemyArchetype.Grunt :
                    roll < .70f ? EnemyArchetype.Swarm : EnemyArchetype.Runner;
            if (progress < .58f)
                return roll < .30f ? EnemyArchetype.Grunt :
                    roll < .50f ? EnemyArchetype.Swarm :
                    roll < .78f ? EnemyArchetype.Runner : EnemyArchetype.Ranged;
            return roll < .24f ? EnemyArchetype.Grunt :
                roll < .40f ? EnemyArchetype.Swarm :
                roll < .62f ? EnemyArchetype.Runner :
                roll < .83f ? EnemyArchetype.Ranged : EnemyArchetype.Tank;
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
            // WAVE = ONE archetype. Raise pressure by count, not by growing HP.
            // Even stages 4-5 must never exceed the shared per-wave hard cap.
            int count = Mathf.RoundToInt(Mathf.Lerp(
                GameBalance.BaseEnemiesPerWave,
                GameBalance.MaxEnemiesPerWave,
                GetDifficultyProgress()));
            return Mathf.Clamp(count, GameBalance.BaseEnemiesPerWave,
                GameBalance.MaxEnemiesPerWave);
        }

        private float GetCurrentWaveInterval()
        {
            return Mathf.Lerp(GameBalance.BaseWaveInterval,
                GameBalance.MinWaveInterval, GetDifficultyProgress()) *
                (_stageProfile.WaveIntervalMultiplier > .01f ? _stageProfile.WaveIntervalMultiplier : 1f);
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

        private bool TryFindSpawnPosition(Bounds bounds, int edge, float zoneCenter,
            out Vector2 position)
        {
            // Search other edges too. A spawn must be both clear and directly reachable
            // from the player at spawn time, never inside isolated terrain pockets.
            for (int i = 0; i < 72; i++)
            {
                int testedEdge = i < 12 ? edge : (edge + (i / 12)) % 4;
                float center = i < 12 ? zoneCenter : GetZoneCenter(bounds, testedEdge);
                Vector2 candidate = GeneratePosition(bounds, testedEdge, center);
                if (!IsSafeSpawnPoint(candidate)) continue;
                position = candidate;
                return true;
            }
            // Edge locations can be completely blocked on hand-edited maps.
            // Try several reachable locations in an annulus around the player;
            // never bypass the obstacle and terrain-visibility checks.
            if (_player != null)
            {
                Vector2 center = _player.position;
                float safeRadius = Mathf.Max(3f, safeDistanceFromPlayer + 0.65f);
                for (int i = 0; i < 64; i++)
                {
                    float angle = (i * 2.39996323f) + (_elapsedRunTime * .37f);
                    float radius = safeRadius + (i % 5) * 1.1f;
                    Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    Vector2 candidate = center + offset * radius;
                    if (!bounds.Contains(new Vector3(candidate.x, candidate.y, bounds.center.z))) continue;
                    if (!IsSafeSpawnPoint(candidate)) continue;
                    position = candidate;
                    return true;
                }
            }
            position = Vector2.zero;
            return false;
        }

        private bool IsSafeSpawnPoint(Vector2 candidate)
        {
            if (!IsFarEnoughFromPlayer(candidate)) return false;
            if (mapBoundary != null && !Stage11WorldGeometry.IsFree(candidate, .62f, mapBoundary))
                return false;
            // A map with only mapBounds still needs a real physics collision check.
            if (mapBoundary == null)
            {
                foreach (Collider2D hit in Physics2D.OverlapCircleAll(candidate, .62f))
                {
                    if (hit == null || !hit.enabled || hit.isTrigger) continue;
                    DienGame.Gameplay.Map.MapObstacle solid =
                        hit.GetComponentInParent<DienGame.Gameplay.Map.MapObstacle>();
                    if (solid != null && solid.BlockEnemy) return false;
                }
            }
            return _player == null || !Stage11WorldGeometry.TerrainOccludes(candidate, _player.position);
        }

        /// <summary>Choose the farthest walkable point inside the map, never inside props.</summary>
        public Vector2 FindSafeBossSpawn(Vector2 preferred)
        {
            if (mapBoundary == null) mapBoundary = FindFirstObjectByType<RectangularMapBoundary>();
            if (mapBoundary == null) return preferred;
            Physics2D.SyncTransforms();
            Vector2 min = mapBoundary.Min + new Vector2(1.6f, 1.6f);
            Vector2 max = mapBoundary.Max - new Vector2(1.6f, 1.6f);
            Vector2 playerPos = _player != null ? (Vector2)_player.position : Vector2.zero;
            Vector2 best = preferred;
            float bestScore = -1f;
            for (float y = min.y; y <= max.y; y += 1.5f)
            for (float x = min.x; x <= max.x; x += 1.5f)
            {
                Vector2 point = new Vector2(x, y);
                if (!Stage11WorldGeometry.IsFree(point, .85f, mapBoundary)) continue;
                if (_player != null && Stage11WorldGeometry.TerrainOccludes(point, _player.position)) continue;
                float away = (point - playerPos).sqrMagnitude;
                // Tiny tie-breaker towards the original preferred corner.
                float score = away - .01f * (point - preferred).sqrMagnitude;
                if (score <= bestScore) continue;
                bestScore = score;
                best = point;
            }
            if (bestScore < 0f)
            {
                Debug.LogError("Balance: No unobstructed safe boss spawn; refusing to force boss into terrain.");
                return new Vector2(float.NaN, float.NaN);
            }
            return best;
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
            _waveIndex = 0;
            _spawnTimer = GameBalance.FirstWaveDelay;
        }
    }
}
