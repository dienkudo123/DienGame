using System.Collections.Generic;
using DienGame.Client;
using DienGame.Gameplay.Equipment;
using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Player;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Mage
{
    /// <summary>
    /// Chimera is an independent pet constrained to the player's moving leash.
    /// AI: wait -> walk to a short random waypoint -> rest; enemies interrupt roaming.
    /// Joystick takes precedence without restarting the AI every drag event.
    /// Physics velocity is set only in FixedUpdate. The leash correction uses the
    /// nearest valid point, not a large teleport to the opposite side of the player.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class ChimeraCompanion : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(.1f)] private float movementSpeed = 3.9f;
        [SerializeField, Min(.1f)] private float roamSpeed = 2.25f;
        [SerializeField, Min(.1f)] private float returnSpeed = 6.5f;
        [SerializeField, Min(.1f)] private float arrivalRadius = .34f;
        [SerializeField, Min(1f)] private float velocityResponse = 24f;
        [SerializeField, Min(.1f)] private float idleMinSeconds = .7f;
        [SerializeField, Min(.1f)] private float idleMaxSeconds = 1.7f;

        [Header("Combat AI")]
        [SerializeField, Min(.3f)] private float searchRadius = 3.8f;
        [SerializeField, Min(.2f)] private float attackRange = .95f;
        [SerializeField, Min(.1f)] private float attackInterval = .85f;
        [SerializeField, Min(.05f)] private float targetRefreshSeconds = .45f;
        [SerializeField, Min(.01f)] private float attackStopBuffer = .12f;
        [SerializeField, Min(.01f)] private float attackResumeBuffer = .32f;

        private MageClassController _owner;
        private MageSkillController _skills;
        private Rigidbody2D _body;
        private Collider2D _collider;
        private SpriteRenderer _renderer;
        private Transform _artTransform;
        private EnemyHealth _pendingAttack;
        private Vector2 _attackDirection = Vector2.right;
        private float _attackVisualElapsed = 1f;
        private const float AttackVisualDuration = .34f;
        private const float AttackImpactTime = .20f;
        private EnemyHealth _target;
        private Vector2 _roamOffset;
        private Vector2 _manualInput;
        private float _roamTimer;
        private float _restTimer;
        private float _retargetTimer;
        private float _attackTimer;
        private float _collisionRefreshTimer;
        private float _maxHealth;
        private EquipmentDefinition _petEquipment;
        private Sprite _equippedPetSprite;
        private BiomechSummonRuntime _biomechSummon;
        private BiomechPetChipRuntime _equippedChip;
        // Phoenix-only signature: all other summons retain their original behavior.
        private FireHawkSignatureRuntime _fireHawkSignature;
        // Other approved biomech pets trigger their own signature (no shared FireHawk spell).
        private SummonSignatureSkill _otherSignature;
        private float _signatureCastElapsed = 99f;
        private const float SignatureCastDuration = .58f;
        private bool _signatureCastLevel5;
        private float EffectiveMoveSpeed => movementSpeed *
            (_biomechSummon != null ? _biomechSummon.MoveFactor : 1f);
        private float EffectiveAttackInterval => attackInterval *
            (_biomechSummon != null ? _biomechSummon.AttackIntervalFactor : 1f);
        private float EffectiveAttackRange => attackRange +
            (_biomechSummon != null ? _biomechSummon.RangeBonus : 0f);
        private float EffectiveSearchRadius => searchRadius +
            (_biomechSummon != null ? _biomechSummon.SearchBonus : 0f);
        private float _animTime;
        private int _facing = 2;
        private bool _manual;
        private bool _isMoving;
        private bool _chasingTarget;
        private bool _hasLastPosition;
        private Vector2 _lastPhysicsPosition;

        private readonly HashSet<Collider2D> _ignoredEnemyColliders = new HashSet<Collider2D>();

        // Chimera is permanently alive and cannot be targeted or damaged by enemies.
        // Keep these properties for compatibility with skills/UI written before immortality.
        public bool Alive => true;
        public float RespawnRemaining => 0f;
        public float CurrentHealth => MaxHealth;
        public float MaxHealth => _maxHealth;
        public float HealthFraction => 1f;
        // Base stat is retained for special skills; calibration changes normal hits only.
        public float AttackDamage => (_owner != null ? _owner.AttackDamage * .7f : 10f) *
            (_petEquipment != null ? _petEquipment.DamageMultiplier : 1f);
        public bool IsManuallyControlled => _manual;

        public void Initialize(MageClassController owner, MageSkillController skills)
        {
            _owner = owner;
            _skills = skills;
            _petEquipment = EquipmentInventory.Selected(PlayableClass.Mage);
            _equippedPetSprite = EquipmentCatalog.LoadWorldSprite(_petEquipment);
            _biomechSummon = BiomechSummonRuntime.Create(_petEquipment);
            // Independent chip runtime only applies to the five new biomechanical forms.
            // The original Chimera and all existing Mage skills remain untouched.
            _equippedChip = BiomechPetChipRuntime.Create(_petEquipment);
            _fireHawkSignature = _petEquipment != null && _petEquipment.Id == "mage_bio_firehawk"
                ? new FireHawkSignatureRuntime() : null;
            // Chimera unchanged; Phoenix keeps the independent Song Trung implementation.
            // Each of the other three biomechs has exactly one skill identity.
            _otherSignature = _petEquipment != null && _biomechSummon != null &&
                _fireHawkSignature == null ? SummonSignatureSkill.Create(_petEquipment) : null;
            _body = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            BuildSeparateVisual();
            _maxHealth = Mathf.Max(1f, owner.CompanionMaxHealth);
            _lastPhysicsPosition = _body.position;
            _hasLastPosition = true;
            _restTimer = Random.Range(.35f, .9f);
            ChooseRoam();
            IgnoreEnemyCollisions();
        }

        public bool DebugPreviewFireHawkSignature(bool level5)
        {
            if (_fireHawkSignature == null) return false;
            _fireHawkSignature.Preview(transform.position, _attackDirection, level5);
            return true;
        }

        /// <summary>QA-only visual preview. No targets damaged or saved data changed.</summary>
        public bool DebugPreviewApprovedSignature(bool level5)
        {
            if (_fireHawkSignature != null)
                return DebugPreviewFireHawkSignature(level5);
            if (_otherSignature == null || _petEquipment == null) return false;
            _signatureCastElapsed = 0f;
            _signatureCastLevel5 = level5;
            string effect = SummonSignatureSkill.EffectName(_petEquipment);
            if (!string.IsNullOrEmpty(effect))
                MageVfx.Play(effect, transform.position + (Vector3)(_attackDirection * .7f),
                    level5 ? .88f : .67f, level5 ? .48f : .38f,
                    evolved: level5);
            return true;
        }

        public void SetManual(Vector2 value, bool enabled)
        {
            bool justReleased = _manual && !enabled;
            _manual = enabled;
            _manualInput = enabled ? Vector2.ClampMagnitude(value, 1f) : Vector2.zero;
            if (justReleased)
            {
                _target = null;
                _chasingTarget = false;
                _restTimer = Random.Range(.4f, .8f);
                ChooseRoam();
            }
        }

        // Deprecated compatibility entry point: immortal summons never lose HP.
        public void TakeDamage(float damage)
        {
            // Intentionally ignored. Enemy AI and projectiles no longer target Chimera.
        }

        private void BuildSeparateVisual()
        {
            // Animating the Rigidbody root would also scale/shift its Collider.
            // Instead, move a separate visual child while physics stays unchanged.
            SpriteRenderer original = GetComponent<SpriteRenderer>();
            GameObject art = new GameObject("Chimera_AnimatedVisual");
            art.transform.SetParent(transform, false);
            _artTransform = art.transform;
            _renderer = art.AddComponent<SpriteRenderer>();
            _renderer.sprite = original != null && original.sprite != null
                ? original.sprite : MageArt.Chimera(2, 0);
            _renderer.sortingLayerID = original != null ? original.sortingLayerID : 0;
            _renderer.sortingOrder = original != null ? original.sortingOrder : 20;
            if (original != null)
            {
                // Visual only: upgraded core shows a gentle glow tint without changing original atlas.
                Color tint = EquipmentResonance.VisualTint(_petEquipment);
                _renderer.color = new Color(original.color.r * tint.r, original.color.g * tint.g,
                    original.color.b * tint.b, original.color.a);
                _renderer.sharedMaterial = original.sharedMaterial;
                original.enabled = false;
            }
            // No cosmetic upgrade ring: pet remains visually unobstructed.
        }

        private void Update()
        {
            if (_owner == null) return;
            _animTime += Time.deltaTime;
            _attackTimer -= Time.deltaTime;
            _retargetTimer -= Time.deltaTime;
            _collisionRefreshTimer -= Time.deltaTime;

            if (_collisionRefreshTimer <= 0f)
            {
                _collisionRefreshTimer = .8f;
                IgnoreEnemyCollisions();
            }

            if (_retargetTimer <= 0f)
            {
                _retargetTimer = targetRefreshSeconds;
                RefreshTarget();
            }

            // Resting is deliberately part of the random roaming behavior.
            if (!_manual && _target == null)
            {
                if (_restTimer > 0f) _restTimer -= Time.deltaTime;
                else
                {
                    _roamTimer -= Time.deltaTime;
                    if (_roamTimer <= 0f) ChooseRoam();
                }
            }

            AdvanceAttackAnimation(Time.deltaTime);
            if (_fireHawkSignature != null)
                _fireHawkSignature.Tick(Time.deltaTime, transform.position);
            AttackIfPossible();

            // Animation Phase09 fix: always advance frames. Idle is no longer frame zero.
            // Combat, damage, navigation and equipment balancing are not modified here.
            bool castingFireHawk = _fireHawkSignature != null && _fireHawkSignature.IsCasting;
            bool castingOther = _otherSignature != null && _signatureCastElapsed < SignatureCastDuration;
            if (castingOther) _signatureCastElapsed += Time.deltaTime;
            bool attacking = _attackVisualElapsed < AttackVisualDuration;
            int frame = attacking
                ? Mathf.Clamp(Mathf.FloorToInt(_attackVisualElapsed / AttackVisualDuration * 5f), 0, 4)
                : Mathf.FloorToInt(_animTime * (_isMoving ? 9.0f : 3.8f)) % 4;

            // Always animate the original Chimera from its original 4-direction atlas.
            Sprite sprite = attacking
                ? MageArt.ChimeraAttack(_facing, frame)
                : MageArt.Chimera(_facing, frame);
            if (sprite == null) sprite = MageArt.Chimera(_facing, Mathf.Min(frame, 3));

            string equipmentId = _petEquipment != null ? _petEquipment.Id : string.Empty;
            if (BiomechMotionArt.IsSupported(equipmentId))
            {
                // REWORK's visual clip was missing from earlier CLEAN assets. This version
                // uses its own eight imported atlases, cached by form and action.
                Sprite petFrame = BiomechMotionArt.GetFrame(equipmentId, _facing, attacking, frame);
                if (petFrame == null && _biomechSummon != null)
                    petFrame = _biomechSummon.GetFrame(_facing, attacking, frame);
                if (petFrame != null) sprite = petFrame;
                else if (_equippedPetSprite != null) sprite = _equippedPetSprite;
                // Phoenix uses separately painted casting frames, not its walking sprite
                // and not a whole-character transform that lifts the head during flight.
                if (castingFireHawk)
                {
                    int castFrame = Mathf.Clamp(Mathf.FloorToInt(
                        (1f - _fireHawkSignature.CastRemainingFraction) * 4f), 0, 3);
                    Sprite cast = BiomechMotionArt.GetCast(equipmentId, _facing,
                        castFrame, _fireHawkSignature.IsLevel5);
                    if (cast != null) sprite = cast;
                }
                else if (castingOther)
                {
                    int castFrame = Mathf.Clamp(Mathf.FloorToInt(
                        _signatureCastElapsed / SignatureCastDuration * 4f), 0, 3);
                    Sprite cast = BiomechMotionArt.GetCast(equipmentId, _facing,
                        castFrame, _signatureCastLevel5);
                    if (cast != null) sprite = cast;
                }
                if (_renderer != null) _renderer.flipX = false;
            }
            // Do not let the equipment icon overwrite Chimera's animated sprite!
            else if (equipmentId != "mage_chimera" && _equippedPetSprite != null)
            {
                sprite = _equippedPetSprite;
                if (_renderer != null) _renderer.flipX = _facing == 3;
            }

            if (sprite != null && _renderer != null) _renderer.sprite = sprite;
        }

        private void RefreshTarget()
        {
            // Lock the existing target until dead or out of the permitted search/leash
            // area. Re-selecting the nearest enemy every 0.24 s caused rapid zig-zags.
            if (_target != null && !_target.IsDead)
            {
                Vector2 enemyPosition = _target.transform.position;
                Vector2 center = _owner.transform.position;
                float allowedRadius = EffectiveSearchRadius + .6f;
                if ((enemyPosition - (Vector2)transform.position).sqrMagnitude <= allowedRadius * allowedRadius &&
                    (enemyPosition - center).sqrMagnitude <= _owner.LeashRadius * _owner.LeashRadius)
                    return;
            }

            _target = MageTargeting.Nearest(transform.position, EffectiveSearchRadius, null,
                _owner.transform.position, _owner.LeashRadius);
            _chasingTarget = false;
        }

        private void FixedUpdate()
        {
            if (_owner == null || _body == null) return;

            Vector2 center = _owner.transform.position;
            Vector2 current = _body.position;
            float leash = Mathf.Max(.8f, _owner.LeashRadius - .25f);
            Vector2 offset = current - center;
            Vector2 desiredVelocity = Vector2.zero;

            if (offset.sqrMagnitude >= leash * leash)
            {
                // Return faster than normal roaming. Manual input cannot escape.
                desiredVelocity = -offset.normalized * returnSpeed;
            }
            else if ((_fireHawkSignature != null && _fireHawkSignature.IsCasting) ||
                     (_otherSignature != null && _signatureCastElapsed < SignatureCastDuration) ||
                     _attackVisualElapsed < AttackImpactTime)
            {
                // Hold the body still for the brief drawn skill windup. Keep physics intact.
                desiredVelocity = Vector2.zero;
            }
            else if (_manual)
            {
                desiredVelocity = _manualInput * EffectiveMoveSpeed;
            }
            else if (_target != null && !_target.IsDead)
            {
                Vector2 difference = (Vector2)_target.transform.position - current;
                float distance = difference.magnitude;

                if (_chasingTarget)
                {
                    if (distance <= EffectiveAttackRange + attackStopBuffer)
                        _chasingTarget = false;
                }
                else if (distance >= EffectiveAttackRange + attackResumeBuffer)
                {
                    _chasingTarget = true;
                }

                if (_chasingTarget && distance > .001f)
                    desiredVelocity = difference / distance * EffectiveMoveSpeed;
            }
            else if (_restTimer <= 0f)
            {
                Vector2 delta = center + _roamOffset - current;
                float distance = delta.magnitude;
                if (distance <= arrivalRadius)
                {
                    _restTimer = Random.Range(idleMinSeconds, idleMaxSeconds);
                    _roamTimer = 0f;
                    desiredVelocity = Vector2.zero;
                }
                else
                {
                    // Slow down before reaching the waypoint, avoiding left/right
                    // oscillation caused by normalized velocity at every distance.
                    float speed = Mathf.Min(roamSpeed, Mathf.Max(0f, distance - arrivalRadius * .45f) / Time.fixedDeltaTime);
                    desiredVelocity = delta / distance * speed;
                }
            }

            // When near the rim, remove any outward component but allow tangential
            // movement. This keeps the companion inside the circle without jitter.
            if (offset.sqrMagnitude > (leash - .2f) * (leash - .2f) && offset.sqrMagnitude > .001f)
            {
                Vector2 radial = offset.normalized;
                float outward = Vector2.Dot(desiredVelocity, radial);
                if (outward > 0f) desiredVelocity -= radial * outward;
            }

            _body.linearVelocity = Vector2.MoveTowards(_body.linearVelocity,
                desiredVelocity, velocityResponse * Time.fixedDeltaTime);

            if (_hasLastPosition)
            {
                float actualSpeed = (current - _lastPhysicsPosition).magnitude / Time.fixedDeltaTime;
                _isMoving = actualSpeed > .23f;
            }
            _lastPhysicsPosition = current;
            _hasLastPosition = true;

            if (_body.linearVelocity.sqrMagnitude > .12f * .12f)
            {
                Vector2 direction = _body.linearVelocity;
                _facing = Mathf.Abs(direction.x) > Mathf.Abs(direction.y)
                    ? (direction.x > 0f ? 1 : 3)
                    : (direction.y > 0f ? 0 : 2);
            }
        }

        private void LateUpdate()
        {
            if (_owner == null || _body == null) return;
            Vector2 center = _owner.transform.position;
            float max = Mathf.Max(.8f, _owner.LeashRadius - .1f);
            if ((_body.position - center).sqrMagnitude > max * max)
                RecallToNearestValidInside(center, max);
            UpdateAttackVisual();
        }

        private void RecallToNearestValidInside(Vector2 center, float max)
        {
            Vector2 from = _body.position;
            Vector2 relative = from - center;
            Vector2 radial = relative.sqrMagnitude > .01f ? relative.normalized : Vector2.right;

            // First try a close point just inside the moving boundary. Only search
            // near the owner when the nearby area is blocked by solid terrain.
            float inside = Mathf.Max(.35f, max - .35f);
            for (int ring = 0; ring < 4; ring++)
            {
                float radius = Mathf.Max(.35f, inside - ring * .55f);
                for (int side = 0; side < 7; side++)
                {
                    float angle = side == 0 ? 0f : ((side + 1) / 2f) * .30f * (side % 2 == 0 ? 1f : -1f);
                    Vector2 candidate = center + Rotate(radial, angle) * radius;
                    if (!IsRecallPositionFree(candidate)) continue;

                    _body.linearVelocity = Vector2.zero;
                    _body.position = candidate;
                    _lastPhysicsPosition = candidate;
                    _isMoving = false;
                    // No VFX on tiny leash corrections: that was flashing every frame.
                    if ((candidate - from).sqrMagnitude > 1.5f * 1.5f)
                        MageVfx.Play("TrongTruongDiGioi", candidate, .45f, .2f);
                    return;
                }
            }

            // Fallback for complex maps. Do not teleport into an obstacle.
            for (int i = 0; i < 16; i++)
            {
                float angle = i * Mathf.PI * 2f / 16f;
                float radius = .9f + (i % 3) * .35f;
                Vector2 candidate = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                if (!IsRecallPositionFree(candidate)) continue;
                _body.linearVelocity = Vector2.zero;
                _body.position = candidate;
                _lastPhysicsPosition = candidate;
                _isMoving = false;
                MageVfx.Play("TrongTruongDiGioi", candidate, .45f, .2f);
                return;
            }

            _body.linearVelocity = Vector2.zero;
        }

        private bool IsRecallPositionFree(Vector2 candidate)
        {
            // Ignore trigger sensors, player and enemies; solids remain blocking.
            foreach (Collider2D c in Physics2D.OverlapCircleAll(candidate, .32f))
            {
                if (c == null || !c.enabled || c.isTrigger || c == _collider ||
                    c.GetComponentInParent<EnemyHealth>() != null ||
                    c.GetComponentInParent<PlayerHealth>() != null)
                    continue;
                return false;
            }
            return true;
        }

        private static Vector2 Rotate(Vector2 value, float angle)
        {
            float s = Mathf.Sin(angle);
            float c = Mathf.Cos(angle);
            return new Vector2(value.x * c - value.y * s, value.x * s + value.y * c);
        }

        private void IgnoreEnemyCollisions()
        {
            if (_collider == null) return;
            foreach (EnemyHealth enemy in EnemyHealth.Active)
            {
                if (enemy == null || enemy.IsDead) continue;
                foreach (Collider2D other in enemy.GetComponentsInChildren<Collider2D>())
                {
                    if (other == null || other.isTrigger || !_ignoredEnemyColliders.Add(other)) continue;
                    // Combat is distance/hit based. Physical pet-enemy pushing is not
                    // needed and made both objects shake; walls still collide normally.
                    Physics2D.IgnoreCollision(_collider, other, true);
                }
            }
        }

        private void AttackIfPossible()
        {
            if (_target == null || _target.IsDead || _attackTimer > 0f ||
                _attackVisualElapsed < AttackVisualDuration ||
                (_fireHawkSignature != null && _fireHawkSignature.IsCasting)) return;

            Vector2 difference = (Vector2)_target.transform.position - (Vector2)transform.position;
            if (difference.sqrMagnitude > EffectiveAttackRange * EffectiveAttackRange) return;
            _attackDirection = difference.sqrMagnitude > .001f ? difference.normalized : Vector2.right;
            _facing = DirectionToFacing(_attackDirection);
            _pendingAttack = _target;
            _attackVisualElapsed = 0f;
            _attackTimer = EffectiveAttackInterval * EquipmentResonance.IntervalFactor(_petEquipment);
        }

        private void AdvanceAttackAnimation(float dt)
        {
            if (_attackVisualElapsed >= AttackVisualDuration) return;
            _attackVisualElapsed += dt;
            if (_pendingAttack == null || _attackVisualElapsed < AttackImpactTime) return;

            EnemyHealth hit = _pendingAttack;
            _pendingAttack = null;
            if (hit == null || hit.IsDead) return;
            Vector2 offset = (Vector2)hit.transform.position - (Vector2)transform.position;
            if (offset.sqrMagnitude > (EffectiveAttackRange + .25f) * (EffectiveAttackRange + .25f)) return;

            // Only the normal strike is tuned; equipped pet passive/active skills keep their budget.
            float normalDamage = AttackDamage * EquipmentResonance.DamageFactor(_petEquipment) *
                (_equippedChip != null ? _equippedChip.NormalDamageFactor : 1f);
            if (_petEquipment != null)
                EnemyEquipmentStatus.ApplyHit(hit, _petEquipment.Effect, normalDamage);
            _skills?.DealDamage(hit, normalDamage, true);
            // Phoenix replaces the legacy generic MageCrystalStrike / FallingCrystal
            // proc, avoiding duplicate skill damage and the unrelated purple effect.
            // Other pets still invoke the exact previous Mage skill callbacks.
            if (_biomechSummon == null) _skills?.OnCompanionNormalHit(hit);
            Vector2 impact = (Vector2)transform.position + _attackDirection * .62f;
            float angle = Mathf.Atan2(_attackDirection.y, _attackDirection.x) * Mathf.Rad2Deg;
            if (_biomechSummon != null)
            {
                if (_fireHawkSignature != null)
                {
                    // Replace the legacy FireHawk auto-proc (which spammed the same VFX
                    // every hit) with its own finite two-bolt signature. Do not stack both.
                    _fireHawkSignature.RegisterNormalHit(hit, AttackDamage, _skills,
                        transform.position);
                }
                else
                {
                    // Dedicated SummonSignatureSkill owns each pet's timed proc.
                    // The old BiomechSummonRuntime proc would double-trigger damage and VFX.
                    // Normal hit itself has already been applied above.
                }
                // Connect the three remaining pet-exclusive signatures, no damage proc duplication.
                if (_otherSignature != null)
                {
                    _otherSignature.OnCompanionHit(hit, AttackDamage, _skills, transform.position);
                    if (_otherSignature.LastTriggered)
                    {
                        _signatureCastElapsed = 0f;
                        SkillId signatureId = SummonSignatureSkill.SkillFor(_petEquipment);
                        _signatureCastLevel5 = _skills != null && _skills.Level(signatureId) >= 5;
                    }
                }
                // Existing pet chips are independent and keep their original damage budget.
                if (_equippedChip != null)
                    _equippedChip.OnNormalHit(hit, AttackDamage, _owner, _skills);
            }
            else MageVfx.Play("TraoLietHopKim", impact, .53f, .23f, false, angle);
        }

        private static int DirectionToFacing(Vector2 direction)
        {
            return Mathf.Abs(direction.x) > Mathf.Abs(direction.y)
                ? (direction.x > 0f ? 1 : 3)
                : (direction.y > 0f ? 0 : 2);
        }

        private void UpdateAttackVisual()
        {
            if (_artTransform == null) return;

            // Phase09: the imported Sprite sheets animate limbs, wings and tail.
            // Do NOT add procedural bobbing, breathing/squash, tilting or attack
            // lunge to the whole visual: that old effect made summons vibrate.
            // Keep only the fixed scale of legacy non-biomech item sprites.
            string equipmentId = _petEquipment != null ? _petEquipment.Id : string.Empty;
            bool useLegacyItemScale = !BiomechMotionArt.IsSupported(equipmentId) &&
                                      equipmentId != "mage_chimera" &&
                                      _equippedPetSprite != null;
            float scale = useLegacyItemScale ? .78f : 1f;
            _artTransform.localPosition = Vector3.zero;
            _artTransform.localRotation = Quaternion.identity;
            _artTransform.localScale = new Vector3(scale, scale, 1f);
        }

        private void ChooseRoam()
        {
            float radius = _owner != null ? _owner.LeashRadius : 5f;
            _roamOffset = Random.insideUnitCircle * radius * .58f;
            _roamTimer = Random.Range(3f, 5f);
        }

    }
}
