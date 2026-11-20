using DienGame.Client;
using DienGame.Gameplay.Equipment;
using UnityEngine;

namespace DienGame.Gameplay.World
{
    /// <summary>
    /// Phase 12 permanent discovery objectives. Each stage is counted once regardless
    /// of how often it is replayed. Keys are independent of the legacy stage index.
    /// No legacy saves are erased or guessed into achievements.
    /// </summary>
    public static class Phase12JourneyProgress
    {
        private const string Prefix = "diengame_phase12_journey_v1_";
        private const string StageMaskKey = Prefix + "cleared_stages_mask";
        private const string TwoIslandsClaimKey = Prefix + "claimed_2_islands";
        private const string FiveIslandsClaimKey = Prefix + "claimed_5_islands";
        private const int AllowedStageMask = 0x1F;

        public static int ClearedStageMask => PlayerPrefs.GetInt(StageMaskKey, 0) & AllowedStageMask;
        public static int DistinctIslandWins
        {
            get
            {
                int bits = ClearedStageMask;
                int count = 0;
                for (int i = 0; i < MetaProgressService.ImplementedStageCount; i++)
                    if ((bits & (1 << i)) != 0) count++;
                return count;
            }
        }

        public static bool HasWonIsland(int stage) => stage >= 1 && stage <= 5 &&
            (ClearedStageMask & (1 << (stage - 1))) != 0;

        /// <summary>Called only after a legitimate Boss victory and once per completed run.</summary>
        public static void RecordVictory(int stage)
        {
            if (EquipmentQaSandbox.Active || stage < 1 || stage > MetaProgressService.ImplementedStageCount)
                return;
            int bit = 1 << (stage - 1);
            int oldMask = ClearedStageMask;
            if ((oldMask & bit) != 0) return;
            PlayerPrefs.SetInt(StageMaskKey, oldMask | bit);
            PlayerPrefs.Save();
        }

        public static bool IsClaimed(int milestone)
        {
            string key = ClaimKey(milestone);
            return key != null && PlayerPrefs.GetInt(key, 0) == 1;
        }

        public static bool TryClaim(int milestone)
        {
            if (EquipmentQaSandbox.Active || DistinctIslandWins < milestone || IsClaimed(milestone))
                return false;
            string key = ClaimKey(milestone);
            if (key == null) return false;

            int gold = milestone == 2 ? 600 : 1500;
            int fragments = milestone == 2 ? 5 : 10;
            // PlayerPrefs is offline-only; the award is one-time, not a repeatable shop.
            PlayerPrefs.SetInt(key, 1);
            MetaProgressService.GrantGold(gold);
            WorldInventory.AddFragments(fragments);
            PlayerPrefs.Save();
            return true;
        }

        private static string ClaimKey(int milestone)
        {
            if (milestone == 2) return TwoIslandsClaimKey;
            if (milestone == 5) return FiveIslandsClaimKey;
            return null;
        }
    }
}
