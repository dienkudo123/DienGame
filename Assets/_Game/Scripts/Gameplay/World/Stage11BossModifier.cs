using System.Collections;
using DienGame.Gameplay.Enemy;
using DienGame.Gameplay.Player;
using UnityEngine;

namespace DienGame.Gameplay.World
{
    /// <summary>
    /// Additional low-damage, strongly signaled boss signature. Existing BossCombatController
    /// and three original attack phases are untouched. No crowd-control chain or damage-over-time.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyHealth))]
    public sealed class Stage11BossModifier : MonoBehaviour
    {
        private Stage11EncounterProfile _profile;
        private EnemyHealth _health;
        private BossCombatController _originalBoss;
        private PlayerHealth _player;
        private float _timer;
        private bool _casting;

        public void Configure(Stage11EncounterProfile profile)
        {
            _profile = profile;
            _timer = 10f;
        }

        private void Awake()
        {
            _health = GetComponent<EnemyHealth>();
            _originalBoss = GetComponent<BossCombatController>();
        }
        private void Start() { _player = FindFirstObjectByType<PlayerHealth>(); }

        private void Update()
        {
            if (_profile.Stage <= 1 || _player == null || _player.IsDead ||
                _health == null || _health.IsDead || _casting ||
                (_originalBoss != null && _originalBoss.IsCasting)) return;
            _timer -= Time.deltaTime;
            if (_timer > 0f) return;
            _timer = _profile.BossSpecialInterval;
            StartCoroutine(_profile.Stage == 3 || _profile.Stage == 5 ? Beam() : Pulse());
        }

        private IEnumerator Pulse()
        {
            _casting = true;
            Vector2 source = transform.position;
            Vector2 center = _profile.Stage == 2 ? (Vector2)_player.transform.position : source;
            float radius = _profile.Stage == 2 ? 1.55f : 2.25f;
            Color color = _profile.Color; color.a = 1f;
            LineRenderer ring = Stage11Telegraph.Circle(transform, center, radius, color);
            yield return new WaitForSeconds(1.25f);
            if (ring != null) Destroy(ring.gameObject);
            if (_health != null && !_health.IsDead && _player != null && !_player.IsDead &&
                (_originalBoss == null || !_originalBoss.IsCasting) &&
                ((Vector2)_player.transform.position - center).sqrMagnitude <= radius * radius &&
                !Stage11WorldGeometry.TerrainOccludes(center, _player.transform.position))
                _player.TakeDamage(_player.MaxHealth * .065f);
            _casting = false;
        }

        private IEnumerator Beam()
        {
            _casting = true;
            Vector2 from = transform.position;
            Vector2 vector = (Vector2)_player.transform.position - from;
            if (vector.sqrMagnitude < .01f) vector = Vector2.right;
            Vector2 direction = vector.normalized;
            const float reach = 8f;
            float halfWidth = _profile.Stage == 3 ? .55f : .70f;
            Vector2 to = from + direction * reach;
            Color color = _profile.Color; color.a = 1f;
            LineRenderer outline = Stage11Telegraph.Strip(transform, from, to, halfWidth, color);
            yield return new WaitForSeconds(1.40f);
            if (outline != null) Destroy(outline.gameObject);
            if (_health != null && !_health.IsDead && _player != null && !_player.IsDead &&
                (_originalBoss == null || !_originalBoss.IsCasting))
            {
                Vector2 delta = (Vector2)_player.transform.position - from;
                float along = Vector2.Dot(delta, direction);
                float sideways = Mathf.Abs(delta.x * direction.y - delta.y * direction.x);
                if (along >= 0f && along <= reach && sideways <= halfWidth &&
                    !Stage11WorldGeometry.TerrainOccludes(from, _player.transform.position))
                    _player.TakeDamage(_player.MaxHealth * .07f);
            }
            _casting = false;
        }
    }
}
