using UnityEngine;

namespace DienGame.Gameplay.Skills.Visuals
{
    public sealed class JudgementVfxCatalog : ScriptableObject
    {
        [Header("Ultimate Animation")]
        [SerializeField]
        private Sprite[] frames;

        public Sprite[] Frames => frames;

        public bool SetFrames(
            Sprite[] newFrames)
        {
            if (newFrames == null)
            {
                newFrames =
                    System.Array.Empty<Sprite>();
            }

            bool changed =
                frames == null ||
                frames.Length != newFrames.Length;

            if (!changed)
            {
                for (
                    int i = 0;
                    i < newFrames.Length;
                    i++
                )
                {
                    if (frames[i] != newFrames[i])
                    {
                        changed = true;
                        break;
                    }
                }
            }

            if (!changed)
            {
                return false;
            }

            frames =
                new Sprite[
                    newFrames.Length
                ];

            for (
                int i = 0;
                i < newFrames.Length;
                i++
            )
            {
                frames[i] =
                    newFrames[i];
            }

            return true;
        }
    }
}
