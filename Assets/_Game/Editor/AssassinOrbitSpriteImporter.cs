using System;
using DienGame.Gameplay.Skills.Assassin;
using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    /// <summary>
    /// Automatically imports art sprites and builds a runtime frame-reference asset.
    /// Does not move or duplicate PNGs from Art into Resources.
    /// </summary>
    public sealed class AssassinOrbitSpriteImporter : AssetPostprocessor
    {
        private const string ArtFolder = "Assets/_Game/Art/Skills/Assassin/DaoAnhHoThan/";
        private const string ResourceAsset =
            "Assets/_Game/Resources/Assassin/DaoAnhHoThan/Frames.asset";
        private const int Ppu = 128;
        private const int Count = 16;
        private static bool _scheduled;

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ArtFolder, StringComparison.OrdinalIgnoreCase)) return;
            TextureImporter importer = assetImporter as TextureImporter;
            if (importer != null) Configure(importer);
        }

        [InitializeOnLoadMethod]
        private static void OnLoad() => Schedule();

        private static void OnPostprocessAllAssets(string[] importedAssets,
            string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string path in importedAssets)
                if (path.StartsWith(ArtFolder, StringComparison.OrdinalIgnoreCase)) { Schedule(); return; }
            foreach (string path in movedAssets)
                if (path.StartsWith(ArtFolder, StringComparison.OrdinalIgnoreCase)) { Schedule(); return; }
        }

        private static void Schedule()
        {
            if (_scheduled) return;
            _scheduled = true;
            EditorApplication.delayCall += () =>
            {
                _scheduled = false;
                Rebuild();
            };
        }

        [MenuItem("DienGame/Assassin/Rebuild Dao Anh Ho Than VFX")]
        public static void Rebuild()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;

            Sprite[] frames = new Sprite[Count];
            for (int i = 0; i < Count; i++)
            {
                string path = ArtFolder + "frame_" + i.ToString("00") + ".png";
                frames[i] = LoadSprite(path);
                if (frames[i] == null) return;
            }
            Sprite blade = LoadSprite(ArtFolder + "blade.png");
            if (blade == null) return;

            EnsureFolder("Assets/_Game/Resources");
            EnsureFolder("Assets/_Game/Resources/Assassin");
            EnsureFolder("Assets/_Game/Resources/Assassin/DaoAnhHoThan");

            AssassinOrbitBladeFrames catalog =
                AssetDatabase.LoadAssetAtPath<AssassinOrbitBladeFrames>(ResourceAsset);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<AssassinOrbitBladeFrames>();
                AssetDatabase.CreateAsset(catalog, ResourceAsset);
            }
            SerializedObject so = new SerializedObject(catalog);
            SerializedProperty arr = so.FindProperty("frames");
            bool changed = arr.arraySize != Count;
            arr.arraySize = Count;
            for (int i = 0; i < Count; i++)
            {
                SerializedProperty slot = arr.GetArrayElementAtIndex(i);
                if (slot.objectReferenceValue != frames[i])
                {
                    slot.objectReferenceValue = frames[i];
                    changed = true;
                }
            }
            SerializedProperty bladeSlot = so.FindProperty("blade");
            if (bladeSlot.objectReferenceValue != blade)
            {
                bladeSlot.objectReferenceValue = blade;
                changed = true;
            }
            if (changed)
            {
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(catalog);
                AssetDatabase.SaveAssets();
                Debug.Log("Dao Anh Ho Than: linked 16 orange VFX frames + blade from Art.");
            }
        }

        private static Sprite LoadSprite(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return null; // Do not create placeholder assets.
            if (!IsConfigured(importer))
            {
                Configure(importer);
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static bool IsConfigured(TextureImporter i) =>
            i.textureType == TextureImporterType.Sprite &&
            i.spriteImportMode == SpriteImportMode.Single &&
            Mathf.Approximately(i.spritePixelsPerUnit, Ppu) &&
            i.alphaIsTransparency && !i.mipmapEnabled &&
            i.wrapMode == TextureWrapMode.Clamp;

        private static void Configure(TextureImporter i)
        {
            i.textureType = TextureImporterType.Sprite;
            i.spriteImportMode = SpriteImportMode.Single;
            i.spritePixelsPerUnit = Ppu;
            i.alphaSource = TextureImporterAlphaSource.FromInput;
            i.alphaIsTransparency = true;
            i.mipmapEnabled = false;
            i.isReadable = false;
            i.filterMode = FilterMode.Bilinear;
            i.wrapMode = TextureWrapMode.Clamp;
            i.textureCompression = TextureImporterCompression.CompressedHQ;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
        }
    }
}
