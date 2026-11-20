using UnityEngine;

namespace DienGame.Gameplay.Map
{
    [ExecuteAlways]
    public sealed class RectangularMapBoundary : MonoBehaviour
    {
        // =========================================
        // MAP
        // =========================================

        [Header("Map Size")]

        [SerializeField]
        private Vector2 mapSize =
            new Vector2(
                18f,
                32f
            );

        // =========================================
        // WALL VISUAL
        // =========================================

        [Header("Wall Visual")]

        [SerializeField, Min(0.1f)]
        private float wallSegmentLength =
            2f;

        [SerializeField, Min(0.1f)]
        private float wallVisualThickness =
            1.2f;

        [SerializeField, Min(0f)]
        private float wallOverlap =
            0.2f;

        [SerializeField, Min(0.1f)]
        private float cornerSize =
            1.5f;

        // =========================================
        // COLLISION
        // =========================================

        [Header("Collision")]

        [SerializeField, Min(0.01f)]
        private float collisionThickness =
            0.8f;

        [SerializeField]
        private string boundaryLayerName =
            "Obstacle";

        // =========================================
        // RENDERING
        // =========================================

        [Header("Rendering")]

        [SerializeField]
        private string sortingLayerName =
            "Default";

        [SerializeField]
        private int sortingOrder =
            0;

        // =========================================
        // PUBLIC MAP DATA
        // =========================================

        public Vector2 MapSize =>
            mapSize;

        public Vector2 WorldSize =>
            mapSize;

        public Vector2 Center =>
            transform.position;

        public Vector2 Min =>
            Center -
            WorldSize * 0.5f;

        public Vector2 Max =>
            Center +
            WorldSize * 0.5f;

        public Bounds WorldBounds =>
            new Bounds(
                transform.position,
                new Vector3(
                    mapSize.x,
                    mapSize.y,
                    0.1f
                )
            );

        // =========================================
        // PUBLIC WALL DATA
        // =========================================

        public float WallSegmentLength =>
            wallSegmentLength;

        public float WallVisualThickness =>
            wallVisualThickness;

        public float WallOverlap =>
            wallOverlap;

        public float CornerSize =>
            cornerSize;

        // =========================================
        // PUBLIC COLLISION DATA
        // =========================================

        public float CollisionThickness =>
            collisionThickness;

        public string BoundaryLayerName =>
            boundaryLayerName;

        // =========================================
        // PUBLIC RENDERING DATA
        // =========================================

        public string SortingLayerName =>
            sortingLayerName;

        public int SortingOrder =>
            sortingOrder;

        // =========================================
        // EDITOR GIZMO
        // =========================================

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(
                transform.position,
                new Vector3(
                    mapSize.x,
                    mapSize.y,
                    0f
                )
            );
        }

#endif
    }
}