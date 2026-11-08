using UnityEngine;

namespace DienGame.Gameplay.Map
{
    public enum MapObstacleType
    {
        Rock,
        House,
        Tree,
        Other
    }

    public sealed class MapObstacle : MonoBehaviour
    {
        [Header("Obstacle")]

        [SerializeField]
        private MapObstacleType obstacleType =
            MapObstacleType.Other;

        [SerializeField]
        private bool blockPlayer = true;

        [SerializeField]
        private bool blockEnemy = true;

        [SerializeField]
        private bool blockProjectile = true;

        public MapObstacleType ObstacleType =>
            obstacleType;

        public bool BlockPlayer =>
            blockPlayer;

        public bool BlockEnemy =>
            blockEnemy;

        public bool BlockProjectile =>
            blockProjectile;

        public void Configure(
            MapObstacleType type,
            bool shouldBlockProjectile)
        {
            obstacleType = type;

            blockPlayer = true;
            blockEnemy = true;

            blockProjectile =
                shouldBlockProjectile;
        }
    }
}