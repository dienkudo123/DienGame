using System.Collections.Generic;
using DienGame.Gameplay.Phase13;
using UnityEngine;

namespace DienGame.Gameplay.Enemy
{
    /// <summary>
    /// Lightweight hit/death feedback. No permanent colored rings, orbiting dots,
    /// elite circles, target reticles or boss auras around ANY enemy.
    /// Boss telegraph overlays are separate, short-lived attack warnings.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyVisualPolish : MonoBehaviour
    {
        private EnemyProfile _profile;
        private EnemyHealth _health;
        private SpriteRenderer[] _renderers;
        private Color[] _originalColors;
        private float _lastHealth;
        private float _flashTimer;
        private bool _died;
        private bool _ready;

        public void Configure(EnemyProfile profile)
        {
            if (_health != null)
            {
                _health.HealthChanged -= HandleHealthChanged;
                _health.Died -= HandleDied;
            }
            _profile = profile;
            _health = GetComponent<EnemyHealth>();
            StripOldAccessories();
            _renderers = GetComponentsInChildren<SpriteRenderer>(true);
            _originalColors = new Color[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
                _originalColors[i] = _renderers[i] != null ? _renderers[i].color : Color.white;
            if (_health != null)
            {
                _health.HealthChanged += HandleHealthChanged;
                _health.Died += HandleDied;
                _lastHealth = _health.CurrentHealth;
            }
            _flashTimer = 0f;
            _died = false;
            _ready = true;
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
            if (!_ready || _renderers == null) return;
            _flashTimer -= Time.deltaTime;
            if (_flashTimer > 0f) return;
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null && _renderers[i].color != _originalColors[i])
                    _renderers[i].color = _originalColors[i];
            }
        }

        private void HandleHealthChanged(EnemyHealth health)
        {
            if (health == null) return;
            float hp = health.CurrentHealth;
            if (Mathf.Approximately(hp, health.MaxHealth))
            {
                _lastHealth = hp;
                return;
            }
            if (hp > 0f && hp < _lastHealth)
            {
                _flashTimer = .07f;
                if (_renderers != null)
                    for (int i = 0; i < _renderers.Length; i++)
                        if (_renderers[i] != null) _renderers[i].color = Color.white;
                // A small one-shot impact is fine; no persistent outline.
                EnemyDeathVfx.SpawnHit(transform.position,
                    _profile != null ? _profile.DisplayColor : Color.white, .42f);
            }
            _lastHealth = hp;
        }

        private void HandleDied(EnemyHealth health)
        {
            if (_died) return;
            _died = true;
            bool boss = _profile != null && _profile.Archetype == EnemyArchetype.Boss;
            bool elite = _profile != null && _profile.IsElite;
            EnemyDeathVfx.SpawnDeath(transform.position,
                _profile != null ? _profile.DisplayColor : Color.white,
                boss ? 1.4f : elite ? .8f : .55f, boss, elite);
        }

        private void StripOldAccessories()
        {
            // These may be present on an upgraded scene or a reused enemy prefab.
            // Only remove known generated decoration objects, never the Visual sprite.
            HashSet<string> prefixes = new HashSet<string>
            {
                "RunnerRing", "TankOuterArmor", "TankInnerArmor",
                "RangedReticle", "RangedOrb", "SwarmPulse",
                "EliteAura", "BossAura", "BossOrb"
            };
            Transform[] children = GetComponentsInChildren<Transform>(true);
            for (int i = children.Length - 1; i >= 0; i--)
            {
                Transform child = children[i];
                if (child == null || child == transform) continue;
                foreach (string prefix in prefixes)
                {
                    if (!child.name.StartsWith(prefix, System.StringComparison.Ordinal)) continue;
                    child.gameObject.SetActive(false);
                    Destroy(child.gameObject);
                    break;
                }
            }
        }
    }
}
