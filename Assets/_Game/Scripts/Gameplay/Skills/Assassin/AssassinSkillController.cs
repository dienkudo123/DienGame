using System.Collections.Generic;
using DienGame.Gameplay.Balance;
using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Player;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Assassin
{
    [RequireComponent(typeof(PlayerSkillManager))]
    public sealed class AssassinSkillController : MonoBehaviour
    {
        // Runtime state is separate for Player and each clone (no shared combo counters).
        public sealed class AttackState
        {
            public int SuccessfulSlashes;
            public float KnifeCooldown;
            public float OrbitCooldown;
            public float OrbitActive;
            public float OrbitTick;
            public float OrbitAngle;
            public AssassinOrbitBladeVfx OrbitVisual;
        }

        private readonly AttackState _playerState = new AttackState();
        private readonly Dictionary<EnemyHealth, int> _deathMarks = new Dictionary<EnemyHealth, int>();
        private readonly List<Collider2D> _hits = new List<Collider2D>(48);
        private readonly List<RaycastHit2D> _wallHits = new List<RaycastHit2D>(12);
        private readonly HashSet<EnemyHealth> _unique = new HashSet<EnemyHealth>();
        private ContactFilter2D _filter;
        private PlayerSkillManager _manager;
        private PlayerCharacter _character;
        private AssassinSpriteAnimator _sourceAnimator;
        private SpriteRenderer _sourceRenderer;
        private AssassinShadowClone _activeClone;
        private float _cloneCooldown;
        private float _dodgeCooldown;
        private float _afterimageSpeedRemaining;
        public bool IsAfterimageSpeedBoostActive => _afterimageSpeedRemaining > 0f;

        [Header("Assassin-only passive ranges")]
        [SerializeField, Min(.2f)] private float meleeRange = 1.8f;
        [SerializeField, Min(1f)] private float daggerSearchRange = 6f;
        [SerializeField, Min(.2f)] private float orbitRadius = 1.05f;
        [SerializeField] private LayerMask obstacleLayers = ~0;

        public float CurrentAttackDamage => _character != null ? _character.Damage : GameBalance.PlayerDamage;
        public float CurrentAttackInterval => _character != null ? Mathf.Max(.05f, _character.AttackInterval) : GameBalance.PlayerAttackInterval;

        private void Awake()
        {
            _manager = GetComponent<PlayerSkillManager>();
            _character = GetComponent<PlayerCharacter>();
            _sourceAnimator = GetComponentInChildren<AssassinSpriteAnimator>(true);
            _sourceRenderer = _sourceAnimator != null ? _sourceAnimator.GetComponent<SpriteRenderer>() : null;
            if (GetComponent<AssassinSoulPillarSystem>() == null)
                gameObject.AddComponent<AssassinSoulPillarSystem>();
            _filter = new ContactFilter2D { useLayerMask = false, useTriggers = false };
        }

        private void Update()
        {
            if (Time.deltaTime <= 0f) return; // Do not start skill effects while LevelUp/Pause is open.
            if (_dodgeCooldown > 0f) _dodgeCooldown -= Time.deltaTime;
            if (_afterimageSpeedRemaining > 0f) _afterimageSpeedRemaining -= Time.deltaTime;
            TickActor(_playerState, transform.position, transform, null, CurrentAttackDamage, Time.deltaTime);
            UpdateClone(Time.deltaTime);
        }

        public int Level(SkillId id) => _manager != null ? _manager.GetSkillLevel(id) : 0;

        // Captured at clone creation: subsequent Player level-ups do not mutate an existing clone.
        public int[] SnapshotLevels()
        {
            int[] levels = new int[AssassinSkillCatalog.PassiveSkills.Length];
            for (int i = 0; i < levels.Length; i++) levels[i] = Level(AssassinSkillCatalog.PassiveSkills[i]);
            return levels;
        }

        private int GetLevel(SkillId id, int[] snapshot)
        {
            if (snapshot == null) return Level(id);
            int index = (int)id - (int)SkillId.AssassinBloodSlash;
            return index >= 0 && index < snapshot.Length ? snapshot[index] : 0;
        }

        public bool TryDodgeIncomingDamage()
        {
            if (_dodgeCooldown > 0f) return false;
            int lvl = Level(SkillId.AssassinAfterimage);
            if (lvl <= 0 || Random.value >= AssassinSkillCatalog.DodgeChance(lvl)) return false;
            _dodgeCooldown = 1.5f;
            // Tàn Ảnh has NO ghost clone. Lv5 gains 20% movement speed for 0.8s.
            if (lvl >= 5) _afterimageSpeedRemaining = .8f;
            AssassinDodgeSpark.Spawn(transform, lvl >= 5);
            return true;
        }

        public float DamageAgainst(EnemyHealth enemy, float damage, int[] snapshot = null)
        {
            if (enemy == null || enemy.IsDead || damage <= 0f) return 0f;
            // SkillId 306 is now Hồn Trụ, not the older Tuyệt Mệnh execute bonus.
            return damage;
        }

        // Call AFTER normal direct damage, never from mark explosion/bonus effects.
        public void RegisterNormalHit(EnemyHealth enemy, float baseDamage, int[] snapshot = null)
        {
            if (enemy == null || enemy.IsDead) return;
            int lvl = GetLevel(SkillId.AssassinDeathMark, snapshot);
            if (lvl <= 0) return;

            _deathMarks.TryGetValue(enemy, out int stacks);
            stacks++;
            AssassinDeathMarkVisual.Show(enemy, stacks, lvl);
            if (stacks < (lvl >= 5 ? 2 : 3))
            {
                _deathMarks[enemy] = stacks;
                if (stacks == 1) enemy.Died += ForgetMark;
                return;
            }

            _deathMarks.Remove(enemy);
            enemy.Died -= ForgetMark;
            AssassinDeathMarkVisual.Clear(enemy);
            float amount = DamageAgainst(enemy, baseDamage * AssassinSkillCatalog.MarkDamageRatio(lvl), snapshot);
            enemy.TakeDamage(amount);
            AssassinVfxBank.Play(AssassinVfxKind.DeathMark, enemy.transform.position,
                Vector2.right, _sourceRenderer, lvl >= 5 ? 1.75f : 1.3f, .32f);
        }

        private void ForgetMark(EnemyHealth enemy)
        {
            if (enemy == null) return;
            enemy.Died -= ForgetMark;
            _deathMarks.Remove(enemy);
        }

        // Called ONCE per normal slash that connected with at least one enemy.
        // Every third successful NORMAL slash releases a travelling sword-wave.
        // This is not a melee arc and does not alter the basic attack animation.
        public void RegisterSuccessfulSlash(Vector2 origin, Vector2 facing, float baseDamage,
            AttackState state = null, int[] snapshot = null)
        {
            if (state == null) state = _playerState;
            int level = GetLevel(SkillId.AssassinBloodSlash, snapshot);
            if (level <= 0 || facing.sqrMagnitude < .0001f) return;

            state.SuccessfulSlashes++;
            if (state.SuccessfulSlashes < 3) return;
            state.SuccessfulSlashes = 0;

            float damage = baseDamage * AssassinSkillCatalog.SlashBonus(level);
            // Projectile handles its OWN piercing hits and obstacle collision.
            AssassinBloodSlashVfx.Launch(origin, facing, damage, this, snapshot,
                _sourceRenderer, obstacleLayers, level);
        }

        private void UpdateClone(float dt)
        {
            int lvl = Level(SkillId.AssassinShadowClone);
            if (lvl <= 0) { _cloneCooldown = 0f; return; }
            _cloneCooldown -= dt;
            if (_cloneCooldown > 0f || _activeClone != null) return;
            _cloneCooldown = AssassinSkillCatalog.CloneCooldown(lvl);
            SpawnClone();
        }

        private void SpawnClone()
        {
            if (_sourceRenderer == null || _sourceAnimator == null) return;
            // Cosmetic clone has no collider, so it does not obstruct or redirect enemies.
            Vector2 origin = transform.position;
            Vector2 position = origin;
            bool found = false;
            for (int i = 0; i < 8; i++)
            {
                float theta = i * Mathf.PI / 4f;
                Vector2 candidate = origin + new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)) * .9f;
                _hits.Clear();
                Physics2D.OverlapCircle(candidate, .28f, _filter, _hits);
                bool blocked = false;
                for (int j = 0; j < _hits.Count; j++)
                {
                    Collider2D c = _hits[j];
                    if (c == null || c.transform.IsChildOf(transform) ||
                        c.GetComponentInParent<EnemyHealth>() != null) continue;
                    blocked = true;
                    break;
                }
                if (!blocked)
                {
                    _wallHits.Clear();
                    Vector2 travel = candidate - origin;
                    Physics2D.Raycast(origin, travel.normalized, _filter, _wallHits, travel.magnitude);
                    for (int j = 0; j < _wallHits.Count; j++)
                    {
                        Collider2D c = _wallHits[j].collider;
                        if (c == null || c.transform.IsChildOf(transform) ||
                            c.GetComponentInParent<EnemyHealth>() != null) continue;
                        blocked = true;
                        break;
                    }
                }
                if (!blocked)
                {
                    position = candidate;
                    found = true;
                    break;
                }
            }
            if (!found) { _cloneCooldown = .35f; return; }

            GameObject body = new GameObject("Assassin Shadow Clone (2s)");
            body.transform.position = position;
            GameObject art = new GameObject("Visual");
            art.transform.SetParent(body.transform, false);
            art.transform.localPosition = _sourceRenderer.transform.localPosition;
            art.transform.localScale = _sourceRenderer.transform.localScale;
            SpriteRenderer image = art.AddComponent<SpriteRenderer>();
            image.sprite = _sourceRenderer.sprite;
            image.sortingLayerID = _sourceRenderer.sortingLayerID;
            image.sortingOrder = _sourceRenderer.sortingOrder;
            image.color = new Color(.65f, .42f, 1f, .8f);
            AssassinSpriteAnimator anim = art.AddComponent<AssassinSpriteAnimator>();
            anim.SetAnimationData(_sourceAnimator.AnimationData);
            AssassinCrescentVfx slashVfx = body.AddComponent<AssassinCrescentVfx>();
            AssassinShadowClone clone = body.AddComponent<AssassinShadowClone>();
            slashVfx.SetLevelFive(Level(SkillId.AssassinBloodSlash) >= 5);
            clone.Initialize(this, anim, slashVfx, SnapshotLevels(), CurrentAttackDamage, CurrentAttackInterval, meleeRange);
            _activeClone = clone;
        }

        public void TickActor(AttackState state, Vector2 position, Transform owner,
            int[] snapshot, float baseDamage, float dt)
        {
            UpdateKnife(state, position, owner, snapshot, baseDamage, dt);
            UpdateOrbit(state, position, owner, snapshot, baseDamage, dt);
        }

        private void UpdateKnife(AttackState state, Vector2 origin, Transform owner,
            int[] snapshot, float baseDamage, float dt)
        {
            int lvl = GetLevel(SkillId.AssassinFlyingDagger, snapshot);
            if (lvl <= 0) { state.KnifeCooldown = 0f; return; }
            state.KnifeCooldown -= dt;
            if (state.KnifeCooldown > 0f) return;
            state.KnifeCooldown = AssassinSkillCatalog.KnifeCooldown(lvl);

            EnemyHealth target = NearestEnemy(origin, meleeRange, daggerSearchRange);
            if (target == null) { state.KnifeCooldown = .25f; return; }
            Vector2 heading = (Vector2)target.transform.position - origin;
            if (heading.sqrMagnitude < .001f) return;
            heading.Normalize();
            AssassinThrowingKnife.Create(origin + heading * .55f, heading,
                baseDamage * AssassinSkillCatalog.KnifeDamageRatio(lvl), owner, this, snapshot, obstacleLayers, lvl);
        }

        private EnemyHealth NearestEnemy(Vector2 origin, float minRange, float maxRange)
        {
            _hits.Clear();
            Physics2D.OverlapCircle(origin, maxRange, _filter, _hits);
            float bestDistance = maxRange * maxRange;
            float minSquared = minRange * minRange;
            EnemyHealth closest = null;
            for (int i = 0; i < _hits.Count; i++)
            {
                Collider2D c = _hits[i];
                if (c == null) continue;
                EnemyHealth e = c.GetComponentInParent<EnemyHealth>();
                if (e == null || e.IsDead) continue;
                float sqr = ((Vector2)e.transform.position - origin).sqrMagnitude;
                if (sqr < minSquared || sqr >= bestDistance) continue;
                closest = e;
                bestDistance = sqr;
            }
            return closest;
        }

        // Two/three circular drone effects orbit around the actor and also self-spin.
        // Each effect damages only enemies touching its own current orbit position.
        private void UpdateOrbit(AttackState state, Vector2 origin, Transform owner,
            int[] snapshot, float baseDamage, float dt)
        {
            int level = GetLevel(SkillId.AssassinOrbitBlades, snapshot);
            if (level <= 0)
            {
                state.OrbitActive = 0f;
                if (state.OrbitVisual != null) state.OrbitVisual.SetVisible(false);
                state.OrbitCooldown = 0f;
                return;
            }

            if (state.OrbitActive <= 0f)
            {
                state.OrbitCooldown -= dt;
                if (state.OrbitCooldown > 0f) return;
                state.OrbitActive = 2f;
                state.OrbitTick = 0f;
                state.OrbitAngle = 0f; // elapsed time used to advance orbit
                state.OrbitCooldown = Mathf.Max(8f, 14f - level);
                if (state.OrbitVisual == null)
                    state.OrbitVisual = owner.gameObject.AddComponent<AssassinOrbitBladeVfx>();
                state.OrbitVisual.SetSkillLevel(level);
                state.OrbitVisual.SetVisible(true);
            }

            state.OrbitActive -= dt;
            state.OrbitAngle += dt;
            float droneDistance = Mathf.Max(1.15f, orbitRadius);
            int count = level >= 5 ? 3 : 2;

            if (state.OrbitVisual != null)
            {
                state.OrbitVisual.SetSkillLevel(level);
                state.OrbitVisual.Draw(state.OrbitAngle, droneDistance);
            }

            state.OrbitTick -= dt;
            if (state.OrbitTick <= 0f)
            {
                state.OrbitTick = .25f;
                _hits.Clear();
                _unique.Clear();
                // One orbit-position calculation per blade and damage tick, not per enemy.
                // This avoids repeatedly calling Sin/Cos when many enemies are nearby.
                Vector2 blade0 = origin + AssassinOrbitBladeVfx.GetOrbitOffset(
                    0, count, droneDistance, state.OrbitAngle);
                Vector2 blade1 = origin + AssassinOrbitBladeVfx.GetOrbitOffset(
                    1, count, droneDistance, state.OrbitAngle);
                Vector2 blade2 = count >= 3
                    ? origin + AssassinOrbitBladeVfx.GetOrbitOffset(
                        2, count, droneDistance, state.OrbitAngle)
                    : Vector2.zero;
                const float hitRadiusSquared = .48f * .48f;
                float hitDamage = baseDamage * AssassinSkillCatalog.OrbitDamageRatio(level);
                Physics2D.OverlapCircle(origin, droneDistance + 1.1f, _filter, _hits);
                for (int i = 0; i < _hits.Count; i++)
                {
                    Collider2D collider = _hits[i];
                    if (collider == null) continue;
                    EnemyHealth enemy = collider.GetComponentInParent<EnemyHealth>();
                    if (enemy == null || enemy.IsDead || !_unique.Add(enemy)) continue;

                    Vector2 enemyPosition = enemy.transform.position;
                    bool touched = (enemyPosition - blade0).sqrMagnitude <= hitRadiusSquared ||
                                   (enemyPosition - blade1).sqrMagnitude <= hitRadiusSquared ||
                                   (count >= 3 &&
                                    (enemyPosition - blade2).sqrMagnitude <= hitRadiusSquared);
                    if (!touched) continue;
                    float amount = DamageAgainst(enemy, hitDamage, snapshot);
                    enemy.TakeDamage(amount);
                }
            }

            if (state.OrbitActive <= 0f && state.OrbitVisual != null)
                state.OrbitVisual.SetVisible(false);
        }

        private void OnDestroy()
        {
            foreach (KeyValuePair<EnemyHealth, int> entry in _deathMarks)
                if (entry.Key != null) entry.Key.Died -= ForgetMark;
            _deathMarks.Clear();
        }
    }
}
