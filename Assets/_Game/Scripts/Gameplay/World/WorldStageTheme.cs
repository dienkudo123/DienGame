using UnityEngine;

namespace DienGame.Gameplay.World
{
    /// <summary>Data-driven art + topology for 5 playable islands. All names are Vietnamese fantasy.</summary>
    public readonly struct WorldStageTheme
    {
        public readonly int Stage;
        public readonly string Name;
        public readonly string Ground;
        public readonly string[] Props;
        public readonly Color Accent;
        public readonly Color WallTint;
        public readonly int ObstacleCount;
        public readonly float BossScale;

        public WorldStageTheme(int stage, string name, string ground, string[] props,
            Color accent, Color wallTint, int obstacleCount, float bossScale)
        {
            Stage = stage; Name = name; Ground = ground; Props = props;
            Accent = accent; WallTint = wallTint;
            ObstacleCount = obstacleCount; BossScale = bossScale;
        }
    }

    public static class WorldStageThemes
    {
        public const int TotalStages = 5;

        public static WorldStageTheme Get(int stage)
        {
            switch (Mathf.Clamp(stage, 1, TotalStages))
            {
                case 2: return new WorldStageTheme(2, "RỪNG THIÊN MỘC", "World/Stages/Stage02_Ground",
                    new[] { "AncientTree", "ForestBoulder", "ForestRuins" },
                    new Color(.43f, .83f, .47f), new Color(.66f, .87f, .65f), 25, 1.15f);
                case 3: return new WorldStageTheme(3, "CỔ TỰ CÁT VÀNG", "World/Stages/Stage03_Ground",
                    new[] { "SacredObelisk", "DesertBoulder", "DesertRelic" },
                    new Color(1f, .79f, .37f), new Color(.98f, .79f, .53f), 23, 1.28f);
                case 4: return new WorldStageTheme(4, "HỎA THÀNH LONG MẠCH", "World/Stages/Stage04_Ground",
                    new[] { "LavaSpire", "VolcanicBoulder", "VolcanicCart" },
                    new Color(1f, .35f, .19f), new Color(.7f, .47f, .43f), 24, 1.40f);
                case 5: return new WorldStageTheme(5, "CUNG BĂNG VÂN THIÊN", "World/Stages/Stage05_Ground",
                    new[] { "IceCrystal", "FrozenBoulder", "FrozenRuins" },
                    new Color(.46f, .85f, 1f), new Color(.63f, .83f, 1f), 25, 1.58f);
                default: return new WorldStageTheme(1, "LÀNG PHÙ SA", string.Empty,
                    new string[0], new Color(1f, .78f, .30f), Color.white, 0, 1f);
            }
        }
    }
}
