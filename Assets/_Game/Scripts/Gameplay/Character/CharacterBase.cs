using UnityEngine;

namespace DienGame.Gameplay.Character
{
    public abstract class CharacterBase : MonoBehaviour
    {
        [Header("Base Stats")]
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField, Min(0f)] private float damage = 10f;
        [SerializeField, Min(0f)] private float moveSpeed = 5f;

        public float MaxHealth => maxHealth;
        public float Damage => damage;
        public float MoveSpeed => moveSpeed;

        public void SetMaxHealth(float value)
        {
            maxHealth = Mathf.Max(1f, value);
        }

        public void SetDamage(float value)
        {
            damage = Mathf.Max(0f, value);
        }

        public void SetMoveSpeed(float value)
        {
            moveSpeed = Mathf.Max(0f, value);
        }

        public void AddMaxHealth(float value)
        {
            if (value > 0f)
            {
                maxHealth += value;
            }
        }

        public void MultiplyDamage(float multiplier)
        {
            if (multiplier > 0f)
            {
                damage *= multiplier;
            }
        }

        public void MultiplyMoveSpeed(float multiplier)
        {
            if (multiplier > 0f)
            {
                moveSpeed *= multiplier;
            }
        }
    }
}
