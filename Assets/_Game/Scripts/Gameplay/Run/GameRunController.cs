using System.Collections.Generic;
using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Player;
using UnityEngine;

namespace DienGame.Gameplay.Run
{
    public sealed class GameRunController : MonoBehaviour
    {
        private enum RunPhase
        {
            Survival,
            Boss,
            Won,
            Lost,
        }

        [Header("Survival")]
        [SerializeField]
        [Min(1f)]
        private float survivalDuration = 60f;

        [Header("Boss")]
        [SerializeField]
        [Min(1f)]
        private float bossMaxHealth = 500f;

        [SerializeField]
        [Min(1f)]
        private float bossScale = 1.5f;

        [SerializeField]
        [Min(0f)]
        private float bossSpawnInset = 1f;

        private EnemyWaveSpawner _waveSpawner;
        private PlayerHealth _playerHealth;
        private EnemyHealth _bossHealth;
        private Collider2D _mapBounds;

        private RunPhase _phase;
        private float _survivalTimer;

        public float RemainingSurvivalTime =>
            Mathf.Max(0f, _survivalTimer);

        private void Start()
        {
            Time.timeScale = 1f;

            FindReferences();

            _phase = RunPhase.Survival;
            _survivalTimer = survivalDuration;

            Debug.Log("RUN START - SURVIVAL PHASE");
        }

        private void Update()
        {
            if (
                _phase == RunPhase.Won ||
                _phase == RunPhase.Lost
            )
            {
                return;
            }

            CheckPlayerDeath();

            if (_phase == RunPhase.Lost)
                return;

            if (_phase == RunPhase.Survival)
            {
                UpdateSurvival();
            }
        }

        private void FindReferences()
        {
            _waveSpawner =
                FindFirstObjectByType<EnemyWaveSpawner>();

            _playerHealth =
                FindFirstObjectByType<PlayerHealth>();

            if (_waveSpawner != null)
            {
                _mapBounds = _waveSpawner.MapBounds;
            }

            if (_waveSpawner == null)
            {
                Debug.LogError(
                    "GameRunController: Không tìm thấy EnemyWaveSpawner.",
                    this
                );
            }

            if (_playerHealth == null)
            {
                Debug.LogError(
                    "GameRunController: Không tìm thấy PlayerHealth.",
                    this
                );
            }

            if (_mapBounds == null)
            {
                Debug.LogError(
                    "GameRunController: Không tìm thấy MapBounds.",
                    this
                );
            }
        }

        private void UpdateSurvival()
        {
            _survivalTimer -= Time.deltaTime;

            if (_survivalTimer > 0f)
                return;

            EnterBossPhase();
        }

        private void EnterBossPhase()
        {
            if (_waveSpawner == null)
                return;

            if (_playerHealth == null)
                return;

            if (_mapBounds == null)
                return;

            if (_waveSpawner.EnemyPrefab == null)
            {
                Debug.LogError(
                    "GameRunController: Enemy Prefab chưa được gán.",
                    this
                );

                return;
            }

            _phase = RunPhase.Boss;

            _waveSpawner.StopSpawning();

            SpawnBoss();

            Debug.Log(
                "BOSS PHASE - wave spawn đã dừng. " +
                "Quái cũ vẫn tồn tại."
            );
        }

        private void SpawnBoss()
        {
            Vector2 spawnPosition =
                GetFarthestSpawnPosition();

            GameObject boss =
                Instantiate(
                    _waveSpawner.EnemyPrefab,
                    spawnPosition,
                    Quaternion.identity
                );

            boss.name = "Boss";

            boss.transform.localScale *= bossScale;

            _bossHealth =
                boss.GetComponent<EnemyHealth>();

            if (_bossHealth == null)
            {
                Debug.LogError(
                    "GameRunController: Boss không có EnemyHealth.",
                    boss
                );

                Destroy(boss);
                return;
            }

            _bossHealth.SetMaxHealth(
                bossMaxHealth
            );

            _bossHealth.Died += OnBossDied;

            Debug.Log(
                $"Boss spawn tại {spawnPosition}. " +
                $"HP = {bossMaxHealth}"
            );
        }

        private Vector2 GetFarthestSpawnPosition()
        {
            Bounds bounds = _mapBounds.bounds;

            float minX =
                bounds.min.x + bossSpawnInset;

            float maxX =
                bounds.max.x - bossSpawnInset;

            float minY =
                bounds.min.y + bossSpawnInset;

            float maxY =
                bounds.max.y - bossSpawnInset;

            float centerX = bounds.center.x;
            float centerY = bounds.center.y;

            var positions = new List<Vector2>
            {
                new(minX, maxY),
                new(centerX, maxY),
                new(maxX, maxY),

                new(maxX, centerY),

                new(maxX, minY),
                new(centerX, minY),
                new(minX, minY),

                new(minX, centerY),
            };

            Vector2 playerPosition =
                _playerHealth.transform.position;

            Vector2 farthestPosition =
                positions[0];

            float farthestDistance = -1f;

            foreach (Vector2 position in positions)
            {
                float distance =
                    (position - playerPosition)
                    .sqrMagnitude;

                if (distance <= farthestDistance)
                    continue;

                farthestDistance = distance;
                farthestPosition = position;
            }

            return farthestPosition;
        }

        private void CheckPlayerDeath()
        {
            if (_playerHealth == null)
                return;

            if (!_playerHealth.IsDead)
                return;

            Lose();
        }

        private void OnBossDied(
            EnemyHealth bossHealth)
        {
            if (_phase != RunPhase.Boss)
                return;

            Win();
        }

        private void Win()
        {
            _phase = RunPhase.Won;

            if (_waveSpawner != null)
            {
                _waveSpawner.StopSpawning();
            }

            Debug.Log("YOU WIN");

            Time.timeScale = 0f;
        }

        private void Lose()
        {
            _phase = RunPhase.Lost;

            if (_waveSpawner != null)
            {
                _waveSpawner.StopSpawning();
            }

            Debug.Log("GAME OVER");

            Time.timeScale = 0f;
        }

        private void OnDestroy()
        {
            if (_bossHealth != null)
            {
                _bossHealth.Died -= OnBossDied;
            }
        }
    }
}