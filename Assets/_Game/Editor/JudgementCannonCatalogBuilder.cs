#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using DienGame.Gameplay.Skills.Visuals;
using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    [InitializeOnLoad]
    public static class JudgementCannonCatalogBuilder
    {
        private const string ArtFolder =
            "Assets/_Game/Art/Skills/Marksman/Judgement";

        private const string ResourcesFolder =
            "Assets/_Game/Resources";

        private const string CatalogPath =
            ResourcesFolder + "/JudgementCannonCatalog.asset";

        private static readonly string[] CannonPrefixes =
        {
            "Judgement_Cannon_Fire_",
            "Judgement_Cannon_",
        };

        private static readonly string[] BeamPrefixes =
        {
            "Judgement_Beam_",
            "Judgement_Laser_",
        };

        private static readonly string[] ShockwavePrefixes =
        {
            "Judgement_Shockwave_",
            "Judgement_ShockWave_",
        };

        static JudgementCannonCatalogBuilder()
        {
            EditorApplication.delayCall += BuildIfPossible;
        }

        [MenuItem("DienGame/Skills/Rebuild Judgement Cannon Catalog")]
        public static void RebuildFromMenu()
        {
            BuildCatalog(true);
        }

        private static void BuildIfPossible()
        {
            BuildCatalog(false);
        }

        private static void BuildCatalog(bool logResult)
        {
            if (!AssetDatabase.IsValidFolder(ArtFolder))
            {
                if (logResult)
                {
                    Debug.LogWarning(
                        "Judgement Cannon Catalog: chưa có folder " + ArtFolder
                    );
                }

                return;
            }

            EnsureFolder("Assets/_Game", "Resources");

            JudgementCannonCatalog catalog =
                AssetDatabase.LoadAssetAtPath<JudgementCannonCatalog>(CatalogPath);

            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<JudgementCannonCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            Sprite[] cannonFrames = FindFramesByName(CannonPrefixes);
            Sprite[] beamFrames = FindFramesByName(BeamPrefixes);
            Sprite[] shockwaveFrames = FindFramesByName(ShockwavePrefixes);

            bool usedLayoutFallback = false;

            if (cannonFrames.Length == 0 ||
                beamFrames.Length == 0 ||
                shockwaveFrames.Length == 0)
            {
                if (TryFindThreeRowSheet(
                        out Sprite[] layoutCannons,
                        out Sprite[] layoutBeams,
                        out Sprite[] layoutShockwaves))
                {
                    if (cannonFrames.Length == 0)
                    {
                        cannonFrames = layoutCannons;
                    }

                    if (beamFrames.Length == 0)
                    {
                        beamFrames = layoutBeams;
                    }

                    if (shockwaveFrames.Length == 0)
                    {
                        shockwaveFrames = layoutShockwaves;
                    }

                    usedLayoutFallback = true;
                }
            }

            bool changed = catalog.SetFrames(
                cannonFrames,
                beamFrames,
                shockwaveFrames
            );

            if (changed)
            {
                EditorUtility.SetDirty(catalog);
                AssetDatabase.SaveAssets();
            }

            if (!logResult)
            {
                return;
            }

            string mode = usedLayoutFallback
                ? "auto-detect theo 3 hàng"
                : "theo tên sprite";

            Debug.Log(
                "Judgement Cannon Catalog: " +
                $"Cannon {cannonFrames.Length} | " +
                $"Beam {beamFrames.Length} | " +
                $"Shockwave {shockwaveFrames.Length} frame " +
                $"({mode}). Asset: {CatalogPath}"
            );

            if (cannonFrames.Length == 0)
            {
                Debug.LogWarning(
                    "Không tìm thấy cannon frame. Slice hàng trên của sheet hoặc dùng tên " +
                    "Judgement_Cannon_Fire_0 ... Judgement_Cannon_Fire_7."
                );
            }

            if (beamFrames.Length == 0)
            {
                Debug.LogWarning(
                    "Không tìm thấy beam frame. Slice hàng giữa của sheet hoặc dùng tên " +
                    "Judgement_Beam_0 ... Judgement_Beam_5."
                );
            }

            if (shockwaveFrames.Length == 0)
            {
                Debug.LogWarning(
                    "Không tìm thấy shockwave frame. Slice hàng dưới của sheet hoặc dùng tên " +
                    "Judgement_Shockwave_0 ... Judgement_Shockwave_7."
                );
            }
        }

        private static Sprite[] FindFramesByName(string[] acceptedPrefixes)
        {
            List<SpriteEntry> entries = new();

            foreach (Sprite sprite in FindAllSprites())
            {
                if (!TryGetFrameIndex(
                        sprite.name,
                        acceptedPrefixes,
                        out int index))
                {
                    continue;
                }

                entries.Add(new SpriteEntry(index, sprite));
            }

            entries.Sort((a, b) => a.Index.CompareTo(b.Index));

            List<Sprite> frames = new(entries.Count);
            HashSet<int> usedIndexes = new();

            foreach (SpriteEntry entry in entries)
            {
                if (!usedIndexes.Add(entry.Index))
                {
                    continue;
                }

                frames.Add(entry.Sprite);
            }

            return frames.ToArray();
        }

        private static bool TryFindThreeRowSheet(
            out Sprite[] cannonFrames,
            out Sprite[] beamFrames,
            out Sprite[] shockwaveFrames)
        {
            cannonFrames = Array.Empty<Sprite>();
            beamFrames = Array.Empty<Sprite>();
            shockwaveFrames = Array.Empty<Sprite>();

            string[] guids = AssetDatabase.FindAssets(
                "t:Texture2D",
                new[] { ArtFolder }
            );

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                List<Sprite> sprites =
                    AssetDatabase.LoadAllAssetsAtPath(path)
                        .OfType<Sprite>()
                        .ToList();

                if (sprites.Count < 18)
                {
                    continue;
                }

                List<List<Sprite>> rows = BuildRows(sprites);

                if (rows.Count < 3)
                {
                    continue;
                }

                rows = rows
                    .OrderByDescending(row => AverageY(row))
                    .Take(3)
                    .ToList();

                Sprite[] top = SortLeftToRight(rows[0]);
                Sprite[] middle = SortLeftToRight(rows[1]);
                Sprite[] bottom = SortLeftToRight(rows[2]);

                if (top.Length < 4 || middle.Length < 3 || bottom.Length < 4)
                {
                    continue;
                }

                cannonFrames = top;
                beamFrames = middle;
                shockwaveFrames = bottom;
                return true;
            }

            return false;
        }

        private static List<List<Sprite>> BuildRows(List<Sprite> sprites)
        {
            List<List<Sprite>> rows = new();

            List<Sprite> ordered = sprites
                .OrderByDescending(sprite => sprite.rect.center.y)
                .ToList();

            foreach (Sprite sprite in ordered)
            {
                float centerY = sprite.rect.center.y;
                List<Sprite> matchingRow = null;

                foreach (List<Sprite> row in rows)
                {
                    float rowY = AverageY(row);
                    float tolerance = Mathf.Max(8f, sprite.rect.height * 0.35f);

                    if (Mathf.Abs(centerY - rowY) <= tolerance)
                    {
                        matchingRow = row;
                        break;
                    }
                }

                if (matchingRow == null)
                {
                    matchingRow = new List<Sprite>();
                    rows.Add(matchingRow);
                }

                matchingRow.Add(sprite);
            }

            return rows;
        }

        private static float AverageY(List<Sprite> row)
        {
            if (row == null || row.Count == 0)
            {
                return 0f;
            }

            float total = 0f;

            foreach (Sprite sprite in row)
            {
                total += sprite.rect.center.y;
            }

            return total / row.Count;
        }

        private static Sprite[] SortLeftToRight(List<Sprite> row)
        {
            return row
                .OrderBy(sprite => sprite.rect.xMin)
                .ToArray();
        }

        private static List<Sprite> FindAllSprites()
        {
            List<Sprite> sprites = new();

            string[] guids = AssetDatabase.FindAssets(
                "t:Texture2D",
                new[] { ArtFolder }
            );

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                foreach (UnityEngine.Object asset in
                         AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    if (asset is Sprite sprite)
                    {
                        sprites.Add(sprite);
                    }
                }
            }

            return sprites;
        }

        private static bool TryGetFrameIndex(
            string spriteName,
            string[] acceptedPrefixes,
            out int index)
        {
            foreach (string prefix in acceptedPrefixes)
            {
                if (!spriteName.StartsWith(prefix, StringComparison.Ordinal))
                {
                    continue;
                }

                string suffix = spriteName.Substring(prefix.Length);

                if (int.TryParse(suffix, out index))
                {
                    return true;
                }
            }

            index = -1;
            return false;
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;

            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private readonly struct SpriteEntry
        {
            public SpriteEntry(int index, Sprite sprite)
            {
                Index = index;
                Sprite = sprite;
            }

            public int Index { get; }
            public Sprite Sprite { get; }
        }
    }
}

#endif
