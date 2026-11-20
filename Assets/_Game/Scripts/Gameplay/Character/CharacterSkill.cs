using UnityEngine;

namespace DienGame.Gameplay.Character
{
    public abstract class CharacterSkill : MonoBehaviour
    {
        public abstract bool CanUse(CharacterBase owner);
        public abstract void Use(CharacterBase owner);
    }
}
