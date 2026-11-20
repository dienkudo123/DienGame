#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using DienGame.Client;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DienGame.EditorTools
{
    public static class LobbySceneBuilder
    {
        private const string ScenesFolder = "Assets/_Game/Scenes";
        private const string LobbyScenePath = ScenesFolder + "/Lobby.unity";
        private const string GameplayScenePath = ScenesFolder + "/GameplayTest.unity";

        private const string BackgroundPath = "Assets/_Game/Art/Client/Background/BG_WorldMap.png";
        private const string IslandsFolder = "Assets/_Game/Art/Client/Islands/";
        private const string UiFolder = "Assets/_Game/Art/Client/UI/";

        private static readonly string[] RequiredArtPaths =
        {
            BackgroundPath,
            IslandsFolder + "Island_01_DongBang.png",
            IslandsFolder + "Island_02_RungTham.png",
            IslandsFolder + "Island_03_TanTich.png",
            IslandsFolder + "Island_04_NuiLua.png",
            IslandsFolder + "Island_05_CuDiem.png",
            UiFolder + "Panel_StageInfo.png",
            UiFolder + "Button_Play.png",
            UiFolder + "Button_Locked.png",
            UiFolder + "Icon_Map.png",
            UiFolder + "Icon_Equipment.png",
            UiFolder + "Icon_Skills.png",
            UiFolder + "Icon_Mission.png",
            UiFolder + "Icon_Shop.png",
            UiFolder + "Icon_Coin.png",
            UiFolder + "Icon_Gem.png",
            UiFolder + "Star_Full.png",
            UiFolder + "Star_Empty.png",
        };

        [MenuItem("DienGame/Client/Rebuild Lobby Scene")]
        public static void RebuildLobbyScene()
        {
            // Important: import files synchronously BEFORE trying to load them.
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            if (!ValidateRequiredArt())
            {
                EditorUtility.DisplayDialog(
                    "DienGame - Thiếu Client Art",
                    "Lobby chưa được rebuild vì thiếu art. Xem Console để biết đúng file nào đang thiếu.\n\n" +
                    "Art phải nằm trong Assets/_Game/Art/Client/Background, Islands và UI.",
                    "OK");
                return;
            }

            EnsureScenesFolder();

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            CreateCamera();
            CreateLobbyCanvas(scene);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, LobbyScenePath);
            RebuildBuildSettings();

            AssetDatabase.SaveAssets();
            Debug.Log("DienGame Lobby: rebuild xong và đã bind art thật vào Lobby.unity.");
        }

        [MenuItem("DienGame/Client/Open Lobby Scene")]
        public static void OpenLobbyScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(LobbyScenePath) == null)
            {
                RebuildLobbyScene();
                return;
            }

            EditorSceneManager.OpenScene(LobbyScenePath, OpenSceneMode.Single);
        }

        private static bool ValidateRequiredArt()
        {
            bool ok = true;

            foreach (string path in RequiredArtPaths)
            {
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture == null)
                {
                    Debug.LogError($"LobbySceneBuilder: THIẾU ART -> {path}");
                    ok = false;
                    continue;
                }

                EnsureSpriteImporter(path);

                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite == null)
                {
                    Debug.LogError($"LobbySceneBuilder: file tồn tại nhưng chưa import được Sprite -> {path}");
                    ok = false;
                }
            }

            return ok;
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new("LobbyCamera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.03f, 0.045f, 1f);
            camera.orthographic = true;
            cameraObject.tag = "MainCamera";
        }

        private static void CreateLobbyCanvas(Scene scene)
        {
            GameObject canvasObject = new("LobbyCanvas");

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            LobbyUI lobbyUI = canvasObject.AddComponent<LobbyUI>();
            lobbyUI.ConfigureArt(
                LoadSprite(BackgroundPath),
                new[]
                {
                    LoadSprite(IslandsFolder + "Island_01_DongBang.png"),
                    LoadSprite(IslandsFolder + "Island_02_RungTham.png"),
                    LoadSprite(IslandsFolder + "Island_03_TanTich.png"),
                    LoadSprite(IslandsFolder + "Island_04_NuiLua.png"),
                    LoadSprite(IslandsFolder + "Island_05_CuDiem.png"),
                },
                LoadSprite(UiFolder + "Panel_StageInfo.png"),
                LoadSprite(UiFolder + "Button_Play.png"),
                LoadSprite(UiFolder + "Button_Locked.png"),
                LoadSprite(UiFolder + "Icon_Map.png"),
                LoadSprite(UiFolder + "Icon_Equipment.png"),
                LoadSprite(UiFolder + "Icon_Skills.png"),
                LoadSprite(UiFolder + "Icon_Mission.png"),
                LoadSprite(UiFolder + "Icon_Shop.png"),
                LoadSprite(UiFolder + "Icon_Coin.png"),
                LoadSprite(UiFolder + "Icon_Gem.png"),
                LoadSprite(UiFolder + "Star_Full.png"),
                LoadSprite(UiFolder + "Star_Empty.png"));

            // ConfigureArt is normal C# assignment; explicitly mark it dirty so Unity serializes references.
            EditorUtility.SetDirty(lobbyUI);
            EditorSceneManager.MarkSceneDirty(scene);
        }

        private static Sprite LoadSprite(string assetPath)
        {
            EnsureSpriteImporter(assetPath);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

            if (sprite == null)
            {
                Debug.LogError($"LobbySceneBuilder: không load được Sprite -> {assetPath}");
            }

            return sprite;
        }

        private static void EnsureSpriteImporter(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            bool changed = false;

            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }

            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }

            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                changed = true;
            }

            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
            else
            {
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            }
        }

        private static void RebuildBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes =
                EditorBuildSettings.scenes
                    .Where(entry =>
                        entry.path != LobbyScenePath &&
                        entry.path != GameplayScenePath)
                    .ToList();

            var ordered = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(LobbyScenePath, true),
            };

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(GameplayScenePath) != null)
            {
                ordered.Add(new EditorBuildSettingsScene(GameplayScenePath, true));
            }

            ordered.AddRange(scenes);
            EditorBuildSettings.scenes = ordered.ToArray();
        }

        private static void EnsureScenesFolder()
        {
            if (AssetDatabase.IsValidFolder(ScenesFolder))
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder("Assets/_Game"))
            {
                AssetDatabase.CreateFolder("Assets", "_Game");
            }

            AssetDatabase.CreateFolder("Assets/_Game", "Scenes");
        }
    }
}

#endif
