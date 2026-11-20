#if UNITY_EDITOR
using DienGame.Gameplay.Map;
using DienGame.Gameplay.Player;
using DienGame.Gameplay.World;
using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    public static class ApprovedArenaMapQA
    {
        [MenuItem("DienGame/Maps/Final/01 - Verify 5 maps and colliders")]
        private static void VerifyAssets()
        {
            bool okay = true;
            for (int stage = 1; stage <= 5; stage++)
            {
                if (!ApprovedArenaMapRuntime.TryLoad(stage, out Sprite image,
                        out ApprovedArenaMapRuntime.StageCollisionData info))
                {
                    Debug.LogError("ApprovedMap: missing art/collision data for stage " + stage);
                    okay = false;
                    continue;
                }
                if (image.texture.width < 900 || image.texture.height < 1600)
                {
                    Debug.LogError("ApprovedMap: unexpectedly small texture for stage " + stage);
                    okay = false;
                }
                foreach (ApprovedArenaMapRuntime.BlockedArea area in info.blockedAreas)
                {
                    if (area.points == null || area.points.Length < 3)
                    {
                        Debug.LogError("ApprovedMap stage " + stage +
                            ": bad collider path " + area.name);
                        okay = false;
                    }
                    else
                    {
                        foreach (Vector2 p in area.points)
                        {
                            if (p.x < 0f || p.x > 1f || p.y < 0f || p.y > 1f)
                            {
                                Debug.LogError("ApprovedMap: point outside image " + area.name);
                                okay = false;
                            }
                        }
                    }
                }
                Debug.Log("ApprovedMap " + stage + " (" + info.title + ") " +
                    image.texture.width + "x" + image.texture.height + "; " +
                    info.blockedAreas.Length + " solid shapes.");
            }
            if (okay) Debug.Log("ApprovedMap: all 5 maps and collision layouts verified.");
        }

        [MenuItem("DienGame/Maps/Final/02 - Inspect map physics (Play Mode)")]
        private static void InspectActive()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("ApprovedMap physics inspection requires Play Mode.");
                return;
            }
            var world = Object.FindFirstObjectByType<WorldStageRuntime>();
            var boundary = Object.FindFirstObjectByType<RectangularMapBoundary>();
            if (world == null || boundary == null)
            {
                Debug.LogError("ApprovedMap: game map runtime/boundary was not found.");
                return;
            }
            MapObstacle[] obstacles = Object.FindObjectsByType<MapObstacle>(
                FindObjectsSortMode.None);
            int solids = 0;
            foreach (MapObstacle obstacle in obstacles)
            {
                if (obstacle == null || !obstacle.gameObject.activeInHierarchy) continue;
                Collider2D collider = obstacle.GetComponent<Collider2D>();
                if (collider != null && collider.enabled && !collider.isTrigger) solids++;
            }
            PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>();
            bool spawnClear = player != null && Stage11WorldGeometry.IsFree(
                player.transform.position, .35f, boundary);
            Debug.Log("ApprovedMap stage " + world.Theme.Stage + ": " +
                solids + " active solid colliders. Player spawn is " +
                (spawnClear ? "CLEAR" : "BLOCKED/MISSING") + ".");
            if (solids < 10 || !spawnClear)
                Debug.LogError("ApprovedMap QA failed. Inspect game geometry before continuing.");
        }
    }
}
#endif
