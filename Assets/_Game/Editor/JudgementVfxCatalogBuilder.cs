#if UNITY_EDITOR

using System.Collections.Generic;
using DienGame.Gameplay.Skills.Visuals;
using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    [InitializeOnLoad]
    public static class JudgementVfxCatalogBuilder
    {
        private const string ArtFolder =
            "Assets/_Game/Art/Skills/Marksman/Judgement";

        private const string ResourcesFolder =
            "Assets/_Game/Resources";

        private const string CatalogPath =
            ResourcesFolder +
            "/JudgementVfxCatalog.asset";

        private const string FramePrefix =
            "Judgement_Ultimate_";

        static JudgementVfxCatalogBuilder()
        {
            EditorApplication.delayCall +=
                BuildIfPossible;
        }

        [MenuItem(
            "DienGame/Skills/Rebuild Judgement VFX Catalog"
        )]
        public static void RebuildFromMenu()
        {
            BuildCatalog(true);
        }

        private static void BuildIfPossible()
        {
            BuildCatalog(false);
        }

        private static void BuildCatalog(
            bool logResult)
        {
            if (
                !AssetDatabase.IsValidFolder(
                    ArtFolder
                )
            )
            {
                if (logResult)
                {
                    Debug.LogWarning(
                        "JudgementVfxCatalogBuilder: " +
                        $"Chưa có folder {ArtFolder}."
                    );
                }

                return;
            }

            EnsureFolder(
                "Assets/_Game",
                "Resources"
            );

            Sprite[] frames =
                FindFrames();

            JudgementVfxCatalog catalog =
                AssetDatabase.LoadAssetAtPath<
                    JudgementVfxCatalog
                >(CatalogPath);

            if (catalog == null)
            {
                catalog =
                    ScriptableObject.CreateInstance<
                        JudgementVfxCatalog
                    >();

                AssetDatabase.CreateAsset(
                    catalog,
                    CatalogPath
                );
            }

            bool changed =
                catalog.SetFrames(
                    frames
                );

            if (changed)
            {
                EditorUtility.SetDirty(
                    catalog
                );

                AssetDatabase.SaveAssets();
            }

            if (!logResult)
            {
                return;
            }

            Debug.Log(
                "Judgement VFX Catalog: " +
                $"bind được {frames.Length} frame. " +
                $"Asset: {CatalogPath}"
            );

            if (frames.Length == 0)
            {
                Debug.LogWarning(
                    "Không tìm thấy frame animation. " +
                    "Hãy slice spritesheet và đặt tên " +
                    "Judgement_Ultimate_0, " +
                    "Judgement_Ultimate_1, ..."
                );
            }
        }

        private static Sprite[] FindFrames()
        {
            List<Sprite> result =
                new List<Sprite>();

            string[] guids =
                AssetDatabase.FindAssets(
                    "t:Texture2D",
                    new[]
                    {
                        ArtFolder
                    }
                );

            foreach (
                string guid in guids
            )
            {
                string path =
                    AssetDatabase.GUIDToAssetPath(
                        guid
                    );

                Object[] assets =
                    AssetDatabase.LoadAllAssetsAtPath(
                        path
                    );

                foreach (
                    Object asset in assets
                )
                {
                    if (
                        asset is Sprite sprite &&
                        sprite.name.StartsWith(
                            FramePrefix
                        )
                    )
                    {
                        result.Add(
                            sprite
                        );
                    }
                }
            }

            result.Sort(
                CompareFrames
            );

            return result.ToArray();
        }

        private static int CompareFrames(
            Sprite left,
            Sprite right)
        {
            int leftIndex =
                ExtractFrameIndex(
                    left != null
                        ? left.name
                        : string.Empty
                );

            int rightIndex =
                ExtractFrameIndex(
                    right != null
                        ? right.name
                        : string.Empty
                );

            int indexCompare =
                leftIndex.CompareTo(
                    rightIndex
                );

            if (indexCompare != 0)
            {
                return indexCompare;
            }

            return string.CompareOrdinal(
                left != null
                    ? left.name
                    : string.Empty,
                right != null
                    ? right.name
                    : string.Empty
            );
        }

        private static int ExtractFrameIndex(
            string spriteName)
        {
            if (
                string.IsNullOrEmpty(
                    spriteName
                )
            )
            {
                return int.MaxValue;
            }

            int lastUnderscore =
                spriteName.LastIndexOf(
                    '_'
                );

            if (
                lastUnderscore >= 0 &&
                lastUnderscore <
                    spriteName.Length - 1 &&
                int.TryParse(
                    spriteName.Substring(
                        lastUnderscore + 1
                    ),
                    out int index
                )
            )
            {
                return index;
            }

            return int.MaxValue;
        }

        private static void EnsureFolder(
            string parent,
            string child)
        {
            string path =
                parent +
                "/" +
                child;

            if (
                AssetDatabase.IsValidFolder(
                    path
                )
            )
            {
                return;
            }

            AssetDatabase.CreateFolder(
                parent,
                child
            );
        }
    }
}

#endif
