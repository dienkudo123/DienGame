using System.IO;
using DienGame.Gameplay.Enemy;
using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    public static class EnemySlimeSetupBuilder
    {
        // =========================================================
        // 4 ẢNH SLIME
        // =========================================================

        private const string SlimeFolder =
            "Assets/_Game/Art/Enemies/Slime";

        private static readonly string[] SlimeSpriteNames =
        {
            "Slime_0",
            "Slime_1",
            "Slime_2",
            "Slime_3",
        };

        // =========================================================
        // TỐC ĐỘ ANIMATION
        // =========================================================
        //
        // 4 = chậm
        // 6 = vừa
        // 8 = nhanh
        //
        private const float SlimeFramesPerSecond = 6f;

        // =========================================================
        // KÍCH THƯỚC NGOẠI HÌNH SLIME
        // =========================================================
        //
        // 0.5 = nhỏ
        // 1.0 = gốc
        // 1.5 = lớn
        //
        private const float SlimeVisualScale = 1f;

        [MenuItem(
            "DienGame/Enemy/Setup Slime Visual"
        )]
        private static void Setup()
        {
            string enemyPrefabPath =
                FindEnemyPrefabPath();

            if (
                string.IsNullOrEmpty(
                    enemyPrefabPath
                )
            )
            {
                Debug.LogError(
                    "Không tìm thấy Enemy Prefab.\n" +
                    "Hãy chọn Enemy Prefab trong cửa sổ Project " +
                    "rồi chạy lại:\n" +
                    "DienGame > Enemy > Setup Slime Visual"
                );

                return;
            }

            GameObject enemyPrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    enemyPrefabPath
                );

            if (enemyPrefab == null)
            {
                Debug.LogError(
                    $"Prefab không hợp lệ: {enemyPrefabPath}"
                );

                return;
            }

            Sprite[] sprites =
                LoadSlimeSprites();

            if (sprites == null)
            {
                return;
            }

            GameObject prefabRoot =
                PrefabUtility.LoadPrefabContents(
                    enemyPrefabPath
                );

            if (prefabRoot == null)
            {
                Debug.LogError(
                    $"Không mở được prefab: {enemyPrefabPath}"
                );

                return;
            }

            try
            {
                SetupVisual(
                    prefabRoot,
                    sprites
                );

                PrefabUtility.SaveAsPrefabAsset(
                    prefabRoot,
                    enemyPrefabPath
                );

                Debug.Log(
                    $"Đã setup Slime cho prefab:\n" +
                    $"{enemyPrefabPath}"
                );
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(
                    prefabRoot
                );
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // =========================================================
        // SETUP VISUAL
        // =========================================================

        private static void SetupVisual(
            GameObject prefabRoot,
            Sprite[] sprites)
        {
            Transform visualTransform =
                prefabRoot.transform.Find(
                    "Visual"
                );

            GameObject visualObject;

            if (visualTransform == null)
            {
                visualObject =
                    new GameObject(
                        "Visual"
                    );

                visualObject.transform.SetParent(
                    prefabRoot.transform,
                    false
                );
            }
            else
            {
                visualObject =
                    visualTransform.gameObject;
            }

            // Visual luôn nằm đúng giữa Enemy.
            visualObject.transform.localPosition =
                Vector3.zero;

            visualObject.transform.localRotation =
                Quaternion.identity;

            visualObject.transform.localScale =
                Vector3.one;

            // =====================================================
            // SPRITE RENDERER
            // =====================================================

            SpriteRenderer spriteRenderer =
                visualObject.GetComponent<SpriteRenderer>();

            if (spriteRenderer == null)
            {
                spriteRenderer =
                    visualObject.AddComponent<SpriteRenderer>();
            }

            spriteRenderer.sprite =
                sprites[0];

            spriteRenderer.color =
                Color.white;

            // =====================================================
            // SLIME ANIMATION
            // =====================================================

            EnemySlimeAnimator slimeAnimator =
                visualObject.GetComponent<EnemySlimeAnimator>();

            if (slimeAnimator == null)
            {
                slimeAnimator =
                    visualObject.AddComponent<EnemySlimeAnimator>();
            }

            slimeAnimator.Configure(
                sprites,
                SlimeFramesPerSecond,
                SlimeVisualScale
            );

            // =====================================================
            // TẮT SPRITE CŨ TRÊN ROOT
            // =====================================================

            SpriteRenderer rootRenderer =
                prefabRoot.GetComponent<SpriteRenderer>();

            if (
                rootRenderer != null &&
                rootRenderer != spriteRenderer
            )
            {
                rootRenderer.enabled =
                    false;
            }

            // =====================================================
            // TẮT ANIMATOR CŨ TRÊN VISUAL NẾU CÓ
            // =====================================================

            Animator oldAnimator =
                visualObject.GetComponent<Animator>();

            if (oldAnimator != null)
            {
                oldAnimator.enabled =
                    false;
            }
        }

        // =========================================================
        // TÌM ENEMY PREFAB
        // =========================================================

        private static string FindEnemyPrefabPath()
        {
            // =====================================================
            // 1. ƯU TIÊN PREFAB ĐANG ĐƯỢC CHỌN
            // =====================================================

            Object selected =
                Selection.activeObject;

            if (selected != null)
            {
                string selectedPath =
                    AssetDatabase.GetAssetPath(
                        selected
                    );

                if (
                    !string.IsNullOrEmpty(
                        selectedPath
                    ) &&
                    selectedPath.EndsWith(
                        ".prefab"
                    )
                )
                {
                    return selectedPath;
                }
            }

            // =====================================================
            // 2. TÌM PREFAB TÊN CHÍNH XÁC "Enemy"
            // =====================================================

            string[] enemyGuids =
                AssetDatabase.FindAssets(
                    "Enemy t:Prefab"
                );

            string firstEnemyCandidate =
                null;

            int candidateCount = 0;

            foreach (string guid in enemyGuids)
            {
                string path =
                    AssetDatabase.GUIDToAssetPath(
                        guid
                    );

                string fileName =
                    Path.GetFileNameWithoutExtension(
                        path
                    );

                if (
                    fileName.Equals(
                        "Enemy",
                        System.StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return path;
                }

                if (
                    fileName.Contains(
                        "Enemy",
                        System.StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    firstEnemyCandidate =
                        path;

                    candidateCount++;
                }
            }

            // =====================================================
            // 3. NẾU CHỈ CÓ ĐÚNG 1 PREFAB CHỨA CHỮ ENEMY
            // =====================================================

            if (
                candidateCount == 1 &&
                !string.IsNullOrEmpty(
                    firstEnemyCandidate
                )
            )
            {
                return firstEnemyCandidate;
            }

            return null;
        }

        // =========================================================
        // LOAD 4 SPRITE
        // =========================================================

        private static Sprite[] LoadSlimeSprites()
        {
            Sprite[] sprites =
                new Sprite[
                    SlimeSpriteNames.Length
                ];

            for (
                int i = 0;
                i < SlimeSpriteNames.Length;
                i++
            )
            {
                string path =
                    $"{SlimeFolder}/" +
                    $"{SlimeSpriteNames[i]}.png";

                Sprite sprite =
                    AssetDatabase.LoadAssetAtPath<Sprite>(
                        path
                    );

                if (sprite == null)
                {
                    Debug.LogError(
                        $"Không tìm thấy Slime Sprite:\n{path}"
                    );

                    return null;
                }

                sprites[i] =
                    sprite;
            }

            return sprites;
        }
    }
}