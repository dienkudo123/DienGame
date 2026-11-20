using System;
using DienGame.Gameplay.Skills.Assassin;
using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    /// <summary>
    /// Auto-import VFX PNGs under Art, then create one lightweight Resources
    /// frame-reference asset (Sprites remain in Art; no duplicated textures).
    /// </summary>
    public sealed class AssassinBloodSlashSpriteImporter : AssetPostprocessor
    {
        private const string ArtFolder = "Assets/_Game/Art/Skills/Assassin/HuyetAnhTram/";
        private const string CollectionPath =
            "Assets/_Game/Resources/Assassin/HuyetAnhTram/Frames.asset";
        private const int PixelsPerUnit = 112;
        private const int FrameCount = 8;

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ArtFolder, StringComparison.OrdinalIgnoreCase))
                return;
            TextureImporter importer = assetImporter as TextureImporter;
            if (importer != null)
                Configure(importer);
        }

        [InitializeOnLoadMethod]
        private static void ScheduleBuildOnLoad()
        {
            EditorApplication.delayCall += Rebuild;
        }

        [MenuItem("DienGame/Assassin/Rebuild Huyet Anh Tram VFX")]
        public static void Rebuild()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += Rebuild;
                return;
            }

            Sprite[] frames = new Sprite[FrameCount];
            for (int i = 0; i < FrameCount; i++)
            {
                string spritePath = ArtFolder + "frame_" + i.ToString("00") + ".png";
                TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
                if (importer == null)
                {
                    Debug.LogWarning("Missing Huyet Anh Tram art: " + spritePath);
                    return;
                }
                if (!IsConfigured(importer))
                {
                    Configure(importer);
                    importer.SaveAndReimport();
                }
                frames[i] = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                if (frames[i] == null)
                {
                    Debug.LogWarning("Huyet Anh Tram PNG is not a Sprite: " + spritePath);
                    return;
                }
            }

            EnsureFolder("Assets/_Game/Resources");
            EnsureFolder("Assets/_Game/Resources/Assassin");
            EnsureFolder("Assets/_Game/Resources/Assassin/HuyetAnhTram");

            AssassinBloodSlashFrameCollection collection =
                AssetDatabase.LoadAssetAtPath<AssassinBloodSlashFrameCollection>(CollectionPath);
            bool newCollection = collection == null;
            if (newCollection)
            {
                collection = ScriptableObject.CreateInstance<AssassinBloodSlashFrameCollection>();
                AssetDatabase.CreateAsset(collection, CollectionPath);
            }

            SerializedObject serialized = new SerializedObject(collection);
            SerializedProperty array = serialized.FindProperty("frames");
            bool changed = newCollection || array.arraySize != FrameCount;
            array.arraySize = FrameCount;
            for (int i = 0; i < FrameCount; i++)
            {
                SerializedProperty item = array.GetArrayElementAtIndex(i);
                if (item.objectReferenceValue != frames[i])
                {
                    item.objectReferenceValue = frames[i];
                    changed = true;
                }
            }
            if (changed)
            {
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(collection);
                AssetDatabase.SaveAssets();
                Debug.Log("Huyet Anh Tram: 8 PNG sprites linked from Art/Skills/Assassin.");
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            int lastSeparator = path.LastIndexOf('/');
            AssetDatabase.CreateFolder(path.Substring(0, lastSeparator),
                path.Substring(lastSeparator + 1));
        }

        private static bool IsConfigured(TextureImporter importer)
        {
            return importer.textureType == TextureImporterType.Sprite &&
                   importer.spriteImportMode == SpriteImportMode.Single &&
                   Mathf.Approximately(importer.spritePixelsPerUnit, PixelsPerUnit) &&
                   importer.alphaIsTransparency &&
                   !importer.mipmapEnabled &&
                   importer.wrapMode == TextureWrapMode.Clamp;
        }

        private static void Configure(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.alphaIsTransparency = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.mipmapEnabled = false;
            importer.isReadable = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
        }
    }
}
