using UnityEngine;

namespace DienGame.Client
{
    public readonly struct LobbyStageDefinition
    {
        public LobbyStageDefinition(
            int id,
            string name,
            string subtitle,
            int difficulty,
            int rewardGold,
            int rewardGem,
            int rewardChest,
            Vector2 mapPosition,
            Vector2 islandSize)
        {
            Id = id;
            Name = name;
            Subtitle = subtitle;
            Difficulty = difficulty;
            RewardGold = rewardGold;
            RewardGem = rewardGem;
            RewardChest = rewardChest;
            MapPosition = mapPosition;
            IslandSize = islandSize;
        }

        public int Id { get; }
        public string Name { get; }
        public string Subtitle { get; }
        public int Difficulty { get; }
        public int RewardGold { get; }
        public int RewardGem { get; }
        public int RewardChest { get; }
        public Vector2 MapPosition { get; }
        public Vector2 IslandSize { get; }
    }

    public static class LobbyStageCatalog
    {
        private static readonly LobbyStageDefinition[] s_stages =
        {
            new(
                1,
                "ĐẢO 1 - LÀNG PHÙ SA",
                "Bến sông và làng Việt nơi bắt đầu hành trình.",
                1,
                500,
                20,
                1,
                new Vector2(235f, 385f),
                new Vector2(390f, 270f)),

            new(
                2,
                "ĐẢO 2 - RỪNG THIÊN MỘC",
                "Đại thụ linh mộc, đình đền và lối mòn cổ.",
                2,
                700,
                30,
                1,
                new Vector2(245f, 770f),
                new Vector2(340f, 250f)),

            new(
                3,
                "ĐẢO 3 - CỔ TỰ CÁT VÀNG",
                "Phế tích đá vàng dưới nắng cát và tượng cổ.",
                3,
                900,
                40,
                1,
                new Vector2(575f, 1040f),
                new Vector2(350f, 250f)),

            new(
                4,
                "ĐẢO 4 - HỎA THÀNH LONG MẠCH",
                "Thành đá trên miệng núi lửa với dòng nham nóng.",
                4,
                1200,
                55,
                1,
                new Vector2(835f, 1060f),
                new Vector2(340f, 250f)),

            new(
                5,
                "ĐẢO 5 - CUNG BĂNG VÂN THIÊN",
                "Cung điện giữa núi tuyết và tinh thể băng.",
                5,
                1800,
                80,
                2,
                new Vector2(825f, 675f),
                new Vector2(390f, 285f)),
        };

        public static int Count => s_stages.Length;

        public static LobbyStageDefinition Get(int stage)
        {
            int index = Mathf.Clamp(stage - 1, 0, s_stages.Length - 1);
            return s_stages[index];
        }
    }
}
