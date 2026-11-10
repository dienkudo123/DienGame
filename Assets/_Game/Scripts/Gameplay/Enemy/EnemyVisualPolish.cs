using System.Collections.Generic;
using UnityEngine;

namespace DienGame.Gameplay.Enemy
{
    [DisallowMultipleComponent]
    public sealed class EnemyVisualPolish : MonoBehaviour
    {
        private static Material s_sharedLineMaterial;

        private readonly List<LineRenderer> _rings = new();
        private readonly List<Transform> _orbiters = new();

        private EnemyProfile _profile;
        private EnemyHealth _health;
        private SpriteRenderer[] _spriteRenderers;
        private Color[] _baseColors;

        private TrailRenderer _trail;
        private float _lastHealth;
        private float _hitFlashTimer;
        private float _spawnPulseTimer;
        private float _time;
        private bool _configured;
        private bool _deathVfxSpawned;

        public void Configure(EnemyProfile profile)
        {
            _profile = profile;
            _health = GetComponent<EnemyHealth>();
            _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            _baseColors = new Color[_spriteRenderers.Length];

            for (int i = 0; i < _spriteRenderers.Length; i++)
            {
                _baseColors[i] = _spriteRenderers[i] != null
                    ? _spriteRenderers[i].color
                    : Color.white;
            }

            if (_health != null)
            {
                _health.HealthChanged -= HandleHealthChanged;
                _health.Died -= HandleDied;
                _health.HealthChanged += HandleHealthChanged;
                _health.Died += HandleDied;
                _lastHealth = _health.CurrentHealth;
            }

            ClearRuntimeAccessories();
            BuildArchetypeAccessories();

            _spawnPulseTimer = 0.2f;
            _configured = true;
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.HealthChanged -= HandleHealthChanged;
                _health.Died -= HandleDied;
            }
        }

        private void Update()
        {
            if (!_configured || _profile == null)
            {
                return;
            }

            _time += Time.deltaTime;

            UpdateHitFlash();
            UpdateSpawnPulse();
            UpdateAccessories();
        }

        private void HandleHealthChanged(EnemyHealth health)
        {
            if (health == null)
            {
                return;
            }

            float currentHealth = health.CurrentHealth;

            if (Mathf.Approximately(currentHealth, health.MaxHealth))
            {
                _lastHealth = currentHealth;
                return;
            }

            if (currentHealth < _lastHealth)
            {
                _hitFlashTimer = 0.085f;

                EnemyDeathVfx.SpawnHit(
                    transform.position,
                    _profile != null ? _profile.DisplayColor : Color.white,
                    GetVisualRadius() * 0.65f
                );
            }

            _lastHealth = currentHealth;
        }

        private void HandleDied(EnemyHealth health)
        {
            if (_deathVfxSpawned)
            {
                return;
            }

            _deathVfxSpawned = true;

            float scale = GetVisualRadius();
            bool boss = _profile != null &&
                        _profile.Archetype == EnemyArchetype.Boss;

            bool elite = _profile != null && _profile.IsElite;

            EnemyDeathVfx.SpawnDeath(
                transform.position,
                _profile != null ? _profile.DisplayColor : Color.white,
                scale,
                boss,
                elite
            );
        }

        private void UpdateHitFlash()
        {
            if (_hitFlashTimer > 0f)
            {
                _hitFlashTimer -= Time.deltaTime;

                for (int i = 0; i < _spriteRenderers.Length; i++)
                {
                    SpriteRenderer renderer = _spriteRenderers[i];

                    if (renderer != null)
                    {
                        renderer.color = Color.white;
                    }
                }

                return;
            }

            RestoreSpriteColors();
        }

        private void UpdateSpawnPulse()
        {
            if (_spawnPulseTimer <= 0f)
            {
                return;
            }

            _spawnPulseTimer -= Time.deltaTime;
        }

        private void RestoreSpriteColors()
        {
            if (_spriteRenderers == null || _baseColors == null)
            {
                return;
            }

            for (int i = 0; i < _spriteRenderers.Length; i++)
            {
                SpriteRenderer renderer = _spriteRenderers[i];

                if (renderer != null)
                {
                    renderer.color = _baseColors[i];
                }
            }
        }

        private void BuildArchetypeAccessories()
        {
            if (_profile == null)
            {
                return;
            }

            switch (_profile.Archetype)
            {
                case EnemyArchetype.Runner:
                    BuildRunnerTrail();
                    CreateRing("RunnerRing", 0.34f, 0.025f, _profile.DisplayColor, 0.42f);
                    break;

                case EnemyArchetype.Tank:
                    CreateRing("TankOuterArmor", 0.64f, 0.07f, _profile.DisplayColor, 0.55f);
                    CreateRing("TankInnerArmor", 0.48f, 0.035f, Color.white, 0.28f);
                    break;

                case EnemyArchetype.Ranged:
                    CreateRing("RangedReticle", 0.5f, 0.035f, _profile.DisplayColor, 0.62f);
                    CreateOrbiter("RangedOrbA", 0.62f, 0f, _profile.DisplayColor);
                    CreateOrbiter("RangedOrbB", 0.62f, 180f, _profile.DisplayColor);
                    break;

                case EnemyArchetype.Swarm:
                    CreateRing("SwarmPulse", 0.34f, 0.025f, _profile.DisplayColor, 0.42f);
                    break;

                case EnemyArchetype.Boss:
                    CreateRing("BossAuraOuter", 1.05f, 0.075f, _profile.DisplayColor, 0.68f);
                    CreateRing("BossAuraInner", 0.82f, 0.04f, Color.white, 0.25f);
                    CreateOrbiter("BossOrbA", 1.18f, 0f, _profile.DisplayColor);
                    CreateOrbiter("BossOrbB", 1.18f, 120f, _profile.DisplayColor);
                    CreateOrbiter("BossOrbC", 1.18f, 240f, _profile.DisplayColor);
                    break;
            }

            if (_profile.IsElite && _profile.Archetype != EnemyArchetype.Boss)
            {
                Color eliteColor = new Color(1f, 0.72f, 0.12f, 1f);
                CreateRing("EliteAuraOuter", 0.78f, 0.055f, eliteColor, 0.72f);
                CreateRing("EliteAuraInner", 0.61f, 0.03f, Color.white, 0.32f);
            }
        }

        private void BuildRunnerTrail()
        {
            _trail = gameObject.AddComponent<TrailRenderer>();
            _trail.time = 0.18f;
            _trail.minVertexDistance = 0.05f;
            _trail.startWidth = 0.22f;
            _trail.endWidth = 0f;
            _trail.material = GetSharedLineMaterial();
            _trail.startColor = new Color(
                _profile.DisplayColor.r,
                _profile.DisplayColor.g,
                _profile.DisplayColor.b,
                0.45f
            );
            _trail.endColor = new Color(
                _profile.DisplayColor.r,
                _profile.DisplayColor.g,
                _profile.DisplayColor.b,
                0f
            );
            _trail.sortingOrder = -2;
        }

        private void UpdateAccessories()
        {
            EnemyArchetype archetype = _profile.Archetype;

            for (int i = 0; i < _rings.Count; i++)
            {
                LineRenderer ring = _rings[i];

                if (ring == null)
                {
                    continue;
                }

                float direction = i % 2 == 0 ? 1f : -1f;
                ring.transform.localRotation = Quaternion.Euler(
                    0f,
                    0f,
                    _time * 30f * direction
                );

                float pulse = 1f;

                if (archetype == EnemyArchetype.Swarm)
                {
                    pulse = 0.88f + Mathf.Sin(_time * 9f) * 0.12f;
                }
                else if (archetype == EnemyArchetype.Boss)
                {
                    pulse = 0.94f + Mathf.Sin(_time * 3.2f + i) * 0.06f;
                }
                else if (_profile.IsElite)
                {
                    pulse = 0.95f + Mathf.Sin(_time * 5f + i) * 0.05f;
                }

                ring.transform.localScale = Vector3.one * pulse;
            }

            for (int i = 0; i < _orbiters.Count; i++)
            {
                Transform orbiter = _orbiters[i];

                if (orbiter == null)
                {
                    continue;
                }

                float speed = archetype == EnemyArchetype.Boss ? 58f : 92f;
                orbiter.localRotation = Quaternion.Euler(
                    0f,
                    0f,
                    _time * speed + i * 137f
                );
            }

            if (archetype == EnemyArchetype.Boss)
            {
                UpdateBossPhaseColors();
            }
        }

        private void UpdateBossPhaseColors()
        {
            if (_health == null)
            {
                return;
            }

            Color phaseColor;

            if (_health.HealthNormalized <= 0.35f)
            {
                phaseColor = new Color(1f, 0.08f, 0.35f, 1f);
            }
            else if (_health.HealthNormalized <= 0.70f)
            {
                phaseColor = new Color(1f, 0.42f, 0.08f, 1f);
            }
            else
            {
                phaseColor = _profile.DisplayColor;
            }

            for (int i = 0; i < _rings.Count; i++)
            {
                if (_rings[i] == null)
                {
                    continue;
                }

                Color start = phaseColor;
                start.a = i == 0 ? 0.72f : 0.34f;
                Color end = start;
                end.a *= 0.75f;

                _rings[i].startColor = start;
                _rings[i].endColor = end;
            }
        }

        private LineRenderer CreateRing(
            string objectName,
            float radius,
            float width,
            Color color,
            float alpha)
        {
            GameObject ringObject = new GameObject(objectName);
            ringObject.transform.SetParent(transform, false);

            LineRenderer line = ringObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = 32;
            line.startWidth = width;
            line.endWidth = width;
            line.material = GetSharedLineMaterial();
            line.sortingOrder = -1;

            Color ringColor = color;
            ringColor.a = alpha;
            line.startColor = ringColor;
            line.endColor = ringColor;

            for (int i = 0; i < line.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2f / line.positionCount;
                line.SetPosition(
                    i,
                    new Vector3(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(angle) * radius * 0.62f,
                        0f
                    )
                );
            }

            _rings.Add(line);
            return line;
        }

        private void CreateOrbiter(
            string objectName,
            float radius,
            float startAngle,
            Color color)
        {
            GameObject pivot = new GameObject(objectName + "_Pivot");
            pivot.transform.SetParent(transform, false);
            pivot.transform.localRotation = Quaternion.Euler(0f, 0f, startAngle);

            GameObject marker = new GameObject(objectName);
            marker.transform.SetParent(pivot.transform, false);
            marker.transform.localPosition = new Vector3(radius, 0f, 0f);

            LineRenderer line = marker.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = 4;
            line.startWidth = 0.04f;
            line.endWidth = 0.04f;
            line.material = GetSharedLineMaterial();
            line.sortingOrder = 3;

            Color orbColor = color;
            orbColor.a = 0.8f;
            line.startColor = orbColor;
            line.endColor = orbColor;

            const float size = 0.10f;
            line.SetPosition(0, new Vector3(0f, size, 0f));
            line.SetPosition(1, new Vector3(size, 0f, 0f));
            line.SetPosition(2, new Vector3(0f, -size, 0f));
            line.SetPosition(3, new Vector3(-size, 0f, 0f));

            _orbiters.Add(pivot.transform);
        }

        private void ClearRuntimeAccessories()
        {
            _rings.Clear();
            _orbiters.Clear();

            Transform[] children = GetComponentsInChildren<Transform>(true);

            for (int i = children.Length - 1; i >= 0; i--)
            {
                Transform child = children[i];

                if (child == null || child == transform)
                {
                    continue;
                }

                string name = child.name;

                if (name.StartsWith("RunnerRing") ||
                    name.StartsWith("Tank") ||
                    name.StartsWith("Ranged") ||
                    name.StartsWith("Swarm") ||
                    name.StartsWith("EliteAura") ||
                    name.StartsWith("BossAura") ||
                    name.StartsWith("BossOrb"))
                {
                    Destroy(child.gameObject);
                }
            }

            TrailRenderer existingTrail = GetComponent<TrailRenderer>();

            if (existingTrail != null)
            {
                Destroy(existingTrail);
            }
        }

        private float GetVisualRadius()
        {
            float radius = 0.65f;

            if (_profile != null)
            {
                radius = _profile.Archetype switch
                {
                    EnemyArchetype.Swarm => 0.38f,
                    EnemyArchetype.Runner => 0.48f,
                    EnemyArchetype.Tank => 0.86f,
                    EnemyArchetype.Ranged => 0.58f,
                    EnemyArchetype.Boss => 1.45f,
                    _ => 0.58f,
                };

                if (_profile.IsElite)
                {
                    radius *= 1.2f;
                }
            }

            return radius;
        }

        private static Material GetSharedLineMaterial()
        {
            if (s_sharedLineMaterial != null)
            {
                return s_sharedLineMaterial;
            }

            Shader shader = Shader.Find("Sprites/Default");

            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Unlit");
            }

            if (shader != null)
            {
                s_sharedLineMaterial = new Material(shader)
                {
                    name = "EnemyVisualPolish_RuntimeMaterial"
                };
            }

            return s_sharedLineMaterial;
        }
    }
}
