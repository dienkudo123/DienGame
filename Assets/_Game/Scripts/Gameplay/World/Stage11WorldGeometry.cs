using DienGame.Gameplay.Map;
using UnityEngine;

namespace DienGame.Gameplay.World
{
    /// <summary>Non-alloc scene-editing is not needed; checks are called only on wave spawn.</summary>
    public static class Stage11WorldGeometry
    {
        public static bool IsFree(Vector2 point, float radius, RectangularMapBoundary boundary)
        {
            if (boundary == null) return false;
            Vector2 min = boundary.Min + Vector2.one * (radius + .45f);
            Vector2 max = boundary.Max - Vector2.one * (radius + .45f);
            if (point.x < min.x || point.x > max.x || point.y < min.y || point.y > max.y)
                return false;

            Collider2D[] touching = Physics2D.OverlapCircleAll(point, radius);
            foreach (Collider2D hit in touching)
            {
                if (hit == null || !hit.enabled || hit.isTrigger) continue;
                MapObstacle obstacle = hit.GetComponentInParent<MapObstacle>();
                if (obstacle != null && obstacle.BlockEnemy) return false;
                if (hit.transform.IsChildOf(boundary.transform)) return false;
            }
            return true;
        }

        /// <summary>Only solid terrain blocks a field attack; enemies/summons do not.</summary>
        public static bool TerrainOccludes(Vector2 from, Vector2 target)
        {
            Vector2 delta = target - from;
            float distance = delta.magnitude;
            if (distance <= .01f) return false;
            foreach (RaycastHit2D hit in Physics2D.RaycastAll(from, delta / distance, distance))
            {
                if (hit.collider == null || hit.collider.isTrigger) continue;
                MapObstacle obstacle = hit.collider.GetComponentInParent<MapObstacle>();
                if (obstacle != null && obstacle.BlockProjectile) return true;
            }
            return false;
        }
    }
}
