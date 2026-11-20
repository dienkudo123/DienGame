using System;
using UnityEngine;

namespace DienGame.Gameplay.Enemy
{
    [Serializable]
    public sealed class EnemyVisualSet
    {
        [SerializeField] private Sprite[] frames = Array.Empty<Sprite>();
        [SerializeField, Min(0.1f)] private float framesPerSecond = 6f;
        [SerializeField, Min(0.01f)] private float visualScale = 1f;

        public Sprite[] Frames => frames;
        public float FramesPerSecond => framesPerSecond;
        public float VisualScale => visualScale;

        public bool HasFrames
        {
            get
            {
                if (frames == null || frames.Length == 0)
                {
                    return false;
                }

                for (int i = 0; i < frames.Length; i++)
                {
                    if (frames[i] != null)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public void Configure(
            Sprite[] newFrames,
            float newFramesPerSecond,
            float newVisualScale)
        {
            frames = newFrames ?? Array.Empty<Sprite>();
            framesPerSecond = Mathf.Max(0.1f, newFramesPerSecond);
            visualScale = Mathf.Max(0.01f, newVisualScale);
        }
    }
}
