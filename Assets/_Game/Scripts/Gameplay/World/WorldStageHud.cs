using DienGame.Client;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DienGame.Gameplay.World
{
    /// <summary>Brief noninteractive stage-name plaque, leaving combat and boss HUD untouched.</summary>
    public sealed class WorldStageHud : MonoBehaviour
    {
        private CanvasGroup _group;
        private float _age;
        public static void Show(WorldStageTheme theme)
        {
            Canvas canvas = null;
            foreach (Canvas c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (c.name == "GameplayCanvas") { canvas = c; break; }
                if (canvas == null && c.renderMode != RenderMode.WorldSpace) canvas = c;
            }
            if (canvas == null) return;
            GameObject go = new GameObject("WorldStage_Intro", typeof(RectTransform),typeof(CanvasGroup),typeof(Image));
            go.transform.SetParent(canvas.transform,false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(.14f,.76f);rt.anchorMax = new Vector2(.86f,.83f);
            rt.offsetMin=Vector2.zero;rt.offsetMax=Vector2.zero;
            Image bg=go.GetComponent<Image>();
            bg.color=new Color(.015f,.05f,.105f,.88f);
            bg.raycastTarget=false;
            DienGameUITheme.Skin(bg,"Header");
            GameObject textObj=new GameObject("StageTitle",typeof(RectTransform),typeof(TextMeshProUGUI));
            textObj.transform.SetParent(go.transform,false);
            RectTransform txt=(RectTransform)textObj.transform;
            txt.anchorMin=Vector2.zero;txt.anchorMax=Vector2.one;txt.offsetMin=Vector2.zero;txt.offsetMax=Vector2.zero;
            TextMeshProUGUI tmp=textObj.GetComponent<TextMeshProUGUI>();
            tmp.text=$"ĐẢO {theme.Stage}  •  {theme.Name}";
            DienGameUITheme.StyleText(tmp, 36f, theme.Accent, true, true, TextAlignmentOptions.Center);
            tmp.margin = new Vector4(12f, 6f, 12f, 6f);
            go.AddComponent<WorldStageHud>();
        }
        private void Awake() { _group=GetComponent<CanvasGroup>(); }
        private void Update()
        {
            _age+=Time.unscaledDeltaTime;
            if (_age>2.1f) _group.alpha=Mathf.Clamp01(1f-(_age-2.1f)/.9f);
            if (_age>3f) Destroy(gameObject);
        }
    }
}
