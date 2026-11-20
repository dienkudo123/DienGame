using System;
using System.IO;
using DienGame.Gameplay.Phase13;
using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    public sealed class DienGameAudioSettingsEditor : EditorWindow
    {
        private const string AudioFolder = "Assets/_Game/Resources/Phase13Audio";
        private static readonly string[] AudioExtensions = { ".ogg", ".wav", ".mp3" };
        private static readonly string[] SfxNames =
        {
            "Shot", "Slash", "Hit", "PlayerHit", "LevelUp", "Boss", "Victory", "Defeat"
        };

        [MenuItem("DienGame/Audio/Background Music and SFX")]
        public static void Open() => GetWindow<DienGameAudioSettingsEditor>("DienGame Audio").Show();

        private static AudioClip FindImportedClip(string name, out string path, out int count, out bool physicalFileFound)
        {
            AudioClip clip = null;
            path = null;
            count = 0;
            physicalFileFound = false;
            foreach (string ext in AudioExtensions)
            {
                string candidate = AudioFolder + "/" + name + ext;
                if (!File.Exists(candidate)) continue;
                physicalFileFound = true;
                count++;
                if (path == null) path = candidate;
                AudioClip imported = AssetDatabase.LoadAssetAtPath<AudioClip>(candidate);
                if (clip == null && imported != null) clip = imported;
            }
            return clip;
        }

        private static void ReimportAudio()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            string[] names = new string[SfxNames.Length + 1];
            names[0] = "BGM";
            Array.Copy(SfxNames, 0, names, 1, SfxNames.Length);
            foreach (string name in names)
            {
                foreach (string ext in AudioExtensions)
                {
                    string assetPath = AudioFolder + "/" + name + ext;
                    if (File.Exists(assetPath))
                        AssetDatabase.ImportAsset(assetPath,
                            ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
                }
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log("[DienGame Audio] Audio import refreshed. Press Play, then use Retry BGM playback if necessary.");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("DIENGAME AUDIO", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "BGM plays only in PLAY MODE. This panel checks the imported asset with AssetDatabase " +
                "rather than relying only on Resources.Load in Edit Mode.", MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Background music", EditorStyles.boldLabel);
            string bgmPath;
            int bgmCount;
            bool bgmFileFound;
            AudioClip bgm = FindImportedClip("BGM", out bgmPath, out bgmCount, out bgmFileFound);
            EditorGUILayout.LabelField("BGM", bgm != null ? "IMPORTED: " + bgm.name :
                (bgmFileFound ? "FILE EXISTS - AUDIO IMPORT FAILED" : "FILE MISSING"));
            if (bgmCount > 1)
                EditorGUILayout.HelpBox("More than one BGM file is present. Keep only ONE BGM.ogg/.mp3/.wav.", MessageType.Warning);
            if (bgm == null)
                EditorGUILayout.HelpBox("File may be absent, named differently or not imported as AudioClip. " +
                    "Use Reimport audio files and inspect the AudioImporter in Inspector.", MessageType.Warning);

            EditorGUI.BeginChangeCheck();
            float volume = EditorGUILayout.Slider("Music volume", Phase13BackgroundMusic.Volume, 0f, 1f);
            bool mute = EditorGUILayout.Toggle("Mute music", Phase13BackgroundMusic.IsMuted);
            if (EditorGUI.EndChangeCheck())
            {
                Phase13BackgroundMusic.SetVolume(volume);
                Phase13BackgroundMusic.SetMuted(mute);
            }

            if (Application.isPlaying)
            {
                AudioSource src = Phase13BackgroundMusic.CurrentSource;
                EditorGUILayout.LabelField("Runtime AudioSource", src == null ? "NOT CREATED" :
                    (src.clip == null ? "NO CLIP" :
                    (src.isPlaying ? "PLAYING: " + src.clip.name : "STOPPED: " + src.clip.name)));
                if (GUILayout.Button("Retry BGM playback (Play Mode)"))
                    Phase13BackgroundMusic.RetryPlayback();
            }
            else
            {
                EditorGUILayout.LabelField("Playback", "Press PLAY to hear music");
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Combat sound effects", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            float sfxVolume = EditorGUILayout.Slider("SFX volume", Phase13Audio.Volume, 0f, 1f);
            if (EditorGUI.EndChangeCheck()) Phase13Audio.SetVolume(sfxVolume);
            foreach (string name in SfxNames)
            {
                string path;
                int count;
                bool fileFound;
                AudioClip clip = FindImportedClip(name, out path, out count, out fileFound);
                EditorGUILayout.LabelField(name, clip != null ? "IMPORTED" :
                    (fileFound ? "IMPORT FAILED" : "MISSING FILE"));
                if (count > 1)
                    EditorGUILayout.HelpBox(name + " has duplicate formats. Keep only one.", MessageType.Warning);
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Reimport audio files")) ReimportAudio();
            if (GUILayout.Button("Find audio folder in Project"))
            {
                UnityEngine.Object folder = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AudioFolder);
                if (folder != null)
                {
                    Selection.activeObject = folder;
                    EditorGUIUtility.PingObject(folder);
                }
                else EditorUtility.DisplayDialog("Missing folder", AudioFolder, "OK");
            }
            EditorGUILayout.HelpBox(
                "For a replacement song, keep one BGM.ogg (or BGM.wav/.mp3) in the folder. " +
                "Run Reimport audio files. This will not replace your song or SFX.", MessageType.None);
        }
    }
}
