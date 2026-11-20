using System;
using System.Collections.Generic;
using UnityEngine;

namespace DienGame.Client
{
    /// <summary>
    /// Runtime art lookup cho Client/Lobby.
    /// Không cần kéo reference trong Inspector.
    /// Chỉ cần đặt sprite vào Assets/_Game/Resources/Client/ và đặt tên đúng convention.
    /// </summary>
    public static class ClientArtProvider
    {
        private const string ResourcesRoot = "Client";

        private static Dictionary<string, Sprite> s_spriteByName;

        public static Sprite Background => FindSprite(
            "BG_Client",
            "Client_BG",
            "Lobby_BG",
            "WorldMap_BG");

        public static Sprite GetIsland(int stage)
        {
            return stage switch
            {
                1 => FindSprite("Island_01", "Island_1", "Dao_01", "Dao1", "DongBang"),
                2 => FindSprite("Island_02", "Island_2", "Dao_02", "Dao2", "RungTham"),
                3 => FindSprite("Island_03", "Island_3", "Dao_03", "Dao3", "TanTichCo"),
                4 => FindSprite("Island_04", "Island_4", "Dao_04", "Dao4", "NuiLua"),
                5 => FindSprite("Island_05", "Island_5", "Dao_05", "Dao5", "CuDiemCuoi"),
                _ => null,
            };
        }

        public static Sprite GetMenuIcon(string key)
        {
            return key switch
            {
                "map" => FindSprite("Icon_Map", "Menu_Map", "Map"),
                "equipment" => FindSprite("Icon_Equipment", "Menu_Equipment", "Equipment"),
                "skills" => FindSprite("Icon_Skills", "Menu_Skills", "Skills"),
                "missions" => FindSprite("Icon_Missions", "Menu_Missions", "Missions"),
                "shop" => FindSprite("Icon_Shop", "Menu_Shop", "Shop"),
                "daily" => FindSprite("Icon_Daily", "Function_Daily", "Daily"),
                "event" => FindSprite("Icon_Event", "Function_Event", "Event"),
                "gift" => FindSprite("Icon_Gift", "Function_Gift", "Gift"),
                _ => null,
            };
        }

        public static Sprite StarFull => FindSprite("Star_Full", "StarFull", "Star_Gold");
        public static Sprite StarEmpty => FindSprite("Star_Empty", "StarEmpty", "Star_Gray");
        public static Sprite Lock => FindSprite("Icon_Lock", "Lock", "Stage_Lock");
        public static Sprite Gold => FindSprite("Reward_Gold", "Icon_Gold", "Gold");
        public static Sprite Gem => FindSprite("Reward_Gem", "Icon_Gem", "Gem");
        public static Sprite Chest => FindSprite("Reward_Chest", "Icon_Chest", "Chest");
        public static Sprite Energy => FindSprite("Icon_Energy", "Energy");

        public static void ClearCache()
        {
            s_spriteByName = null;
        }

        private static Sprite FindSprite(params string[] aliases)
        {
            EnsureCache();

            for (int i = 0; i < aliases.Length; i++)
            {
                string alias = aliases[i];
                if (string.IsNullOrWhiteSpace(alias))
                {
                    continue;
                }

                if (s_spriteByName.TryGetValue(alias, out Sprite exact))
                {
                    return exact;
                }

                foreach (KeyValuePair<string, Sprite> pair in s_spriteByName)
                {
                    if (pair.Key.Equals(alias, StringComparison.OrdinalIgnoreCase))
                    {
                        return pair.Value;
                    }
                }
            }

            return null;
        }

        private static void EnsureCache()
        {
            if (s_spriteByName != null)
            {
                return;
            }

            s_spriteByName = new Dictionary<string, Sprite>(StringComparer.Ordinal);

            Sprite[] sprites = Resources.LoadAll<Sprite>(ResourcesRoot);
            for (int i = 0; i < sprites.Length; i++)
            {
                Sprite sprite = sprites[i];
                if (sprite == null || string.IsNullOrWhiteSpace(sprite.name))
                {
                    continue;
                }

                if (!s_spriteByName.ContainsKey(sprite.name))
                {
                    s_spriteByName.Add(sprite.name, sprite);
                }
            }
        }
    }
}
