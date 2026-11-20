#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using DienGame.Gameplay.Balance;
using DienGame.Gameplay.Enemy;
using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    [InitializeOnLoad]
    public static class EnemyCatalogBuilder
    {
        private const string CatalogFolder = "Assets/_Game/Resources";
        private const string CatalogPath = CatalogFolder + "/EnemyCatalog.asset";

        private const string RunnerPath =
            "Assets/_Game/Art/Enemies/Runner/Enemy_Runner_Run.png";
        private const string TankPath =
            "Assets/_Game/Art/Enemies/Tank/Enemy_Tank_Run.png";
        private const string RangedPath =
            "Assets/_Game/Art/Enemies/Ranged/Enemy_Ranged_Hover.png";
        private const string SwarmPath =
            "Assets/_Game/Art/Enemies/Swarm/Enemy_Swarm_Run.png";
        private const string ElitePath =
            "Assets/_Game/Art/Enemies/Elite/Enemy_Elite_Run.png";
        private const string BossPath =
            "Assets/_Game/Art/Enemies/Boss/Enemy_Boss_Run.png";

        static EnemyCatalogBuilder()
        {
            EditorApplication.delayCall += EnsureCatalogExists;
        }

        [MenuItem("DienGame/Enemies/Rebuild Enemy Catalog")]
        public static void RebuildCatalog()
        {
            EnsureFolder(CatalogFolder);

            EnemyCatalog catalog =
                AssetDatabase.LoadAssetAtPath<EnemyCatalog>(CatalogPath);

            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<EnemyCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            EnemyVisualSet eliteVisual = CreateVisual(
                ElitePath,
                8f,
                1.15f
            );

            var definitions = new List<EnemyDefinition>
            {
                CreateDefinition(
                    EnemyArchetype.Grunt,
                    "Grunt",
                    EnemyAttackKind.Melee,
                    GameBalance.GruntHealth,
                    GameBalance.GruntDamage,
                    GameBalance.GruntMoveSpeed,
                    GameBalance.GruntExperience,
                    0.8f,
                    0f,
                    GameBalance.EnemyAttackInterval,
                    0f,
                    0f,
                    CreateEmptyVisual(6f, 1f),
                    eliteVisual
                ),
                CreateDefinition(
                    EnemyArchetype.Runner,
                    "Runner",
                    EnemyAttackKind.Melee,
                    GameBalance.RunnerHealth,
                    GameBalance.RunnerDamage,
                    GameBalance.RunnerMoveSpeed,
                    GameBalance.RunnerExperience,
                    0.8f,
                    0f,
                    GameBalance.EnemyAttackInterval,
                    0f,
                    0f,
                    CreateVisual(RunnerPath, 10f, 0.88f),
                    eliteVisual
                ),
                CreateDefinition(
                    EnemyArchetype.Tank,
                    "Tank",
                    EnemyAttackKind.Melee,
                    GameBalance.TankHealth,
                    GameBalance.TankDamage,
                    GameBalance.TankMoveSpeed,
                    GameBalance.TankExperience,
                    0.9f,
                    0f,
                    GameBalance.EnemyAttackInterval,
                    0f,
                    0f,
                    CreateVisual(TankPath, 5f, 1.22f),
                    eliteVisual
                ),
                CreateDefinition(
                    EnemyArchetype.Ranged,
                    "Ranged",
                    EnemyAttackKind.Ranged,
                    GameBalance.RangedHealth,
                    GameBalance.RangedProjectileDamage,
                    GameBalance.RangedMoveSpeed,
                    GameBalance.RangedExperience,
                    GameBalance.RangedPreferredDistance,
                    GameBalance.RangedRetreatDistance,
                    GameBalance.RangedAttackInterval,
                    GameBalance.RangedProjectileSpeed,
                    GameBalance.RangedMaxAttackDistance,
                    CreateVisual(RangedPath, 6f, 0.95f),
                    eliteVisual
                ),
                CreateDefinition(
                    EnemyArchetype.Swarm,
                    "Swarm",
                    EnemyAttackKind.Melee,
                    GameBalance.SwarmHealth,
                    GameBalance.SwarmDamage,
                    GameBalance.SwarmMoveSpeed,
                    GameBalance.SwarmExperience,
                    0.65f,
                    0f,
                    GameBalance.EnemyAttackInterval,
                    0f,
                    0f,
                    CreateVisual(SwarmPath, 11f, 0.60f),
                    eliteVisual
                ),
                CreateDefinition(
                    EnemyArchetype.Boss,
                    "Boss_Judgement",
                    EnemyAttackKind.Boss,
                    GameBalance.BossMaxHealth,
                    GameBalance.BossContactDamage,
                    GameBalance.BossMoveSpeed,
                    0,
                    1.05f,
                    0f,
                    GameBalance.EnemyAttackInterval,
                    0f,
                    0f,
                    CreateVisual(BossPath, 5f, 1.80f),
                    CreateEmptyVisual(5f, 1.80f)
                ),
            };

            catalog.ReplaceDefinitions(definitions);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EnemyCatalogProvider.ClearCache();

            Debug.Log(
                "DienGame: EnemyCatalog rebuilt. " +
                "Art + stats + attack configuration đã được bind tự động."
            );
        }

        private static void EnsureCatalogExists()
        {
            EnemyCatalog current =
                AssetDatabase.LoadAssetAtPath<EnemyCatalog>(CatalogPath);

            if (current == null)
            {
                RebuildCatalog();
            }
        }

        private static EnemyDefinition CreateDefinition(
            EnemyArchetype archetype,
            string displayName,
            EnemyAttackKind attackKind,
            float health,
            float damage,
            float moveSpeed,
            int experience,
            float preferredDistance,
            float retreatDistance,
            float attackInterval,
            float projectileSpeed,
            float maxAttackDistance,
            EnemyVisualSet visual,
            EnemyVisualSet eliteVisual)
        {
            var definition = new EnemyDefinition();

            definition.Configure(
                archetype,
                displayName,
                attackKind,
                health,
                damage,
                moveSpeed,
                experience,
                preferredDistance,
                retreatDistance,
                attackInterval,
                projectileSpeed,
                maxAttackDistance,
                visual,
                eliteVisual
            );

            return definition;
        }

        private static EnemyVisualSet CreateVisual(
            string texturePath,
            float fps,
            float scale)
        {
            var visual = new EnemyVisualSet();
            visual.Configure(LoadFrames(texturePath), fps, scale);
            return visual;
        }

        private static EnemyVisualSet CreateEmptyVisual(
            float fps,
            float scale)
        {
            var visual = new EnemyVisualSet();
            visual.Configure(Array.Empty<Sprite>(), fps, scale);
            return visual;
        }

        private static Sprite[] LoadFrames(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return Array.Empty<Sprite>();
            }

            UnityEngine.Object[] assets =
                AssetDatabase.LoadAllAssetsAtPath(assetPath);

            var sprites = new List<Sprite>();

            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Sprite sprite)
                {
                    sprites.Add(sprite);
                }
            }

            sprites.Sort((a, b) =>
                string.Compare(
                    a.name,
                    b.name,
                    StringComparison.OrdinalIgnoreCase
                )
            );

            if (sprites.Count == 0)
            {
                Debug.LogWarning(
                    $"DienGame: chưa tìm thấy sprite frame tại {assetPath}. " +
                    "Kiểm tra Sprite Mode = Multiple và đã Slice + Apply."
                );
            }

            return sprites.ToArray();
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath)
                ?.Replace('\\', '/');
            string name = Path.GetFileName(folderPath);

            if (!string.IsNullOrWhiteSpace(parent))
            {
                EnsureFolder(parent);
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }

    public sealed class EnemyArtAssetPostprocessor : AssetPostprocessor
    {
        private const string EnemyArtRoot = "Assets/_Game/Art/Enemies/";

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (!ContainsEnemyArt(importedAssets) &&
                !ContainsEnemyArt(deletedAssets) &&
                !ContainsEnemyArt(movedAssets) &&
                !ContainsEnemyArt(movedFromAssetPaths))
            {
                return;
            }

            EditorApplication.delayCall += EnemyCatalogBuilder.RebuildCatalog;
        }

        private static bool ContainsEnemyArt(string[] paths)
        {
            if (paths == null)
            {
                return false;
            }

            for (int i = 0; i < paths.Length; i++)
            {
                if (!string.IsNullOrEmpty(paths[i]) &&
                    paths[i].StartsWith(
                        EnemyArtRoot,
                        StringComparison.OrdinalIgnoreCase
                    ))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
#endif
