using DienGame.Client;
using DienGame.Gameplay.Equipment;
using UnityEngine;

namespace DienGame.Gameplay.World
{
    /// <summary>Persistent, small but functional equipment economy for the vertical-slice client.</summary>
    public static class WorldInventory
    {
        private const string Prefix="diengame_world_v1_";
        private const int MaxUpgrade=5;
        public static int Fragments => PlayerPrefs.GetInt(Prefix+"fragments",0);
        public static int Kills => PlayerPrefs.GetInt(Prefix+"kills",0);
        public static int Chests => PlayerPrefs.GetInt(Prefix+"chests",0);
        public static int WonCount => PlayerPrefs.GetInt(Prefix+"wins",0);
        public static int WeaponLevel => GetLevel(0);
        public static int ArmorLevel => GetLevel(1);
        public static int CharmLevel => GetLevel(2);

        public static readonly string[] GearNames={"KIẾM / SÚNG / TRƯỢNG","GIÁP HỘ MỆNH","PHÙ ẤN LONG MẠCH"};
        public static readonly string[] GearDescriptions={"Tăng sát thương đánh và kỹ năng", "Tăng máu tối đa", "Tăng tốc độ di chuyển"};

        public static int GetLevel(int slot)
        {
            int safeSlot = Mathf.Clamp(slot, 0, 2);
            return EquipmentQaSandbox.Active ? EquipmentQaSandbox.SharedLevel(safeSlot) :
                PlayerPrefs.GetInt(Prefix + "gear_" + safeSlot, 0);
        }
        public static int UpgradeGoldCost(int slot) => 120*(GetLevel(slot)+1);
        public static int UpgradeFragmentCost(int slot) => 2+GetLevel(slot)*2;
        /// <summary>Deduct fragments only after purchase checks pass.</summary>
        public static bool SpendFragments(int count)
        {
            if (count < 0 || Fragments < count) return false;
            PlayerPrefs.SetInt(Prefix + "fragments", Fragments - count);
            PlayerPrefs.Save();
            return true;
        }
        public static void AddFragments(int count)
        {
            if (count<=0) return;
            PlayerPrefs.SetInt(Prefix+"fragments",Fragments+count); PlayerPrefs.Save();
        }
        public static void AddChest(int count)
        {
            if(count<=0)return;
            PlayerPrefs.SetInt(Prefix+"chests",Chests+count);PlayerPrefs.Save();
        }
        public static void RecordKill()
        {
            PlayerPrefs.SetInt(Prefix+"kills",Kills+1);PlayerPrefs.Save();
        }
        public static bool TryUpgrade(int slot)
        {
            if (slot < 0 || slot > 2 || GetLevel(slot) >= MaxUpgrade) return false;
            if (EquipmentQaSandbox.Active)
            {
                EquipmentQaSandbox.SetSharedLevel(slot, GetLevel(slot) + 1);
                return true; // Test-only, keep real coins and shards unchanged.
            }
            if (Fragments < UpgradeFragmentCost(slot)) return false;
            int cost=UpgradeGoldCost(slot);
            if(!MetaProgressService.TrySpendGold(cost)) return false;
            PlayerPrefs.SetInt(Prefix+"fragments",Fragments-UpgradeFragmentCost(slot));
            PlayerPrefs.SetInt(Prefix+"gear_"+slot,GetLevel(slot)+1);
            PlayerPrefs.Save(); return true;
        }
        public static bool BuyFragments(int count,int price)
        {
            if(!MetaProgressService.TrySpendGold(price))return false;
            AddFragments(count);return true;
        }
        public static bool BuyChest()
        {
            if(!MetaProgressService.TrySpendGold(450))return false;
            AddChest(1);AddFragments(3);return true;
        }
        public static void GrantStageReward(int stage)
        {
            stage=Mathf.Clamp(stage,1,5);
            int gold=LobbyStageCatalog.Get(stage).RewardGold;
            MetaProgressService.GrantGold(gold);
            AddFragments(stage+1);
            AddChest(LobbyStageCatalog.Get(stage).RewardChest);
            PlayerPrefs.SetInt(Prefix+"wins",WonCount+1);
            PlayerPrefs.Save();
        }
        public static int LobbyStageRewardGold(int stage)
        {
            switch(stage) { case 1:return 500;case 2:return 700;case 3:return 900;case 4:return 1200;default:return 1800; }
        }
        public static bool TryClaimKillQuest()
        {
            if(Kills<20 || PlayerPrefs.GetInt(Prefix+"kill_quest",0)!=0)return false;
            PlayerPrefs.SetInt(Prefix+"kill_quest",1);MetaProgressService.GrantGold(400);AddFragments(4);return true;
        }
        public static bool KillQuestClaimed => PlayerPrefs.GetInt(Prefix+"kill_quest",0)!=0;
    }
}
