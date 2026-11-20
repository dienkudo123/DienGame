#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    /// <summary>Static project structure checks. Never edits or deletes user assets.</summary>
    public static class DienGameAssetStructureAudit
    {
        [MenuItem("DienGame/Repair/Check Assets Structure")]
        public static void Check()
        {
            string root = Application.dataPath;
            int errors = 0;
            int warnings = 0;
            string[] obsolete = { "Assets", "FixedFiles", "ReferenceScripts", "Source" };
            foreach (string folder in obsolete)
            {
                if (!Directory.Exists(Path.Combine(root, folder))) continue;
                Debug.LogError("DienGame Assets: obsolete nested folder: Assets/" + folder);
                errors++;
            }
            string[] required =
            {
                "_Game/Scripts/Gameplay/Equipment/EquipmentCatalog.cs",
                "_Game/Scripts/Gameplay/Equipment/EquipmentInventory.cs",
                "_Game/Resources/World/IcePremium/IceArenaBackground.png",
                "_Game/Scripts/Gameplay/World/IceArenaPremiumRuntime.cs",
                "_Game/Scenes/Lobby.unity"
            };
            foreach (string file in required)
            {
                if (File.Exists(Path.Combine(root, file.Replace('/', Path.DirectorySeparatorChar)))) continue;
                Debug.LogError("DienGame Assets: missing file: Assets/" + file);
                errors++;
            }
            var unique = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string meta in Directory.GetFiles(root, "*.meta", SearchOption.AllDirectories))
            {
                string asset = meta.Substring(0, meta.Length - 5);
                if (!File.Exists(asset) && !Directory.Exists(asset))
                {
                    Debug.LogWarning("Orphan .meta: " + meta);
                    warnings++;
                }
                Match m = Regex.Match(File.ReadAllText(meta), @"(?m)^guid:\s*([0-9a-f]{32})");
                if (!m.Success) continue;
                string guid = m.Groups[1].Value;
                if (unique.TryGetValue(guid, out string previous))
                {
                    Debug.LogError("Duplicate GUID " + guid + ": " + previous + " | " + meta);
                    errors++;
                }
                else unique.Add(guid, meta);
            }
            Debug.Log($"DienGame Assets audit completed: {errors} errors, {warnings} warnings. This checks files only, not gameplay or Android builds.");
        }
    }
}
#endif
