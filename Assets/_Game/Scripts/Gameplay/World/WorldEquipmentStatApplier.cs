using DienGame.Client;
using DienGame.Gameplay.Equipment;
using DienGame.Gameplay.Player;
using UnityEngine;

namespace DienGame.Gameplay.World
{
    /// <summary>
    /// Applies persistent core/armor/charm upgrades once per gameplay scene.
    /// Core variant multipliers are reserved for actual normal attacks, not all skills;
    /// only its +level boost affects the global damage stat.
    /// </summary>
    public sealed class WorldEquipmentStatApplier : MonoBehaviour
    {
        private void Start()
        {
            PlayerCharacter pc = FindFirstObjectByType<PlayerCharacter>();
            if (pc == null) return;
            PlayerHealth health = pc.GetComponent<PlayerHealth>();
            float oldHp = pc.MaxHealth;
            EquipmentDefinition core = EquipmentInventory.Selected(LobbySessionService.CurrentClass);
            int coreLevel = EquipmentInventory.Level(core);
            int armor = WorldInventory.ArmorLevel;
            int charm = WorldInventory.CharmLevel;
            if (coreLevel > 0) pc.MultiplyDamage(1f + .035f * coreLevel);
            // Calibration changes NORMAL attack rhythm, not skill level or skill cooldown.
            pc.MultiplyAttackInterval(EquipmentResonance.IntervalFactor(core));
            if (armor > 0) pc.AddMaxHealth(15f * armor);
            if (charm > 0) pc.MultiplyMoveSpeed(1f + .05f * charm);
            if (armor > 0 && health != null) health.Heal(pc.MaxHealth - oldHp);
        }
    }
}
