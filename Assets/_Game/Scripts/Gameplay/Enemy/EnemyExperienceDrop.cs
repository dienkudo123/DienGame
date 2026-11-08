using DienGame.Gameplay.Experience;
using UnityEngine;

namespace DienGame.Gameplay.Enemy
{
    [RequireComponent(typeof(EnemyHealth))]
    public sealed class EnemyExperienceDrop : MonoBehaviour
    {
        private GameObject _experienceOrbPrefab;

        private int _experienceAmount = 10;

        private EnemyHealth _enemyHealth;

        private bool _hasDropped;

        // =========================================
        // INITIALIZE
        // =========================================

        public void Initialize(
            GameObject experienceOrbPrefab,
            int experienceAmount)
        {
            _experienceOrbPrefab =
                experienceOrbPrefab;

            _experienceAmount =
                Mathf.Max(
                    1,
                    experienceAmount
                );
        }

        // =========================================
        // UNITY
        // =========================================

        private void Awake()
        {
            _enemyHealth =
                GetComponent<EnemyHealth>();
        }

        private void OnEnable()
        {
            _hasDropped = false;

            if (_enemyHealth == null)
            {
                _enemyHealth =
                    GetComponent<EnemyHealth>();
            }

            if (_enemyHealth != null)
            {
                _enemyHealth.Died +=
                    OnEnemyDied;
            }
        }

        private void OnDisable()
        {
            if (_enemyHealth != null)
            {
                _enemyHealth.Died -=
                    OnEnemyDied;
            }
        }

        // =========================================
        // ENEMY DEATH
        // =========================================

        private void OnEnemyDied(
            EnemyHealth enemyHealth)
        {
            if (_hasDropped)
            {
                return;
            }

            _hasDropped = true;

            SpawnExperienceOrb();
        }

        // =========================================
        // DROP XP
        // =========================================

        private void SpawnExperienceOrb()
        {
            if (_experienceOrbPrefab == null)
            {
                Debug.LogError(
                    "EnemyExperienceDrop: " +
                    "Không có ExperienceOrb Prefab.",
                    this
                );

                return;
            }

            GameObject orbObject =
                Instantiate(
                    _experienceOrbPrefab,
                    transform.position,
                    Quaternion.identity
                );

            if (!orbObject.TryGetComponent(
                    out ExperienceOrb experienceOrb))
            {
                Debug.LogError(
                    "ExperienceOrb Prefab không có " +
                    "component ExperienceOrb.",
                    orbObject
                );

                Destroy(orbObject);

                return;
            }

            experienceOrb.Initialize(
                _experienceAmount
            );
        }
    }
}