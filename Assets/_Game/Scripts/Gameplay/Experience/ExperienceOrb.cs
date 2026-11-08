using DienGame.Gameplay.Player;
using UnityEngine;

namespace DienGame.Gameplay.Experience
{
    public sealed class ExperienceOrb : MonoBehaviour
    {
        // =========================================
        // PICKUP
        // =========================================

        [Header("Pickup")]

        [SerializeField, Min(0.1f)]
        private float pickupRadius = 3f;

        [SerializeField, Min(0.1f)]
        private float moveSpeed = 10f;

        [SerializeField, Min(0.05f)]
        private float collectDistance = 0.35f;

        // =========================================
        // RUNTIME
        // =========================================

        private PlayerExperience _playerExperience;

        private Transform _player;

        private int _experienceAmount = 1;

        private bool _isAttracted;

        private bool _isCollected;

        // =========================================
        // INITIALIZE
        // =========================================

        public void Initialize(
            int experienceAmount)
        {
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
            FindPlayer();
        }

        private void Start()
        {
            // Fallback:
            // đề phòng Player chưa sẵn sàng ở Awake.
            if (_playerExperience == null)
            {
                FindPlayer();
            }
        }

        private void Update()
        {
            if (_isCollected)
            {
                return;
            }

            // Nếu lúc spawn chưa tìm được Player
            // thì tiếp tục thử lại.
            if (
                _player == null ||
                _playerExperience == null
            )
            {
                FindPlayer();

                return;
            }

            float distanceSquared =
                GetDistanceSquaredToPlayer();

            // =====================================
            // CHƯA BỊ HÚT
            // =====================================

            if (!_isAttracted)
            {
                float pickupRadiusSquared =
                    pickupRadius *
                    pickupRadius;

                if (
                    distanceSquared <=
                    pickupRadiusSquared
                )
                {
                    _isAttracted =
                        true;
                }
                else
                {
                    return;
                }
            }

            // =====================================
            // ĐANG BỊ HÚT
            // =====================================

            MoveTowardsPlayer();

            // Kiểm tra lại sau khi di chuyển.
            distanceSquared =
                GetDistanceSquaredToPlayer();

            float collectDistanceSquared =
                collectDistance *
                collectDistance;

            if (
                distanceSquared <=
                collectDistanceSquared
            )
            {
                Collect();
            }
        }

        // =========================================
        // FIND PLAYER
        // =========================================

        private void FindPlayer()
        {
            _playerExperience =
                FindFirstObjectByType<
                    PlayerExperience
                >();

            if (_playerExperience == null)
            {
                return;
            }

            _player =
                _playerExperience.transform;
        }

        // =========================================
        // DISTANCE
        // =========================================

        private float GetDistanceSquaredToPlayer()
        {
            Vector2 orbPosition =
                transform.position;

            Vector2 playerPosition =
                _player.position;

            return (
                orbPosition -
                playerPosition
            ).sqrMagnitude;
        }

        // =========================================
        // MOVEMENT
        // =========================================

        private void MoveTowardsPlayer()
        {
            Vector3 targetPosition =
                _player.position;

            // Giữ nguyên Z của orb.
            targetPosition.z =
                transform.position.z;

            transform.position =
                Vector3.MoveTowards(
                    transform.position,
                    targetPosition,
                    moveSpeed *
                    Time.deltaTime
                );
        }

        // =========================================
        // COLLECT
        // =========================================

        private void Collect()
        {
            if (_isCollected)
            {
                return;
            }

            if (_playerExperience == null)
            {
                return;
            }

            _isCollected =
                true;

            _playerExperience.AddExperience(
                _experienceAmount
            );

            Debug.Log(
                $"EXP +{_experienceAmount} " +
                $"| Current: " +
                $"{_playerExperience.CurrentExperience}" +
                $"/{_playerExperience.ExperienceToNextLevel}",
                this
            );

            Destroy(
                gameObject
            );
        }

        // =========================================
        // DEBUG
        // =========================================

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(
                transform.position,
                pickupRadius
            );
        }
    }
}