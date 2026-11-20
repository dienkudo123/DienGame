using System.Collections;
using System.Collections.Generic;
using DienGame.Gameplay.Balance;
using DienGame.Gameplay.Player;
using DienGame.Gameplay.World;
using UnityEngine;

namespace DienGame.Gameplay.Enemy
{
    /// <summary>
    /// Four explicitly telegraphed attacks. The hit geometry uses exactly the frozen
    /// warning geometry; none of the boss's normal attacks deal invisible contact damage.
    /// All four moves rotate from phase 1; phases 2/3 only shorten the recovery interval.
    /// </summary>
    [RequireComponent(typeof(EnemyHealth))]
    [RequireComponent(typeof(EnemyMovement))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class BossCombatController : MonoBehaviour
    {
        private EnemyHealth _health;
        private EnemyMovement _movement;
        private Rigidbody2D _body;
        private PlayerHealth _player;
        private float _timer;
        private int _attackIndex;
        private bool _casting;

        public bool IsCasting => _casting;
        private static readonly Color Danger = new Color(1f, .24f, .12f, .95f);
        private static readonly Color ProjectileWarning = new Color(1f, .72f, .20f, .95f);
        private static readonly Color TargetWarning = new Color(.98f, .17f, .45f, .95f);

        private void Awake()
        {
            _health = GetComponent<EnemyHealth>();
            _movement = GetComponent<EnemyMovement>();
            _body = GetComponent<Rigidbody2D>();
            // Ensure an old prefab cannot deliver untelegraphed melee hits.
            EnemyAttack melee = GetComponent<EnemyAttack>();
            if (melee != null) melee.enabled = false;
        }

        private void Start()
        {
            _player = FindFirstObjectByType<PlayerHealth>();
            _timer = 3.0f;
        }

        private void Update()
        {
            if (_casting || _health == null || _health.IsDead ||
                _player == null || _player.IsDead) return;
            _timer -= Time.deltaTime;
            if (_timer <= 0f) StartCoroutine(AttackSequence());
        }

        private IEnumerator AttackSequence()
        {
            _casting = true;
            LockMovement();
            // A clear warning is always displayed BEFORE every damage event.
            switch (_attackIndex % 4)
            {
                case 0: yield return GroundSlam(); break;
                case 1: yield return RadialVolley(); break;
                case 2: yield return CorridorCharge(); break;
                default: yield return LockedTargetBlast(); break;
            }
            _attackIndex++;
            if (_health != null && !_health.IsDead)
            {
                UnlockMovement();
                _timer = GetRecoveryInterval();
            }
            _casting = false;
        }

        private int GetPhase()
        {
            float fraction = _health != null ? _health.HealthNormalized : 1f;
            if (fraction <= GameBalance.BossPhase3HealthNormalized) return 3;
            if (fraction <= GameBalance.BossPhase2HealthNormalized) return 2;
            return 1;
        }

        private float GetRecoveryInterval()
        {
            switch (GetPhase())
            {
                case 3: return GameBalance.BossPhase3SpecialInterval;
                case 2: return GameBalance.BossPhase2SpecialInterval;
                default: return GameBalance.BossPhase1SpecialInterval;
            }
        }

        private bool CanDamagePlayer => _health != null && !_health.IsDead &&
            _player != null && !_player.IsDead;

        // Attack 1: short-range circular shockwave. Its circle is NOT a permanent aura.
        private IEnumerator GroundSlam()
        {
            Vector2 origin = transform.position;
            float radius = GameBalance.BossSlamRadius;
            LineRenderer warning = Stage11Telegraph.Circle(transform, origin, radius, Danger);
            yield return new WaitForSeconds(GameBalance.BossSlamWarningDuration);
            if (warning != null) Destroy(warning.gameObject);
            if (CanDamagePlayer &&
                ((Vector2)_player.transform.position - origin).sqrMagnitude <= radius * radius &&
                !Stage11WorldGeometry.TerrainOccludes(origin, _player.transform.position))
                _player.TakeDamage(GameBalance.BossSlamDamage);
            yield return new WaitForSeconds(.20f);
        }

        // Attack 2: every bullet lane is drawn and LOCKED before projectiles launch.
        private IEnumerator RadialVolley()
        {
            Vector2 origin = transform.position;
            int count = GetPhase() == 3 ? GameBalance.BossPhase3ProjectileCount :
                GameBalance.BossPhase2ProjectileCount;
            float angleOffset = Random.Range(0f, 360f / count);
            Vector2[] directions = new Vector2[count];
            List<LineRenderer> guides = new List<LineRenderer>(count);
            for (int i = 0; i < count; i++)
            {
                float rad = (angleOffset + i * 360f / count) * Mathf.Deg2Rad;
                directions[i] = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                guides.Add(Stage11Telegraph.Strip(transform, origin + directions[i] * .6f,
                    origin + directions[i] * 8f, .19f, ProjectileWarning));
            }
            yield return new WaitForSeconds(1.35f);
            foreach (LineRenderer guide in guides)
                if (guide != null) Destroy(guide.gameObject);
            if (CanDamagePlayer)
            {
                foreach (Vector2 direction in directions)
                    EnemyProjectile.Create(origin + direction * .75f, direction,
                        GameBalance.BossBurstProjectileSpeed,
                        GameBalance.BossBurstProjectileDamage, 1.20f,
                        new Color(1f, .35f, .12f, 1f), .14f);
            }
            yield return new WaitForSeconds(.22f);
        }

        // Attack 3: static corridor telegraph, then collision-respecting Rigidbody2D dash.
        private IEnumerator CorridorCharge()
        {
            if (_player == null) yield break;
            Vector2 from = transform.position;
            Vector2 delta = (Vector2)_player.transform.position - from;
            Vector2 direction = delta.sqrMagnitude > .001f ? delta.normalized : Vector2.down;
            float distance = GameBalance.BossChargeSpeed * GameBalance.BossChargeDuration;
            LineRenderer guide = Stage11Telegraph.Strip(transform, from,
                from + direction * distance, GameBalance.BossChargeHitRadius, Danger);
            yield return new WaitForSeconds(GameBalance.BossChargeWarningDuration);
            if (guide != null) Destroy(guide.gameObject);
            float elapsed = 0f;
            bool hit = false;
            while (elapsed < GameBalance.BossChargeDuration && CanDamagePlayer)
            {
                elapsed += Time.fixedDeltaTime;
                _body.linearVelocity = direction * GameBalance.BossChargeSpeed;
                if (!hit && Vector2.Distance(_body.position, _player.transform.position) <=
                    GameBalance.BossChargeHitRadius)
                {
                    _player.TakeDamage(GameBalance.BossChargeDamage);
                    hit = true;
                }
                yield return new WaitForFixedUpdate();
            }
            _body.linearVelocity = Vector2.zero;
            yield return new WaitForSeconds(.22f);
        }

        // Attack 4: player position is locked on telegraph creation, then stays fixed.
        private IEnumerator LockedTargetBlast()
        {
            if (_player == null) yield break;
            Vector2 center = _player.transform.position;
            const float radius = 1.45f;
            LineRenderer guide = Stage11Telegraph.Circle(transform, center, radius, TargetWarning);
            yield return new WaitForSeconds(1.55f);
            if (guide != null) Destroy(guide.gameObject);
            if (CanDamagePlayer &&
                ((Vector2)_player.transform.position - center).sqrMagnitude <= radius * radius &&
                !Stage11WorldGeometry.TerrainOccludes(center, _player.transform.position))
                _player.TakeDamage(13f);
            yield return new WaitForSeconds(.20f);
        }

        private void LockMovement()
        {
            if (_movement != null)
            {
                _movement.StopImmediately();
                _movement.enabled = false;
            }
            if (_body != null) _body.linearVelocity = Vector2.zero;
        }

        private void UnlockMovement()
        {
            if (_body != null) _body.linearVelocity = Vector2.zero;
            if (_movement != null) _movement.enabled = true;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            if (_body != null) _body.linearVelocity = Vector2.zero;
        }
    }
}
