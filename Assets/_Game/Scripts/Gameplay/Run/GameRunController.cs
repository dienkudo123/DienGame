using System;
using System.Collections.Generic;
using DienGame.Gameplay.Balance;
using DienGame.Gameplay.Core;
using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Player;
using DienGame.Gameplay.UI;
using UnityEngine;

namespace DienGame.Gameplay.Run
{
    public sealed class GameRunController : MonoBehaviour
    {
        private const string RunEndedPauseReason = "RunEnded";

        public enum RunPhase
        {
            Survival,
            Boss,
            Won,
            Lost,
        }

        [Header("Boss Spawn")]
        [SerializeField, Min(0f)] private float bossSpawnInset = 1f;

        private EnemyWaveSpawner _waveSpawner;
        private PlayerHealth _playerHealth;
        private PlayerExperience _playerExperience;
        private EnemyHealth _bossHealth;
        private Collider2D _mapBounds;
        private RunPhase _phase;
        private float _survivalTimer;
        private bool _runRecorded;

        public RunPhase Phase => _phase;
        public float RemainingSurvivalTime => Mathf.Max(0f, _survivalTimer);
        public float SurvivalDuration => GameBalance.SurvivalDuration;
        public EnemyHealth BossHealth => _bossHealth;

        public event Action<EnemyHealth> BossSpawned;
        public event Action<RunPhase> RunEnded;

        private void Start()
        {
            FindReferences();
            EnsureRunUI();
            _phase = RunPhase.Survival;
            _survivalTimer = GameBalance.SurvivalDuration;
            Debug.Log("RUN START - SURVIVAL PHASE");
        }

        private void Update()
        {
            if (_phase == RunPhase.Won || _phase == RunPhase.Lost)
            {
                return;
            }

            CheckPlayerDeath();

            if (_phase == RunPhase.Lost)
            {
                return;
            }

            if (_phase == RunPhase.Survival)
            {
                _survivalTimer -= Time.deltaTime;

                if (_survivalTimer <= 0f)
                {
                    EnterBossPhase();
                }
            }
        }

        private void FindReferences()
        {
            _waveSpawner = FindFirstObjectByType<EnemyWaveSpawner>();
            _playerHealth = FindFirstObjectByType<PlayerHealth>();
            _playerExperience = FindFirstObjectByType<PlayerExperience>();

            if (_waveSpawner != null)
            {
                _mapBounds = _waveSpawner.MapBounds;
            }

            if (_waveSpawner == null) Debug.LogError("GameRunController: Không tìm thấy EnemyWaveSpawner.", this);
            if (_playerHealth == null) Debug.LogError("GameRunController: Không tìm thấy PlayerHealth.", this);
        }

        private void EnsureRunUI()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            GameRunUI runUI = canvas.GetComponent<GameRunUI>();
            if (runUI == null) runUI = canvas.gameObject.AddComponent<GameRunUI>();
            runUI.Bind(this);
        }

        private void EnterBossPhase()
        {
            if (_waveSpawner == null || _playerHealth == null) return;

            _phase = RunPhase.Boss;
            _waveSpawner.StopSpawning();
            SpawnBoss();
        }

        private void SpawnBoss()
        {
            Vector2 spawnPosition = GetFarthestSpawnPosition();
            _bossHealth = _waveSpawner.SpawnBoss(spawnPosition);

            if (_bossHealth == null)
            {
                Debug.LogError("GameRunController: EnemyFactory không spawn được Boss.", this);
                return;
            }

            _bossHealth.Died += OnBossDied;
            BossSpawned?.Invoke(_bossHealth);
        }

        private Vector2 GetFarthestSpawnPosition()
        {
            if (_mapBounds == null)
            {
                Vector2 playerPosition = _playerHealth != null ? _playerHealth.transform.position : Vector2.zero;
                return playerPosition + Vector2.up * 7f;
            }

            Bounds bounds = _mapBounds.bounds;
            float minX = bounds.min.x + bossSpawnInset;
            float maxX = bounds.max.x - bossSpawnInset;
            float minY = bounds.min.y + bossSpawnInset;
            float maxY = bounds.max.y - bossSpawnInset;
            float centerX = bounds.center.x;
            float centerY = bounds.center.y;

            var positions = new List<Vector2>
            {
                new(minX, maxY), new(centerX, maxY), new(maxX, maxY), new(maxX, centerY),
                new(maxX, minY), new(centerX, minY), new(minX, minY), new(minX, centerY),
            };

            Vector2 player = _playerHealth.transform.position;
            Vector2 farthest = positions[0];
            float farthestDistance = -1f;

            foreach (Vector2 position in positions)
            {
                float distance = (position - player).sqrMagnitude;
                if (distance > farthestDistance)
                {
                    farthestDistance = distance;
                    farthest = position;
                }
            }

            return farthest;
        }

        private void CheckPlayerDeath()
        {
            if (_playerHealth != null && _playerHealth.IsDead)
            {
                Lose();
            }
        }

        private void OnBossDied(EnemyHealth bossHealth)
        {
            if (_phase == RunPhase.Boss) Win();
        }

        private void Win()
        {
            _phase = RunPhase.Won;
            _waveSpawner?.StopSpawning();
            FinishRun(true);
        }

        private void Lose()
        {
            _phase = RunPhase.Lost;
            _waveSpawner?.StopSpawning();
            FinishRun(false);
        }

        private void FinishRun(bool won)
        {
            if (!_runRecorded)
            {
                int level = _playerExperience != null ? _playerExperience.CurrentLevel : 1;
                GameProgressService.RecordRun(won, level);
                _runRecorded = true;
            }

            RunEnded?.Invoke(_phase);
            GamePauseService.RequestPause(RunEndedPauseReason);

            GameShellUI shell = FindFirstObjectByType<GameShellUI>();
            shell?.RefreshStats();
        }

        private void OnDestroy()
        {
            if (_bossHealth != null)
            {
                _bossHealth.Died -= OnBossDied;
            }

            GamePauseService.ReleasePause(RunEndedPauseReason);
        }
    }
}
