using UnityEngine;

namespace DienGame.Gameplay.Player
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PlayerAnimator : MonoBehaviour
    {
        // =========================================
        // REFERENCES
        // =========================================

        [Header("References")]
        [SerializeField]
        private PlayerMovement playerMovement;

        [SerializeField]
        private PlayerTargeting playerTargeting;

        [SerializeField]
        private PlayerShootVisualSignal shootSignal;

        [SerializeField]
        private PlayerSpriteAnimationData combatAnimationData;

        // =========================================
        // SCALE
        // =========================================

        [Header("Visual Scale")]

        // Kích thước animation chạy/đứng bình thường.
        [SerializeField, Min(0.01f)]
        private float normalVisualScale = 0.20f;

        // Kích thước animation cầm súng/chạy+bắn.
        [SerializeField, Min(0.01f)]
        private float combatVisualScale = 0.20f;

        // =========================================
        // SPEED
        // =========================================

        [Header("Animation Speed")]

        [SerializeField, Min(0.1f)]
        private float combatRunFps = 8f;

        [SerializeField, Min(0.01f)]
        private float normalAnimationSpeed = 1f;

        // =========================================

        private SpriteRenderer _spriteRenderer;
        private Animator _animator;

        private Vector2 _lastDirection =
            Vector2.down;

        private int _combatRunFrame;
        private float _combatRunTimer;

        private int _normalStateHash;

        private bool _hadTarget;

        private bool _currentCombatScale;

        // =========================================
        // NORMAL IDLE
        // =========================================

        private static readonly int[] IdleStates =
        {
            Animator.StringToHash("Idle_Down"),
            Animator.StringToHash("Idle_DownLeft"),
            Animator.StringToHash("Idle_Left"),
            Animator.StringToHash("Idle_UpLeft"),
            Animator.StringToHash("Idle_Up"),
            Animator.StringToHash("Idle_UpRight"),
            Animator.StringToHash("Idle_Right"),
            Animator.StringToHash("Idle_DownRight"),
        };

        // =========================================
        // NORMAL RUN
        // =========================================

        private static readonly int[] RunStates =
        {
            Animator.StringToHash("Run_Down"),
            Animator.StringToHash("Run_DownLeft"),
            Animator.StringToHash("Run_Left"),
            Animator.StringToHash("Run_UpLeft"),
            Animator.StringToHash("Run_Up"),
            Animator.StringToHash("Run_UpRight"),
            Animator.StringToHash("Run_Right"),
            Animator.StringToHash("Run_DownRight"),
        };

        private void Awake()
        {
            _spriteRenderer =
                GetComponent<SpriteRenderer>();

            _animator =
                GetComponent<Animator>();

            ResolveReferences();

            ApplyScale(false);
        }

        private void Update()
        {
            ResolveReferences();

            bool hasTarget =
                playerTargeting != null &&
                playerTargeting.CurrentTarget != null;

            bool isMoving =
                playerMovement != null &&
                playerMovement.IsMoving;

            // =====================================
            // ĐỔI SCALE THEO LOẠI SPRITE
            // =====================================

            ApplyScale(
                hasTarget
            );

            UpdateFacingDirection(
                hasTarget,
                isMoving
            );

            if (hasTarget)
            {
                UpdateCombatAnimation(
                    isMoving
                );
            }
            else
            {
                UpdateNormalAnimation(
                    isMoving
                );
            }

            _hadTarget =
                hasTarget;
        }

        // =========================================
        // SCALE
        // =========================================

        private void ApplyScale(
            bool combat)
        {
            // Chỉ set Transform khi trạng thái
            // Normal <-> Combat thực sự thay đổi.

            if (
                _currentCombatScale ==
                    combat &&
                transform.localScale.x > 0f
            )
            {
                float expectedScale =
                    combat
                        ? combatVisualScale
                        : normalVisualScale;

                if (
                    Mathf.Approximately(
                        transform.localScale.x,
                        expectedScale
                    )
                )
                {
                    return;
                }
            }

            float scale =
                combat
                    ? combatVisualScale
                    : normalVisualScale;

            transform.localScale =
                new Vector3(
                    scale,
                    scale,
                    1f
                );

            _currentCombatScale =
                combat;
        }

        // =========================================
        // REFERENCES
        // =========================================

        private void ResolveReferences()
        {
            if (playerMovement == null)
            {
                playerMovement =
                    GetComponentInParent<
                        PlayerMovement
                    >();
            }

            if (playerTargeting == null)
            {
                playerTargeting =
                    GetComponentInParent<
                        PlayerTargeting
                    >();
            }

            if (shootSignal == null)
            {
                shootSignal =
                    GetComponentInParent<
                        PlayerShootVisualSignal
                    >();
            }
        }

        // =========================================
        // HƯỚNG NHÌN
        // =========================================

        private void UpdateFacingDirection(
            bool hasTarget,
            bool isMoving)
        {
            Vector2 direction =
                _lastDirection;

            // Có target:
            // nhân vật + súng quay về target.
            if (
                hasTarget &&
                playerTargeting != null
            )
            {
                direction =
                    (Vector2)playerTargeting
                        .CurrentTarget
                        .position -
                    (Vector2)playerTargeting
                        .transform
                        .position;
            }

            // Không có target:
            // hướng theo joystick.
            else if (
                isMoving &&
                playerMovement != null
            )
            {
                direction =
                    playerMovement
                        .MoveDirection;
            }

            if (
                direction.sqrMagnitude <=
                0.0001f
            )
            {
                return;
            }

            _lastDirection =
                direction.normalized;
        }

        // =========================================
        // KHÔNG TARGET
        //
        // Dùng Animator:
        //
        // đứng = Idle
        // chạy = Run
        // =========================================

        private void UpdateNormalAnimation(
            bool isMoving)
        {
            if (_animator == null)
            {
                return;
            }

            if (!_animator.enabled)
            {
                _animator.enabled =
                    true;

                _normalStateHash =
                    0;
            }

            int directionIndex =
                GetDirectionIndex(
                    _lastDirection
                );

            int stateHash =
                isMoving
                    ? GetNormalRunState(
                        directionIndex
                    )
                    : GetNormalIdleState(
                        directionIndex
                    );

            _animator.speed =
                isMoving
                    ? normalAnimationSpeed
                    : 1f;

            if (
                stateHash ==
                _normalStateHash
            )
            {
                return;
            }

            _normalStateHash =
                stateHash;

            _animator.Play(
                stateHash,
                0,
                0f
            );
        }

        // =========================================
        // CÓ TARGET
        //
        // Hàng trên = combat bình thường
        // Hàng dưới = cùng pose đang bắn
        // =========================================

        private void UpdateCombatAnimation(
            bool isMoving)
        {
            if (
                combatAnimationData == null ||
                _spriteRenderer == null
            )
            {
                return;
            }

            // Animator không được ghi đè
            // sprite combat.
            if (
                _animator != null &&
                _animator.enabled
            )
            {
                _animator.enabled =
                    false;

                _normalStateHash =
                    0;
            }

            if (!_hadTarget)
            {
                _combatRunFrame =
                    0;

                _combatRunTimer =
                    0f;
            }

            UpdateCombatRunFrame(
                isMoving
            );

            int directionIndex =
                GetDirectionIndex(
                    _lastDirection
                );

            bool isShooting =
                shootSignal != null &&
                shootSignal.IsShooting;

            Sprite sprite =
                combatAnimationData
                    .GetSprite(
                        directionIndex,
                        isMoving,
                        _combatRunFrame,
                        isShooting
                    );

            if (sprite != null)
            {
                _spriteRenderer.sprite =
                    sprite;
            }
        }

        // =========================================
        // COMBAT RUN
        // =========================================

        private void UpdateCombatRunFrame(
            bool isMoving)
        {
            if (!isMoving)
            {
                _combatRunFrame =
                    0;

                _combatRunTimer =
                    0f;

                return;
            }

            float frameDuration =
                1f /
                combatRunFps;

            _combatRunTimer +=
                Time.deltaTime;

            while (
                _combatRunTimer >=
                frameDuration
            )
            {
                _combatRunTimer -=
                    frameDuration;

                _combatRunFrame =
                    (_combatRunFrame + 1) %
                    4;
            }
        }

        // =========================================
        // NORMAL IDLE
        // =========================================

        private int GetNormalIdleState(
            int directionIndex)
        {
            int requestedState =
                IdleStates[
                    directionIndex
                ];

            if (
                _animator.HasState(
                    0,
                    requestedState
                )
            )
            {
                return requestedState;
            }

            return directionIndex switch
            {
                1 or 2 or 3 =>
                    IdleStates[2],

                5 or 6 or 7 =>
                    IdleStates[6],

                4 =>
                    IdleStates[4],

                _ =>
                    IdleStates[0],
            };
        }

        // =========================================
        // NORMAL RUN
        // =========================================

        private int GetNormalRunState(
            int directionIndex)
        {
            int requestedState =
                RunStates[
                    directionIndex
                ];

            if (
                _animator.HasState(
                    0,
                    requestedState
                )
            )
            {
                return requestedState;
            }

            return directionIndex switch
            {
                1 or 2 or 3 =>
                    RunStates[2],

                5 or 6 or 7 =>
                    RunStates[6],

                4 =>
                    RunStates[4],

                _ =>
                    RunStates[0],
            };
        }

        // =========================================
        // 8 HƯỚNG
        //
        // 0 Down
        // 1 DownLeft
        // 2 Left
        // 3 UpLeft
        // 4 Up
        // 5 UpRight
        // 6 Right
        // 7 DownRight
        // =========================================

        private static int GetDirectionIndex(
            Vector2 direction)
        {
            float angle =
                Mathf.Atan2(
                    direction.y,
                    direction.x
                ) *
                Mathf.Rad2Deg;

            if (angle < 0f)
            {
                angle +=
                    360f;
            }

            if (
                angle >= 337.5f ||
                angle < 22.5f
            )
            {
                return 6;
            }

            if (angle < 67.5f)
            {
                return 5;
            }

            if (angle < 112.5f)
            {
                return 4;
            }

            if (angle < 157.5f)
            {
                return 3;
            }

            if (angle < 202.5f)
            {
                return 2;
            }

            if (angle < 247.5f)
            {
                return 1;
            }

            if (angle < 292.5f)
            {
                return 0;
            }

            return 7;
        }

        // =========================================
        // BUILDER
        // =========================================

        public void SetAnimationData(
            PlayerSpriteAnimationData data)
        {
            combatAnimationData =
                data;
        }
    }
}