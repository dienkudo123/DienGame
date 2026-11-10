using System.Collections;
using DienGame.Gameplay.Balance;
using DienGame.Gameplay.Player;
using UnityEngine;

namespace DienGame.Gameplay.Enemy
{
    [RequireComponent(typeof(EnemyHealth))]
    [RequireComponent(typeof(EnemyMovement))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class BossCombatController : MonoBehaviour
    {
        private EnemyHealth _health;
        private EnemyMovement _movement;
        private Rigidbody2D _rigidbody;
        private PlayerHealth _player;

        private float _specialTimer;
        private int _specialIndex;
        private bool _casting;
        private Material _lineMaterial;

        private void Awake()
        {
            _health = GetComponent<EnemyHealth>();
            _movement = GetComponent<EnemyMovement>();
            _rigidbody = GetComponent<Rigidbody2D>();
            CreateLineMaterial();
        }

        private void Start()
        {
            _player = FindFirstObjectByType<PlayerHealth>();
            _specialTimer = 2.5f;
        }

        private void Update()
        {
            if (_health == null ||
                _health.IsDead ||
                _player == null ||
                _player.IsDead ||
                _casting)
            {
                return;
            }

            _specialTimer -= Time.deltaTime;

            if (_specialTimer > 0f)
            {
                return;
            }

            StartCoroutine(UseNextSpecial());
        }

        private IEnumerator UseNextSpecial()
        {
            _casting = true;

            int phase = GetPhase();

            if (phase == 1)
            {
                yield return SlamRoutine();
            }
            else if (phase == 2)
            {
                if (_specialIndex % 2 == 0)
                {
                    yield return SlamRoutine();
                }
                else
                {
                    yield return BurstRoutine(
                        GameBalance.BossPhase2ProjectileCount
                    );
                }
            }
            else
            {
                int action = _specialIndex % 3;

                if (action == 0)
                {
                    yield return SlamRoutine();
                }
                else if (action == 1)
                {
                    yield return BurstRoutine(
                        GameBalance.BossPhase3ProjectileCount
                    );
                }
                else
                {
                    yield return ChargeRoutine();
                }
            }

            _specialIndex++;
            _specialTimer = GetSpecialInterval(phase);
            _casting = false;
        }

        private int GetPhase()
        {
            float healthNormalized = _health.HealthNormalized;

            if (healthNormalized <=
                GameBalance.BossPhase3HealthNormalized)
            {
                return 3;
            }

            if (healthNormalized <=
                GameBalance.BossPhase2HealthNormalized)
            {
                return 2;
            }

            return 1;
        }

        private static float GetSpecialInterval(int phase)
        {
            return phase switch
            {
                3 => GameBalance.BossPhase3SpecialInterval,
                2 => GameBalance.BossPhase2SpecialInterval,
                _ => GameBalance.BossPhase1SpecialInterval,
            };
        }

        private IEnumerator SlamRoutine()
        {
            SetMovementEnabled(false);

            LineRenderer ring = CreateRing(
                "Boss_Slam_Warning",
                GameBalance.BossSlamRadius,
                new Color(1f, 0.28f, 0.12f, 0.9f)
            );

            float timer = 0f;

            while (timer < GameBalance.BossSlamWarningDuration)
            {
                timer += Time.deltaTime;

                if (ring != null)
                {
                    float pulse =
                        0.75f +
                        Mathf.PingPong(timer * 2.5f, 0.25f);

                    ring.widthMultiplier = 0.06f * pulse;
                }

                yield return null;
            }

            if (_player != null && !_player.IsDead)
            {
                float distance = Vector2.Distance(
                    transform.position,
                    _player.transform.position
                );

                if (distance <= GameBalance.BossSlamRadius)
                {
                    _player.TakeDamage(GameBalance.BossSlamDamage);
                }
            }

            if (ring != null)
            {
                Destroy(ring.gameObject);
            }

            yield return new WaitForSeconds(0.12f);
            SetMovementEnabled(true);
        }

        private IEnumerator BurstRoutine(int projectileCount)
        {
            SetMovementEnabled(false);

            LineRenderer ring = CreateRing(
                "Boss_Burst_Warning",
                1.2f,
                new Color(1f, 0.72f, 0.18f, 0.9f)
            );

            yield return new WaitForSeconds(0.35f);

            float angleStep = 360f / Mathf.Max(1, projectileCount);
            float angleOffset = Random.Range(0f, angleStep);

            for (int i = 0; i < projectileCount; i++)
            {
                float angle =
                    (angleOffset + angleStep * i) * Mathf.Deg2Rad;

                Vector2 direction = new Vector2(
                    Mathf.Cos(angle),
                    Mathf.Sin(angle)
                );

                EnemyProjectile.Create(
                    (Vector2)transform.position + direction * 0.7f,
                    direction,
                    GameBalance.BossBurstProjectileSpeed,
                    GameBalance.BossBurstProjectileDamage,
                    4f,
                    new Color(1f, 0.25f, 0.16f, 1f),
                    0.16f
                );
            }

            if (ring != null)
            {
                Destroy(ring.gameObject);
            }

            yield return new WaitForSeconds(0.18f);
            SetMovementEnabled(true);
        }

        private IEnumerator ChargeRoutine()
        {
            if (_player == null)
            {
                yield break;
            }

            SetMovementEnabled(false);

            Vector2 direction =
                ((Vector2)_player.transform.position -
                 (Vector2)transform.position).normalized;

            LineRenderer line = CreateChargeLine(direction);

            yield return new WaitForSeconds(
                GameBalance.BossChargeWarningDuration
            );

            if (line != null)
            {
                Destroy(line.gameObject);
            }

            float timer = 0f;
            bool damagedPlayer = false;

            while (timer < GameBalance.BossChargeDuration)
            {
                timer += Time.fixedDeltaTime;

                _rigidbody.linearVelocity =
                    direction * GameBalance.BossChargeSpeed;

                if (!damagedPlayer &&
                    _player != null &&
                    !_player.IsDead)
                {
                    float distance = Vector2.Distance(
                        transform.position,
                        _player.transform.position
                    );

                    if (distance <= GameBalance.BossChargeHitRadius)
                    {
                        _player.TakeDamage(
                            GameBalance.BossChargeDamage
                        );
                        damagedPlayer = true;
                    }
                }

                yield return new WaitForFixedUpdate();
            }

            _rigidbody.linearVelocity = Vector2.zero;
            yield return new WaitForSeconds(0.12f);
            SetMovementEnabled(true);
        }

        private void SetMovementEnabled(bool value)
        {
            if (_movement != null)
            {
                _movement.StopImmediately();
                _movement.enabled = value;
            }

            if (!value && _rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector2.zero;
            }
        }

        private LineRenderer CreateRing(
            string objectName,
            float radius,
            Color color)
        {
            GameObject ringObject = new GameObject(objectName);
            ringObject.transform.SetParent(transform, false);

            LineRenderer lineRenderer =
                ringObject.AddComponent<LineRenderer>();

            const int Segments = 48;

            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = true;
            lineRenderer.positionCount = Segments;
            lineRenderer.startWidth = 0.07f;
            lineRenderer.endWidth = 0.07f;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.sortingOrder = 30;
            lineRenderer.material = _lineMaterial;

            for (int i = 0; i < Segments; i++)
            {
                float angle =
                    (Mathf.PI * 2f * i) / Segments;

                lineRenderer.SetPosition(
                    i,
                    new Vector3(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(angle) * radius,
                        0f
                    )
                );
            }

            return lineRenderer;
        }

        private LineRenderer CreateChargeLine(Vector2 direction)
        {
            GameObject lineObject =
                new GameObject("Boss_Charge_Warning");

            lineObject.transform.SetParent(transform, false);

            LineRenderer lineRenderer =
                lineObject.AddComponent<LineRenderer>();

            lineRenderer.useWorldSpace = false;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(
                1,
                (Vector3)(direction * 6f)
            );
            lineRenderer.startWidth = 0.14f;
            lineRenderer.endWidth = 0.04f;
            lineRenderer.startColor =
                new Color(1f, 0.18f, 0.1f, 0.95f);
            lineRenderer.endColor =
                new Color(1f, 0.18f, 0.1f, 0.25f);
            lineRenderer.sortingOrder = 30;
            lineRenderer.material = _lineMaterial;

            return lineRenderer;
        }

        private void CreateLineMaterial()
        {
            Shader shader = Shader.Find("Sprites/Default");

            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Unlit");
            }

            if (shader != null)
            {
                _lineMaterial = new Material(shader)
                {
                    name = "BossTelegraph_RuntimeMaterial"
                };
            }
        }

        private void OnDestroy()
        {
            if (_lineMaterial != null)
            {
                Destroy(_lineMaterial);
            }
        }
    }
}
