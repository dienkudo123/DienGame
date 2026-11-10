#if UNITY_EDITOR

using DienGame.Gameplay.Skills.Visuals;
using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    [InitializeOnLoad]
    public static class SkillIconCatalogBuilder
    {
        private const string SkillArtFolder =
            "Assets/_Game/Art/UI/Skills";

        private const string ResourcesFolder =
            "Assets/_Game/Resources";

        private const string CatalogPath =
            ResourcesFolder + "/SkillIconCatalog.asset";

        private const string PiercingName =
            "Icon_Skill_Piercing";

        private const string ThunderName =
            "Icon_Skill_Thunder";

        private const string DroneName =
            "Icon_Skill_Drone";

        private const string AirStrikeName =
            "Icon_Skill_AirStrike";

        private const string JudgementName =
            "Icon_Skill_Judgement";

        static SkillIconCatalogBuilder()
        {
            EditorApplication.delayCall +=
                BuildIfPossible;
        }

        [MenuItem(
            "DienGame/Skills/Rebuild Skill Icon Catalog"
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
                    SkillArtFolder
                )
            )
            {
                if (logResult)
                {
                    Debug.LogWarning(
                        "SkillIconCatalogBuilder: " +
                        $"Chưa có folder {SkillArtFolder}."
                    );
                }

                return;
            }

            EnsureFolder(
                "Assets/_Game",
                "Resources"
            );

            SkillIconCatalog catalog =
                AssetDatabase.LoadAssetAtPath<
                    SkillIconCatalog
                >(CatalogPath);

            if (catalog == null)
            {
                catalog =
                    ScriptableObject.CreateInstance<
                        SkillIconCatalog
                    >();

                AssetDatabase.CreateAsset(
                    catalog,
                    CatalogPath
                );
            }

            Sprite piercing =
                FindSprite(
                    PiercingName
                );

            Sprite thunder =
                FindSprite(
                    ThunderName
                );

            Sprite drone =
                FindSprite(
                    DroneName
                );

            Sprite airStrike =
                FindSprite(
                    AirStrikeName
                );

            Sprite judgement =
                FindSprite(
                    JudgementName
                );

            bool changed =
                catalog.SetIcons(
                    piercing,
                    thunder,
                    drone,
                    airStrike,
                    judgement
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

            int found = 0;

            if (piercing != null)
            {
                found++;
            }

            if (thunder != null)
            {
                found++;
            }

            if (drone != null)
            {
                found++;
            }

            if (airStrike != null)
            {
                found++;
            }

            if (judgement != null)
            {
                found++;
            }

            Debug.Log(
                $"Skill Icon Catalog: bind được {found}/5 icon. " +
                $"Asset: {CatalogPath}"
            );

            LogMissing(
                PiercingName,
                piercing
            );

            LogMissing(
                ThunderName,
                thunder
            );

            LogMissing(
                DroneName,
                drone
            );

            LogMissing(
                AirStrikeName,
                airStrike
            );

            LogMissing(
                JudgementName,
                judgement
            );
        }

        private static Sprite FindSprite(
            string spriteName)
        {
            string[] guids =
                AssetDatabase.FindAssets(
                    "t:Texture2D",
                    new[]
                    {
                        SkillArtFolder
                    }
                );

            foreach (
                string guid
                in guids
            )
            {
                string path =
                    AssetDatabase
                        .GUIDToAssetPath(
                            guid
                        );

                Object[] assets =
                    AssetDatabase
                        .LoadAllAssetsAtPath(
                            path
                        );

                foreach (
                    Object asset
                    in assets
                )
                {
                    if (
                        asset is Sprite sprite &&
                        sprite.name ==
                        spriteName
                    )
                    {
                        return sprite;
                    }
                }
            }

            return null;
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

        private static void LogMissing(
            string expectedName,
            Sprite sprite)
        {
            if (sprite != null)
            {
                return;
            }

            Debug.LogWarning(
                "SkillIconCatalogBuilder: " +
                $"Không tìm thấy sprite con tên '{expectedName}' " +
                $"trong {SkillArtFolder}. " +
                "Kiểm tra tên trong Sprite Editor."
            );
        }
    }
}

#endif