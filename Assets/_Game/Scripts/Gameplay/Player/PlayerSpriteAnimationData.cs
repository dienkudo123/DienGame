using System;
using UnityEngine;

namespace DienGame.Gameplay.Player
{
    [CreateAssetMenu(
        fileName = "PlayerGunSpriteData",
        menuName = "DienGame/Player Gun Sprite Data"
    )]
    public sealed class PlayerSpriteAnimationData : ScriptableObject
    {
        [Serializable]
        private sealed class DirectionFrames
        {
            public Sprite idle;
            public Sprite[] run = new Sprite[4];

            public Sprite idleShot;
            public Sprite[] runShot = new Sprite[4];
        }

        [SerializeField]
        private DirectionFrames[] directions =
            new DirectionFrames[8];

        public Sprite GetSprite(
            int directionIndex,
            bool isMoving,
            int runFrame,
            bool isShooting)
        {
            EnsureData();

            directionIndex = Mathf.Clamp(directionIndex, 0, 7);
            DirectionFrames direction = directions[directionIndex];

            if (!isMoving)
            {
                return isShooting
                    ? direction.idleShot
                    : direction.idle;
            }

            runFrame = Mathf.Clamp(runFrame, 0, 3);

            return isShooting
                ? direction.runShot[runFrame]
                : direction.run[runFrame];
        }

        public void SetDirection(
            int directionIndex,
            Sprite[] sprites)
        {
            EnsureData();

            if (sprites == null || sprites.Length != 10)
            {
                Debug.LogError("Mỗi hướng phải có đúng 10 sprite.");
                return;
            }

            DirectionFrames direction = directions[directionIndex];

            direction.idle = sprites[0];

            for (int i = 0; i < 4; i++)
            {
                direction.run[i] = sprites[i + 1];
            }

            direction.idleShot = sprites[5];

            for (int i = 0; i < 4; i++)
            {
                direction.runShot[i] = sprites[i + 6];
            }
        }

        private void EnsureData()
        {
            if (directions == null || directions.Length != 8)
            {
                directions = new DirectionFrames[8];
            }

            for (int i = 0; i < directions.Length; i++)
            {
                if (directions[i] == null)
                {
                    directions[i] = new DirectionFrames();
                }

                if (directions[i].run == null || directions[i].run.Length != 4)
                {
                    directions[i].run = new Sprite[4];
                }

                if (directions[i].runShot == null || directions[i].runShot.Length != 4)
                {
                    directions[i].runShot = new Sprite[4];
                }
            }
        }
    }
}
