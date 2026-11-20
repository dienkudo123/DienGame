using UnityEngine;

namespace DienGame.Gameplay.Skills.Visuals
{
    public sealed class JudgementCannonCatalog : ScriptableObject
    {
        [SerializeField]
        private Sprite[] cannonFrames = System.Array.Empty<Sprite>();

        [SerializeField]
        private Sprite[] beamFrames = System.Array.Empty<Sprite>();

        [SerializeField]
        private Sprite[] shockwaveFrames = System.Array.Empty<Sprite>();

        public Sprite[] CannonFrames => cannonFrames;
        public Sprite[] BeamFrames => beamFrames;
        public Sprite[] ShockwaveFrames => shockwaveFrames;

        public bool SetFrames(
            Sprite[] newCannonFrames,
            Sprite[] newBeamFrames,
            Sprite[] newShockwaveFrames)
        {
            newCannonFrames ??= System.Array.Empty<Sprite>();
            newBeamFrames ??= System.Array.Empty<Sprite>();
            newShockwaveFrames ??= System.Array.Empty<Sprite>();

            bool changed =
                !AreSame(cannonFrames, newCannonFrames) ||
                !AreSame(beamFrames, newBeamFrames) ||
                !AreSame(shockwaveFrames, newShockwaveFrames);

            cannonFrames = newCannonFrames;
            beamFrames = newBeamFrames;
            shockwaveFrames = newShockwaveFrames;

            return changed;
        }

        private static bool AreSame(
            Sprite[] current,
            Sprite[] next)
        {
            if (ReferenceEquals(current, next))
            {
                return true;
            }

            if (current == null || next == null || current.Length != next.Length)
            {
                return false;
            }

            for (int i = 0; i < current.Length; i++)
            {
                if (current[i] != next[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
