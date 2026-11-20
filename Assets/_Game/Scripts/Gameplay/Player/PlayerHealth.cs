using DienGame.Gameplay.Balance;
using DienGame.Gameplay.Skills;
using DienGame.Gameplay.Skills.Marksman;
using DienGame.Gameplay.Skills.Assassin;
using DienGame.Gameplay.Skills.Mage;
using DienGame.Gameplay.Skills.Warrior;
using DienGame.Gameplay.Phase13;
using UnityEngine;

namespace DienGame.Gameplay.Player
{
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private PlayerCharacter playerCharacter;

        private float _currentHealth;
        private float _invulnerabilityTimer;
        private MarksmanSkillController _marksmanSkills;
        private PlayerSkillManager _skillManager;
        private AssassinSkillController _assassinSkills;
        private MageSkillController _mageSkills;

        public float MaxHealth
        {
            get
            {
                float baseMaxHealth = playerCharacter != null
                    ? playerCharacter.MaxHealth
                    : GameBalance.PlayerMaxHealth;

                float multiplier = _marksmanSkills != null
                    ? _marksmanSkills.MaxHealthMultiplier
                    : 1f;

                return Mathf.Max(1f, baseMaxHealth * multiplier);
            }
        }

        public float CurrentHealth => _currentHealth;
        public bool IsDead => _currentHealth <= 0f;
        public bool IsInvulnerable => _invulnerabilityTimer > 0f;

        private void Awake()
        {
            if (playerCharacter == null)
            {
                playerCharacter = GetComponent<PlayerCharacter>();
            }

            _marksmanSkills = GetComponent<MarksmanSkillController>();
            _skillManager = GetComponent<PlayerSkillManager>();
            _assassinSkills = GetComponent<AssassinSkillController>();
            _mageSkills = GetComponent<MageSkillController>();

            _currentHealth = MaxHealth;
        }

        private void OnEnable()
        {
            if (_skillManager != null)
            {
                _skillManager.SkillLevelChanged += OnSkillLevelChanged;
            }
        }

        private void OnDisable()
        {
            if (_skillManager != null)
            {
                _skillManager.SkillLevelChanged -= OnSkillLevelChanged;
            }
        }

        private void Update()
        {
            if (_invulnerabilityTimer > 0f)
            {
                _invulnerabilityTimer -= Time.deltaTime;
            }
        }

        public void TakeDamage(float damage)
        {
            if (IsDead || IsInvulnerable || damage <= 0f)
            {
                return;
            }

            if (_marksmanSkills != null && _marksmanSkills.TryBlockIncomingDamage())
            {
                // Tránh cùng một collider contact gây thêm hit ngay frame kế tiếp
                // sau khi khiên vừa block.
                _invulnerabilityTimer = GameBalance.PlayerDamageInvulnerability;
                return;
            }

            // Assassin controller may have been added after Awake by the scene hook.
            if (_assassinSkills == null)
                _assassinSkills = GetComponent<AssassinSkillController>();
            _mageSkills = GetComponent<MageSkillController>();
            // Only Assassin has this component. Marksman damage logic stays unchanged.
            if (_assassinSkills != null && _assassinSkills.isActiveAndEnabled &&
                _assassinSkills.TryDodgeIncomingDamage())
            {
                _invulnerabilityTimer = GameBalance.PlayerDamageInvulnerability;
                return;
            }

            if (_mageSkills != null && _mageSkills.isActiveAndEnabled)
            {
                damage = _mageSkills.AbsorbPlayerDamage(damage);
                if (damage <= 0f) return;
            }

            // Warrior-only kinetic guard; all other class hit handling is unchanged.
            WarriorSkillController warriorGuard = GetComponent<WarriorSkillController>();
            if (warriorGuard != null && warriorGuard.isActiveAndEnabled)
                damage *= warriorGuard.GuardFactor;

            float beforeDamage = _currentHealth;
            _currentHealth = Mathf.Max(0f, _currentHealth - damage);
            Phase13CombatMetrics.RecordPlayerDamage(beforeDamage - _currentHealth);
            Phase13Audio.Play(Phase13Cue.PlayerHit);

            if (IsDead)
            {
                Die();
                return;
            }

            _invulnerabilityTimer =
                GameBalance.PlayerDamageInvulnerability;
        }

        public void Heal(float amount)
        {
            if (amount <= 0f || IsDead)
            {
                return;
            }

            float beforeHeal = _currentHealth;
            _currentHealth = Mathf.Min(_currentHealth + amount, MaxHealth);
            Phase13CombatMetrics.RecordHealing(_currentHealth - beforeHeal);
        }

        public void AddMaxHealth(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            if (playerCharacter != null)
            {
                playerCharacter.AddMaxHealth(amount);
            }

            float beforeHeal = _currentHealth;
            _currentHealth = Mathf.Min(_currentHealth + amount, MaxHealth);
            Phase13CombatMetrics.RecordHealing(_currentHealth - beforeHeal);
        }

        private void OnSkillLevelChanged(SkillId skillId, int level)
        {
            // Không Kích có thể giảm Max HP theo level.
            // Clamp ngay để HUD và gameplay không tồn tại CurrentHP > MaxHP.
            _currentHealth = Mathf.Min(_currentHealth, MaxHealth);
        }

        private void Die()
        {
            Debug.Log("PLAYER DEAD - GAME OVER");
        }
    }
}
