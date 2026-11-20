using System.Collections.Generic;
using DienGame.Client;
using DienGame.Gameplay.Map;
using DienGame.Gameplay.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DienGame.Gameplay.World
{
    /// <summary>
    /// Builds real traversable 2D maps inside the existing gameplay scene selected for each class.
    /// Stage 1 is preserved. Stages 2–5 replace visual ground and objects, retaining game systems.
    /// All solid structures get blocking colliders and obstacle metadata.
    /// </summary>
    public sealed class WorldStageRuntime : MonoBehaviour
    {
        private WorldStageTheme _theme;
        private RectangularMapBoundary _boundary;
        private Transform _root;
        private readonly List<Collider2D> _placed = new List<Collider2D>();
        private int _obstacleLayer;

        public static WorldStageRuntime Current { get; private set; }
        public WorldStageTheme Theme => _theme;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            SceneManager.sceneLoaded -= Loaded;
            SceneManager.sceneLoaded += Loaded;
        }

        private static void Loaded(Scene scene, LoadSceneMode mode)
        {
            if (!LobbySessionService.IsGameplayScene(scene.name))
            {
                Current = null;
                return;
            }
            WorldStageRuntime existing = Object.FindFirstObjectByType<WorldStageRuntime>();
            if (existing != null) return;
            GameObject obj = new GameObject("WorldStage_Runtime");
            WorldStageRuntime world = obj.AddComponent<WorldStageRuntime>();
            world.Build(Mathf.Clamp(LobbySessionService.SelectedStage, 1, WorldStageThemes.TotalStages));
            obj.AddComponent<WorldStageLoot>();
            obj.AddComponent<WorldEquipmentStatApplier>();
            if (world._theme.Stage > 1)
            {
                Stage11ArenaEvent arena = obj.AddComponent<Stage11ArenaEvent>();
                arena.Configure(Stage11EncounterProfiles.Get(world._theme.Stage));
            }
            WorldStageHud.Show(world._theme);
        }

        private void Build(int stage)
        {
            Current = this;
            _theme = WorldStageThemes.Get(stage);
            _boundary = Object.FindFirstObjectByType<RectangularMapBoundary>();
            _root = new GameObject("WorldStage_DecorationAndObstacles").transform;
            _root.SetParent(transform, false);
            _obstacleLayer = LayerMask.NameToLayer("Obstacle");
            if (_boundary == null)
            {
                Debug.LogError("WorldStageRuntime: Thiếu RectangularMapBoundary, không thể dựng map.");
                return;
            }
            // The five approved paintings have their OWN authored collider data.
            // Never add random props over them: a painted wall without matching
            // collision (or a collider on empty floor) breaks gameplay.
            if (ApprovedArenaMapRuntime.TryLoad(stage, out Sprite painted,
                    out ApprovedArenaMapRuntime.StageCollisionData layout))
            {
                HideOriginalObstacles();
                HideOriginalGroundAndWallVisuals();
                int colliderCount = ApprovedArenaMapRuntime.Install(
                    painted, layout, _boundary, _root);
                if (stage == 5)
                    gameObject.AddComponent<ApprovedArenaStage05Guard>();
                Debug.Log($"DienGame Approved Map [{stage}] {_theme.Name}: " +
                    $"{colliderCount} solid collider objects (including boundary).");
                return;
            }

            // Fallback for projects where the new art hasn't been imported yet.
            // No existing gameplay is lost by a missing resource.
            if (stage == 1) return;
            HideOriginalObstacles();
            ReplaceGround();
            TintBoundaryWalls();
            PlaceProps();
            Debug.Log($"DienGame World: [{stage}] {_theme.Name}: {_placed.Count} legacy obstacles.");
        }


        private void HideOriginalGroundAndWallVisuals()
        {
            // The original MapGround is kept as a component, because other
            // gameplay scripts may look it up, but its tiled image is hidden.
            MapGround ground = Object.FindFirstObjectByType<MapGround>();
            if (ground != null)
            {
                SpriteRenderer renderer = ground.GetComponent<SpriteRenderer>();
                if (renderer != null) renderer.enabled = false;
            }

            // Keep any old physical world-boundary colliders, but do not
            // draw the old walls on top of the new illustration.
            SpriteRenderer[] sprites = Object.FindObjectsByType<SpriteRenderer>(
                FindObjectsSortMode.None);
            foreach (SpriteRenderer renderer in sprites)
            {
                if (renderer == null) continue;
                string objectName = renderer.gameObject.name;
                if (objectName.StartsWith("Wall_", System.StringComparison.Ordinal) ||
                    objectName.StartsWith("Corner_", System.StringComparison.Ordinal))
                    renderer.enabled = false;
            }
        }

        private void HideOriginalObstacles()
        {
            MapObstacle[] all = Object.FindObjectsByType<MapObstacle>(FindObjectsSortMode.None);
            foreach (MapObstacle obstacle in all)
            {
                if (obstacle == null) continue;
                // Keep generated hard world-boundaries; only replace the original interior props.
                if (obstacle.transform.IsChildOf(_boundary.transform)) continue;
                string n = obstacle.gameObject.name.ToLowerInvariant();
                if (n.Contains("wall") || n.Contains("boundary") || n.Contains("corner")) continue;
                obstacle.gameObject.SetActive(false);
            }
            // Raised ground blocks the old arena and its art conflicts with themed maps.
            GameObject raised = GameObject.Find("RaisedGround01");
            if (raised != null) raised.SetActive(false);
        }

        private void ReplaceGround()
        {
            Sprite ground = Resources.Load<Sprite>(_theme.Ground);
            MapGround map = Object.FindFirstObjectByType<MapGround>();
            if (map == null || ground == null)
            {
                Debug.LogError($"WorldStageRuntime: Cannot load stage ground {_theme.Ground}");
                return;
            }
            SpriteRenderer sr = map.GetComponent<SpriteRenderer>();
            sr.sprite = ground;
            sr.color = Color.white;
            map.Build();
        }

        private void TintBoundaryWalls()
        {
            SpriteRenderer[] all = Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
            foreach (SpriteRenderer sr in all)
            {
                if (sr == null || sr.GetComponent<MapGround>() != null) continue;
                string n = sr.gameObject.name;
                if (n.StartsWith("Wall_") || n.StartsWith("Corner_"))
                    sr.color = _theme.WallTint;
            }
        }

        private void PlaceProps()
        {
            Vector2 min = _boundary.Min + new Vector2(2.2f, 2.2f);
            Vector2 max = _boundary.Max - new Vector2(2.2f, 2.2f);
            if (min.x >= max.x || min.y >= max.y) return;
            System.Random rand = new System.Random(7319 + _theme.Stage * 1049);
            Sprite[] sprites = new Sprite[_theme.Props.Length];
            for (int i = 0; i < sprites.Length; i++)
                sprites[i] = Resources.Load<Sprite>("World/Props/" + _theme.Props[i]);

            Vector2 playerSpawn = Vector2.zero;
            PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>();
            if (player != null) playerSpawn = player.transform.position;

            for (int index = 0, attempts = 0; index < _theme.ObstacleCount && attempts < 400; attempts++)
            {
                int type = index % sprites.Length;
                Sprite sprite = sprites[type];
                if (sprite == null) continue;
                float size = type == 0 ? 2.8f : type == 1 ? 2.1f : 2.3f;
                float radius = type == 0 ? 1.2f : .93f;
                Vector2 p = new Vector2(Lerp(min.x,max.x,rand.NextDouble()),Lerp(min.y,max.y,rand.NextDouble()));
                // Reserve a spacious player/companion spawn zone and readable central paths.
                if ((p-playerSpawn).sqrMagnitude < 17f) continue;
                // Reserve continuous perpendicular traversal lanes to avoid boxing in the player.
                if (Mathf.Abs(p.x-playerSpawn.x) < 1.7f ||
                    Mathf.Abs(p.y-playerSpawn.y) < 1.45f) continue;
                bool overlaps = false;
                foreach (Collider2D old in _placed)
                {
                    if (old == null) continue;
                    if (((Vector2)old.transform.position-p).sqrMagnitude < 6.3f) { overlaps=true; break; }
                }
                if (overlaps) continue;
                CreateSolid(_theme.Props[type] + "_" + index, sprite, p, size, radius, type);
                index++;
            }
        }

        private static float Lerp(float a,float b,double t) => a+(b-a)*(float)t;

        private void CreateSolid(string name,Sprite sprite,Vector2 p,float height,float radius,int type)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(_root,false);
            obj.transform.position = new Vector3(p.x,p.y,0f);
            float nativeHeight = sprite.rect.height / sprite.pixelsPerUnit;
            float s = nativeHeight > 0f ? height/nativeHeight : 1f;
            obj.transform.localScale = Vector3.one * s;
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = Mathf.RoundToInt(-p.y * 10f) + 5;
            // Every protruding obstacle is solid; no walk-through decorative structures.
            CircleCollider2D collider = obj.AddComponent<CircleCollider2D>();
            collider.radius = radius / Mathf.Max(.01f,s);
            collider.isTrigger = false;
            if (_obstacleLayer >= 0) obj.layer = _obstacleLayer;
            MapObstacle obstacle = obj.AddComponent<MapObstacle>();
            obstacle.Configure(type == 0 ? MapObstacleType.Tree : MapObstacleType.Rock,true);
            _placed.Add(collider);
        }

        private void OnDestroy()
        {
            if (Current == this) Current = null;
        }
    }
}
