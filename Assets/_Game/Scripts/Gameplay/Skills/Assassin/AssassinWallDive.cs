using System.Collections.Generic;
using DienGame.Gameplay.Balance;
using DienGame.Gameplay.Core;
using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Map;
using DienGame.Gameplay.Player;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Assassin
{
    /// <summary>
    /// Assassin-only active skill with MOBA-style hold aiming:
    /// 1) hold to preview hook range + direction,
    /// 2) release to hook a real wall,
    /// 3) cling briefly, showing second-stage dash preview,
    /// 4) hold/release again (or tap) to dash out.
    /// Also renders runtime visuals: aim circles, aim line, hook marker and rope.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class AssassinWallDive : MonoBehaviour
    {
        public enum Phase
        {
            Ready,
            AimingHook,
            Hooking,
            Clinging,
            AimingDash,
            Dashing,
            Cooldown
        }

        [Header("Phi Anh Bat Tuong - Logic")]
        [SerializeField, Min(0.5f)] private float hookRange = 5f;
        [SerializeField, Min(0.5f)] private float dashRange = 5.5f;
        [SerializeField, Min(0.5f)] private float hookSpeed = 16f;
        [SerializeField, Min(0.5f)] private float dashSpeed = 18f;
        [SerializeField, Min(0.1f)] private float maxClingSeconds = 1f;
        [SerializeField, Min(0.1f)] private float cooldownSeconds = 12f;
        [SerializeField, Min(0.1f)] private float damageMultiplier = 1.7f;
        [SerializeField, Min(0.1f)] private float hitRadius = 0.65f;
        [SerializeField, Min(0.01f)] private float wallClearance = 0.05f;

        [Header("Phi Anh Bat Tuong - Aiming")]
        [SerializeField, Min(8f)] private float holdDeadZonePixels = 28f;
        // Code-owned visual settings: old Inspector overrides are deliberately
        // ignored, so the new matte style works in an already-authored scene.
        private const float ropeSlackAmplitude = 0.025f;
        private const int ringSegments = 64;
        private const float ringWidth = 0.025f;
        private const float aimLineWidth = 0.040f;
        private const float ropeWidth = 0.028f;
        private const float markerSize = 0.18f;

        private readonly List<RaycastHit2D> _rayHits = new List<RaycastHit2D>(32);
        private readonly List<RaycastHit2D> _castHits = new List<RaycastHit2D>(16);
        private readonly List<Collider2D> _hitColliders = new List<Collider2D>(32);
        private readonly HashSet<EnemyHealth> _damaged = new HashSet<EnemyHealth>();

        private ContactFilter2D _solidFilter;
        private Rigidbody2D _body;
        private Collider2D _bodyCollider;
        private PlayerMovement _movement;
        private PlayerCharacter _character;
        private PlayerHealth _health;
        private AssassinSkillController _passives;
        private AssassinSpriteAnimator _animator;
        private AssassinCrescentVfx _slashVisual;

        private Vector2 _lastAim = Vector2.down;
        private Vector2 _clingPoint;
        private Vector2 _hookAnchorPoint;
        private Vector2 _wallNormal;
        private Vector2 _dashDirection;
        private Vector2 _previewDirection;
        private Vector2 _previewHookPoint;
        private Vector2 _previewHookNormal;
        private Vector2 _holdScreenOrigin;

        private float _clingRemaining;
        private float _dashRemaining;
        private float _cooldownRemaining;
        private Phase _phase = Phase.Ready;
        private Collider2D _hookedWall;
        private bool _hasValidHookPreview;
        private bool _holdActive;

        // Runtime visual objects
        private Transform _visualRoot;
        private LineRenderer _aimRing;
        private LineRenderer _aimLine;
        private LineRenderer _aimArrow;
        private LineRenderer _ropeLine;
        private LineRenderer _anchorMarker;
        private Material _lineMaterial;

        // Matte, restrained colors. The aim ring is deliberately less opaque
        // than the aim line so it does not cover the terrain or the character.
        private readonly Color _hookValidColor = new Color(.58f, .82f, .79f, .83f);
        private readonly Color _hookInvalidColor = new Color(.91f, .49f, .46f, .68f);
        private readonly Color _dashColor = new Color(.94f, .76f, .47f, .85f);
        private readonly Color _ropeColor = new Color(.73f, .78f, .82f, .88f);

        public Phase CurrentPhase => _phase;
        public float CooldownRemaining => Mathf.Max(0f, _cooldownRemaining);
        public float CooldownDuration => cooldownSeconds;
        public bool IsControllingMovement => _phase == Phase.AimingHook ||
                                             _phase == Phase.Hooking ||
                                             _phase == Phase.Clinging ||
                                             _phase == Phase.AimingDash ||
                                             _phase == Phase.Dashing;
        public bool CanPress => isActiveAndEnabled && (_health == null || !_health.IsDead) &&
                                !GamePauseService.IsPaused && Time.timeScale > 0f &&
                                (_phase == Phase.Ready || _phase == Phase.Clinging) &&
                                (_phase != Phase.Ready || _cooldownRemaining <= 0f);

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _bodyCollider = GetComponent<Collider2D>();
            _movement = GetComponent<PlayerMovement>();
            _character = GetComponent<PlayerCharacter>();
            _health = GetComponent<PlayerHealth>();
            _passives = GetComponent<AssassinSkillController>();
            _animator = GetComponentInChildren<AssassinSpriteAnimator>(true);
            _slashVisual = GetComponent<AssassinCrescentVfx>();
            _solidFilter = new ContactFilter2D { useLayerMask = false, useTriggers = false };
            EnsureVisuals();
        }

        private void Update()
        {
            if (Time.deltaTime <= 0f) return;
            if (_health != null && _health.IsDead)
            {
                CancelSkill();
                enabled = false;
                return;
            }

            if (_movement != null && _movement.MoveDirection.sqrMagnitude > .025f)
                _lastAim = _movement.MoveDirection.normalized;

            if (_phase == Phase.Cooldown)
            {
                _cooldownRemaining -= Time.deltaTime;
                if (_cooldownRemaining <= 0f)
                {
                    _cooldownRemaining = 0f;
                    _phase = Phase.Ready;
                }
            }
            else if (_phase == Phase.Clinging && !_holdActive)
            {
                Vector2 input = _movement != null ? _movement.MoveDirection : Vector2.zero;
                if (input.sqrMagnitude > .025f)
                    _previewDirection = SanitizeDashDirection(input.normalized, false);
                else if (_previewDirection.sqrMagnitude < .01f)
                    _previewDirection = _wallNormal;
            }
            else if (_phase == Phase.AimingDash && _previewDirection.sqrMagnitude < .01f)
            {
                _previewDirection = _wallNormal;
            }
        }

        private void LateUpdate()
        {
            UpdateVisuals();
        }

        /// <summary>
        /// Backward-compatible quick activate for keyboard/testing:
        /// - Ready: immediately hook using last facing direction.
        /// - Clinging: immediately dash using current preview direction.
        /// </summary>
        public void TryActivate()
        {
            if (!CanPress) return;
            if (_phase == Phase.Ready)
            {
                TryBeginHook(_lastAim);
            }
            else if (_phase == Phase.Clinging)
            {
                Vector2 input = _movement != null ? _movement.MoveDirection : Vector2.zero;
                BeginDash(input.sqrMagnitude > .025f ? input.normalized : _previewDirection);
            }
        }

        public void BeginHold(Vector2 screenPosition)
        {
            if (!isActiveAndEnabled || GamePauseService.IsPaused || Time.timeScale <= 0f) return;
            if (_health != null && _health.IsDead) return;

            _holdActive = true;
            _holdScreenOrigin = screenPosition;

            if (_phase == Phase.Ready && _cooldownRemaining <= 0f)
            {
                _phase = Phase.AimingHook;
                _previewDirection = _lastAim.sqrMagnitude > .01f ? _lastAim.normalized : Vector2.right;
                _movement.SetExternalMovementControl(true);
                _body.linearVelocity = Vector2.zero;
                RefreshHookPreview();
            }
            else if (_phase == Phase.Clinging)
            {
                _phase = Phase.AimingDash;
                if (_previewDirection.sqrMagnitude < .01f)
                    _previewDirection = _wallNormal;
            }
        }

        public void UpdateHold(Vector2 screenPosition)
        {
            if (!_holdActive) return;
            Vector2 delta = screenPosition - _holdScreenOrigin;
            if (delta.sqrMagnitude >= holdDeadZonePixels * holdDeadZonePixels)
                _previewDirection = delta.normalized;

            if (_phase == Phase.AimingHook)
            {
                RefreshHookPreview();
            }
            else if (_phase == Phase.AimingDash)
            {
                _previewDirection = SanitizeDashDirection(_previewDirection, false);
            }
        }

        public void ReleaseHold()
        {
            bool wasHolding = _holdActive;
            _holdActive = false;
            if (!wasHolding) return;

            if (_phase == Phase.AimingHook)
            {
                if (_hasValidHookPreview)
                    CommitHookPreview();
                else
                    ReturnFromHookAim();
            }
            else if (_phase == Phase.AimingDash)
            {
                BeginDash(_previewDirection);
            }
        }

        public void CancelHold()
        {
            _holdActive = false;
            if (_phase == Phase.AimingHook)
                ReturnFromHookAim();
            else if (_phase == Phase.AimingDash)
                _phase = Phase.Clinging;
        }

        private void TryBeginHook(Vector2 aim)
        {
            if (aim.sqrMagnitude < .0001f) return;
            aim.Normalize();

            if (!TryFindHook(aim, out RaycastHit2D nearest))
                return;

            StartHookFromHit(nearest, aim);
        }

        private bool TryFindHook(Vector2 aim, out RaycastHit2D nearest)
        {
            _rayHits.Clear();
            Physics2D.Raycast(_body.position, aim, _solidFilter, _rayHits, hookRange);

            nearest = default;
            bool found = false;
            float distance = hookRange + .01f;
            for (int i = 0; i < _rayHits.Count; i++)
            {
                RaycastHit2D hit = _rayHits[i];
                if (hit.collider == null || !IsHookableWall(hit.collider) ||
                    hit.distance >= distance || hit.distance <= .025f)
                    continue;
                nearest = hit;
                distance = hit.distance;
                found = true;
            }
            return found;
        }

        private void StartHookFromHit(RaycastHit2D hit, Vector2 aim)
        {
            _hookedWall = hit.collider;
            _hookAnchorPoint = hit.point;
            _wallNormal = hit.normal.sqrMagnitude > .1f ? hit.normal.normalized : -aim;
            Vector2 bodyExtents = _bodyCollider.bounds.extents;
            float bodyRadius = Mathf.Max(bodyExtents.x, bodyExtents.y);
            _clingPoint = hit.point + _wallNormal * (bodyRadius + wallClearance);
            _clingRemaining = maxClingSeconds;
            _damaged.Clear();
            _phase = Phase.Hooking;
            _movement.SetExternalMovementControl(true);
            _body.linearVelocity = Vector2.zero;
            _previewDirection = _wallNormal;
        }

        private void CommitHookPreview()
        {
            Vector2 aim = _previewDirection.sqrMagnitude > .01f ? _previewDirection.normalized : _lastAim;
            RaycastHit2D hit = new RaycastHit2D();
            if (!TryFindHook(aim, out hit))
            {
                ReturnFromHookAim();
                return;
            }
            StartHookFromHit(hit, aim);
        }

        private void ReturnFromHookAim()
        {
            _hasValidHookPreview = false;
            _phase = Phase.Ready;
            _movement.SetExternalMovementControl(false);
            _body.linearVelocity = Vector2.zero;
        }

        private void FixedUpdate()
        {
            if (_phase == Phase.Hooking)
                FixedHook();
            else if (_phase == Phase.Clinging || _phase == Phase.AimingDash)
            {
                _body.linearVelocity = Vector2.zero;
                _clingRemaining -= Time.fixedDeltaTime;
                if (_clingRemaining <= 0f)
                    BeginDash(_previewDirection.sqrMagnitude > .01f ? _previewDirection : _wallNormal, true);
            }
            else if (_phase == Phase.Dashing)
                FixedDash();
        }

        private void FixedHook()
        {
            if (_hookedWall == null || !_hookedWall.enabled)
            {
                FinishSkill();
                return;
            }

            Vector2 remaining = _clingPoint - _body.position;
            if (remaining.sqrMagnitude <= .0064f)
            {
                _body.linearVelocity = Vector2.zero;
                _phase = Phase.Clinging;
                _previewDirection = _wallNormal;
                return;
            }

            float requested = Mathf.Min(hookSpeed * Time.fixedDeltaTime, remaining.magnitude);
            float safe = LimitTravel(remaining.normalized, requested);
            if (safe <= .001f)
            {
                if (remaining.magnitude <= .2f)
                {
                    _phase = Phase.Clinging;
                    _previewDirection = _wallNormal;
                }
                else
                {
                    FinishSkill();
                }
                return;
            }

            _body.MovePosition(_body.position + remaining.normalized * safe);
            if (safe + .02f < requested && remaining.magnitude > .2f)
                FinishSkill();
        }

        private void BeginDash(Vector2 requestedDirection, bool forceAwayFromWall = false)
        {
            if (_phase != Phase.Clinging && _phase != Phase.AimingDash) return;

            _holdActive = false;
            _dashDirection = SanitizeDashDirection(requestedDirection, forceAwayFromWall);
            if (_dashDirection.sqrMagnitude < .01f)
                _dashDirection = _wallNormal.sqrMagnitude > .01f ? _wallNormal : -_lastAim;

            _dashRemaining = dashRange;
            _phase = Phase.Dashing;
            _animator?.PlaySlash(_dashDirection);
            _slashVisual?.ShowSlash(_dashDirection);
        }

        private Vector2 SanitizeDashDirection(Vector2 requestedDirection, bool forceAwayFromWall)
        {
            Vector2 input = requestedDirection.sqrMagnitude > .01f ? requestedDirection.normalized : _wallNormal;
            if (input.sqrMagnitude < .01f)
                input = _lastAim.sqrMagnitude > .01f ? -_lastAim : Vector2.right;

            if (forceAwayFromWall)
                return _wallNormal.sqrMagnitude > .01f ? _wallNormal.normalized : input;

            float intoWall = Vector2.Dot(input, _wallNormal);
            if (intoWall < 0f)
            {
                Vector2 tangent = input - _wallNormal * intoWall;
                input = tangent.sqrMagnitude > .02f ? tangent.normalized : _wallNormal;
            }

            if (input.sqrMagnitude < .01f)
                input = _wallNormal.sqrMagnitude > .01f ? _wallNormal.normalized : input;

            return input;
        }

        private void FixedDash()
        {
            float requested = Mathf.Min(dashSpeed * Time.fixedDeltaTime, _dashRemaining);
            if (requested <= .001f)
            {
                FinishSkill();
                return;
            }

            float safe = LimitTravel(_dashDirection, requested);
            Vector2 next = _body.position + _dashDirection * safe;
            if (safe > .001f)
            {
                _body.MovePosition(next);
                DamageEnemiesAt(next);
                _dashRemaining -= safe;
            }

            if (safe + .015f < requested || _dashRemaining <= .01f)
                FinishSkill();
        }

        private float LimitTravel(Vector2 direction, float requested)
        {
            _castHits.Clear();
            _bodyCollider.Cast(direction, _solidFilter, _castHits, requested + wallClearance);
            float safe = requested;
            for (int i = 0; i < _castHits.Count; i++)
            {
                RaycastHit2D hit = _castHits[i];
                Collider2D collider = hit.collider;
                if (collider == null || !IsSolidBlocker(collider)) continue;
                safe = Mathf.Min(safe, Mathf.Max(0f, hit.distance - wallClearance));
            }
            return safe;
        }

        private bool IsSolidBlocker(Collider2D collider)
        {
            if (collider == null || collider.isTrigger || collider.transform.IsChildOf(transform))
                return false;
            if (collider.GetComponentInParent<EnemyHealth>() != null)
                return false;
            return true;
        }

        private bool IsHookableWall(Collider2D collider)
        {
            if (!IsSolidBlocker(collider)) return false;
            MapObstacle obstacle = collider.GetComponentInParent<MapObstacle>();
            if (obstacle != null) return obstacle.BlockPlayer;
            RectangularMapBoundary boundary = collider.GetComponentInParent<RectangularMapBoundary>();
            if (boundary != null)
                return collider.name.StartsWith("Collider_", System.StringComparison.Ordinal);
            int obstacleLayer = LayerMask.NameToLayer("Obstacle");
            return obstacleLayer >= 0 && collider.gameObject.layer == obstacleLayer;
        }

        private void DamageEnemiesAt(Vector2 position)
        {
            _hitColliders.Clear();
            Physics2D.OverlapCircle(position, hitRadius, _solidFilter, _hitColliders);
            float baseDamage = (_character != null ? _character.Damage : GameBalance.PlayerDamage)
                               * damageMultiplier;
            for (int i = 0; i < _hitColliders.Count; i++)
            {
                Collider2D hit = _hitColliders[i];
                if (hit == null) continue;
                EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
                if (enemy == null || enemy.IsDead || !_damaged.Add(enemy)) continue;
                float damage = _passives != null && _passives.isActiveAndEnabled
                    ? _passives.DamageAgainst(enemy, baseDamage)
                    : baseDamage;
                enemy.TakeDamage(damage);
            }
        }

        private void FinishSkill()
        {
            _holdActive = false;
            _body.linearVelocity = Vector2.zero;
            _movement.SetExternalMovementControl(false);
            _hookedWall = null;
            _dashRemaining = 0f;
            _cooldownRemaining = cooldownSeconds;
            _phase = Phase.Cooldown;
            _damaged.Clear();
            _hasValidHookPreview = false;
        }

        private void CancelSkill()
        {
            _holdActive = false;
            if (_body != null) _body.linearVelocity = Vector2.zero;
            if (_movement != null) _movement.SetExternalMovementControl(false);
            _hookedWall = null;
            _damaged.Clear();
            _phase = Phase.Ready;
            _hasValidHookPreview = false;
        }

        private void OnDisable()
        {
            CancelSkill();
            HideAllVisuals();
        }

        #region Visuals

        // This rendering intentionally uses only thin flat lines: no filled
        // markers, glow, particle bursts or ornamental circles. The skill logic
        // above is unchanged and the HUD is created independently by code.
        private void EnsureVisuals()
        {
            if (_visualRoot != null) return;

            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                Debug.LogWarning("AssassinWallDive: Sprites/Default shader missing; no aim VFX.", this);
                return;
            }

            _lineMaterial = new Material(shader) { name = "WallDive_MatteLine_Runtime" };
            _visualRoot = new GameObject("WallDive_MatteAimAndCable").transform;
            _visualRoot.SetParent(transform, false);
            _visualRoot.localPosition = Vector3.zero;

            _aimRing = CreateLine("RangeOutline", ringWidth, true, 60);
            _aimLine = CreateLine("AimDirection", aimLineWidth, false, 62);
            _aimArrow = CreateLine("ArrowTip", aimLineWidth, false, 63);
            _ropeLine = CreateLine("Cable", ropeWidth, false, 65);
            _anchorMarker = CreateLine("SmallAnchorDiamond", ringWidth, true, 66);
            HideAllVisuals();
        }

        private LineRenderer CreateLine(string name, float width, bool loop, int sortingOrder)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(_visualRoot, false);
            LineRenderer line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.loop = loop;
            line.widthMultiplier = width;
            line.material = _lineMaterial;
            line.numCornerVertices = 1;
            line.numCapVertices = 1;
            line.textureMode = LineTextureMode.Stretch;
            line.alignment = LineAlignment.View;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.sortingLayerName = "Default";
            line.sortingOrder = sortingOrder;
            line.enabled = false;
            return line;
        }

        private void RefreshHookPreview()
        {
            if (_previewDirection.sqrMagnitude < .01f)
                _previewDirection = _lastAim.sqrMagnitude > .01f ? _lastAim : Vector2.right;
            _previewDirection.Normalize();

            if (TryFindHook(_previewDirection, out RaycastHit2D hit))
            {
                _hasValidHookPreview = true;
                _previewHookPoint = hit.point;
                _previewHookNormal = hit.normal.sqrMagnitude > .1f ? hit.normal.normalized : -_previewDirection;
            }
            else
            {
                _hasValidHookPreview = false;
                _previewHookPoint = _body.position + _previewDirection * hookRange;
                _previewHookNormal = -_previewDirection;
            }
        }

        private void UpdateVisuals()
        {
            EnsureVisuals();
            if (_visualRoot == null) return;

            Vector2 origin = _body != null ? _body.position : (Vector2)transform.position;
            switch (_phase)
            {
                case Phase.AimingHook:
                    RefreshHookPreview();
                    Color aim = _hasValidHookPreview ? _hookValidColor : _hookInvalidColor;
                    ShowAimCircle(origin, hookRange, aim);
                    ShowAimLine(origin, _previewHookPoint, aim);
                    if (_hasValidHookPreview)
                        ShowMarker(_previewHookPoint, aim);
                    else
                        HideMarker();
                    HideRope();
                    break;

                case Phase.Hooking:
                    HideAim();
                    ShowRope(origin, _hookAnchorPoint, false);
                    ShowMarker(_hookAnchorPoint, _hookValidColor);
                    break;

                case Phase.Clinging:
                case Phase.AimingDash:
                    ShowAimCircle(origin, dashRange, _dashColor);
                    Vector2 direction = _previewDirection.sqrMagnitude > .01f
                        ? _previewDirection.normalized : _wallNormal;
                    ShowAimLine(origin, origin + direction * dashRange, _dashColor);
                    ShowRope(origin, _hookAnchorPoint, true);
                    ShowMarker(_hookAnchorPoint, _ropeColor);
                    break;

                default:
                    HideAllVisuals();
                    break;
            }
        }

        private void ShowAimCircle(Vector2 origin, float radius, Color tint)
        {
            if (_aimRing == null) return;
            int count = Mathf.Max(20, ringSegments);
            if (_aimRing.positionCount != count)
                _aimRing.positionCount = count;

            for (int i = 0; i < count; i++)
            {
                float angle = 2f * Mathf.PI * i / count;
                _aimRing.SetPosition(i, origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }

            Color subtle = new Color(tint.r, tint.g, tint.b, .24f);
            _aimRing.startColor = subtle;
            _aimRing.endColor = subtle;
            _aimRing.enabled = true;
        }

        private void ShowAimLine(Vector2 from, Vector2 to, Color color)
        {
            if (_aimLine == null || _aimArrow == null) return;
            _aimLine.positionCount = 2;
            _aimLine.SetPosition(0, from);
            _aimLine.SetPosition(1, to);
            _aimLine.startColor = color;
            _aimLine.endColor = color;
            _aimLine.enabled = true;

            Vector2 direction = to - from;
            if (direction.sqrMagnitude <= .0001f)
            {
                _aimArrow.enabled = false;
                return;
            }

            direction.Normalize();
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);
            const float headLength = .19f;
            const float halfWidth = .12f;
            _aimArrow.positionCount = 3;
            _aimArrow.SetPosition(0, to - direction * headLength + perpendicular * halfWidth);
            _aimArrow.SetPosition(1, to);
            _aimArrow.SetPosition(2, to - direction * headLength - perpendicular * halfWidth);
            _aimArrow.startColor = color;
            _aimArrow.endColor = color;
            _aimArrow.enabled = true;
        }

        private void ShowRope(Vector2 from, Vector2 to, bool slightlySlack)
        {
            if (_ropeLine == null) return;
            // A restrained thin cable connected to the outer side of the player,
            // not a glowing tube drawn through the character's body.
            Vector2 cable = to - from;
            float distance = cable.magnitude;
            if (distance < .01f)
            {
                _ropeLine.enabled = false;
                return;
            }

            Vector2 forward = cable / distance;
            Vector2 start = from + forward * Mathf.Min(.20f, distance * .22f);
            Vector2 normal = new Vector2(-forward.y, forward.x);
            const int count = 5;
            _ropeLine.positionCount = count;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)(count - 1);
                Vector2 pos = Vector2.Lerp(start, to, t);
                if (slightlySlack && i > 0 && i < count - 1)
                    pos += normal * (Mathf.Sin(t * Mathf.PI) * ropeSlackAmplitude);
                _ropeLine.SetPosition(i, pos);
            }
            _ropeLine.startColor = _ropeColor;
            _ropeLine.endColor = _ropeColor;
            _ropeLine.enabled = true;
        }

        private void ShowMarker(Vector2 position, Color color)
        {
            if (_anchorMarker == null) return;
            float radius = markerSize * .5f;
            _anchorMarker.positionCount = 4;
            _anchorMarker.SetPosition(0, position + Vector2.up * radius);
            _anchorMarker.SetPosition(1, position + Vector2.right * radius);
            _anchorMarker.SetPosition(2, position + Vector2.down * radius);
            _anchorMarker.SetPosition(3, position + Vector2.left * radius);
            _anchorMarker.startColor = color;
            _anchorMarker.endColor = color;
            _anchorMarker.enabled = true;
        }

        private void HideAim()
        {
            if (_aimRing != null) _aimRing.enabled = false;
            if (_aimLine != null) _aimLine.enabled = false;
            if (_aimArrow != null) _aimArrow.enabled = false;
        }

        private void HideRope()
        {
            if (_ropeLine != null) _ropeLine.enabled = false;
        }

        private void HideMarker()
        {
            if (_anchorMarker != null) _anchorMarker.enabled = false;
        }

        private void HideAllVisuals()
        {
            HideAim();
            HideRope();
            HideMarker();
        }

        private void OnDestroy()
        {
            if (_lineMaterial != null) Destroy(_lineMaterial);
        }

        #endregion
    }
}
