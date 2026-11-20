using System.Collections;
using System.Collections.Generic;
using DienGame.Client;
using DienGame.Gameplay.Map;
using DienGame.Gameplay.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DienGame.Gameplay.World
{
    /// <summary>
    /// Stage-5 visual + collision upgrade. Designed to work alongside the existing
    /// WorldStageRuntime, without replacing or duplicating any existing gameplay class.
    /// No Inspector steps required. Stage 1-4, classes and combat skills stay unchanged.
    /// </summary>
    public sealed class IceArenaPremiumRuntime : MonoBehaviour
    {
        private const string ResourcesRoot = "World/IcePremium/";
        private Transform _decorations;
        private RectangularMapBoundary _boundary;
        private int _obstacleLayer;
        private bool _installed;
        private SpriteRenderer _terrainRenderer;
        private readonly List<Collider2D> _solidGeometry = new List<Collider2D>(48);
        private int _playerLayer = -1;
        private bool _changedLayerCollision;
        private CollisionDetectionMode2D _originalCollisionDetection;
        private Rigidbody2D _playerBody;

        // Pixel positions are measured against IceArenaBackground.png (941 x 1672),
        // origin at top-left. These polygons trace painted cliffs, ruins and crystals.
        // No object can be walked through merely because it is painted onto the ground.
        private static readonly Dictionary<string, Vector2[]> PaintedShapes =
            new Dictionary<string, Vector2[]>
            {
                { "NorthWestFrozenKeep", new[] { new Vector2(0f,0f), new Vector2(445f,0f), new Vector2(438f,70f), new Vector2(418f,104f), new Vector2(388f,119f), new Vector2(363f,143f), new Vector2(342f,147f), new Vector2(322f,173f), new Vector2(288f,185f), new Vector2(257f,192f), new Vector2(228f,211f), new Vector2(206f,226f), new Vector2(174f,249f), new Vector2(153f,287f), new Vector2(123f,292f), new Vector2(67f,296f), new Vector2(0f,306f) } },
                { "WesternRaisedIceCliff", new[] { new Vector2(0f,300f), new Vector2(63f,301f), new Vector2(90f,321f), new Vector2(124f,337f), new Vector2(159f,353f), new Vector2(180f,394f), new Vector2(185f,423f), new Vector2(211f,457f), new Vector2(237f,473f), new Vector2(280f,496f), new Vector2(306f,520f), new Vector2(317f,547f), new Vector2(301f,569f), new Vector2(266f,597f), new Vector2(230f,611f), new Vector2(190f,633f), new Vector2(153f,660f), new Vector2(116f,681f), new Vector2(0f,700f) } },
                { "NorthEastFrozenKeep", new[] { new Vector2(770f,0f), new Vector2(941f,0f), new Vector2(941f,403f), new Vector2(897f,400f), new Vector2(867f,368f), new Vector2(839f,349f), new Vector2(800f,338f), new Vector2(766f,324f), new Vector2(752f,291f), new Vector2(770f,269f), new Vector2(784f,236f), new Vector2(805f,213f), new Vector2(819f,185f), new Vector2(817f,145f), new Vector2(795f,119f) } },
                { "EasternCrystalSpire", new[] { new Vector2(815f,483f), new Vector2(851f,496f), new Vector2(871f,537f), new Vector2(883f,574f), new Vector2(906f,608f), new Vector2(933f,680f), new Vector2(941f,700f), new Vector2(941f,826f), new Vector2(902f,821f), new Vector2(863f,808f), new Vector2(821f,811f), new Vector2(781f,802f), new Vector2(733f,802f), new Vector2(687f,794f), new Vector2(646f,768f), new Vector2(635f,738f), new Vector2(649f,707f), new Vector2(680f,679f), new Vector2(700f,645f), new Vector2(724f,612f), new Vector2(761f,580f), new Vector2(780f,550f) } },
                { "WesternLowerIceShelf", new[] { new Vector2(0f,860f), new Vector2(56f,859f), new Vector2(89f,896f), new Vector2(113f,930f), new Vector2(124f,964f), new Vector2(122f,1002f), new Vector2(97f,1039f), new Vector2(0f,1056f) } },
                { "SouthWestRuinedRampart", new[] { new Vector2(0f,1125f), new Vector2(55f,1117f), new Vector2(113f,1107f), new Vector2(170f,1115f), new Vector2(207f,1165f), new Vector2(248f,1203f), new Vector2(284f,1217f), new Vector2(328f,1241f), new Vector2(366f,1256f), new Vector2(422f,1295f), new Vector2(464f,1307f), new Vector2(502f,1343f), new Vector2(523f,1394f), new Vector2(514f,1430f), new Vector2(476f,1458f), new Vector2(434f,1451f), new Vector2(402f,1432f), new Vector2(359f,1410f), new Vector2(317f,1415f), new Vector2(277f,1404f), new Vector2(229f,1404f), new Vector2(190f,1441f), new Vector2(131f,1442f), new Vector2(85f,1428f), new Vector2(0f,1423f) } },
                { "SouthEastRuinedRampart", new[] { new Vector2(632f,1179f), new Vector2(666f,1171f), new Vector2(696f,1198f), new Vector2(718f,1242f), new Vector2(728f,1285f), new Vector2(760f,1319f), new Vector2(795f,1331f), new Vector2(820f,1370f), new Vector2(836f,1406f), new Vector2(803f,1444f), new Vector2(775f,1463f), new Vector2(714f,1466f), new Vector2(680f,1446f), new Vector2(625f,1459f), new Vector2(587f,1432f), new Vector2(577f,1388f), new Vector2(594f,1347f), new Vector2(612f,1298f), new Vector2(613f,1237f) } },
                { "SouthWestFrozenCliffEdge", new[] { new Vector2(0f,1462f), new Vector2(64f,1477f), new Vector2(121f,1490f), new Vector2(166f,1535f), new Vector2(196f,1578f), new Vector2(206f,1620f), new Vector2(181f,1672f), new Vector2(0f,1672f) } },
                { "SouthEastFrozenCliffEdge", new[] { new Vector2(941f,1103f), new Vector2(941f,1672f), new Vector2(736f,1672f), new Vector2(751f,1635f), new Vector2(786f,1587f), new Vector2(807f,1558f), new Vector2(831f,1512f), new Vector2(875f,1487f), new Vector2(905f,1440f), new Vector2(915f,1350f), new Vector2(922f,1250f), new Vector2(925f,1130f) } },
            };

        // Convex alpha silhouettes baked from the actual 19 art PNGs. This makes
        // colliders follow the VISIBLE rock/crystal/wall, not just a tiny box at its foot.
        // Layout is in sprite pixel space, measured from each PNG's top-left corner.
        private static readonly Dictionary<string, Vector2[]> PropSilhouettes =
            new Dictionary<string, Vector2[]>
            {
                { "IceBoulderGrand", new[] { new Vector2(377f,144f), new Vector2(377f,172f), new Vector2(322f,213f), new Vector2(219f,240f), new Vector2(7f,188f), new Vector2(50f,93f), new Vector2(204f,9f), new Vector2(264f,14f) } },
                { "IceBoulderRidge", new[] { new Vector2(202f,61f), new Vector2(214f,113f), new Vector2(162f,158f), new Vector2(100f,170f), new Vector2(8f,141f), new Vector2(19f,78f), new Vector2(45f,32f), new Vector2(78f,11f), new Vector2(111f,7f) } },
                { "IceBoulderSmall", new[] { new Vector2(181f,85f), new Vector2(142f,121f), new Vector2(76f,120f), new Vector2(7f,92f), new Vector2(25f,48f), new Vector2(82f,7f), new Vector2(110f,15f) } },
                { "IceBoulderTall", new[] { new Vector2(137f,10f), new Vector2(202f,112f), new Vector2(191f,157f), new Vector2(93f,201f), new Vector2(20f,192f), new Vector2(8f,180f), new Vector2(80f,32f) } },
                { "IceCrystalGrand", new[] { new Vector2(271f,146f), new Vector2(153f,183f), new Vector2(7f,148f), new Vector2(101f,7f), new Vector2(209f,32f) } },
                { "IceCrystalMid", new[] { new Vector2(191f,120f), new Vector2(117f,151f), new Vector2(7f,134f), new Vector2(35f,62f), new Vector2(122f,7f) } },
                { "IceCrystalObelisk", new[] { new Vector2(7f,219f), new Vector2(21f,80f), new Vector2(51f,7f), new Vector2(97f,54f), new Vector2(109f,215f) } },
                { "IceCrystalSmall", new[] { new Vector2(143f,107f), new Vector2(115f,124f), new Vector2(71f,129f), new Vector2(9f,121f), new Vector2(7f,103f), new Vector2(83f,8f), new Vector2(137f,66f) } },
                { "IceFlagstones", new[] { new Vector2(129f,43f), new Vector2(85f,100f), new Vector2(46f,109f), new Vector2(11f,87f), new Vector2(8f,42f), new Vector2(69f,7f) } },
                { "IceGateRuined", new[] { new Vector2(267f,130f), new Vector2(205f,153f), new Vector2(7f,127f), new Vector2(86f,23f), new Vector2(138f,7f), new Vector2(251f,93f) } },
                { "IcePillarRound", new[] { new Vector2(48f,158f), new Vector2(7f,141f), new Vector2(18f,23f), new Vector2(45f,7f), new Vector2(77f,22f), new Vector2(87f,138f) } },
                { "IcePillarRunic", new[] { new Vector2(182f,136f), new Vector2(8f,139f), new Vector2(22f,46f), new Vector2(87f,8f), new Vector2(122f,17f), new Vector2(164f,45f) } },
                { "IcePillarTall", new[] { new Vector2(26f,186f), new Vector2(7f,174f), new Vector2(31f,47f), new Vector2(72f,7f), new Vector2(110f,45f), new Vector2(127f,168f), new Vector2(83f,186f) } },
                { "IceShrine", new[] { new Vector2(209f,158f), new Vector2(128f,180f), new Vector2(7f,164f), new Vector2(49f,59f), new Vector2(102f,7f), new Vector2(160f,61f) } },
                { "IceSpikes", new[] { new Vector2(225f,103f), new Vector2(145f,134f), new Vector2(26f,120f), new Vector2(7f,105f), new Vector2(35f,38f), new Vector2(95f,7f), new Vector2(191f,8f) } },
                { "IceSpikesShort", new[] { new Vector2(199f,80f), new Vector2(193f,93f), new Vector2(137f,108f), new Vector2(7f,96f), new Vector2(55f,18f), new Vector2(167f,7f) } },
                { "IceWallBroken", new[] { new Vector2(257f,129f), new Vector2(208f,150f), new Vector2(58f,141f), new Vector2(8f,119f), new Vector2(25f,41f), new Vector2(62f,8f), new Vector2(224f,74f), new Vector2(255f,103f) } },
                { "IceWallGrand", new[] { new Vector2(453f,136f), new Vector2(368f,173f), new Vector2(7f,153f), new Vector2(28f,46f), new Vector2(65f,7f), new Vector2(376f,7f) } },
                { "IceWallShort", new[] { new Vector2(170f,46f), new Vector2(175f,67f), new Vector2(152f,89f), new Vector2(43f,110f), new Vector2(7f,102f), new Vector2(15f,38f), new Vector2(39f,16f), new Vector2(80f,7f), new Vector2(140f,18f) } },
            };

        private struct Placement
        {
            public string SpriteName;
            public Vector2 Location;
            public float Height;
            public float BaseWidth;
            public float BaseDepth;
            public bool Solid;

            public Placement(string spriteName, float x, float y,
                float height, float baseWidth, float baseDepth, bool solid = true)
            {
                SpriteName = spriteName;
                Location = new Vector2(x, y);
                Height = height;
                BaseWidth = baseWidth;
                BaseDepth = baseDepth;
                Solid = solid;
            }
        }

        // Normalized positions in [-1,1]. Chosen to keep generous combat lanes.
        // Artwork visible at the boundary gets matching physical collision in BuildPaintedColliders.
        private static readonly Placement[] Layout =
        {
            new Placement("IceCrystalGrand",      .65f,  .31f, 2.70f, 1.42f, .78f),
            new Placement("IceBoulderGrand",     -.76f,  .18f, 2.75f, 1.65f, .82f),
            new Placement("IcePillarTall",        .72f,  .79f, 2.50f, .72f, .72f),
            new Placement("IceCrystalMid",       -.70f,  .78f, 2.35f, 1.15f, .62f),
            new Placement("IcePillarRound",      -.73f, -.66f, 2.05f, .76f, .62f),
            new Placement("IceWallGrand",        -.48f, -.74f, 2.30f, 2.72f, .64f),
            new Placement("IceWallBroken",        .42f, -.75f, 2.03f, 2.35f, .64f),
            new Placement("IceBoulderTall",       .74f, -.24f, 2.45f, 1.25f, .70f),
            new Placement("IceBoulderRidge",     -.63f, -.24f, 2.30f, 1.40f, .68f),
            new Placement("IceGateRuined",       -.09f,  .84f, 2.42f, 1.56f, .74f),
            new Placement("IceCrystalSmall",      .40f,  .69f, 1.62f, .90f, .49f),
            new Placement("IceShrine",           -.80f,  .56f, 2.30f, 1.25f, .71f),
            new Placement("IceSpikes",            .81f, -.57f, 1.41f, 1.45f, .46f),
            new Placement("IceWallShort",         .76f, -.83f, 1.38f, 1.66f, .46f),
            new Placement("IceBoulderSmall",     -.77f, -.89f, 1.32f, .90f, .48f),
            new Placement("IceSpikesShort",      -.31f,  .66f, 1.11f, 1.25f, .37f),
            new Placement("IceFlagstones",        .14f, -.46f, 1.20f, 0f, 0f, false),
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!LobbySessionService.IsGameplayScene(scene.name)) return;
            if (LobbySessionService.SelectedStage != 5) return;
            if (Object.FindFirstObjectByType<IceArenaPremiumRuntime>() != null) return;
            new GameObject("DienGame_IceArenaPremium_Stage05")
                .AddComponent<IceArenaPremiumRuntime>();
        }

        private IEnumerator Start()
        {
            // Other sceneLoaded subscribers may construct WorldStageRuntime after this one.
            int tries = 0;
            while ((WorldStageRuntime.Current == null ||
                    WorldStageRuntime.Current.Theme.Stage != 5) && tries++ < 90)
                yield return null;

            if (WorldStageRuntime.Current == null ||
                WorldStageRuntime.Current.Theme.Stage != 5)
            {
                Debug.LogWarning("IceArenaPremium: stage 5 runtime wasn't initialized.");
                Destroy(gameObject);
                yield break;
            }

            // Wait for the original ground and procedural props to finish building.
            yield return null;
            Install();
        }

        private static Sprite LoadSprite(string path)
        {
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite != null) return sprite;
            Sprite[] multiple = Resources.LoadAll<Sprite>(path);
            return multiple != null && multiple.Length > 0 ? multiple[0] : null;
        }

        private void Install()
        {
            if (_installed) return;
            _installed = true;

            _boundary = Object.FindFirstObjectByType<RectangularMapBoundary>();
            MapGround ground = Object.FindFirstObjectByType<MapGround>();
            if (_boundary == null || ground == null)
            {
                Debug.LogError("IceArenaPremium: MapGround or RectangularMapBoundary missing.");
                return;
            }

            Sprite background = LoadSprite(ResourcesRoot + "IceArenaBackground");
            if (background == null)
            {
                Debug.LogError("IceArenaPremium: missing IceArenaBackground sprite. Check the PNG import mode.");
                return;
            }

            // The art is ONE portrait composition. Using SpriteDrawMode.Tiled would repeat it,
            // produce seams, and visibly destroy the artwork; use Simple instead.
            ground.enabled = false;
            SpriteRenderer renderer = ground.GetComponent<SpriteRenderer>();
            renderer.sprite = background;
            renderer.color = Color.white;
            renderer.drawMode = SpriteDrawMode.Simple;
            renderer.sortingOrder = -80;
            _terrainRenderer = renderer;

            Vector2 world = _boundary.WorldSize;
            float targetHeight = world.y + 3.0f;
            float imageAspect = (float)background.rect.width / background.rect.height;
            float targetWidth = targetHeight * imageAspect;
            if (targetWidth < world.x + .6f)
            {
                targetWidth = world.x + .6f;
                targetHeight = targetWidth / imageAspect;
            }

            float nativeW = background.rect.width / background.pixelsPerUnit;
            float nativeH = background.rect.height / background.pixelsPerUnit;
            ground.transform.position = new Vector3(_boundary.Center.x, _boundary.Center.y,
                ground.transform.position.z);
            ground.transform.localScale = new Vector3(targetWidth / nativeW, targetHeight / nativeH, 1f);

            // Keep the existing invisible collision walls; hide only obsolete wall art.
            SpriteRenderer[] all = Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
            foreach (SpriteRenderer visual in all)
            {
                if (visual == null || visual == renderer) continue;
                string name = visual.gameObject.name;
                if (name.StartsWith("Wall_") || name.StartsWith("Corner_"))
                    visual.enabled = false;
            }

            // Hide ALL old stage-5 random props and their colliders, not just their sprites.
            Transform legacy = WorldStageRuntime.Current.transform
                .Find("WorldStage_DecorationAndObstacles");
            if (legacy != null) legacy.gameObject.SetActive(false);

            _obstacleLayer = LayerMask.NameToLayer("Obstacle");
            _decorations = new GameObject("IceArena_Premium_VisualsAndCollision").transform;
            _decorations.SetParent(transform, false);
            BuildProps();
            BuildPaintedColliders();
            EnsurePlayerCollidesAndStartsOnWalkableGround();
            Debug.Log("DienGame: Ice Arena collision V3 installed: " +
                _solidGeometry.Count + " solid shapes following the real artwork; " +
                "stage 1-4 unchanged.");
        }

        private void BuildProps()
        {
            Vector2 min = _boundary.Min;
            Vector2 max = _boundary.Max;
            PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>();
            Vector2 spawn = player != null ? (Vector2)player.transform.position : _boundary.Center;

            foreach (Placement p in Layout)
            {
                Sprite sprite = LoadSprite(ResourcesRoot + "Props/" + p.SpriteName);
                if (sprite == null)
                {
                    Debug.LogWarning("IceArenaPremium: missing sprite " + p.SpriteName);
                    continue;
                }

                // Allow a small safe margin from the hard arena bounds.
                Vector2 center = _boundary.Center;
                Vector2 pos = center + new Vector2(p.Location.x * (_boundary.WorldSize.x * .46f),
                    p.Location.y * (_boundary.WorldSize.y * .46f));
                if (p.Solid && (pos - spawn).sqrMagnitude < 13f) continue;
                CreateProp(p, sprite, pos);
            }
        }

        private void CreateProp(Placement def, Sprite sprite, Vector2 location)
        {
            GameObject root = new GameObject(def.SpriteName + "_IceArena");
            root.transform.SetParent(_decorations, false);
            root.transform.position = new Vector3(location.x, location.y, 0f);
            if (_obstacleLayer >= 0) root.layer = _obstacleLayer;

            GameObject art = new GameObject("Sprite");
            art.transform.SetParent(root.transform, false);
            if (_obstacleLayer >= 0) art.layer = _obstacleLayer;
            SpriteRenderer sr = art.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = Color.white;
            sr.sortingOrder = Mathf.RoundToInt(-location.y * 10f) + 12;

            float nativeHeight = sprite.rect.height / sprite.pixelsPerUnit;
            float scale = def.Height / Mathf.Max(.01f, nativeHeight);
            art.transform.localScale = Vector3.one * scale;
            art.transform.localPosition = new Vector3(0f, def.Height * .46f, 0f);
            if (!def.Solid) return;

            // The collider is on the SAME child and transform as the visible sprite.
            // Unlike a foot-only BoxCollider, this blocks the entire painted structure.
            PolygonCollider2D polygon = art.AddComponent<PolygonCollider2D>();
            polygon.isTrigger = false;
            polygon.pathCount = 1;
            Vector2[] pixels;
            if (!PropSilhouettes.TryGetValue(def.SpriteName, out pixels))
            {
                float w = sprite.rect.width, h = sprite.rect.height;
                pixels = new[] {new Vector2(0, 0), new Vector2(w, 0),
                    new Vector2(w, h), new Vector2(0, h)};
            }
            Vector2[] shape = new Vector2[pixels.Length];
            float ppu = Mathf.Max(.01f, sprite.pixelsPerUnit);
            for (int i = 0; i < pixels.Length; i++)
            {
                // Sprite physics coordinates have their origin at the Sprite pivot.
                shape[i] = new Vector2((pixels[i].x - sprite.pivot.x) / ppu,
                    (sprite.rect.height - pixels[i].y - sprite.pivot.y) / ppu);
            }
            polygon.SetPath(0, shape);
            _solidGeometry.Add(polygon);
            MapObstacle mapObstacle = art.AddComponent<MapObstacle>();
            mapObstacle.Configure(MapObstacleType.Rock, true);
        }

        private void BuildPaintedColliders()
        {
            if (_terrainRenderer == null || _terrainRenderer.sprite == null) return;
            // Create one closed physics polygon per separate painted obstacle.
            // Shapes are tied to the background's actual transform, not map-size guesses.
            foreach (KeyValuePair<string, Vector2[]> entry in PaintedShapes)
            {
                GameObject obj = new GameObject("SolidPainted_" + entry.Key);
                obj.transform.SetParent(_decorations, false);
                obj.transform.position = _terrainRenderer.transform.position;
                if (_obstacleLayer >= 0) obj.layer = _obstacleLayer;
                PolygonCollider2D polygon = obj.AddComponent<PolygonCollider2D>();
                polygon.isTrigger = false;
                polygon.pathCount = 1;
                Vector2[] path = new Vector2[entry.Value.Length];
                for (int i = 0; i < path.Length; i++)
                    path[i] = BackgroundPixelToColliderLocal(entry.Value[i], obj.transform);
                polygon.SetPath(0, path);
                _solidGeometry.Add(polygon);
                MapObstacle obstacle = obj.AddComponent<MapObstacle>();
                obstacle.Configure(MapObstacleType.Rock, true);
            }
        }

        private Vector2 BackgroundPixelToColliderLocal(Vector2 pixel, Transform body)
        {
            Sprite sprite = _terrainRenderer.sprite;
            float ppu = Mathf.Max(.01f, sprite.pixelsPerUnit);
            // Image pixel Y grows down; Sprite local Y grows up.
            Vector3 local = new Vector3((pixel.x - sprite.pivot.x) / ppu,
                (sprite.rect.height - pixel.y - sprite.pivot.y) / ppu, 0f);
            Vector3 world = _terrainRenderer.transform.TransformPoint(local);
            return body.InverseTransformPoint(world);
        }

        private void EnsurePlayerCollidesAndStartsOnWalkableGround()
        {
            PlayerMovement movement = Object.FindFirstObjectByType<PlayerMovement>();
            if (movement == null) return;
            _playerBody = movement.GetComponent<Rigidbody2D>();
            Collider2D playerCollider = movement.GetComponent<Collider2D>();
            if (_playerBody == null || playerCollider == null)
            {
                Debug.LogError("IceArena: the Player needs a Rigidbody2D and a non-trigger Collider2D.");
                return;
            }
            _originalCollisionDetection = _playerBody.collisionDetectionMode;
            _playerBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _playerLayer = movement.gameObject.layer;
            if (_obstacleLayer >= 0 && Physics2D.GetIgnoreLayerCollision(_playerLayer, _obstacleLayer))
            {
                _changedLayerCollision = true;
                Physics2D.IgnoreLayerCollision(_playerLayer, _obstacleLayer, false);
            }

            Physics2D.SyncTransforms();
            // Never spawn directly inside a painted ice cliff. If a previous save/spawn
            // has placed the character there, find nearby unobstructed ground.
            Vector2 spawn = movement.transform.position;
            if (!OverlapsSolid(spawn, .38f)) return;
            for (float radius = .6f; radius <= 14f; radius += .6f)
            {
                for (int step = 0; step < 24; step++)
                {
                    float angle = step * Mathf.PI * 2f / 24f;
                    Vector2 test = spawn + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                    if (!_boundary.WorldBounds.Contains(new Vector3(test.x, test.y, _boundary.transform.position.z))) continue;
                    if (OverlapsSolid(test, .45f)) continue;
                    _playerBody.position = test;
                    movement.transform.position = test;
                    _playerBody.linearVelocity = Vector2.zero;
                    Physics2D.SyncTransforms();
                    Debug.Log("IceArena: player spawn moved to nearby walkable ground.");
                    return;
                }
            }
            Debug.LogWarning("IceArena: player overlaps solid terrain, no nearby safe spawn found.");
        }

        private bool OverlapsSolid(Vector2 point, float radius)
        {
            for (int i = 0; i < _solidGeometry.Count; i++)
            {
                Collider2D c = _solidGeometry[i];
                if (c == null || !c.enabled) continue;
                // OverlapPoint handles irregular polygons; Distance handles a player's radius.
                if (c.OverlapPoint(point)) return true;
                Vector2 closest = c.ClosestPoint(point);
                if ((closest - point).sqrMagnitude < radius * radius) return true;
            }
            return false;
        }

        private void OnDestroy()
        {
            if (_playerBody != null) _playerBody.collisionDetectionMode = _originalCollisionDetection;
            if (_changedLayerCollision && _playerLayer >= 0 && _obstacleLayer >= 0)
                Physics2D.IgnoreLayerCollision(_playerLayer, _obstacleLayer, true);
        }
    }
}
