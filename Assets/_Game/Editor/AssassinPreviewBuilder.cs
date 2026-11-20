using System;
using System.IO;
using System.Linq;
using DienGame.Gameplay.Player;
using DienGame.Gameplay.Skills.Assassin;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DienGame.EditorTools
{
    public static class AssassinPreviewBuilder
    {
        private const string ArtFolder = "Assets/_Game/Art/Characters/Player/Assassin8Dir";
        private const string DataFolder = "Assets/_Game/Animations/Player/Assassin8Dir";
        private const string DataPath = DataFolder + "/AssassinSpriteData.asset";
        private const string PreviewScenePath = "Assets/_Game/Scenes/GameplayTest_AssassinPreview.unity";

        // The generated sheets are top-to-bottom: Up, UpRight, Right, DownRight,
        // Down, DownLeft, Left, UpLeft. Existing PlayerAnimator uses another order.
        private static readonly int[] SourceRowToDirection = { 4, 5, 6, 7, 0, 1, 2, 3 };

        [MenuItem("DienGame/Assassin/1 - Build 8-Direction Sprite Data")]
        public static void BuildSpriteData()
        {
            string runPath = ArtFolder + "/Run8Dir.png";
            string slashPath = ArtFolder + "/Slash8Dir.png";

            if (!File.Exists(runPath) || !File.Exists(slashPath))
            {
                Debug.LogError("Assassin: missing Run8Dir.png or Slash8Dir.png at " + ArtFolder);
                return;
            }

            Sprite[] runSprites = ImportSheet(runPath);
            Sprite[] slashSprites = ImportSheet(slashPath);
            if (runSprites == null || slashSprites == null)
                return;

            EnsureFolder(DataFolder);
            AssassinSpriteAnimationData data = AssetDatabase.LoadAssetAtPath<AssassinSpriteAnimationData>(DataPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<AssassinSpriteAnimationData>();
                AssetDatabase.CreateAsset(data, DataPath);
            }

            for (int row = 0; row < 8; row++)
            {
                Sprite[] run = new Sprite[4];
                Sprite[] slash = new Sprite[4];
                for (int column = 0; column < 4; column++)
                {
                    run[column] = runSprites[row * 4 + column];
                    slash[column] = slashSprites[row * 4 + column];
                }

                data.SetDirection(SourceRowToDirection[row], run, slash);
            }

            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            Selection.activeObject = data;
            Debug.Log("Assassin: built sprite data (8 directions x 4 Run frames + 4 Slash frames). Source animation is preserved.");
        }

        [MenuItem("DienGame/Assassin/2 - Create Separate Assassin Preview Scene")]
        public static void CreatePreviewScene()
        {
            Scene active = SceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(active.path))
            {
                Debug.LogError("Assassin: save and open GameplayTest.unity first.");
                return;
            }

            if (!active.path.EndsWith("/GameplayTest.unity", StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError("Assassin: open GameplayTest.unity (the original) before running this menu. No changes made.");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(PreviewScenePath) != null)
            {
                Debug.LogWarning("Assassin preview scene already exists: " + PreviewScenePath + ". Delete/rename that scene if you wish to rebuild.");
                return;
            }

            AssassinSpriteAnimationData data = AssetDatabase.LoadAssetAtPath<AssassinSpriteAnimationData>(DataPath);
            if (data == null)
            {
                Debug.LogError("Assassin: run menu 1 to build sprite data first.");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            if (!AssetDatabase.CopyAsset(active.path, PreviewScenePath))
            {
                Debug.LogError("Assassin: failed to duplicate GameplayTest scene.");
                return;
            }

            AssetDatabase.Refresh();
            Scene preview = EditorSceneManager.OpenScene(PreviewScenePath, OpenSceneMode.Single);
            PlayerMovement player = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();
            if (player == null)
            {
                Debug.LogError("Assassin preview: PlayerMovement not found; copied scene remains unchanged.");
                return;
            }

            GameObject root = player.gameObject;
            PlayerAutoAttack gunAttack = root.GetComponent<PlayerAutoAttack>();
            if (gunAttack != null) gunAttack.enabled = false;

            // Keep all common Player scripts. Disable only Marksman-specific rendering and attacks.
            PlayerAnimator marksmanAnimator = root.GetComponentInChildren<PlayerAnimator>(true);
            if (marksmanAnimator != null) marksmanAnimator.enabled = false;

            AssassinMeleeAttack attack = root.GetComponent<AssassinMeleeAttack>();
            if (attack == null) attack = root.AddComponent<AssassinMeleeAttack>();

            AssassinCrescentVfx vfx = root.GetComponent<AssassinCrescentVfx>();
            if (vfx == null) vfx = root.AddComponent<AssassinCrescentVfx>();

            SpriteRenderer visual = marksmanAnimator != null
                ? marksmanAnimator.GetComponent<SpriteRenderer>()
                : root.GetComponentInChildren<SpriteRenderer>(true);

            if (visual == null)
            {
                Debug.LogError("Assassin preview: SpriteRenderer visual not found.");
                return;
            }

            AssassinSpriteAnimator assassinAnimator = visual.GetComponent<AssassinSpriteAnimator>();
            if (assassinAnimator == null) assassinAnimator = visual.gameObject.AddComponent<AssassinSpriteAnimator>();
            assassinAnimator.SetAnimationData(data);

            EditorUtility.SetDirty(root);
            EditorUtility.SetDirty(visual.gameObject);
            EditorSceneManager.MarkSceneDirty(preview);
            EditorSceneManager.SaveScene(preview);
            Debug.Log("Assassin preview created: " + PreviewScenePath +
                      ". IMPORTANT: LevelUpController/UpgradePool remain Marksman-only and are not yet adapted.");
        }

        private static Sprite[] ImportSheet(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError("Assassin: cannot load TextureImporter at " + path);
                return null;
            }

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null) return null;

            int width = texture.width;
            int height = texture.height;
            var metadata = new SpriteMetaData[32];
            int index = 0;

            for (int rowTop = 0; rowTop < 8; rowTop++)
            {
                int bottom = Mathf.RoundToInt((7 - rowTop) * height / 8f);
                int top = Mathf.RoundToInt((8 - rowTop) * height / 8f);
                for (int column = 0; column < 4; column++)
                {
                    int left = Mathf.RoundToInt(column * width / 4f);
                    int right = Mathf.RoundToInt((column + 1) * width / 4f);
                    metadata[index++] = new SpriteMetaData
                    {
                        name = "r" + rowTop + "c" + column,
                        rect = new Rect(left, bottom, right - left, top - bottom),
                        alignment = (int)SpriteAlignment.Custom,
                        pivot = new Vector2(0.5f, 0.14f)
                    };
                }
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 40f;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;
#pragma warning disable 0618
            importer.spritesheet = metadata;
#pragma warning restore 0618
            importer.SaveAndReimport();

            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<Sprite>()
                .OrderByDescending(sprite => sprite.rect.y)
                .ThenBy(sprite => sprite.rect.x)
                .ToArray();

            if (sprites.Length != 32)
            {
                Debug.LogError("Assassin: expected 32 sliced sprites at " + path + ", got " + sprites.Length);
                return null;
            }

            return sprites;
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
