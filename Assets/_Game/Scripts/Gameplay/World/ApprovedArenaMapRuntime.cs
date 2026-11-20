using System;
using DienGame.Gameplay.Map;
using DienGame.Gameplay.Player;
using UnityEngine;

namespace DienGame.Gameplay.World
{
    /// <summary>
    /// Builds the five approved illustrated arenas AND their authored solid terrain.
    /// Coordinates in StageXX_Collision.json are normalized from the artwork's top-left.
    /// The static PolygonCollider2D shapes are authoritative; painted images alone
    /// must never be treated as passable geometry.
    /// </summary>
    public static class ApprovedArenaMapRuntime
    {
        private const string ResourceRoot = "World/ApprovedMaps/Stage";

        [Serializable]
        public sealed class BlockedArea
        {
            public string name;
            public Vector2[] points;
        }

        [Serializable]
        public sealed class StageCollisionData
        {
            public int stage;
            public string title;
            public BlockedArea[] blockedAreas;
        }

        public static bool TryLoad(int stage, out Sprite sprite, out StageCollisionData layout)
        {
            string prefix = ResourceRoot + stage.ToString("00");
            sprite = Resources.Load<Sprite>(prefix + "_Approved");
            TextAsset text = Resources.Load<TextAsset>(prefix + "_Collision");
            layout = text != null ? JsonUtility.FromJson<StageCollisionData>(text.text) : null;
            return sprite != null && layout != null && layout.stage == stage &&
                   layout.blockedAreas != null && layout.blockedAreas.Length > 0;
        }

        /// <summary>
        /// Call after the old scene's interior props are disabled and before enemies spawn.
        /// Rigidbody2D + solid collider physics handles player, enemies, bosses and pet.
        /// </summary>
        public static int Install(Sprite sprite, StageCollisionData data,
            RectangularMapBoundary boundary, Transform parent)
        {
            if (sprite == null || data == null || boundary == null || parent == null)
                return 0;

            Transform root = new GameObject("ApprovedMap_Stage" + data.stage.ToString("00"))
                .transform;
            root.SetParent(parent, false);
            root.position = boundary.transform.position;

            // Do not tile the artwork. Preserve the exact rectangular arena size.
            GameObject imageObject = new GameObject("IllustratedMap_Background");
            imageObject.transform.SetParent(root, false);
            SpriteRenderer renderer = imageObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = Color.white;
            renderer.sortingOrder = -500;
            renderer.drawMode = SpriteDrawMode.Simple;
            Vector2 worldSize = boundary.WorldSize;
            imageObject.transform.localScale = new Vector3(
                worldSize.x / sprite.bounds.size.x,
                worldSize.y / sprite.bounds.size.y, 1f);

            int placed = 0;
            foreach (BlockedArea area in data.blockedAreas)
            {
                if (area == null || area.points == null || area.points.Length < 3)
                    continue;

                GameObject obstacleObject = new GameObject("SOLID_" + area.name);
                obstacleObject.transform.SetParent(root, false);
                // Keep terrain on Default layer, whose collisions are enabled
                // in the standard Unity physics matrix regardless of optional
                // custom Obstacle layer settings.
                obstacleObject.layer = 0;
                PolygonCollider2D collider = obstacleObject.AddComponent<PolygonCollider2D>();
                collider.isTrigger = false;
                Vector2[] vertices = new Vector2[area.points.Length];
                for (int index = 0; index < vertices.Length; index++)
                {
                    Vector2 p = area.points[index];
                    vertices[index] = new Vector2(
                        (p.x - .5f) * worldSize.x,
                        (.5f - p.y) * worldSize.y);
                }
                collider.pathCount = 1;
                collider.SetPath(0, vertices);
                obstacleObject.AddComponent<MapObstacle>()
                    .Configure(MapObstacleType.Other, true);
                placed++;
            }

            // An independent closed physical perimeter: never rely on decorative
            // old wall sprites, so the player can't escape the rectangle.
            GameObject edgeObject = new GameObject("SOLID_RectangularWorldPerimeter");
            edgeObject.transform.SetParent(root, false);
            EdgeCollider2D edge = edgeObject.AddComponent<EdgeCollider2D>();
            edge.isTrigger = false;
            edge.edgeRadius = .12f;
            float x = worldSize.x * .5f;
            float y = worldSize.y * .5f;
            edge.points = new[]
            {
                new Vector2(-x, -y), new Vector2(x, -y),
                new Vector2(x, y), new Vector2(-x, y), new Vector2(-x, -y)
            };
            edgeObject.AddComponent<MapObstacle>()
                .Configure(MapObstacleType.Other, true);

            // Make sure physics queries (wave spawns/boss) see the fresh colliders
            // in the same scene-load frame.
            Physics2D.SyncTransforms();
            return placed + 1;
        }
    }
}
