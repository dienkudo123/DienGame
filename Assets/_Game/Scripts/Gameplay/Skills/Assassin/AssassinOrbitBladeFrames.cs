using UnityEngine;

namespace DienGame.Gameplay.Skills.Assassin
{
    /// <summary>
    /// Lightweight runtime references to art sprites. The PNGs themselves remain under
    /// Assets/_Game/Art/Skills/Assassin/DaoAnhHoThan (not inside Resources).
    /// The Editor creates Assets/_Game/Resources/Assassin/DaoAnhHoThan/Frames.asset.
    /// </summary>
    public sealed class AssassinOrbitBladeFrames : ScriptableObject
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField] private Sprite blade;

        public Sprite[] Frames => frames;
        public Sprite Blade => blade;
    }
}
