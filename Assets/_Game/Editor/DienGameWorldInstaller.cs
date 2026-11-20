#if UNITY_EDITOR
using System.Collections.Generic;
using DienGame.Client;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DienGame.EditorTools
{
    /// <summary>
    /// One-click: imports thematic sprites, rebinds the existing illustrated lobby,
    /// makes sure all three classes have usable gameplay scenes, enables them in builds.
    /// No hand-placed gameobjects or Inspector setup is necessary.
    /// </summary>
    public static class DienGameWorldInstaller
    {
        private const string Root="Assets/_Game/Resources/World";
        private const string Isles="Assets/_Game/Art/Client/Islands";
        private static readonly string[] Scenes =
        {
            "Assets/_Game/Scenes/Lobby.unity",
            "Assets/_Game/Scenes/GameplayTest.unity",
            "Assets/_Game/Scenes/GameplayTest_AssassinPreview.unity",
            "Assets/_Game/Scenes/GameplayTest_Mage.unity",
        };

        [MenuItem("DienGame/World/1 - Install ALL (5 Islands + Items + Lobby)")]
        public static void InstallAll()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("DienGame World","Thoát Play Mode trước khi cài.","OK");
                return;
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            string[] guids=AssetDatabase.FindAssets("t:Texture2D",new[]{Root,Isles});
            int imported=0;
            foreach(string guid in guids)
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);
                if(!path.EndsWith(".png",System.StringComparison.OrdinalIgnoreCase))continue;
                TextureImporter imp=AssetImporter.GetAtPath(path) as TextureImporter;
                if(imp==null) continue;
                bool world=path.StartsWith(Root);
                imp.textureType=TextureImporterType.Sprite;
                imp.spriteImportMode=SpriteImportMode.Single;
                // Mesh Type lives on TextureImporterSettings, not TextureImporter.
                TextureImporterSettings spriteSettings = new TextureImporterSettings();
                imp.ReadTextureSettings(spriteSettings);
                spriteSettings.spriteMeshType = SpriteMeshType.FullRect;
                imp.SetTextureSettings(spriteSettings);
                imp.spritePixelsPerUnit=100f;
                imp.alphaIsTransparency=true;
                imp.mipmapEnabled=false;
                imp.isReadable=world;
                imp.maxTextureSize=2048;
                imp.textureCompression=TextureImporterCompression.Uncompressed;
                imp.npotScale=TextureImporterNPOTScale.None;
                imp.filterMode=FilterMode.Bilinear;
                if(path.Contains("/World/Stages/"))imp.wrapMode=TextureWrapMode.Repeat;
                imp.SaveAndReimport();imported++;
            }
            // Existing Mage scene is generated rather than duplicated manually.
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Scenes[3])==null)
            {
                Debug.Log("DienGame World: Mage scene missing, invoking Mage installer.");
                MageSceneInstaller.Install();
            }
            LobbySceneBuilder.RebuildLobbyScene();
            EnsureScenesInBuild();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("DienGame World",
                "Đã cài bản đồ 5 đảo, UI client, đồ rơi và trang bị.\n"+
                "Các đảo 2–5 mở sẵn để test.\n"+
                "Mở Lobby > Play > chọn đảo > VÀO TRẬN.\n\n"+
                "Map ingame được dựng theo stage bằng code khi scene chạy; không cần tạo 15 scene riêng.","OK");
            Debug.Log("DienGame World: Installed. Sprites imported: "+imported);
        }

        private static void EnsureScenesInBuild()
        {
            var newScenes=new List<EditorBuildSettingsScene>();
            foreach(string path in Scenes)
            {
                if(AssetDatabase.LoadAssetAtPath<SceneAsset>(path)!=null)
                    newScenes.Add(new EditorBuildSettingsScene(path,true));
                else Debug.LogWarning("World installer: scene missing: "+path);
            }
            foreach(EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                bool duplicate=false;
                foreach(EditorBuildSettingsScene wanted in newScenes)
                    if(scene.path==wanted.path) duplicate=true;
                if(!duplicate)newScenes.Add(scene);
            }
            EditorBuildSettings.scenes=newScenes.ToArray();
        }

        [MenuItem("DienGame/World/2 - Open Lobby")]
        public static void OpenLobby()
        {
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            EditorSceneManager.OpenScene(Scenes[0],UnityEditor.SceneManagement.OpenSceneMode.Single);
        }
    }
}
#endif
