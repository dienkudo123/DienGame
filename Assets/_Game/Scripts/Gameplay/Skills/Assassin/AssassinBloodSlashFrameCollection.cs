using UnityEngine;

namespace DienGame.Gameplay.Skills.Assassin
{
    // This tiny Resources asset stores direct references to PNG Sprites in Art/Skills.
    // The PNG files themselves are NOT inside Resources.
    public sealed class AssassinBloodSlashFrameCollection : ScriptableObject
    {
        [SerializeField] private Sprite[] frames;
        public Sprite[] Frames => frames;
    }
}
