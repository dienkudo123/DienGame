using UnityEngine;
using DienGame.Gameplay.Phase13;
using UnityEngine.Serialization;

namespace DienGame.Gameplay.Player
{
    public sealed class PlayerShootVisualSignal :
        MonoBehaviour
    {
        [Header("Shot Visual")]

        [FormerlySerializedAs(
            "fireVisualDuration"
        )]
        [SerializeField, Min(0.01f)]
        private float shotVisualDuration =
            0.12f;

        private float _shotEndTime;

        public bool IsShooting =>
            Time.time <
            _shotEndTime;

        public void TriggerShot()
        {
            _shotEndTime = Time.time + shotVisualDuration;
            Phase13Audio.Play(Phase13Cue.Shot);
        }
    }
}
