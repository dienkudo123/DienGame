#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using DienGame.Gameplay.Map;
using DienGame.Gameplay.Player;
using DienGame.Gameplay.World;
using UnityEditor;
using UnityEngine;

namespace DienGame.EditorTools
{
    /// <summary>
    /// Edit the collision polygons of the five painted maps in the Scene view.
    /// The generated preview is temporary. Only the StageXX_Collision.json files
    /// are saved, and ApprovedArenaMapRuntime reads those files in Play Mode.
    /// </summary>
    public sealed class ApprovedMapManualColliderEditor : EditorWindow
    {
        private const string PreviewName = "DienGame_COLLIDER_EDIT_PREVIEW_NOT_SAVED";
        private const string AssetDirectory = "Assets/_Game/Resources/World/ApprovedMaps/";
        private static readonly string[] StageNames =
        {
            "01 - Lang Phu Sa", "02 - Rung Thien Moc", "03 - Co Tu Cat Vang",
            "04 - Hoa Thanh Long Mach", "05 - Cung Bang Van Thien"
        };

        [SerializeField] private int _stage = 1;
        [SerializeField] private bool _showNames = true;
        private GameObject _previewRoot;
        private RectangularMapBoundary _boundary;
        private ApprovedArenaMapRuntime.StageCollisionData _source;
        private Sprite _createdSprite;

        [MenuItem("DienGame/Maps/Manual Collision/01 - Open Collider Editor")]
        public static void OpenWindow()
        {
            GetWindow<ApprovedMapManualColliderEditor>("Map Collision Editor").Show();
        }

        [MenuItem("DienGame/Maps/Manual Collision/02 - Inspect Player Physics (Play Mode)")]
        public static void InspectPhysics()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("DienGame: Enter Play Mode to inspect player physics.");
                return;
            }

            PlayerMovement player = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();
            Rigidbody2D rigidbody = player != null ? player.GetComponent<Rigidbody2D>() : null;
            Collider2D hitbox = player != null ? player.GetComponent<Collider2D>() : null;
            int solids = 0;
            foreach (PolygonCollider2D polygon in UnityEngine.Object.FindObjectsByType<PolygonCollider2D>(FindObjectsSortMode.None))
            {
                if (polygon.gameObject.name.StartsWith("SOLID_", StringComparison.Ordinal) &&
                    polygon.enabled && !polygon.isTrigger && polygon.gameObject.activeInHierarchy)
                    solids++;
            }

            bool ignoresDefault = player != null && Physics2D.GetIgnoreLayerCollision(player.gameObject.layer, 0);
            Debug.Log("DienGame map physics | active SOLID polygons=" + solids +
                " | player=" + (player != null ? player.name : "MISSING") +
                " | Rigidbody2D=" + (rigidbody != null ? rigidbody.bodyType.ToString() : "MISSING") +
                " | simulated=" + (rigidbody != null && rigidbody.simulated) +
                " | collider=" + (hitbox != null ? hitbox.GetType().Name : "MISSING") +
                " | colliderTrigger=" + (hitbox != null && hitbox.isTrigger) +
                " | playerLayer=" + (player != null ? player.gameObject.layer.ToString() : "MISSING") +
                " | ignoresDefaultLayer=" + ignoresDefault);
            if (solids == 0 || player == null || rigidbody == null || hitbox == null || ignoresDefault)
                Debug.LogError("DienGame: Missing physics setup. A drawn obstacle cannot block the player in this state.");
        }

        [InitializeOnLoadMethod]
        private static void RegisterPlayModeCleanup()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChange;
            EditorApplication.playModeStateChanged += OnPlayModeStateChange;
        }

        private static void OnPlayModeStateChange(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode) return;
            GameObject temporary = GameObject.Find(PreviewName);
            if (temporary != null) UnityEngine.Object.DestroyImmediate(temporary);
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "EDIT MODE: Load a stage, select a red SOLID polygon in Hierarchy, " +
                "then drag its points in Scene view. Save writes StageXX_Collision.json, " +
                "which is loaded during the next Play Mode.", MessageType.Info);

            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            _stage = EditorGUILayout.Popup("Island", Mathf.Clamp(_stage - 1, 0, 4), StageNames) + 1;
            if (GUILayout.Button("LOAD / EDIT STAGE " + _stage.ToString("00"), GUILayout.Height(32)))
                LoadStage();
            EditorGUI.EndDisabledGroup();

            if (FindPreview() == null)
            {
                EditorGUILayout.HelpBox("Click LOAD to create an editable preview. Open the Scene tab to edit.", MessageType.None);
                return;
            }
            _showNames = EditorGUILayout.ToggleLeft("Show obstacle labels in Scene", _showNames);

            EditorGUILayout.Space(7f);
            EditorGUILayout.LabelField("Editing tools", EditorStyles.boldLabel);
            if (GUILayout.Button("ADD BLOCKED AREA (at Scene camera center)")) AddArea();
            EditorGUI.BeginDisabledGroup(!IsSelectedShape());
            if (GUILayout.Button("DELETE SELECTED AREA")) RemoveSelectedArea();
            EditorGUI.EndDisabledGroup();

            GameObject activeShape = IsSelectedShape() ? GetSelectedShape() : null;
            if (activeShape != null)
            {
                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Selected area", EditorStyles.boldLabel);
                string newName = EditorGUILayout.TextField("Name", AreaName(activeShape));
                if (newName != AreaName(activeShape))
                {
                    Undo.RecordObject(activeShape, "Rename collision area");
                    activeShape.name = "SOLID_" + newName;
                    EditorUtility.SetDirty(activeShape);
                }
                EditorGUILayout.HelpBox("Drag red handles in Scene. To insert or remove vertices, select the PolygonCollider2D component and click 'Edit Collider' in Inspector.", MessageType.None);
            }

            EditorGUILayout.Space(9f);
            GUI.backgroundColor = new Color(.7f, 1f, .75f);
            if (GUILayout.Button("SAVE ALL COLLIDERS TO MAP JSON", GUILayout.Height(36))) SaveStage();
            GUI.backgroundColor = Color.white;
            if (GUILayout.Button("CLOSE PREVIEW (does not overwrite JSON)")) ClearPreview();
            EditorGUILayout.HelpBox("Save before Play. The preview is automatically discarded on entering Play Mode. A timestamped backup of the previous JSON is made beside the Unity project.", MessageType.Warning);
        }

        private GameObject FindPreview()
        {
            if (_previewRoot == null) _previewRoot = GameObject.Find(PreviewName);
            return _previewRoot;
        }

        private static GameObject GetSelectedShape()
        {
            GameObject selected = Selection.activeGameObject;
            return selected != null && selected.GetComponent<PolygonCollider2D>() != null
                && selected.name.StartsWith("SOLID_", StringComparison.Ordinal) ? selected : null;
        }

        private static string AreaName(GameObject shape)
        {
            return shape.name.StartsWith("SOLID_", StringComparison.Ordinal)
                ? shape.name.Substring("SOLID_".Length) : shape.name;
        }

        private bool IsSelectedShape()
        {
            GameObject marker = GetSelectedShape();
            return marker != null && FindPreview() != null && marker.transform.IsChildOf(_previewRoot.transform);
        }

        private string StagePath(int stage)
        {
            return AssetDirectory + "Stage" + stage.ToString("00") + "_Collision.json";
        }

        private void LoadStage()
        {
            if (Application.isPlaying)
            {
                Debug.LogError("DienGame: Exit Play Mode before editing collision shapes.");
                return;
            }
            RectangularMapBoundary boundary = UnityEngine.Object.FindFirstObjectByType<RectangularMapBoundary>();
            if (boundary == null)
            {
                EditorUtility.DisplayDialog("DienGame", "Open one of the GameplayTest scenes first: no RectangularMapBoundary is present.", "OK");
                return;
            }
            string path = StagePath(_stage);
            TextAsset json = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(
                AssetDirectory + "Stage" + _stage.ToString("00") + "_Approved.png");
            if (json == null || texture == null)
            {
                EditorUtility.DisplayDialog("DienGame", "Missing approved map art or collision file for stage " + _stage + ". Install the 5-map PATCH first.", "OK");
                return;
            }

            if (FindPreview() != null)
            {
                bool discard = EditorUtility.DisplayDialog("Replace collision preview?",
                    "Unsaved collider changes will be discarded. Save first if needed.", "Replace", "Cancel");
                if (!discard) return;
                ClearPreview();
            }

            ApprovedArenaMapRuntime.StageCollisionData data =
                JsonUtility.FromJson<ApprovedArenaMapRuntime.StageCollisionData>(json.text);
            if (data == null || data.blockedAreas == null)
            {
                Debug.LogError("DienGame: Invalid collision JSON at " + path);
                return;
            }
            _source = data;
            _boundary = boundary;
            _previewRoot = new GameObject(PreviewName);
            _previewRoot.tag = "EditorOnly";
            _previewRoot.transform.position = boundary.Center;
            _previewRoot.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;

            _createdSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(.5f, .5f), 100f, 0u, SpriteMeshType.FullRect);
            _createdSprite.name = "PREVIEW_Map_Stage" + _stage.ToString("00");
            GameObject background = new GameObject("PREVIEW_Map_Illustration");
            background.transform.SetParent(_previewRoot.transform, false);
            background.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            SpriteRenderer image = background.AddComponent<SpriteRenderer>();
            image.sprite = _createdSprite;
            image.sortingOrder = 25000; // Clearly visible over the old arena in Edit Mode.
            Vector2 mapSize = boundary.WorldSize;
            background.transform.localScale = new Vector3(
                mapSize.x / _createdSprite.bounds.size.x,
                mapSize.y / _createdSprite.bounds.size.y, 1f);

            foreach (ApprovedArenaMapRuntime.BlockedArea area in data.blockedAreas)
            {
                if (area == null || area.points == null || area.points.Length < 3) continue;
                Vector2[] vertices = new Vector2[area.points.Length];
                for (int i = 0; i < vertices.Length; i++)
                    vertices[i] = ToMapLocal(area.points[i], mapSize);
                AddShape(area.name, vertices, false);
            }
            Selection.activeGameObject = _previewRoot;
            SceneView view = SceneView.lastActiveSceneView;
            if (view != null)
            {
                view.in2DMode = true;
                view.Frame(new Bounds(boundary.Center, new Vector3(mapSize.x + 2, mapSize.y + 2, 1)), false);
                view.Repaint();
            }
            Repaint();
            Debug.Log("DienGame: Manual collision preview loaded. Shapes are red and editable. Nothing has been saved yet.");
        }

        private GameObject AddShape(string areaName, Vector2[] localVertices, bool select)
        {
            GameObject item = new GameObject("SOLID_" + areaName);
            item.tag = "EditorOnly";
            item.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            item.transform.SetParent(_previewRoot.transform, false);
            PolygonCollider2D polygon = item.AddComponent<PolygonCollider2D>();
            polygon.isTrigger = false;
            polygon.pathCount = 1;
            polygon.SetPath(0, localVertices);
            if (select) Selection.activeGameObject = item;
            return item;
        }

        private static Vector2 ToMapLocal(Vector2 normalized, Vector2 size)
        {
            return new Vector2((normalized.x - .5f) * size.x, (.5f - normalized.y) * size.y);
        }

        private static Vector2 ToNormalized(Vector2 local, Vector2 size)
        {
            return new Vector2(local.x / size.x + .5f, .5f - local.y / size.y);
        }

        private void AddArea()
        {
            if (FindPreview() == null || _boundary == null) return;
            int next = 1;
            while (_previewRoot.transform.Find("SOLID_Custom_" + next.ToString("00")) != null) next++;
            Vector3 scenePosition = SceneView.lastActiveSceneView != null
                ? SceneView.lastActiveSceneView.pivot : _boundary.Center;
            Vector2 local = scenePosition - _previewRoot.transform.position;
            Vector2 half = _boundary.WorldSize * .5f - Vector2.one;
            local = new Vector2(Mathf.Clamp(local.x, -half.x, half.x),
                                Mathf.Clamp(local.y, -half.y, half.y));
            float r = .7f;
            AddShape("Custom_" + next.ToString("00"), new[]
            {
                local + new Vector2(-r, -r), local + new Vector2(-r, r),
                local + new Vector2(r, r), local + new Vector2(r, -r)
            }, true);
            SceneView.RepaintAll();
        }

        private void RemoveSelectedArea()
        {
            if (!IsSelectedShape()) return;
            Undo.DestroyObjectImmediate(Selection.activeGameObject);
            Selection.activeGameObject = _previewRoot;
            SceneView.RepaintAll();
        }

        private void SaveStage()
        {
            if (FindPreview() == null || _boundary == null || _source == null) return;
            Vector2 size = _boundary.WorldSize;
            List<ApprovedArenaMapRuntime.BlockedArea> areas = new List<ApprovedArenaMapRuntime.BlockedArea>();
            foreach (PolygonCollider2D collider in _previewRoot.GetComponentsInChildren<PolygonCollider2D>())
            {
                GameObject marker = collider.gameObject;
                if (collider == null || collider.pathCount == 0) continue;
                Vector2[] path = collider.GetPath(0);
                if (path == null || path.Length < 3)
                {
                    EditorUtility.DisplayDialog("Cannot save", AreaName(marker) + " requires at least three points.", "OK");
                    return;
                }
                Vector2[] normalized = new Vector2[path.Length];
                for (int i = 0; i < path.Length; i++)
                {
                    Vector3 worldPoint = marker.transform.TransformPoint(path[i] + collider.offset);
                    Vector2 local = new Vector2(worldPoint.x - _boundary.Center.x, worldPoint.y - _boundary.Center.y);
                    normalized[i] = ToNormalized(local, size);
                    if (normalized[i].x < -.0001f || normalized[i].x > 1.0001f ||
                        normalized[i].y < -.0001f || normalized[i].y > 1.0001f)
                    {
                        EditorUtility.DisplayDialog("Cannot save", AreaName(marker) + " has a point outside the map. Move it inside the rectangular border.", "OK");
                        return;
                    }
                    normalized[i] = new Vector2(Mathf.Clamp01(normalized[i].x), Mathf.Clamp01(normalized[i].y));
                }
                if (Mathf.Abs(SignedArea(normalized)) < .00001f)
                {
                    EditorUtility.DisplayDialog("Cannot save", AreaName(marker) + " is too small or has zero area.", "OK");
                    return;
                }
                if (HasSelfIntersections(normalized))
                {
                    EditorUtility.DisplayDialog("Cannot save", AreaName(marker) + " crosses itself. Drag vertices to make a simple closed polygon.", "OK");
                    return;
                }
                areas.Add(new ApprovedArenaMapRuntime.BlockedArea
                {
                    name = string.IsNullOrWhiteSpace(AreaName(marker)) ? "Custom" : AreaName(marker).Trim(),
                    points = normalized
                });
            }
            if (areas.Count == 0)
            {
                EditorUtility.DisplayDialog("Cannot save", "The map needs blocked areas. Keep at least one collider.", "OK");
                return;
            }

            _source.blockedAreas = areas.ToArray();
            string assetPath = StagePath(_source.stage);
            string fullPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, assetPath);
            string backupFolder = Path.Combine(Directory.GetParent(Application.dataPath).FullName,
                "DienGame_MapCollisionBackups");
            string backup = Path.Combine(backupFolder,
                "Stage" + _source.stage.ToString("00") + "_Collision_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".json");
            try
            {
                Directory.CreateDirectory(backupFolder);
                if (File.Exists(fullPath)) File.Copy(fullPath, backup, false);
                File.WriteAllText(fullPath, JsonUtility.ToJson(_source, true), System.Text.Encoding.UTF8);
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh();
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                EditorUtility.DisplayDialog("Cannot save map", "Could not write collision JSON. Check file permissions and backup folder. " + error.Message, "OK");
                return;
            }
            Debug.Log("DienGame: saved " + areas.Count + " collision areas to " + assetPath +
                ". Backup: " + backup + ". Restart Play Mode to apply changes.");
            EditorUtility.DisplayDialog("Collision saved", "Saved " + areas.Count +
                " blocked areas for island " + _source.stage + ". The next Play Mode uses these new colliders.", "OK");
        }

        private static float SignedArea(Vector2[] p)
        {
            float sum = 0f;
            for (int i = 0; i < p.Length; i++)
                sum += p[i].x * p[(i + 1) % p.Length].y - p[(i + 1) % p.Length].x * p[i].y;
            return sum * .5f;
        }

        private static bool HasSelfIntersections(Vector2[] p)
        {
            for (int i = 0; i < p.Length; i++)
            {
                int nextI = (i + 1) % p.Length;
                for (int j = i + 1; j < p.Length; j++)
                {
                    int nextJ = (j + 1) % p.Length;
                    if (i == j || nextI == j || nextJ == i) continue;
                    if (SegmentsCross(p[i], p[nextI], p[j], p[nextJ])) return true;
                }
            }
            return false;
        }

        private static bool SegmentsCross(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float abC = Cross(b - a, c - a);
            float abD = Cross(b - a, d - a);
            float cdA = Cross(d - c, a - c);
            float cdB = Cross(d - c, b - c);
            return ((abC > 0f && abD < 0f) || (abC < 0f && abD > 0f)) &&
                   ((cdA > 0f && cdB < 0f) || (cdA < 0f && cdB > 0f));
        }

        private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

        private void ClearPreview()
        {
            if (FindPreview() != null)
            {
                if (Selection.activeGameObject != null &&
                    Selection.activeGameObject.transform.IsChildOf(_previewRoot.transform))
                    Selection.activeGameObject = null;
                UnityEngine.Object.DestroyImmediate(_previewRoot);
            }
            _previewRoot = null;
            _boundary = null;
            _source = null;
            if (_createdSprite != null)
            {
                UnityEngine.Object.DestroyImmediate(_createdSprite);
                _createdSprite = null;
            }
            SceneView.RepaintAll();
            Repaint();
        }

        private void OnSceneGUI(SceneView view)
        {
            if (FindPreview() == null || _boundary == null || Application.isPlaying) return;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            foreach (PolygonCollider2D polygon in _previewRoot.GetComponentsInChildren<PolygonCollider2D>())
            {
                GameObject marker = polygon.gameObject;
                if (polygon == null || polygon.pathCount == 0) continue;
                Vector2[] points = polygon.GetPath(0);
                Vector3[] world = new Vector3[points.Length + 1];
                for (int i = 0; i < points.Length; i++)
                {
                    world[i] = marker.transform.TransformPoint(points[i] + polygon.offset);
                    world[i].z = -1f;
                }
                world[points.Length] = world[0];
                bool selected = Selection.activeGameObject == marker.gameObject;
                Handles.color = selected ? new Color(1f, .8f, .2f, 1f) : new Color(1f, .1f, .17f, .95f);
                Handles.DrawAAPolyLine(selected ? 4f : 2.5f, world);
                if (_showNames)
                {
                    Vector3 center = Vector3.zero;
                    for (int i = 0; i < points.Length; i++) center += world[i];
                    center /= points.Length;
                    Handles.Label(center, AreaName(marker));
                }
                if (!selected) continue;
                EditorGUI.BeginChangeCheck();
                for (int i = 0; i < points.Length; i++)
                {
                    Vector3 pos = marker.transform.TransformPoint(points[i] + polygon.offset);
                    pos.z = -1f;
                    float size = HandleUtility.GetHandleSize(pos) * .085f;
                    Vector3 moved = Handles.FreeMoveHandle(pos, size, Vector3.zero,
                        Handles.DotHandleCap);
                    if ((moved - pos).sqrMagnitude > .0000001f)
                    {
                        Undo.RecordObject(polygon, "Move map collision vertex");
                        Vector3 local = marker.transform.InverseTransformPoint(moved);
                        points[i] = new Vector2(local.x, local.y) - polygon.offset;
                        polygon.SetPath(0, points);
                        EditorUtility.SetDirty(polygon);
                    }
                }
                EditorGUI.EndChangeCheck();
            }
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        }
    }

}
#endif
