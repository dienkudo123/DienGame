using System.Collections.Generic;
using DienGame.Gameplay.Enemy;
using UnityEngine;

namespace DienGame.Gameplay.Player
{
    public sealed class PlayerTargeting : MonoBehaviour
    {
        [Header("Targeting")]

        [SerializeField, Min(0f)]
        private float targetRange = 5f;

        [SerializeField]
        private LayerMask enemyLayer;

        [SerializeField]
        private Transform currentTarget;

        [SerializeField, Min(0.02f)]
        private float refreshInterval = 0.1f;

        private readonly List<Collider2D> _results =
            new(32);

        private ContactFilter2D _contactFilter;

        private float _refreshTimer;

        public Transform CurrentTarget =>
            currentTarget;

        public bool HasTarget =>
            currentTarget != null;

        public float TargetRange =>
            targetRange;

        private void Awake()
        {
            SetupFilter();
        }

        private void Update()
        {
            _refreshTimer -=
                Time.deltaTime;

            if (_refreshTimer > 0f)
            {
                return;
            }

            _refreshTimer =
                refreshInterval;

            FindNearestTarget();
        }

        private void SetupFilter()
        {
            _contactFilter =
                new ContactFilter2D();

            _contactFilter.useLayerMask =
                true;

            _contactFilter.layerMask =
                enemyLayer;

            _contactFilter.useTriggers =
                false;
        }

        private void FindNearestTarget()
        {
            _results.Clear();

            Physics2D.OverlapCircle(
                transform.position,
                targetRange,
                _contactFilter,
                _results
            );

            Transform nearestTarget =
                null;

            float nearestDistanceSquared =
                float.MaxValue;

            Vector2 playerPosition =
                transform.position;

            foreach (
                Collider2D enemyCollider
                in _results
            )
            {
                if (enemyCollider == null)
                {
                    continue;
                }

                EnemyHealth enemyHealth =
                    enemyCollider
                        .GetComponentInParent<
                            EnemyHealth
                        >();

                if (
                    enemyHealth == null ||
                    enemyHealth.IsDead
                )
                {
                    continue;
                }

                Transform enemyTransform =
                    enemyHealth.transform;

                Vector2 difference =
                    (Vector2)enemyTransform.position -
                    playerPosition;

                float distanceSquared =
                    difference.sqrMagnitude;

                if (
                    distanceSquared >=
                    nearestDistanceSquared
                )
                {
                    continue;
                }

                nearestDistanceSquared =
                    distanceSquared;

                nearestTarget =
                    enemyTransform;
            }

            currentTarget =
                nearestTarget;
        }

        public void ClearTarget()
        {
            currentTarget = null;
        }

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(
                transform.position,
                targetRange
            );
        }

#endif
    }
}