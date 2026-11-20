using DienGame.Gameplay.Enemy;
using UnityEngine;

namespace DienGame.Gameplay.Equipment
{
    /// <summary>
    /// One component per affected enemy. Slow and two damage-over-time effects
    /// refresh rather than stack. Boss effects are reduced, never hard crowd-control.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyEquipmentStatus : MonoBehaviour
    {
        private EnemyHealth _health;
        private SpriteRenderer _statusIcon;
        private static Sprite s_slowIcon, s_poisonIcon, s_burnIcon;
        private float _slowUntil;
        private float _slowRatio;
        private float _poisonUntil;
        private float _poisonDps;
        private float _burnUntil;
        private float _burnDps;

        public float MoveMultiplier => Time.time < _slowUntil ? 1f - _slowRatio : 1f;

        private void Awake()
        {
            _health = GetComponent<EnemyHealth>();
            GameObject visual = new GameObject("EquipmentStatusIndicator");
            visual.transform.SetParent(transform, false);
            visual.transform.localPosition = new Vector3(0f, .85f, 0f);
            visual.transform.localScale = Vector3.one * .18f;
            _statusIcon = visual.AddComponent<SpriteRenderer>();
            _statusIcon.sortingOrder = 95;
            _statusIcon.enabled = false;
        }
        private void OnEnable()
        {
            _slowUntil = _poisonUntil = _burnUntil = 0f;
            _slowRatio = _poisonDps = _burnDps = 0f;
            if (_statusIcon != null) _statusIcon.enabled = false;
        }
        private void Update()
        {
            if (_health == null || _health.IsDead || Time.deltaTime <= 0f) return;
            if (_statusIcon != null)
            {
                Sprite symbol = null;
                if (Time.time < _poisonUntil)
                    symbol = s_poisonIcon != null ? s_poisonIcon : (s_poisonIcon = Resources.Load<Sprite>("Equipment/Statuses/Poison"));
                else if (Time.time < _burnUntil)
                    symbol = s_burnIcon != null ? s_burnIcon : (s_burnIcon = Resources.Load<Sprite>("Equipment/Statuses/Burn"));
                else if (Time.time < _slowUntil)
                    symbol = s_slowIcon != null ? s_slowIcon : (s_slowIcon = Resources.Load<Sprite>("Equipment/Statuses/Slow"));
                _statusIcon.sprite = symbol;
                _statusIcon.enabled = symbol != null;
            }
            float damage = 0f;
            if (Time.time < _poisonUntil) damage += _poisonDps * Time.deltaTime;
            if (Time.time < _burnUntil) damage += _burnDps * Time.deltaTime;
            if (damage > 0f) _health.TakeDamage(damage);
        }

        public static void ApplyHit(EnemyHealth enemy, EquipmentEffect effect, float baseDamage)
        {
            if (enemy == null || enemy.IsDead || baseDamage <= 0f) return;
            if (effect == EquipmentEffect.None || effect == EquipmentEffect.Piercing) return;
            if (effect == EquipmentEffect.Splash || effect == EquipmentEffect.Arcane)
            {
                SplashAround(enemy, baseDamage, effect == EquipmentEffect.Arcane ? .22f : .30f);
                return;
            }
            EnemyEquipmentStatus status = enemy.GetComponent<EnemyEquipmentStatus>();
            if (status == null) status = enemy.gameObject.AddComponent<EnemyEquipmentStatus>();
            bool boss = enemy.GetComponent<BossCombatController>() != null;
            switch (effect)
            {
                case EquipmentEffect.Slow:
                    if (Time.time >= status._slowUntil) status._slowRatio = 0f;
                    status._slowRatio = Mathf.Max(status._slowRatio, boss ? .10f : .22f);
                    status._slowUntil = Mathf.Max(status._slowUntil, Time.time + (boss ? .6f : 1.2f));
                    break;
                case EquipmentEffect.Poison:
                    if (Time.time >= status._poisonUntil) status._poisonDps = 0f;
                    status._poisonDps = Mathf.Max(status._poisonDps, baseDamage * .22f / 3f);
                    status._poisonUntil = Time.time + 3f;
                    break;
                case EquipmentEffect.Burn:
                    if (Time.time >= status._burnUntil) status._burnDps = 0f;
                    status._burnDps = Mathf.Max(status._burnDps, baseDamage * .25f / 2.5f);
                    status._burnUntil = Time.time + 2.5f;
                    break;
            }
        }

        private static void SplashAround(EnemyHealth primary, float damage, float fraction)
        {
            Vector2 center = primary.transform.position;
            int targets = 0;
            // Never affect the original enemy twice; cap targets for balance and mobile CPU.
            foreach (EnemyHealth other in EnemyHealth.Active)
            {
                if (other == null || other == primary || other.IsDead) continue;
                if (((Vector2)other.transform.position - center).sqrMagnitude > 1.4f * 1.4f) continue;
                other.TakeDamage(damage * fraction);
                if (++targets >= 2) break;
            }
        }
    }
}
