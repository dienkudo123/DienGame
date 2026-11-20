using System.Collections.Generic;
using System.IO;
using DienGame.Gameplay.Map;
using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    public static class MapObstaclePrefabBuilder
    {
        private const string SourceFolder =
            "Assets/_Game/Art/Map/Obstacles";

        private const string OutputFolder =
            "Assets/_Game/Prefabs/Map/Obstacles";

        private const string ObstacleLayerName =
            "Obstacle";

        // =========================================
        // MENU
        // =========================================

        [MenuItem(
            "DienGame/Map/Build Obstacle Prefabs"
        )]
        private static void BuildAll()
        {
            EnsureFolder(
                OutputFolder
            );

            string[] guids =
                AssetDatabase.FindAssets(
                    "t:Texture2D",
                    new[]
                    {
                        SourceFolder
                    }
                );

            if (guids.Length == 0)
            {
                Debug.LogWarning(
                    $"Không tìm thấy ảnh obstacle trong:\n{SourceFolder}"
                );

                return;
            }

            int builtCount = 0;

            foreach (string guid in guids)
            {
                string assetPath =
                    AssetDatabase.GUIDToAssetPath(
                        guid
                    );

                if (
                    !assetPath.EndsWith(
                        ".png",
                        System.StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    continue;
                }

                PrepareTexture(
                    assetPath
                );

                Sprite sprite =
                    AssetDatabase
                        .LoadAssetAtPath<Sprite>(
                            assetPath
                        );

                if (sprite == null)
                {
                    Debug.LogWarning(
                        $"Không load được Sprite: {assetPath}"
                    );

                    continue;
                }

                BuildPrefab(
                    assetPath,
                    sprite
                );

                builtCount++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"Build obstacle hoàn tất: {builtCount} prefab."
            );
        }

        // =========================================
        // BUILD PREFAB
        // =========================================

        private static void BuildPrefab(
            string sourcePath,
            Sprite sprite)
        {
            string fileName =
                Path.GetFileNameWithoutExtension(
                    sourcePath
                );

            MapObstacleType type =
                DetectType(
                    fileName
                );

            GameObject root =
                new GameObject(
                    fileName
                );

            try
            {
                // =================================
                // LAYER
                // =================================

                int obstacleLayer =
                    LayerMask.NameToLayer(
                        ObstacleLayerName
                    );

                if (obstacleLayer >= 0)
                {
                    root.layer =
                        obstacleLayer;
                }

                // =================================
                // MAP OBSTACLE DATA
                // =================================

                MapObstacle obstacle =
                    root.AddComponent<
                        MapObstacle
                    >();

                obstacle.Configure(
                    type,
                    true
                );

                // =================================
                // VISUAL
                // =================================

                GameObject visual =
                    new GameObject(
                        "Visual"
                    );

                visual.transform.SetParent(
                    root.transform,
                    false
                );

                visual.layer =
                    root.layer;

                SpriteRenderer renderer =
                    visual.AddComponent<
                        SpriteRenderer
                    >();

                renderer.sprite =
                    sprite;

                renderer.sortingOrder =
                    5;

                // =================================
                // POLYGON COLLIDER
                //
                // Collider nằm trên ROOT.
                // Outline lấy trực tiếp từ Sprite.
                // =================================

                PolygonCollider2D polygonCollider =
                    root.AddComponent<
                        PolygonCollider2D
                    >();

                BuildColliderFromSprite(
                    polygonCollider,
                    sprite
                );

                polygonCollider.isTrigger =
                    false;

                // =================================
                // SAVE
                // =================================

                string prefabPath =
                    $"{OutputFolder}/" +
                    $"{fileName}.prefab";

                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    prefabPath
                );

                Debug.Log(
                    $"Created: {prefabPath}"
                );
            }
            finally
            {
                Object.DestroyImmediate(
                    root
                );
            }
        }

        // =========================================
        // BUILD COLLIDER FROM SPRITE
        // =========================================

        private static void BuildColliderFromSprite(
            PolygonCollider2D collider,
            Sprite sprite)
        {
            int shapeCount =
                sprite.GetPhysicsShapeCount();

            if (shapeCount <= 0)
            {
                Debug.LogWarning(
                    $"Sprite '{sprite.name}' không có physics shape."
                );

                return;
            }

            collider.pathCount =
                shapeCount;

            List<Vector2> points =
                new List<Vector2>(
                    128
                );

            for (
                int shapeIndex = 0;
                shapeIndex < shapeCount;
                shapeIndex++
            )
            {
                points.Clear();

                sprite.GetPhysicsShape(
                    shapeIndex,
                    points
                );

                if (points.Count < 3)
                {
                    continue;
                }

                collider.SetPath(
                    shapeIndex,
                    points
                );
            }
        }

        // =========================================
        // DETECT TYPE
        // =========================================

        private static MapObstacleType DetectType(
            string fileName)
        {
            string lower =
                fileName.ToLowerInvariant();

            if (
                lower.Contains("rock") ||
                lower.Contains("stone")
            )
            {
                return
                    MapObstacleType.Rock;
            }

            if (
                lower.Contains("house") ||
                lower.Contains("building")
            )
            {
                return
                    MapObstacleType.House;
            }

            if (
                lower.Contains("tree")
            )
            {
                return
                    MapObstacleType.Tree;
            }

            return
                MapObstacleType.Other;
        }

        // =========================================
        // PREPARE TEXTURE
        // =========================================

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

            TextureImporterSettings settings =
                new TextureImporterSettings();

            importer.ReadTextureSettings(
                settings
            );

            bool changed =
                false;

            if (
                importer.textureType !=
                TextureImporterType.Sprite
            )
            {
                importer.textureType =
                    TextureImporterType.Sprite;

                changed =
                    true;
            }

            if (
                importer.spriteImportMode !=
                SpriteImportMode.Single
            )
            {
                importer.spriteImportMode =
                    SpriteImportMode.Single;

                changed =
                    true;
            }

            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency =
                    true;

                changed =
                    true;
            }

            if (
                importer.textureCompression !=
                TextureImporterCompression
                    .Uncompressed
            )
            {
                importer.textureCompression =
                    TextureImporterCompression
                        .Uncompressed;

                changed =
                    true;
            }

            // Unity tự tạo physics outline
            // nếu Sprite chưa có shape custom.
            if (
                !settings
                    .spriteGenerateFallbackPhysicsShape
            )
            {
                settings
                    .spriteGenerateFallbackPhysicsShape =
                    true;

                importer.SetTextureSettings(
                    settings
                );

                changed =
                    true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        // =========================================
        // FOLDER
        // =========================================

        private static void EnsureFolder(
            string path)
        {
            if (
                AssetDatabase.IsValidFolder(
                    path
                )
            )
            {
                return;
            }

            string parent =
                Path.GetDirectoryName(
                    path
                )?.Replace(
                    "\\",
                    "/"
                );

            string folder =
                Path.GetFileName(
                    path
                );

            if (
                string.IsNullOrEmpty(parent) ||
                string.IsNullOrEmpty(folder)
            )
            {
                return;
            }

            if (
                !AssetDatabase.IsValidFolder(
                    parent
                )
            )
            {
                EnsureFolder(
                    parent
                );
            }

            AssetDatabase.CreateFolder(
                parent,
                folder
            );
        }
    }
}