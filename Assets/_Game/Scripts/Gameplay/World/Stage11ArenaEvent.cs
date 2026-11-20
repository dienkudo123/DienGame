using System.Collections;
using DienGame.Gameplay.Map;
using DienGame.Gameplay.Player;
using DienGame.Gameplay.Run;
using UnityEngine;

namespace DienGame.Gameplay.World
{
    /// <summary>
    /// One readable, telegraphed environmental event per interval on islands 2-5.
    /// It is survival-only so the existing three-phase boss and its own extra attack
    /// are not stacked with environmental damage in the boss phase.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Stage11ArenaEvent : MonoBehaviour
    {
        private Stage11EncounterProfile _profile;
        private PlayerHealth _player;
        private RectangularMapBoundary _boundary;
        private GameRunController _run;
        private float _timer;
        private bool _casting;

        public void Configure(Stage11EncounterProfile profile)
        {
            _profile = profile;
            _timer = 38f;
        }

        private void Start()
        {
            _player = FindFirstObjectByType<PlayerHealth>();
            _boundary = FindFirstObjectByType<RectangularMapBoundary>();
            _run = FindFirstObjectByType<GameRunController>();
        }

        private void Update()
        {
            if (_profile.Stage <= 1 || _player == null || _player.IsDead || _casting ||
                _run == null || _run.Phase != GameRunController.RunPhase.Survival) return;
            _timer -= Time.deltaTime;
            if (_timer > 0f) return;
            _timer = _profile.HazardInterval;
            StartCoroutine(Cast());
        }

        private IEnumerator Cast()
        {
            _casting = true;
            Vector2 candidate = (Vector2)_player.transform.position + Random.insideUnitCircle * 1.1f;
            if (_boundary != null)
            {
                Vector2 low = _boundary.Min + Vector2.one * 1.5f;
                Vector2 high = _boundary.Max - Vector2.one * 1.5f;
                candidate = new Vector2(Mathf.Clamp(candidate.x, low.x, high.x),
                    Mathf.Clamp(candidate.y, low.y, high.y));
            }
            if (!Stage11WorldGeometry.IsFree(candidate, .30f, _boundary))
            {
                _casting = false;
                yield break;
            }

            Color color = _profile.Color;
            color.a = .95f;
            LineRenderer warning = Stage11Telegraph.Circle(transform, candidate, _profile.HazardRadius, color);
            float duration = _profile.HazardWarning;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                // Cancel if round has ended or boss phase has begun.
                if (_run == null || _run.Phase != GameRunController.RunPhase.Survival ||
                    _player == null || _player.IsDead)
                {
                    if (warning != null) Destroy(warning.gameObject);
                    _casting = false;
                    yield break;
                }
                elapsed += Time.deltaTime;
                if (warning != null) warning.widthMultiplier = .06f + .07f * elapsed / duration;
                yield return null;
            }
            if (warning != null) Destroy(warning.gameObject);
            if (_player != null && !_player.IsDead &&
                ((Vector2)_player.transform.position - candidate).sqrMagnitude <=
                    _profile.HazardRadius * _profile.HazardRadius &&
                !Stage11WorldGeometry.TerrainOccludes(candidate, _player.transform.position))
                _player.TakeDamage(_player.MaxHealth * _profile.HazardHealthFraction);
            _casting = false;
        }
    }
}
