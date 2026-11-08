using DienGame.Gameplay.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace DienGame.EditorTools
{
    public static class GameplayUIBuilder
    {
        private const string CanvasName =
            "GameplayCanvas";

        private const string RootName =
            "GameplayHUDRoot";

        private const string ArtFolder =
            "Assets/_Game/UI/Art/HUD";

        // =====================================================
        // KÍCH THƯỚC TOÀN BỘ HUD
        // =====================================================
        //
        // CHỈNH TO / NHỎ Ở ĐÂY
        //
        // 0.18f = nhỏ
        // 0.23f = hiện tại
        // 0.30f = lớn hơn
        // 0.40f = rất lớn
        //
        private const float HudScale =
            0.3f;

        // =====================================================
        // VỊ TRÍ TOÀN BỘ HUD
        // =====================================================

        private static readonly Vector2 HudPosition =
            new(20f, -20f);

        // =====================================================
        // TEXT POSITION
        // =====================================================

           private static readonly Vector2 ExpTextPosition =
            new(620f, -390f);

        private static readonly Vector2  HpTextPosition =
            new(310f, -650f);

        private static readonly Vector2 LevelTextPosition =
            new(881f, -960f);


        [MenuItem("DienGame/UI/Build HP EXP HUD")]
        private static void Build()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning(
                    "Thoát Play Mode trước khi build HUD."
                );

                return;
            }

            GameObject canvasObject =
                GameObject.Find(CanvasName);

            if (canvasObject == null)
            {
                Debug.LogError(
                    $"Không tìm thấy {CanvasName}."
                );

                return;
            }

            Sprite hpSprite =
                LoadSprite("HUD_HP");

            Sprite expSprite =
                LoadSprite("HUD_EXP");

            Sprite frameSprite =
                LoadSprite("HUD_Frame");

            Sprite levelSprite =
                LoadSprite("HUD_Level");

            if (
                hpSprite == null ||
                expSprite == null ||
                frameSprite == null ||
                levelSprite == null
            )
            {
                return;
            }

            if (!HaveSameSize(
                    hpSprite,
                    expSprite,
                    frameSprite,
                    levelSprite
                ))
            {
                Debug.LogError(
                    "4 Sprite HUD không cùng kích thước."
                );

                return;
            }

            RemoveOldHUD(
                canvasObject.transform
            );

            GameObject root =
                CreateUIObject(
                    RootName,
                    canvasObject.transform
                );

            RectTransform rootRect =
                root.GetComponent<RectTransform>();

            SetupTopLeft(
                rootRect
            );

            // Lấy đúng kích thước ảnh gốc
            rootRect.sizeDelta =
                frameSprite.rect.size;

            // Vị trí cả cụm
            rootRect.anchoredPosition =
                HudPosition;

            // KÍCH THƯỚC CẢ CỤM
            rootRect.localScale =
                Vector3.one * HudScale;

            GameplayHUD hud =
                root.AddComponent<GameplayHUD>();

            // =================================================
            // HP
            // =================================================

            Image hpImage =
                CreateFullLayer(
                    "HUD_HP",
                    root.transform,
                    hpSprite
                );

            // =================================================
            // EXP
            // =================================================

            Image expImage =
                CreateFullLayer(
                    "HUD_EXP",
                    root.transform,
                    expSprite
                );

            // =================================================
            // FRAME
            // =================================================

            CreateFullLayer(
                "HUD_Frame",
                root.transform,
                frameSprite
            );

            // =================================================
            // LEVEL FRAME
            // =================================================

            CreateFullLayer(
                "HUD_Level",
                root.transform,
                levelSprite
            );

            // =================================================
            // FILL
            // =================================================

            ConfigureVerticalFill(
                hpImage,
                1f
            );

            ConfigureVerticalFill(
                expImage,
                0f
            );

            // =================================================
            // HP TEXT
            // =================================================

            TMP_Text hpText =
                CreateText(
                    "HPText",
                    root.transform,
                    "100 / 100",
                    70f
                );

            SetTextRect(
                hpText.rectTransform,
                HpTextPosition,
                new Vector2(
                    320f,
                    70f
                )
            );

            // =================================================
            // EXP TEXT
            // =================================================

            TMP_Text expText =
                CreateText(
                    "EXPText",
                    root.transform,
                    "0 / 30",
                    70f
                );

            SetTextRect(
                expText.rectTransform,
                ExpTextPosition,
                new Vector2(
                    320f,
                    70f
                )
            );

            // =================================================
            // LEVEL TEXT
            // CHỈ HIỆN SỐ
            // =================================================

            TMP_Text levelText =
                CreateText(
                    "LevelText",
                    root.transform,
                    "1",
                    70f
                );

            SetTextRect(
                levelText.rectTransform,
                LevelTextPosition,
                new Vector2(
                    220f,
                    70f
                )
            );

            hud.Configure(
                hpImage,
                hpText,
                expImage,
                expText,
                levelText
            );

            EditorUtility.SetDirty(
                hud
            );

            EditorSceneManager.MarkSceneDirty(
                EditorSceneManager.GetActiveScene()
            );

            Selection.activeGameObject =
                root;

            Debug.Log(
                $"Build HUD hoàn tất | Scale = {HudScale}"
            );
        }

        private static Sprite LoadSprite(
            string fileName)
        {
            string path =
                $"{ArtFolder}/{fileName}.png";

            Sprite sprite =
                AssetDatabase.LoadAssetAtPath<Sprite>(
                    path
                );

            if (sprite == null)
            {
                Debug.LogError(
                    $"Không tìm thấy Sprite: {path}"
                );
            }

            return sprite;
        }

        private static bool HaveSameSize(
            params Sprite[] sprites)
        {
            if (
                sprites == null ||
                sprites.Length == 0
            )
            {
                return false;
            }

            Vector2 size =
                sprites[0].rect.size;

            foreach (Sprite sprite in sprites)
            {
                if (sprite == null)
                {
                    return false;
                }

                if (
                    sprite.rect.size != size
                )
                {
                    return false;
                }
            }

            return true;
        }

        private static void RemoveOldHUD(
            Transform canvas)
        {
            Transform oldHUD =
                canvas.Find(RootName);

            if (oldHUD == null)
            {
                return;
            }

            Object.DestroyImmediate(
                oldHUD.gameObject
            );
        }

        private static Image CreateFullLayer(
            string objectName,
            Transform parent,
            Sprite sprite)
        {
            GameObject obj =
                CreateUIObject(
                    objectName,
                    parent
                );

            RectTransform rect =
                obj.GetComponent<RectTransform>();

            rect.anchorMin =
                Vector2.zero;

            rect.anchorMax =
                Vector2.one;

            rect.offsetMin =
                Vector2.zero;

            rect.offsetMax =
                Vector2.zero;

            rect.localScale =
                Vector3.one;

            Image image =
                obj.AddComponent<Image>();

            image.sprite =
                sprite;

            image.color =
                Color.white;

            image.preserveAspect =
                false;

            image.raycastTarget =
                false;

            return image;
        }

        private static void ConfigureVerticalFill(
            Image image,
            float amount)
        {
            image.type =
                Image.Type.Filled;

            image.fillMethod =
                Image.FillMethod.Vertical;

            image.fillOrigin = 0;

            image.fillAmount =
                amount;

            image.raycastTarget =
                false;
        }

        private static TMP_Text CreateText(
            string objectName,
            Transform parent,
            string value,
            float fontSize)
        {
            GameObject obj =
                CreateUIObject(
                    objectName,
                    parent
                );

            TextMeshProUGUI text =
                obj.AddComponent<TextMeshProUGUI>();

            text.text =
                value;

            text.fontSize =
                fontSize;

            text.color =
                Color.white;

            text.alignment =
                TextAlignmentOptions.Center;

            text.raycastTarget =
                false;

            return text;
        }

        private static GameObject CreateUIObject(
            string objectName,
            Transform parent)
        {
            GameObject obj =
                new GameObject(
                    objectName,
                    typeof(RectTransform)
                );

            obj.transform.SetParent(
                parent,
                false
            );

            return obj;
        }

        private static void SetupTopLeft(
            RectTransform rect)
        {
            rect.anchorMin =
                new Vector2(
                    0f,
                    1f
                );

            rect.anchorMax =
                new Vector2(
                    0f,
                    1f
                );

            rect.pivot =
                new Vector2(
                    0f,
                    1f
                );
        }

        private static void SetTextRect(
            RectTransform rect,
            Vector2 position,
            Vector2 size)
        {
            SetupTopLeft(
                rect
            );

            rect.anchoredPosition =
                position;

            rect.sizeDelta =
                size;
        }
    }
}