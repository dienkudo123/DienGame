using System;
using UnityEngine;

namespace DienGame.Gameplay.Skills.Assassin
{
    [CreateAssetMenu(fileName = "AssassinSpriteData", menuName = "DienGame/Assassin/8-Direction Sprite Data")]
    public sealed class AssassinSpriteAnimationData : ScriptableObject
    {
        // Indices follow PlayerAnimator: Down, DownLeft, Left, UpLeft,
        // Up, UpRight, Right, DownRight.
        [Serializable]
        private sealed class DirectionFrames
        {
            public Sprite[] run = new Sprite[4];
            public Sprite[] slash = new Sprite[4];
        }

        [SerializeField] private DirectionFrames[] directions = new DirectionFrames[8];

        public Sprite GetRun(int direction, int frame)
        {
            return GetSprite(direction, frame, false);
        }

        public Sprite GetSlash(int direction, int frame)
        {
            return GetSprite(direction, frame, true);
        }

        private Sprite GetSprite(int direction, int frame, bool slash)
        {
            if (directions == null || directions.Length != 8)
                return null;

            DirectionFrames data = directions[Mathf.Clamp(direction, 0, 7)];
            if (data == null)
                return null;

            Sprite[] frames = slash ? data.slash : data.run;
            if (frames == null || frames.Length == 0)
                return null;

            return frames[Mathf.Clamp(frame, 0, frames.Length - 1)];
        }

#if UNITY_EDITOR
        public void SetDirection(int index, Sprite[] runFrames, Sprite[] slashFrames)
        {
            if (index < 0 || index >= 8 || runFrames == null || slashFrames == null ||
                runFrames.Length != 4 || slashFrames.Length != 4)
                throw new ArgumentException("Each Assassin direction requires four Run and four Slash sprites.");

            if (directions == null || directions.Length != 8)
                directions = new DirectionFrames[8];

            if (directions[index] == null)
                directions[index] = new DirectionFrames();

            directions[index].run = (Sprite[])runFrames.Clone();
            directions[index].slash = (Sprite[])slashFrames.Clone();
        }
#endif
    }
}
