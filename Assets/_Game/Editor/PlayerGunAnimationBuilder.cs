using System.IO;
using System.Linq;
using DienGame.Gameplay.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DienGame.EditorTools
{
    public static class PlayerGunSpriteDataBuilder
    {
        private const string SpriteFolder =
            "Assets/_Game/Art/Characters/Player/Gun8Dir";

        private const string OutputFolder =
            "Assets/_Game/Animations/Player/Gun8Dir";

        private const string DataPath =
            OutputFolder +
            "/PlayerGunSpriteData.asset";

        private static readonly string[] Directions =
        {
            "Down",
            "DownLeft",
            "Left",
            "UpLeft",
            "Up",
            "UpRight",
            "Right",
            "DownRight",
        };

        [MenuItem(
            "DienGame/Animation/Build 8Dir Gun Sprite Data"
        )]
        private static void Build()
        {
            EnsureFolder(
                OutputFolder
            );

            PlayerSpriteAnimationData data =
                AssetDatabase
                    .LoadAssetAtPath<
                        PlayerSpriteAnimationData
                    >(DataPath);

            if (data == null)
            {
                data =
                    ScriptableObject
                        .CreateInstance<
                            PlayerSpriteAnimationData
                        >();

                AssetDatabase.CreateAsset(
                    data,
                    DataPath
                );
            }

            for (
                int directionIndex = 0;
                directionIndex <
                Directions.Length;
                directionIndex++
            )
            {
                string direction =
                    Directions[
                        directionIndex
                    ];

                string path =
                    FindSheetPath(
                        direction
                    );

                if (
                    string.IsNullOrEmpty(
                        path
                    )
                )
                {
                    Debug.LogError(
                        $"Không tìm thấy sheet: {direction}"
                    );

                    return;
                }

                Sprite[] sprites =
                    AssetDatabase
                        .LoadAllAssetsAtPath(
                            path
                        )
                        .OfType<Sprite>()
                        .OrderByDescending(
                            sprite =>
                                sprite.rect.y
                        )
                        .ThenBy(
                            sprite =>
                                sprite.rect.x
                        )
                        .ToArray();

                if (sprites.Length != 10)
                {
                    Debug.LogError(
                        $"{path} phải có đúng 10 sprite. " +
                        $"Hiện tại = {sprites.Length}"
                    );

                    return;
                }

                data.SetDirection(
                    directionIndex,
                    sprites
                );
            }

            EditorUtility.SetDirty(
                data
            );

            // Tự gắn Data vào PlayerAnimator.
            PlayerAnimator playerAnimator =
                Object.FindFirstObjectByType<
                    PlayerAnimator
                >();

            if (playerAnimator != null)
            {
                playerAnimator.SetAnimationData(
                    data
                );

                EditorUtility.SetDirty(
                    playerAnimator
                );
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorSceneManager.MarkSceneDirty(
                EditorSceneManager
                    .GetActiveScene()
            );

            Selection.activeObject =
                data;

            Debug.Log(
                "Build 8Dir Gun Sprite Data hoàn tất."
            );
        }

        private static string FindSheetPath(
            string direction)
        {
            string[] guids =
                AssetDatabase.FindAssets(
                    "t:Texture2D",
                    new[]
                    {
                        SpriteFolder
                    }
                );

            foreach (string guid in guids)
            {
                string path =
                    AssetDatabase
                        .GUIDToAssetPath(
                            guid
                        );

                string fileName =
                    Path.GetFileNameWithoutExtension(
                        path
                    );

                if (
                    fileName == direction ||
                    fileName == $"Run_{direction}"
                )
                {
                    return path;
                }
            }

            return null;
        }

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
                Path.GetDirectoryName(path)
                    ?.Replace("\\", "/");

            string folder =
                Path.GetFileName(path);

            if (
                !string.IsNullOrEmpty(parent) &&
                !AssetDatabase.IsValidFolder(
                    parent
                )
            )
            {
                EnsureFolder(
                    parent
                );
            }

            if (
                !string.IsNullOrEmpty(parent)
            )
            {
                AssetDatabase.CreateFolder(
                    parent,
                    folder
                );
            }
        }
    }
}