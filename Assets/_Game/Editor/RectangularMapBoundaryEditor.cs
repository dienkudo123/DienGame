using System.IO;
using DienGame.Gameplay.Map;
using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    [CustomEditor(typeof(RectangularMapBoundary))]
    public sealed class RectangularMapBoundaryEditor : Editor
    {
        // =========================================================
        // ASSET PATHS
        // =========================================================

        private const string WallFolder =
            "Assets/_Game/Art/Map/Walls";

        private const string HorizontalPath =
            WallFolder + "/Wall_Horizontal.png";

        private const string VerticalPath =
            WallFolder + "/Wall_Vertical.png";

        private const string CornerTopLeftPath =
            WallFolder + "/Corner_TopLeft.png";

        private const string CornerTopRightPath =
            WallFolder + "/Corner_TopRight.png";

        private const string CornerBottomRightPath =
            WallFolder + "/Corner_BottomRight.png";

        private const string CornerBottomLeftPath =
            WallFolder + "/Corner_BottomLeft.png";

        // =========================================================
        // GENERATED ROOTS
        // =========================================================

        private const string VisualRootName =
            "GeneratedVisuals";

        private const string CollisionRootName =
            "GeneratedColliders";

        // =========================================================
        // INSPECTOR
        // =========================================================

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(12f);

            EditorGUILayout.HelpBox(
                "Corner và wall dùng chung một đường tâm.\n" +
                "Wall sẽ chạy hơi vào dưới corner để không tạo khe hở.",
                MessageType.Info
            );

            RectangularMapBoundary boundary =
                (RectangularMapBoundary)target;

            if (
                GUILayout.Button(
                    "BUILD RECTANGULAR MAP",
                    GUILayout.Height(42f)
                )
            )
            {
                Build(boundary);
            }

            if (
                GUILayout.Button(
                    "CLEAR GENERATED MAP",
                    GUILayout.Height(30f)
                )
            )
            {
                Clear(boundary);
            }
        }

        // =========================================================
        // BUILD
        // =========================================================

        private static void Build(
            RectangularMapBoundary boundary)
        {
            if (!ValidateFiles())
            {
                return;
            }

            ImportAllSprites();

            Sprite horizontal =
                LoadSprite(HorizontalPath);

            Sprite vertical =
                LoadSprite(VerticalPath);

            Sprite topLeft =
                LoadSprite(CornerTopLeftPath);

            Sprite topRight =
                LoadSprite(CornerTopRightPath);

            Sprite bottomRight =
                LoadSprite(CornerBottomRightPath);

            Sprite bottomLeft =
                LoadSprite(CornerBottomLeftPath);

            if (
                horizontal == null ||
                vertical == null ||
                topLeft == null ||
                topRight == null ||
                bottomRight == null ||
                bottomLeft == null
            )
            {
                Debug.LogError(
                    "Không load được đầy đủ 6 sprite tường.",
                    boundary
                );

                return;
            }

            Undo.SetCurrentGroupName(
                "Build Rectangular Map"
            );

            int undoGroup =
                Undo.GetCurrentGroup();

            Clear(boundary);

            Transform visualRoot =
                CreateRoot(
                    boundary.transform,
                    VisualRootName
                );

            Transform collisionRoot =
                CreateRoot(
                    boundary.transform,
                    CollisionRootName
                );

            BuildHorizontalWalls(
                boundary,
                visualRoot,
                horizontal
            );

            BuildVerticalWalls(
                boundary,
                visualRoot,
                vertical
            );

            BuildCorners(
                boundary,
                visualRoot,
                topLeft,
                topRight,
                bottomRight,
                bottomLeft
            );

            BuildColliders(
                boundary,
                collisionRoot
            );

            EditorUtility.SetDirty(
                boundary
            );

            Undo.CollapseUndoOperations(
                undoGroup
            );

            Debug.Log(
                "Build map hình chữ nhật hoàn tất.",
                boundary
            );
        }

        // =========================================================
        // COMMON WALL CENTER LINES
        // =========================================================

        private static void GetWallLines(
            RectangularMapBoundary boundary,
            out float leftX,
            out float rightX,
            out float topY,
            out float bottomY)
        {
            float halfWidth =
                boundary.MapSize.x * 0.5f;

            float halfHeight =
                boundary.MapSize.y * 0.5f;

            float halfThickness =
                boundary.WallVisualThickness *
                0.5f;

            // TẤT CẢ wall + corner đều dùng
            // chính 4 đường tâm này.

            leftX =
                -halfWidth +
                halfThickness;

            rightX =
                halfWidth -
                halfThickness;

            topY =
                halfHeight -
                halfThickness;

            bottomY =
                -halfHeight +
                halfThickness;
        }

        // =========================================================
        // HORIZONTAL WALLS
        // =========================================================

        private static void BuildHorizontalWalls(
            RectangularMapBoundary boundary,
            Transform parent,
            Sprite sprite)
        {
            GetWallLines(
                boundary,
                out float leftX,
                out float rightX,
                out float topY,
                out float bottomY
            );

            float totalLength =
                rightX - leftX;

            int count =
                Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        totalLength /
                        boundary.WallSegmentLength
                    )
                );

            float step =
                totalLength /
                count;

            // Cho tường chồng nhẹ lên nhau.
            float visualLength =
                step +
                boundary.WallOverlap;

            for (
                int i = 0;
                i < count;
                i++
            )
            {
                float t =
                    (i + 0.5f) /
                    count;

                float x =
                    Mathf.Lerp(
                        leftX,
                        rightX,
                        t
                    );

                CreateSpriteObject(
                    parent,
                    $"Wall_Top_{i:00}",
                    sprite,
                    new Vector3(
                        x,
                        topY,
                        0f
                    ),
                    visualLength,
                    boundary.WallVisualThickness,
                    boundary,
                    0
                );

                CreateSpriteObject(
                    parent,
                    $"Wall_Bottom_{i:00}",
                    sprite,
                    new Vector3(
                        x,
                        bottomY,
                        0f
                    ),
                    visualLength,
                    boundary.WallVisualThickness,
                    boundary,
                    0
                );
            }
        }

        // =========================================================
        // VERTICAL WALLS
        // =========================================================

        private static void BuildVerticalWalls(
            RectangularMapBoundary boundary,
            Transform parent,
            Sprite sprite)
        {
            GetWallLines(
                boundary,
                out float leftX,
                out float rightX,
                out float topY,
                out float bottomY
            );

            float totalLength =
                topY - bottomY;

            int count =
                Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        totalLength /
                        boundary.WallSegmentLength
                    )
                );

            float step =
                totalLength /
                count;

            float visualLength =
                step +
                boundary.WallOverlap;

            for (
                int i = 0;
                i < count;
                i++
            )
            {
                float t =
                    (i + 0.5f) /
                    count;

                float y =
                    Mathf.Lerp(
                        bottomY,
                        topY,
                        t
                    );

                CreateSpriteObject(
                    parent,
                    $"Wall_Left_{i:00}",
                    sprite,
                    new Vector3(
                        leftX,
                        y,
                        0f
                    ),
                    boundary.WallVisualThickness,
                    visualLength,
                    boundary,
                    0
                );

                CreateSpriteObject(
                    parent,
                    $"Wall_Right_{i:00}",
                    sprite,
                    new Vector3(
                        rightX,
                        y,
                        0f
                    ),
                    boundary.WallVisualThickness,
                    visualLength,
                    boundary,
                    0
                );
            }
        }

        // =========================================================
        // CORNERS
        // =========================================================

        private static void BuildCorners(
            RectangularMapBoundary boundary,
            Transform parent,
            Sprite topLeft,
            Sprite topRight,
            Sprite bottomRight,
            Sprite bottomLeft)
        {
            GetWallLines(
                boundary,
                out float leftX,
                out float rightX,
                out float topY,
                out float bottomY
            );

            float size =
                boundary.CornerSize;

            // Corner nằm CHÍNH XÁC tại giao điểm
            // của horizontal + vertical wall.

            CreateSpriteObject(
                parent,
                "Corner_TopLeft",
                topLeft,
                new Vector3(
                    leftX,
                    topY,
                    0f
                ),
                size,
                size,
                boundary,
                1
            );

            CreateSpriteObject(
                parent,
                "Corner_TopRight",
                topRight,
                new Vector3(
                    rightX,
                    topY,
                    0f
                ),
                size,
                size,
                boundary,
                1
            );

            CreateSpriteObject(
                parent,
                "Corner_BottomRight",
                bottomRight,
                new Vector3(
                    rightX,
                    bottomY,
                    0f
                ),
                size,
                size,
                boundary,
                1
            );

            CreateSpriteObject(
                parent,
                "Corner_BottomLeft",
                bottomLeft,
                new Vector3(
                    leftX,
                    bottomY,
                    0f
                ),
                size,
                size,
                boundary,
                1
            );
        }

        // =========================================================
        // CREATE SPRITE OBJECT
        // =========================================================

        private static GameObject CreateSpriteObject(
            Transform parent,
            string objectName,
            Sprite sprite,
            Vector3 localPosition,
            float targetWidth,
            float targetHeight,
            RectangularMapBoundary boundary,
            int sortingOffset)
        {
            GameObject obj =
                new GameObject(
                    objectName
                );

            Undo.RegisterCreatedObjectUndo(
                obj,
                "Create Wall Sprite"
            );

            obj.transform.SetParent(
                parent,
                false
            );

            obj.transform.localPosition =
                localPosition;

            SpriteRenderer renderer =
                obj.AddComponent<
                    SpriteRenderer
                >();

            renderer.sprite =
                sprite;

            renderer.sortingLayerName =
                boundary.SortingLayerName;

            renderer.sortingOrder =
                boundary.SortingOrder +
                sortingOffset;

            FitSpriteToSize(
                obj.transform,
                sprite,
                targetWidth,
                targetHeight
            );

            return obj;
        }

        // =========================================================
        // AUTO FIT
        // =========================================================

        private static void FitSpriteToSize(
            Transform target,
            Sprite sprite,
            float targetWidth,
            float targetHeight)
        {
            if (sprite == null)
            {
                return;
            }

            Vector2 spriteSize =
                sprite.bounds.size;

            float scaleX =
                spriteSize.x > 0f
                    ? targetWidth /
                      spriteSize.x
                    : 1f;

            float scaleY =
                spriteSize.y > 0f
                    ? targetHeight /
                      spriteSize.y
                    : 1f;

            target.localScale =
                new Vector3(
                    scaleX,
                    scaleY,
                    1f
                );
        }

        // =========================================================
        // COLLIDERS
        // =========================================================

        private static void BuildColliders(
            RectangularMapBoundary boundary,
            Transform parent)
        {
            GetWallLines(
                boundary,
                out float leftX,
                out float rightX,
                out float topY,
                out float bottomY
            );

            float thickness =
                boundary.CollisionThickness;

            float width =
                rightX -
                leftX +
                boundary.WallVisualThickness;

            float height =
                topY -
                bottomY +
                boundary.WallVisualThickness;

            int layer =
                LayerMask.NameToLayer(
                    boundary.BoundaryLayerName
                );

            if (layer < 0)
            {
                layer = 0;

                Debug.LogWarning(
                    $"Layer '{boundary.BoundaryLayerName}' " +
                    "không tồn tại. Dùng Default.",
                    boundary
                );
            }

            CreateCollider(
                parent,
                "Collider_Top",
                new Vector2(
                    0f,
                    topY
                ),
                new Vector2(
                    width,
                    thickness
                ),
                layer
            );

            CreateCollider(
                parent,
                "Collider_Bottom",
                new Vector2(
                    0f,
                    bottomY
                ),
                new Vector2(
                    width,
                    thickness
                ),
                layer
            );

            CreateCollider(
                parent,
                "Collider_Left",
                new Vector2(
                    leftX,
                    0f
                ),
                new Vector2(
                    thickness,
                    height
                ),
                layer
            );

            CreateCollider(
                parent,
                "Collider_Right",
                new Vector2(
                    rightX,
                    0f
                ),
                new Vector2(
                    thickness,
                    height
                ),
                layer
            );
        }

        private static void CreateCollider(
            Transform parent,
            string objectName,
            Vector2 localPosition,
            Vector2 size,
            int layer)
        {
            GameObject obj =
                new GameObject(
                    objectName
                );

            Undo.RegisterCreatedObjectUndo(
                obj,
                "Create Boundary Collider"
            );

            obj.transform.SetParent(
                parent,
                false
            );

            obj.transform.localPosition =
                localPosition;

            obj.layer =
                layer;

            BoxCollider2D collider =
                obj.AddComponent<
                    BoxCollider2D
                >();

            collider.size =
                size;
        }

        // =========================================================
        // IMPORT
        // =========================================================

        private static void ImportAllSprites()
        {
            PrepareTexture(HorizontalPath);
            PrepareTexture(VerticalPath);
            PrepareTexture(CornerTopLeftPath);
            PrepareTexture(CornerTopRightPath);
            PrepareTexture(CornerBottomRightPath);
            PrepareTexture(CornerBottomLeftPath);

            AssetDatabase.Refresh();
        }

        private static void PrepareTexture(
            string path)
        {
            TextureImporter importer =
                AssetImporter.GetAtPath(
                    path
                ) as TextureImporter;

            if (importer == null)
            {
                return;
            }

            bool changed =
                false;

            if (
                importer.textureType !=
                TextureImporterType.Sprite
            )
            {
                importer.textureType =
                    TextureImporterType.Sprite;

                changed = true;
            }

            if (
                importer.spriteImportMode !=
                SpriteImportMode.Single
            )
            {
                importer.spriteImportMode =
                    SpriteImportMode.Single;

                changed = true;
            }

            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency =
                    true;

                changed = true;
            }

            if (
                importer.textureCompression !=
                TextureImporterCompression.Uncompressed
            )
            {
                importer.textureCompression =
                    TextureImporterCompression
                        .Uncompressed;

                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        // =========================================================
        // LOAD
        // =========================================================

        private static Sprite LoadSprite(
            string path)
        {
            return
                AssetDatabase
                    .LoadAssetAtPath<
                        Sprite
                    >(path);
        }

        // =========================================================
        // VALIDATE
        // =========================================================

        private static bool ValidateFiles()
        {
            string[] required =
            {
                HorizontalPath,
                VerticalPath,
                CornerTopLeftPath,
                CornerTopRightPath,
                CornerBottomRightPath,
                CornerBottomLeftPath,
            };

            bool valid =
                true;

            foreach (
                string path
                in required
            )
            {
                if (
                    File.Exists(
                        Path.GetFullPath(
                            path
                        )
                    )
                )
                {
                    continue;
                }

                Debug.LogError(
                    $"Thiếu ảnh:\n{path}"
                );

                valid = false;
            }

            return valid;
        }

        // =========================================================
        // ROOT
        // =========================================================

        private static Transform CreateRoot(
            Transform parent,
            string rootName)
        {
            GameObject obj =
                new GameObject(
                    rootName
                );

            Undo.RegisterCreatedObjectUndo(
                obj,
                "Create Generated Root"
            );

            obj.transform.SetParent(
                parent,
                false
            );

            return obj.transform;
        }

        // =========================================================
        // CLEAR
        // =========================================================

        private static void Clear(
            RectangularMapBoundary boundary)
        {
            DeleteChild(
                boundary.transform,
                VisualRootName
            );

            DeleteChild(
                boundary.transform,
                CollisionRootName
            );
        }

        private static void DeleteChild(
            Transform parent,
            string childName)
        {
            Transform child =
                parent.Find(
                    childName
                );

            if (child == null)
            {
                return;
            }

            Undo.DestroyObjectImmediate(
                child.gameObject
            );
        }
    }
}