using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DienGame.EditorTools
{
    public static class PlayerRunAnimationGenerator
    {
        // =========================================================
        // CẤU HÌNH
        // =========================================================

        private const string SourceFolder =
            "Assets/_Game/Art/Characters/Player";

        private const string AnimationFolder =
            "Assets/_Game/Animations/Player";

        private const string ControllerPath =
            AnimationFolder + "/Player.controller";

        // FPS của 4 frame chạy.
        // Muốn chạy nhanh/chậm chỉnh ở đây.
        private const float RunFrameRate = 4f;

        private static readonly DirectionConfig[] Directions =
        {
            new(
                "Run_Down",
                "Idle_Down",
                "Run_Down"
            ),

            new(
                "Run_Left",
                "Idle_Left",
                "Run_Left"
            ),

            new(
                "Run_Right",
                "Idle_Right",
                "Run_Right"
            ),

            new(
                "Run_Up",
                "Idle_Up",
                "Run_Up"
            ),
        };

        [MenuItem("DienGame/Animation/Build Player Animations")]
        private static void Build()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning(
                    "Thoát Play Mode trước khi tạo animation."
                );

                return;
            }

            EnsureAnimationFolder();

            Dictionary<string, AnimationClip> clips =
                new();

            foreach (DirectionConfig direction in Directions)
            {
                Sprite[] frames =
                    LoadFrames(direction.SheetName);

                if (frames == null)
                {
                    return;
                }

                if (frames.Length != 5)
                {
                    Debug.LogError(
                        $"{direction.SheetName} phải có đúng 5 sprite. " +
                        $"Hiện tại tìm thấy {frames.Length}."
                    );

                    return;
                }

                // Frame 1 = Idle
                AnimationClip idleClip =
                    CreateIdleClip(
                        direction.IdleClipName,
                        frames[0]
                    );

                // Frame 2 -> 5 = Run
                AnimationClip runClip =
                    CreateRunClip(
                        direction.RunClipName,
                        frames.Skip(1).ToArray()
                    );

                clips.Add(
                    direction.IdleClipName,
                    idleClip
                );

                clips.Add(
                    direction.RunClipName,
                    runClip
                );
            }

            BuildController(clips);

            RemoveLegacyAnimations();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "Player Animation hoàn tất: " +
                "4 Idle + 4 Run."
            );
        }

        // =========================================================
        // LOAD 5 FRAME TỪ SPRITE SHEET
        // =========================================================

        private static Sprite[] LoadFrames(
            string sheetName)
        {
            string texturePath =
                FindTexturePath(sheetName);

            if (string.IsNullOrEmpty(texturePath))
            {
                Debug.LogError(
                    $"Không tìm thấy ảnh '{sheetName}' " +
                    $"trong {SourceFolder}."
                );

                return null;
            }

            Sprite[] sprites =
                AssetDatabase
                    .LoadAllAssetsAtPath(texturePath)
                    .OfType<Sprite>()
                    .OrderBy(sprite => sprite.rect.x)
                    .ThenByDescending(sprite => sprite.rect.y)
                    .ToArray();

            if (sprites.Length == 0)
            {
                Debug.LogError(
                    $"{sheetName} chưa được Slice thành Sprite Multiple."
                );

                return null;
            }

            return sprites;
        }

        private static string FindTexturePath(
            string sheetName)
        {
            string[] guids =
                AssetDatabase.FindAssets(
                    "t:Texture2D",
                    new[]
                    {
                        SourceFolder,
                    }
                );

            foreach (string guid in guids)
            {
                string path =
                    AssetDatabase.GUIDToAssetPath(guid);

                string fileName =
                    Path.GetFileNameWithoutExtension(path);

                if (
                    fileName.Equals(
                        sheetName,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return path;
                }
            }

            return null;
        }

        // =========================================================
        // IDLE
        // =========================================================

        private static AnimationClip CreateIdleClip(
            string clipName,
            Sprite idleSprite)
        {
            string path =
                $"{AnimationFolder}/{clipName}.anim";

            AnimationClip clip =
                GetOrCreateClip(
                    path,
                    clipName
                );

            clip.frameRate = 1f;

            EditorCurveBinding binding =
                CreateSpriteBinding();

            ObjectReferenceKeyframe[] keyframes =
            {
                new()
                {
                    time = 0f,
                    value = idleSprite,
                },
            };

            AnimationUtility.SetObjectReferenceCurve(
                clip,
                binding,
                keyframes
            );

            SetLoop(
                clip,
                true
            );

            EditorUtility.SetDirty(clip);

            return clip;
        }

        // =========================================================
        // RUN
        // =========================================================

        private static AnimationClip CreateRunClip(
            string clipName,
            Sprite[] runFrames)
        {
            string path =
                $"{AnimationFolder}/{clipName}.anim";

            AnimationClip clip =
                GetOrCreateClip(
                    path,
                    clipName
                );

            clip.frameRate =
                RunFrameRate;

            ObjectReferenceKeyframe[] keyframes =
                new ObjectReferenceKeyframe[
                    runFrames.Length
                ];

            for (
                int i = 0;
                i < runFrames.Length;
                i++
            )
            {
                keyframes[i] =
                    new ObjectReferenceKeyframe
                    {
                        time =
                            i / RunFrameRate,

                        value =
                            runFrames[i],
                    };
            }

            AnimationUtility.SetObjectReferenceCurve(
                clip,
                CreateSpriteBinding(),
                keyframes
            );

            SetLoop(
                clip,
                true
            );

            EditorUtility.SetDirty(clip);

            return clip;
        }

        private static AnimationClip GetOrCreateClip(
            string path,
            string clipName)
        {
            AnimationClip clip =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(
                    path
                );

            if (clip != null)
            {
                clip.name =
                    clipName;

                return clip;
            }

            clip =
                new AnimationClip
                {
                    name = clipName,
                };

            AssetDatabase.CreateAsset(
                clip,
                path
            );

            return clip;
        }

        private static EditorCurveBinding CreateSpriteBinding()
        {
            return new EditorCurveBinding
            {
                path = "",
                type = typeof(SpriteRenderer),
                propertyName = "m_Sprite",
            };
        }

        private static void SetLoop(
            AnimationClip clip,
            bool loop)
        {
            SerializedObject serializedClip =
                new(clip);

            SerializedProperty settings =
                serializedClip.FindProperty(
                    "m_AnimationClipSettings"
                );

            if (settings == null)
            {
                return;
            }

            SerializedProperty loopTime =
                settings.FindPropertyRelative(
                    "m_LoopTime"
                );

            if (loopTime == null)
            {
                return;
            }

            loopTime.boolValue =
                loop;

            serializedClip.ApplyModifiedProperties();
        }

        // =========================================================
        // ANIMATOR CONTROLLER
        // =========================================================

        private static void BuildController(
            IReadOnlyDictionary<string, AnimationClip> clips)
        {
            AnimatorController controller =
                AssetDatabase.LoadAssetAtPath<AnimatorController>(
                    ControllerPath
                );

            if (controller == null)
            {
                controller =
                    AnimatorController
                        .CreateAnimatorControllerAtPath(
                            ControllerPath
                        );
            }

            AnimatorStateMachine stateMachine =
                controller.layers[0]
                    .stateMachine;

            // Xóa state cũ nhưng KHÔNG xóa controller.
            // Như vậy Animator trên Player không mất reference.
            ChildAnimatorState[] oldStates =
                stateMachine.states;

            foreach (
                ChildAnimatorState childState
                in oldStates
            )
            {
                stateMachine.RemoveState(
                    childState.state
                );
            }

            AnimatorState idleDown =
                AddState(
                    stateMachine,
                    "Idle_Down",
                    clips["Idle_Down"],
                    new Vector3(100f, 50f)
                );

            AddState(
                stateMachine,
                "Run_Down",
                clips["Run_Down"],
                new Vector3(350f, 50f)
            );

            AddState(
                stateMachine,
                "Idle_Left",
                clips["Idle_Left"],
                new Vector3(100f, 150f)
            );

            AddState(
                stateMachine,
                "Run_Left",
                clips["Run_Left"],
                new Vector3(350f, 150f)
            );

            AddState(
                stateMachine,
                "Idle_Right",
                clips["Idle_Right"],
                new Vector3(100f, 250f)
            );

            AddState(
                stateMachine,
                "Run_Right",
                clips["Run_Right"],
                new Vector3(350f, 250f)
            );

            AddState(
                stateMachine,
                "Idle_Up",
                clips["Idle_Up"],
                new Vector3(100f, 350f)
            );

            AddState(
                stateMachine,
                "Run_Up",
                clips["Run_Up"],
                new Vector3(350f, 350f)
            );

            stateMachine.defaultState =
                idleDown;

            EditorUtility.SetDirty(
                controller
            );
        }

        private static AnimatorState AddState(
            AnimatorStateMachine stateMachine,
            string stateName,
            AnimationClip clip,
            Vector3 position)
        {
            AnimatorState state =
                stateMachine.AddState(
                    stateName,
                    position
                );

            state.motion =
                clip;

            state.speed =
                1f;

            return state;
        }

        // =========================================================
        // CLEANUP ANIMATION 8 HƯỚNG CŨ
        // =========================================================

        private static void RemoveLegacyAnimations()
        {
            string[] oldClips =
            {
                "Run_DownLeft.anim",
                "Run_DownRight.anim",
                "Run_UpLeft.anim",
                "Run_UpRight.anim",

                "Idle_DownLeft.anim",
                "Idle_DownRight.anim",
                "Idle_UpLeft.anim",
                "Idle_UpRight.anim",
            };

            foreach (string fileName in oldClips)
            {
                string path =
                    $"{AnimationFolder}/{fileName}";

                if (
                    AssetDatabase.LoadAssetAtPath<AnimationClip>(
                        path
                    ) != null
                )
                {
                    AssetDatabase.DeleteAsset(
                        path
                    );
                }
            }
        }

        private static void EnsureAnimationFolder()
        {
            if (
                AssetDatabase.IsValidFolder(
                    AnimationFolder
                )
            )
            {
                return;
            }

            const string parent =
                "Assets/_Game/Animations";

            if (
                !AssetDatabase.IsValidFolder(parent)
            )
            {
                AssetDatabase.CreateFolder(
                    "Assets/_Game",
                    "Animations"
                );
            }

            AssetDatabase.CreateFolder(
                parent,
                "Player"
            );
        }

        private readonly struct DirectionConfig
        {
            public string SheetName { get; }
            public string IdleClipName { get; }
            public string RunClipName { get; }

            public DirectionConfig(
                string sheetName,
                string idleClipName,
                string runClipName)
            {
                SheetName =
                    sheetName;

                IdleClipName =
                    idleClipName;

                RunClipName =
                    runClipName;
            }
        }
    }
}