#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DienGame.EditorTools
{
    /// <summary>
    /// Read-only UI audit of currently open modals + GameplayCanvas/LobbyCanvas.
    /// Checks rendered text, preferred line height, local bounds and missing joystick sprites.
    /// Writes a report that can be attached to a bug. Does not edit prefab/scene/PlayerPrefs.
    /// </summary>
    public static class DienGameUIBoundsAudit
    {
        [MenuItem("DienGame/UI/Check Visible Layout (Play Mode)")]
        public static void Check()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("DienGame UI QA",
                    "Vào Play Mode và mở màn cần kiểm tra, sau đó chạy lại menu này.", "OK");
                return;
            }

            var issues = new List<string>();
            int labels = 0, buttons = 0, icons = 0;
            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas canvas in canvases)
            {
                if (canvas == null || canvas.renderMode == RenderMode.WorldSpace ||
                    !canvas.isActiveAndEnabled) continue;
                Canvas.ForceUpdateCanvases();
                foreach (TMP_Text label in canvas.GetComponentsInChildren<TMP_Text>(true))
                {
                    if (label == null || !label.isActiveAndEnabled ||
                        string.IsNullOrWhiteSpace(label.text)) continue;
                    labels++;
                    RectTransform r = label.rectTransform;
                    if (r == null || r.rect.width <= 1f || r.rect.height <= 1f)
                    {
                        issues.Add("TEXT RECT ZERO: " + Path(label));
                        continue;
                    }
                    label.ForceMeshUpdate();
                    Vector2 drawn = label.GetRenderedValues(false);
                    if (drawn.x > r.rect.width + 3f || drawn.y > r.rect.height + 3f)
                        issues.Add($"TEXT RENDER OUTSIDE: {Path(label)} " +
                            $"drawn={drawn.x:0}x{drawn.y:0} rect={r.rect.width:0}x{r.rect.height:0}");
                    // AutoSize + Ellipsis can hide a title even when rendered bounds fit.
                    // Check preferred height at the effective font size, not original requested size.
                    float width = Mathf.Max(1f, r.rect.width - label.margin.x - label.margin.z);
                    Vector2 preferred = label.GetPreferredValues(label.text, width, 0f);
                    if (preferred.y > r.rect.height + 5f)
                        issues.Add($"TEXT CUT/TOO MANY LINES: {Path(label)} " +
                            $"preferredHeight={preferred.y:0}, rectHeight={r.rect.height:0}, text='{Compact(label.text)}'");
                    if (!IsWithinParent(r, 2f))
                        issues.Add("TEXT RECT OUTSIDE PARENT: " + Path(label));
                }
                foreach (Button button in canvas.GetComponentsInChildren<Button>(true))
                {
                    if (button == null || !button.isActiveAndEnabled) continue;
                    buttons++;
                    if (button.targetGraphic == null)
                        issues.Add("BUTTON MISSING GRAPHIC: " + Path(button));
                }
                foreach (Image image in canvas.GetComponentsInChildren<Image>(true))
                {
                    if (image == null || !image.isActiveAndEnabled) continue;
                    icons++;
                    string n = image.name;
                    bool isJoystick = n == "MovementJoystick" || n == "Handle" ||
                        n == "ChimeraJoystick_Right" || n == "Thumb";
                    if (isJoystick && image.sprite == null)
                        issues.Add("JOYSTICK IMAGE MISSING SPRITE: " + Path(image));
                }
            }
            StringBuilder report = new StringBuilder();
            report.AppendLine("DienGame UI v7 — layout QA — " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            report.AppendLine($"Resolution: {Screen.width}x{Screen.height}  SafeArea: {Screen.safeArea}");
            report.AppendLine($"Active labels: {labels}; Buttons: {buttons}; Images: {icons}; Issues: {issues.Count}");
            foreach (string issue in issues)
            {
                report.AppendLine(issue);
                Debug.LogWarning("DienGame UI QA: " + issue);
            }
            string path = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,
                "../DienGame_UI_V7_QA_Report.txt"));
            File.WriteAllText(path, report.ToString(), Encoding.UTF8);
            Debug.Log($"DienGame UI QA: {labels} labels, {buttons} buttons, {issues.Count} problems. " +
                "Saved report to " + path);
        }

        [MenuItem("DienGame/UI/Capture Game View (Play Mode)")]
        public static void Capture()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("DienGame UI QA", "Bật Play Mode trước.", "OK");
                return;
            }
            string path = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,
                "../DienGame_UI_V7_GameView.png"));
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log("DienGame UI screenshot requested: " + path);
        }

        private static bool IsWithinParent(RectTransform r, float epsilon)
        {
            RectTransform parent = r.parent as RectTransform;
            if (parent == null) return true;
            Vector3[] corners = new Vector3[4];
            r.GetWorldCorners(corners);
            Rect pr = parent.rect;
            for (int i = 0; i < corners.Length; i++)
            {
                Vector3 p = parent.InverseTransformPoint(corners[i]);
                if (p.x < pr.xMin - epsilon || p.x > pr.xMax + epsilon ||
                    p.y < pr.yMin - epsilon || p.y > pr.yMax + epsilon)
                    return false;
            }
            return true;
        }
        private static string Compact(string s)
        {
            s = (s ?? "").Replace('\n', ' ');
            return s.Length <= 90 ? s : s.Substring(0, 90) + "...";
        }
        private static string Path(Component c)
        {
            string result = c.name;
            Transform p = c.transform.parent;
            while (p != null) { result = p.name + "/" + result; p = p.parent; }
            return result;
        }
    }
}
#endif
