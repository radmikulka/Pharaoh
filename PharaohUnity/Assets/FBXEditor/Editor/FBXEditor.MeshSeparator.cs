// FBXEditor_MeshSeparator.cs
// This partial class contains all logic for the "Mesh Separator" tab.
// OPTIMIZED: Added mesh data caching, screen position caching, and improved drawing performance.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace FBXEditor.Editor
{
    public partial class FBXEditor : EditorWindow
    {
        // --- Mesh Separator Variables ---
        private enum SelectionMode { Single, Paint, Box }
        private SelectionMode selectionMode = SelectionMode.Single;
        private bool deselectOnMiss = true;
        private bool isXRaySelectionEnabled = false;
        private GameObject separatorTargetObject;
        private Mesh separatorTargetMesh;
        private Dictionary<Mesh, HashSet<int>> selectedTriangles = new Dictionary<Mesh, HashSet<int>>();
        private Dictionary<Mesh, int[][]> meshTriangleAdjacency = new Dictionary<Mesh, int[][]>();
        private Dictionary<Mesh, List<int>[]> vertexToTrianglesMapCache = new Dictionary<Mesh, List<int>[]>();
        private bool isBoxSelecting = false;
        private Vector2 boxStartPos;
        private Vector2 boxEndPos;
        private float lastPaintTime;
        private const float paintCooldown = 0.05f;
        private float paintBrushRadius = 20f;

        // --- Variables for Hierarchy Isolation ---
        private bool isolateToHierarchySelection = false;
        private List<Renderer> isolatedRendererCache = new List<Renderer>();
        private List<GameObject> isolatedObjectCache = new List<GameObject>();
        private Vector2 isolationListScrollPos;

        // --- Variables for Hover Preview ---
        private int hoveredTriangleIndex = -1;
        private GameObject hoveredObject = null;

        // --- OPTIMIZATION: Visibility Cache ---
        private Dictionary<Mesh, HashSet<int>> visibleTrianglesCache = new Dictionary<Mesh, HashSet<int>>();
        private bool isVisibilityCacheValid = false;
        private Rect lastVisibilityCacheArea = Rect.zero;

        // --- OPTIMIZATION: Cached mesh arrays to avoid repeated allocations ---
        private Dictionary<Mesh, Vector3[]> cachedVertices = new Dictionary<Mesh, Vector3[]>();
        private Dictionary<Mesh, int[]> cachedTriangles = new Dictionary<Mesh, int[]>();
        private Dictionary<Mesh, Vector2[]> cachedScreenPositions = new Dictionary<Mesh, Vector2[]>();
        private Dictionary<Mesh, Rect> cachedScreenBounds = new Dictionary<Mesh, Rect>();
        private int lastScreenCacheFrame = -1;

        // --- OPTIMIZATION: Camera state tracking (avoid GetHashCode on floats) ---
        private Vector3 lastCameraPosition;
        private Quaternion lastCameraRotation;
        private float lastCameraFOV;

        // --- OPTIMIZATION: Cached selection count (avoid LINQ every frame) ---
        private int cachedSelectedTriangleCount = 0;

        // --- OPTIMIZATION: Cached world vertices for drawing (avoid per-frame allocation) ---
        private Dictionary<Mesh, Vector3[]> cachedWorldVertsForDrawing = new Dictionary<Mesh, Vector3[]>();
        private int selectionVersion = 0;
        private int lastDrawSelectionVersion = -1;

        // --- OPTIMIZATION: Reusable list for raycast candidates ---
        private List<(Renderer renderer, float distance)> reusableCandidateList = new List<(Renderer, float)>();

        // --- OPTIMIZATION: Reusable list for stale key cleanup ---
        private List<Mesh> reusableStaleKeyList = new List<Mesh>();

        #region Mesh Cache Helpers

        private Vector3[] GetCachedVertices(Mesh mesh)
        {
            if (mesh == null) return null;
            if (!cachedVertices.TryGetValue(mesh, out var verts))
            {
                verts = mesh.vertices;
                cachedVertices[mesh] = verts;
            }
            return verts;
        }

        private int[] GetCachedTriangles(Mesh mesh)
        {
            if (mesh == null) return null;
            if (!cachedTriangles.TryGetValue(mesh, out var tris))
            {
                tris = mesh.triangles;
                cachedTriangles[mesh] = tris;
            }
            return tris;
        }

        private void ClearMeshCaches()
        {
            cachedVertices.Clear();
            cachedTriangles.Clear();
            cachedScreenPositions.Clear();
            cachedScreenBounds.Clear();
            lastScreenCacheFrame = -1;
        }

        /// <summary>
        /// Removes entries from all caches that reference destroyed or null meshes.
        /// Call this periodically to prevent memory accumulation from dead references.
        /// OPTIMIZED: Uses a single reusable list instead of multiple LINQ allocations.
        /// </summary>
        private void CleanupStaleMeshCaches()
        {
            // Helper to clean a dictionary using the reusable list
            void CleanDictionary<T>(Dictionary<Mesh, T> dict)
            {
                reusableStaleKeyList.Clear();
                foreach (var key in dict.Keys)
                    if (key == null) reusableStaleKeyList.Add(key);
                foreach (var key in reusableStaleKeyList)
                    dict.Remove(key);
            }

            CleanDictionary(selectedTriangles);
            CleanDictionary(meshTriangleAdjacency);
            CleanDictionary(vertexToTrianglesMapCache);
            CleanDictionary(visibleTrianglesCache);
            CleanDictionary(cachedVertices);
            CleanDictionary(cachedTriangles);
            CleanDictionary(cachedScreenPositions);
            CleanDictionary(cachedScreenBounds);
            CleanDictionary(cachedWorldVertsForDrawing);
        }

        private void UpdateScreenPositionCache(Mesh mesh, Transform objectTransform)
        {
            if (mesh == null || objectTransform == null) return;

            // Check camera state using direct comparison (more reliable than GetHashCode)
            var sceneView = SceneView.lastActiveSceneView;
            bool cameraChanged = false;
            if (sceneView != null && sceneView.camera != null)
            {
                var cam = sceneView.camera;
                Vector3 currentPos = cam.transform.position;
                Quaternion currentRot = cam.transform.rotation;
                float currentFOV = cam.fieldOfView;

                cameraChanged = currentPos != lastCameraPosition ||
                               currentRot != lastCameraRotation ||
                               !Mathf.Approximately(currentFOV, lastCameraFOV);

                if (cameraChanged)
                {
                    lastCameraPosition = currentPos;
                    lastCameraRotation = currentRot;
                    lastCameraFOV = currentFOV;
                }
            }

            // Only update if camera changed or mesh not cached yet
            bool meshCached = cachedScreenPositions.ContainsKey(mesh);

            if (!cameraChanged && meshCached)
                return;

            var verts = GetCachedVertices(mesh);
            if (verts == null) return;

            if (!cachedScreenPositions.TryGetValue(mesh, out var screenPos) || screenPos.Length != verts.Length)
            {
                screenPos = new Vector2[verts.Length];
                cachedScreenPositions[mesh] = screenPos;
            }

            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 worldPos = objectTransform.TransformPoint(verts[i]);
                Vector2 sp = HandleUtility.WorldToGUIPoint(worldPos);
                screenPos[i] = sp;

                minX = Mathf.Min(minX, sp.x);
                minY = Mathf.Min(minY, sp.y);
                maxX = Mathf.Max(maxX, sp.x);
                maxY = Mathf.Max(maxY, sp.y);
            }

            cachedScreenBounds[mesh] = Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

        private void InvalidateScreenPositionCaches()
        {
            lastScreenCacheFrame = -1;
            lastCameraPosition = Vector3.zero;
            lastCameraRotation = Quaternion.identity;
            lastCameraFOV = 0f;
            cachedScreenPositions.Clear();
            cachedScreenBounds.Clear();
        }

        #endregion

        // Track when we last cleaned up caches to avoid doing it every frame
        private double lastCacheCleanupTime = 0;
        private const double cacheCleanupInterval = 5.0; // Clean up every 5 seconds
        private bool isolatedRendererCacheDirty = true;
        private GameObject lastActiveEditingCopy = null;
        private bool lastIsolateToHierarchySelection = false;
        private int lastSelectionHash = 0;

        /// <summary>
        /// Marks the isolated renderer cache as needing rebuild.
        /// </summary>
        private void InvalidateIsolatedRendererCache()
        {
            isolatedRendererCacheDirty = true;
        }

        /// <summary>
        /// Caches the list of renderers that selection should be limited to.
        /// Only rebuilds when necessary.
        /// </summary>
        private void UpdateIsolatedRendererCache()
        {
            if (activeEditingCopy == null)
            {
                isolatedRendererCache.Clear();
                isolatedObjectCache.Clear();
                lastActiveEditingCopy = null;
                return;
            }

            // Only clean up stale mesh references periodically, not every frame
            double currentTime = EditorApplication.timeSinceStartup;
            if (currentTime - lastCacheCleanupTime > cacheCleanupInterval)
            {
                CleanupStaleMeshCaches();
                lastCacheCleanupTime = currentTime;
            }

            // Check if we need to rebuild the cache
            int currentSelectionHash = isolateToHierarchySelection ? Selection.gameObjects.GetHashCode() : 0;
            bool needsRebuild = isolatedRendererCacheDirty ||
                                lastActiveEditingCopy != activeEditingCopy ||
                                lastIsolateToHierarchySelection != isolateToHierarchySelection ||
                                (isolateToHierarchySelection && currentSelectionHash != lastSelectionHash);

            if (!needsRebuild) return;

            isolatedRendererCache.Clear();
            isolatedObjectCache.Clear();

            if (isolateToHierarchySelection)
            {
                List<GameObject> validSelectedChildren = GetValidSelectedChildren(activeEditingCopy);
                if (validSelectedChildren.Count > 0)
                {
                    foreach (var go in validSelectedChildren)
                    {
                        isolatedRendererCache.AddRange(go.GetComponentsInChildren<Renderer>(true));
                        isolatedObjectCache.Add(go);
                    }
                    isolatedRendererCache = isolatedRendererCache.Distinct().ToList();
                }
            }
            else
            {
                isolatedRendererCache.AddRange(activeEditingCopy.GetComponentsInChildren<Renderer>(true));
            }

            // Update tracking variables
            isolatedRendererCacheDirty = false;
            lastActiveEditingCopy = activeEditingCopy;
            lastIsolateToHierarchySelection = isolateToHierarchySelection;
            lastSelectionHash = currentSelectionHash;
        }

        /// <summary>
        /// Draws the UI for the Mesh Separator tab.
        /// </summary>
        private void DrawMeshSeparatorTab()
        {
            UpdateIsolatedRendererCache();

            if (activeEditingCopy == null)
            {
                EditorGUILayout.HelpBox("Load a model in the 'Hierarchy Editor' tab first.", MessageType.Warning);
                return;
            }

            GameObject currentSelection = Selection.activeGameObject;
            if (currentSelection != null &&
                currentSelection.transform.IsChildOf(activeEditingCopy.transform) &&
                currentSelection != separatorTargetObject)
            {
                Mesh selectedMesh = GetMeshFromObject(currentSelection);
                if (selectedMesh != null)
                {
                    if (!isXRaySelectionEnabled && !isolateToHierarchySelection) ClearTriangleSelection();
                    separatorTargetObject = currentSelection;
                    separatorTargetMesh = selectedMesh;
                }
            }

            EditorGUILayout.HelpBox("Select triangles on a mesh in the Scene View to separate them into a new object. Use SHIFT to add to selection and CTRL to remove.", MessageType.Info);

            // --- Selection Mode & Options ---
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Selection Mode", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            selectionMode = (SelectionMode)GUILayout.Toolbar((int)selectionMode, new string[] { "Single", "Paint", "Box" });
            if (EditorGUI.EndChangeCheck()) { isVisibilityCacheValid = false; }

            if (selectionMode == SelectionMode.Paint)
            {
                paintBrushRadius = EditorGUILayout.Slider("Brush Size", paintBrushRadius, 1f, 150f);
            }

            deselectOnMiss = EditorGUILayout.Toggle(new GUIContent("Deselect on Miss", "If enabled, clicking in empty space will clear the triangle selection."), deselectOnMiss);
            isXRaySelectionEnabled = EditorGUILayout.Toggle(new GUIContent("X-Ray Selection", "If enabled, selection will pass through objects to select triangles behind them."), isXRaySelectionEnabled);

            EditorGUI.BeginChangeCheck();
            isolateToHierarchySelection = EditorGUILayout.Toggle(new GUIContent("Isolate to Hierarchy", "If enabled, triangle selection is limited to the GameObjects currently selected in the Hierarchy."), isolateToHierarchySelection);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateIsolatedRendererCache();
            }

            if (isolateToHierarchySelection)
            {
                string helpText;
                if (isolatedObjectCache.Count == 0)
                {
                    helpText = "Select one or more objects from the Hierarchy to begin selection.";
                }
                else
                {
                    helpText = $"Isolating selection to {isolatedObjectCache.Count} object(s):";
                }

                EditorGUILayout.HelpBox(helpText, isolatedObjectCache.Count == 0 ? MessageType.Warning : MessageType.Info);

                if (isolatedObjectCache.Count > 0)
                {
                    isolationListScrollPos = EditorGUILayout.BeginScrollView(isolationListScrollPos, GUILayout.Height(Mathf.Min(5 * 20, 100)));
                    foreach (var obj in isolatedObjectCache)
                    {
                        EditorGUILayout.LabelField("- " + obj.name, EditorStyles.miniLabel);
                    }
                    EditorGUILayout.EndScrollView();
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);

            // --- Selection Status ---
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Selection Status", EditorStyles.boldLabel);

            // Use cached count instead of LINQ to avoid per-frame allocations
            EditorGUILayout.LabelField("Triangle Selection:", $"{cachedSelectedTriangleCount:N0} (across {selectedTriangles.Count} meshes)");

            if (separatorTargetObject == null && cachedSelectedTriangleCount == 0)
            {
                EditorGUILayout.HelpBox("Select a valid mesh object from the editing copy in the Hierarchy or by clicking on it in the Scene View.", MessageType.Info);
            }
            EditorGUILayout.EndVertical();

            // --- Actions ---
            bool canOperate = cachedSelectedTriangleCount > 0;

            using (new EditorGUI.DisabledScope(!canOperate))
            {
                // --- Selection Tools ---
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label("Selection Tools", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();

                using (new EditorGUI.DisabledScope(separatorTargetObject == null))
                {
                    if (GUILayout.Button("Grow")) { InitiateGrowSelection(); }
                    if (GUILayout.Button("Shrink")) { ShrinkSelection(); }
                    if (GUILayout.Button("Invert")) { InvertSelection(); }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                using (new EditorGUI.DisabledScope(separatorTargetObject == null))
                {
                    if (GUILayout.Button("Select All")) { SelectAllTriangles(); }
                }
                if (GUILayout.Button("Clear Selection")) { ClearTriangleSelection(); }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                // --- Mesh Operations ---
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label("Mesh Operations", EditorStyles.boldLabel);

                GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
                if (GUILayout.Button(new GUIContent("Separate Selected", "Replaces the original object with two new objects: the selection and the remainder.")))
                {
                    SeparateSelectedTriangles();
                }

                GUI.backgroundColor = new Color(0.8f, 0.9f, 1f);
                if (GUILayout.Button(new GUIContent("Extract Selected", "Creates a new object from the selection and leaves the original object unchanged.")))
                {
                    ExtractSelectedTriangles();
                }

                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndVertical();
            }
        }

        /// <summary>
        /// Handles all scene view logic for the mesh separator.
        /// </summary>
        private void HandleMeshSeparatorSceneGUI(SceneView sceneView)
        {
            UpdateIsolatedRendererCache();

            // If no active editing copy, just return - don't modify state during scene GUI
            // The main OnGUI loop will handle redirecting to the correct tab
            if (activeEditingCopy == null)
            {
                return;
            }

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            Event e = Event.current;

            // Invalidate caches if the camera moves
            if (sceneView.camera.transform.hasChanged)
            {
                isVisibilityCacheValid = false;
                InvalidateScreenPositionCaches();
                sceneView.camera.transform.hasChanged = false;
            }

            if (e.type == EventType.MouseMove)
            {
                HandleHoverPreview(e);
            }

            if (selectionMode == SelectionMode.Box)
            {
                HandleBoxSelection(e, sceneView);
            }
            else
            {
                HandleRaycastSelection(e);
            }

            DrawSelectedTriangles();
            DrawHoveredTriangle();
            DrawSelectionBox(e);
            DrawPaintBrush(e);
        }

        #region Scene UI & Interaction

        private void HandleHoverPreview(Event e)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (RaycastToClosestTriangle(ray, out GameObject hitObject, out int hitTriangleIndex, out float _))
            {
                if (hoveredObject != hitObject || hoveredTriangleIndex != hitTriangleIndex)
                {
                    hoveredObject = hitObject;
                    hoveredTriangleIndex = hitTriangleIndex;
                    RepaintSceneAndWindow();
                }
            }
            else
            {
                if (hoveredTriangleIndex != -1)
                {
                    hoveredTriangleIndex = -1;
                    hoveredObject = null;
                    RepaintSceneAndWindow();
                }
            }
        }

        private void HandleBoxSelection(Event e, SceneView sceneView)
        {
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                isBoxSelecting = true;
                boxStartPos = e.mousePosition;
                boxEndPos = e.mousePosition;
                isVisibilityCacheValid = false;
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && e.button == 0 && isBoxSelecting)
            {
                boxEndPos = e.mousePosition;
                e.Use();
                sceneView.Repaint();
            }
            else if (e.type == EventType.MouseUp && e.button == 0 && isBoxSelecting)
            {
                isBoxSelecting = false;
                InitiateBoxSelection(boxStartPos, boxEndPos, e.shift, e.control);
                e.Use();
                Repaint();
            }
        }

        private void HandleRaycastSelection(Event e)
        {
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                isVisibilityCacheValid = false;
                bool hitSuccess;
                if (selectionMode == SelectionMode.Paint)
                {
                    hitSuccess = DoPaintSelection(e.mousePosition, e.shift, e.control);
                }
                else
                {
                    hitSuccess = DoRaycastSelection(e.mousePosition, e.shift, e.control, true);
                }

                if (!hitSuccess && deselectOnMiss)
                {
                    if (!isolateToHierarchySelection)
                    {
                        ClearTriangleSelection();
                    }
                }
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && e.button == 0 && selectionMode == SelectionMode.Paint)
            {
                if (Time.realtimeSinceStartup > lastPaintTime + paintCooldown)
                {
                    // Don't invalidate visibility cache on drag - it only changes when camera moves
                    DoPaintSelection(e.mousePosition, e.shift, e.control);
                    lastPaintTime = Time.realtimeSinceStartup;
                    e.Use();
                }
            }
        }

        private void DrawSelectionBox(Event e)
        {
            if (isBoxSelecting)
            {
                Handles.BeginGUI();
                Rect boxRect = new Rect(boxStartPos.x, boxStartPos.y, boxEndPos.x - boxStartPos.x, boxEndPos.y - boxStartPos.y);
                Handles.DrawSolidRectangleWithOutline(boxRect, new Color(0.5f, 0.7f, 1f, 0.1f), new Color(0.5f, 0.7f, 1f, 0.8f));
                Handles.EndGUI();
            }
        }

        private void DrawPaintBrush(Event e)
        {
            if (selectionMode == SelectionMode.Paint)
            {
                Handles.BeginGUI();
                Handles.color = new Color(0.2f, 0.8f, 1f, 0.9f);
                Handles.DrawWireDisc(e.mousePosition, Vector3.forward, paintBrushRadius);
                Handles.EndGUI();
            }
        }

        /// <summary>
        /// OPTIMIZED: Caches world vertices to avoid per-frame allocations.
        /// Only rebuilds when selection changes (tracked by selectionVersion).
        /// </summary>
        private void DrawSelectedTriangles()
        {
            // Check if we need to rebuild the cached world vertices
            bool needsRebuild = lastDrawSelectionVersion != selectionVersion;

            foreach (var pair in selectedTriangles)
            {
                Mesh mesh = pair.Key;
                HashSet<int> selection = pair.Value;
                if (mesh == null || selection.Count == 0) continue;

                GameObject obj = FindObjectForMesh(mesh);
                if (obj == null) continue;

                var tris = GetCachedTriangles(mesh);
                var verts = GetCachedVertices(mesh);
                if (tris == null || verts == null) continue;

                Transform objectTransform = obj.transform;

                // Get or create cached world vertices array
                if (!cachedWorldVertsForDrawing.TryGetValue(mesh, out var worldVerts) ||
                    worldVerts.Length != selection.Count * 3 ||
                    needsRebuild)
                {
                    // Rebuild the cached world vertices
                    int requiredSize = selection.Count * 3;
                    if (worldVerts == null || worldVerts.Length != requiredSize)
                    {
                        worldVerts = new Vector3[requiredSize];
                        cachedWorldVertsForDrawing[mesh] = worldVerts;
                    }

                    int idx = 0;
                    foreach (int triIndex in selection)
                    {
                        int baseIdx = triIndex * 3;
                        if (baseIdx + 2 >= tris.Length) continue;

                        worldVerts[idx++] = objectTransform.TransformPoint(verts[tris[baseIdx + 0]]);
                        worldVerts[idx++] = objectTransform.TransformPoint(verts[tris[baseIdx + 1]]);
                        worldVerts[idx++] = objectTransform.TransformPoint(verts[tris[baseIdx + 2]]);
                    }
                }

                // Draw filled triangles
                Handles.color = new Color(1f, 0.5f, 0f, 0.35f);
                for (int i = 0; i < worldVerts.Length; i += 3)
                {
                    Handles.DrawAAConvexPolygon(worldVerts[i], worldVerts[i + 1], worldVerts[i + 2]);
                }

                // Draw outlines
                var originalZTest = Handles.zTest;
                Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
                Handles.color = new Color(0f, 1f, 1f, 0.75f);
                for (int i = 0; i < worldVerts.Length; i += 3)
                {
                    Handles.DrawPolyLine(worldVerts[i], worldVerts[i + 1], worldVerts[i + 2], worldVerts[i]);
                }
                Handles.zTest = originalZTest;
            }

            // Update the version tracker so we don't rebuild next frame
            lastDrawSelectionVersion = selectionVersion;
        }

        private void DrawHoveredTriangle()
        {
            if (hoveredTriangleIndex == -1 || hoveredObject == null || selectionMode == SelectionMode.Paint) return;

            Mesh mesh = GetMeshFromObject(hoveredObject);
            if (mesh == null) return;

            if (selectedTriangles.ContainsKey(mesh) && selectedTriangles[mesh].Contains(hoveredTriangleIndex)) return;

            var tris = GetCachedTriangles(mesh);
            var verts = GetCachedVertices(mesh);
            if (tris == null || verts == null) return;
            
            Transform objectTransform = hoveredObject.transform;

            int baseIdx = hoveredTriangleIndex * 3;
            if (baseIdx + 2 >= tris.Length) return;

            Vector3 v1 = objectTransform.TransformPoint(verts[tris[baseIdx + 0]]);
            Vector3 v2 = objectTransform.TransformPoint(verts[tris[baseIdx + 1]]);
            Vector3 v3 = objectTransform.TransformPoint(verts[tris[baseIdx + 2]]);

            Handles.color = new Color(1f, 1f, 0f, 0.5f);
            var originalZTest = Handles.zTest;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            Handles.DrawAAConvexPolygon(v1, v2, v3);
            Handles.zTest = originalZTest;
        }

        #endregion

        #region Selection Logic

        /// <summary>
        /// OPTIMIZED: Uses cached mesh data for per-mesh raycast.
        /// </summary>
        private bool RaycastToClosestTriangleOnMesh(Ray ray, Mesh mesh, Transform transform, out int hitTriangleIndex, out float closestDist)
        {
            hitTriangleIndex = -1;
            closestDist = float.MaxValue;

            var tris = GetCachedTriangles(mesh);
            var verts = GetCachedVertices(mesh);
            if (tris == null || verts == null) return false;

            for (int i = 0; i < tris.Length; i += 3)
            {
                Vector3 v0 = transform.TransformPoint(verts[tris[i]]);
                Vector3 v1 = transform.TransformPoint(verts[tris[i + 1]]);
                Vector3 v2 = transform.TransformPoint(verts[tris[i + 2]]);

                if (RayIntersectsTriangle(ray, v0, v1, v2, out float dist))
                {
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        hitTriangleIndex = i / 3;
                    }
                }
            }
            return hitTriangleIndex != -1;
        }

        /// <summary>
        /// OPTIMIZED: Uses cached mesh data for X-ray raycast.
        /// </summary>
        private List<(GameObject hitObject, int hitTriangleIndex)> RaycastToAllTriangles(Ray ray)
        {
            var allHits = new List<(GameObject, int)>();
            if (activeEditingCopy == null) return allHits;

            var renderers = this.isolatedRendererCache;
            foreach (var renderer in renderers)
            {
                var mesh = GetMeshFromObject(renderer.gameObject);
                if (mesh == null) continue;
                
                var transform = renderer.transform;
                var tris = GetCachedTriangles(mesh);
                var verts = GetCachedVertices(mesh);
                if (tris == null || verts == null) continue;

                for (int i = 0; i < tris.Length; i += 3)
                {
                    Vector3 v0 = transform.TransformPoint(verts[tris[i]]);
                    Vector3 v1 = transform.TransformPoint(verts[tris[i + 1]]);
                    Vector3 v2 = transform.TransformPoint(verts[tris[i + 2]]);

                    if (RayIntersectsTriangle(ray, v0, v1, v2, out float _))
                    {
                        allHits.Add((renderer.gameObject, i / 3));
                    }
                }
            }
            return allHits;
        }

        /// <summary>
        /// OPTIMIZED: Uses cached mesh data, bounding box early-out, and reusable list.
        /// </summary>
        private bool RaycastToClosestTriangle(Ray ray, out GameObject hitObject, out int hitTriangleIndex, out float closestDist)
        {
            hitObject = null;
            hitTriangleIndex = -1;
            closestDist = float.MaxValue;

            if (activeEditingCopy == null) return false;

            var renderers = this.isolatedRendererCache;

            // Use reusable list instead of allocating new one each raycast
            reusableCandidateList.Clear();

            // OPTIMIZATION PASS 1: Fast bounding box intersection test
            foreach (var renderer in renderers)
            {
                if (renderer.bounds.IntersectRay(ray, out float distance))
                {
                    reusableCandidateList.Add((renderer, distance));
                }
            }

            // Sort candidates by distance
            reusableCandidateList.Sort((a, b) => a.distance.CompareTo(b.distance));

            // OPTIMIZATION PASS 2: Per-triangle check using CACHED arrays
            foreach (var candidate in reusableCandidateList)
            {
                if (candidate.distance > closestDist) break;

                var mesh = GetMeshFromObject(candidate.renderer.gameObject);
                if (mesh == null) continue;
                
                var transform = candidate.renderer.transform;
                var tris = GetCachedTriangles(mesh);
                var verts = GetCachedVertices(mesh);
                if (tris == null || verts == null) continue;

                for (int i = 0; i < tris.Length; i += 3)
                {
                    Vector3 v0 = transform.TransformPoint(verts[tris[i]]);
                    Vector3 v1 = transform.TransformPoint(verts[tris[i + 1]]);
                    Vector3 v2 = transform.TransformPoint(verts[tris[i + 2]]);

                    if (RayIntersectsTriangle(ray, v0, v1, v2, out float dist))
                    {
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            hitTriangleIndex = i / 3;
                            hitObject = candidate.renderer.gameObject;
                        }
                    }
                }
            }
            return hitObject != null;
        }

        private bool DoRaycastSelection(Vector2 mousePosition, bool isShiftHeld, bool isCtrlHeld, bool clearPrevious)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

            if (isXRaySelectionEnabled)
            {
                var allHits = RaycastToAllTriangles(ray);
                if (allHits.Count == 0) return false;

                if (clearPrevious && !isShiftHeld && !isCtrlHeld)
                {
                    if (!isolateToHierarchySelection)
                    {
                        ClearTriangleSelection();
                    }
                }

                foreach (var (hitObject, hitIndex) in allHits)
                {
                    var mesh = GetMeshFromObject(hitObject);
                    if (mesh == null) continue;
                    if (!selectedTriangles.ContainsKey(mesh))
                    {
                        selectedTriangles[mesh] = new HashSet<int>();
                    }

                    if (isCtrlHeld) selectedTriangles[mesh].Remove(hitIndex);
                    else selectedTriangles[mesh].Add(hitIndex);
                }

                if (allHits.Any())
                {
                    separatorTargetObject = allHits[0].hitObject;
                    separatorTargetMesh = GetMeshFromObject(separatorTargetObject);
                }
                UpdateCachedSelectionCount();
                Repaint();
                return true;
            }
            else
            {
                if (RaycastToClosestTriangle(ray, out GameObject bestObject, out int bestTriangleIndex, out float _))
                {
                    if (clearPrevious && !isShiftHeld && !isCtrlHeld)
                    {
                        if (!isolateToHierarchySelection)
                        {
                            ClearTriangleSelection();
                        }
                    }

                    separatorTargetObject = bestObject;
                    separatorTargetMesh = GetMeshFromObject(bestObject);

                    if (!selectedTriangles.ContainsKey(separatorTargetMesh))
                    {
                        selectedTriangles[separatorTargetMesh] = new HashSet<int>();
                    }
                    if (!isShiftHeld && !isCtrlHeld && !isolateToHierarchySelection)
                    {
                        selectedTriangles[separatorTargetMesh].Clear();
                    }

                    if (isCtrlHeld) selectedTriangles[separatorTargetMesh].Remove(bestTriangleIndex);
                    else selectedTriangles[separatorTargetMesh].Add(bestTriangleIndex);

                    UpdateCachedSelectionCount();
                    Repaint();
                    return true;
                }
                return false;
            }
        }

        private void BuildVisibilityCacheWithJobs(Rect area, List<Renderer> renderers)
        {
            if (isVisibilityCacheValid) return;

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            visibleTrianglesCache.Clear();

            // Store the area this cache was built for
            lastVisibilityCacheArea = area;

            int steps = 64;
            int rayCount = steps * steps;
            var rays = new NativeArray<Ray>(rayCount, Allocator.TempJob);
            NativeArray<float3> vertices = default;
            NativeArray<int> triangles = default;
            NativeArray<MeshData> meshData = default;
            NativeArray<int2> results = default;

            try
            {
                int rayIndex = 0;
                for (int i = 0; i < steps; i++)
                {
                    for (int j = 0; j < steps; j++)
                    {
                        float x = area.xMin + (area.width * (i / (float)(steps - 1)));
                        float y = area.yMin + (area.height * (j / (float)(steps - 1)));
                        rays[rayIndex++] = HandleUtility.GUIPointToWorldRay(new Vector2(x, y));
                    }
                }

                var sourceMeshes = new List<Mesh>();
                var meshDataList = new List<MeshData>();
                var vertList = new List<float3>();
                var triList = new List<int>();

                foreach (var renderer in renderers)
                {
                    var mesh = GetMeshFromObject(renderer.gameObject);
                    if (mesh == null) continue;

                    var verts = GetCachedVertices(mesh);
                    var tris = GetCachedTriangles(mesh);
                    if (verts == null || tris == null) continue;

                    sourceMeshes.Add(mesh);
                    meshDataList.Add(new MeshData
                    {
                        vertexOffset = vertList.Count,
                        triangleOffset = triList.Count,
                        localToWorldMatrix = renderer.transform.localToWorldMatrix
                    });
                    foreach (var v in verts) vertList.Add(v);
                    triList.AddRange(tris);
                }

                if (sourceMeshes.Count == 0)
                {
                    isVisibilityCacheValid = true;
                    return;
                }

                vertices = new NativeArray<float3>(vertList.ToArray(), Allocator.TempJob);
                triangles = new NativeArray<int>(triList.ToArray(), Allocator.TempJob);
                meshData = new NativeArray<MeshData>(meshDataList.ToArray(), Allocator.TempJob);
                results = new NativeArray<int2>(rayCount, Allocator.TempJob);

                var job = new RaycastJob
                {
                    rays = rays,
                    vertices = vertices,
                    triangles = triangles,
                    meshData = meshData,
                    results = results
                };

                JobHandle handle = job.Schedule(rayCount, 32);
                handle.Complete();

                for (int i = 0; i < results.Length; i++)
                {
                    int2 result = results[i];
                    if (result.x != -1)
                    {
                        Mesh hitMesh = sourceMeshes[result.x];
                        if (!visibleTrianglesCache.ContainsKey(hitMesh))
                        {
                            visibleTrianglesCache[hitMesh] = new HashSet<int>();
                        }
                        visibleTrianglesCache[hitMesh].Add(result.y);
                    }
                }

                stopwatch.Stop();
                // Debug.Log($"Visibility cache built with Jobs in {stopwatch.ElapsedMilliseconds}ms");

                isVisibilityCacheValid = true;
            }
            finally
            {
                // Dispose all NativeArrays in finally block to prevent memory leaks
                if (rays.IsCreated) rays.Dispose();
                if (vertices.IsCreated) vertices.Dispose();
                if (triangles.IsCreated) triangles.Dispose();
                if (meshData.IsCreated) meshData.Dispose();
                if (results.IsCreated) results.Dispose();
            }
        }

        private bool DoPaintSelection(Vector2 mousePosition, bool isShiftHeld, bool isCtrlHeld)
        {
            if (activeEditingCopy == null) return false;

            var renderersToPaint = this.isolatedRendererCache
                .Where(r => GetMeshFromObject(r.gameObject) != null)
                .ToList();

            if (renderersToPaint.Count == 0) return false;

            if (!isXRaySelectionEnabled)
            {
                // Build a larger area for the visibility cache to allow some drag room
                // Use 3x brush size to reduce frequency of cache rebuilds during painting
                float cacheRadius = paintBrushRadius * 3f;
                Rect currentBrushArea = new Rect(mousePosition.x - paintBrushRadius, mousePosition.y - paintBrushRadius, paintBrushRadius * 2, paintBrushRadius * 2);

                // Check if current brush area is outside the cached area - if so, invalidate
                bool needsRebuild = !isVisibilityCacheValid || !lastVisibilityCacheArea.Contains(currentBrushArea.min) || !lastVisibilityCacheArea.Contains(currentBrushArea.max);

                if (needsRebuild)
                {
                    isVisibilityCacheValid = false;
                    Rect area = new Rect(mousePosition.x - cacheRadius, mousePosition.y - cacheRadius, cacheRadius * 2, cacheRadius * 2);
                    BuildVisibilityCacheWithJobs(area, renderersToPaint);
                }
            }

            bool anyHit = false;
            foreach (var renderer in renderersToPaint)
            {
                if (PaintOnObject(renderer.gameObject, mousePosition, isShiftHeld, isCtrlHeld))
                {
                    anyHit = true;
                }
            }

            if (anyHit) RepaintSceneAndWindow();
            return anyHit;
        }

        /// <summary>
        /// OPTIMIZED: Uses cached screen positions and early bounds check.
        /// </summary>
        private bool PaintOnObject(GameObject targetObject, Vector2 mousePosition, bool isShiftHeld, bool isCtrlHeld)
        {
            Mesh meshToPaint = GetMeshFromObject(targetObject);
            if (meshToPaint == null) return false;

            var transform = targetObject.transform;

            // Update screen position cache
            UpdateScreenPositionCache(meshToPaint, transform);

            // EARLY OUT: Check if brush overlaps mesh screen bounds
            if (cachedScreenBounds.TryGetValue(meshToPaint, out var bounds))
            {
                Rect brushRect = new Rect(
                    mousePosition.x - paintBrushRadius - 10,
                    mousePosition.y - paintBrushRadius - 10,
                    paintBrushRadius * 2 + 20,
                    paintBrushRadius * 2 + 20);

                if (!bounds.Overlaps(brushRect))
                    return false;
            }

            if (!cachedScreenPositions.TryGetValue(meshToPaint, out var screenPos))
                return false;

            var vertToTriMap = GetVertexToTrianglesMap(meshToPaint);
            var trisToChange = new HashSet<int>();
            float radiusSq = paintBrushRadius * paintBrushRadius;

            // Use cached screen positions
            for (int i = 0; i < screenPos.Length; i++)
            {
                float distSq = (mousePosition - screenPos[i]).sqrMagnitude;

                if (distSq < radiusSq)
                {
                    foreach (var triIndex in vertToTriMap[i])
                    {
                        bool isVisible = isXRaySelectionEnabled ||
                            (visibleTrianglesCache.ContainsKey(meshToPaint) &&
                             visibleTrianglesCache[meshToPaint].Contains(triIndex));

                        if (isVisible)
                        {
                            trisToChange.Add(triIndex);
                        }
                    }
                }
            }

            if (trisToChange.Count > 0)
            {
                if (!selectedTriangles.ContainsKey(meshToPaint))
                {
                    selectedTriangles[meshToPaint] = new HashSet<int>();
                }

                if (isCtrlHeld)
                    selectedTriangles[meshToPaint].ExceptWith(trisToChange);
                else
                    selectedTriangles[meshToPaint].UnionWith(trisToChange);

                separatorTargetObject = targetObject;
                separatorTargetMesh = meshToPaint;
                UpdateCachedSelectionCount();
                return true;
            }
            return false;
        }

        private void InitiateBoxSelection(Vector2 start, Vector2 end, bool isShiftHeld, bool isCtrlHeld)
        {
            if (isProcessingAsync) return;

            isProcessingAsync = true;
            asyncMessage = "Processing Box Selection...";
            asyncProcessor = ProcessBoxSelectionAsync(start, end, isShiftHeld, isCtrlHeld);
        }

        private IEnumerator<float> ProcessBoxSelectionAsync(Vector2 start, Vector2 end, bool isShiftHeld, bool isCtrlHeld)
        {
            Rect selectionRect = new Rect(Mathf.Min(start.x, end.x), Mathf.Min(start.y, end.y), Mathf.Abs(start.x - end.x), Mathf.Abs(start.y - end.y));
            var trianglesToModify = new Dictionary<Mesh, HashSet<int>>();

            if (activeEditingCopy == null)
            {
                isProcessingAsync = false;
                yield break;
            }

            var renderersToProcess = this.isolatedRendererCache
                .Where(r => GetMeshFromObject(r.gameObject) != null)
                .ToList();

            if (!isXRaySelectionEnabled)
            {
                asyncMessage = "Building Visibility Map (Jobs)...";
                BuildVisibilityCacheWithJobs(selectionRect, renderersToProcess);
                yield return 0.2f;
            }

            asyncMessage = "Finding triangles in selection...";

            int totalTriangleCount = 0;
            foreach (var r in renderersToProcess)
            {
                var m = GetMeshFromObject(r.gameObject);
                if (m != null)
                {
                    var tris = GetCachedTriangles(m);
                    if (tris != null) totalTriangleCount += tris.Length;
                }
            }
            totalTriangleCount /= 3;

            int processedTriangleCount = 0;
            const int trianglesPerFrame = 10000;

            foreach (var renderer in renderersToProcess)
            {
                var mesh = GetMeshFromObject(renderer.gameObject);
                if (mesh == null) continue;

                var tris = GetCachedTriangles(mesh);
                var verts = GetCachedVertices(mesh);
                if (tris == null || verts == null) continue;
                
                var objectTransform = renderer.transform;

                for (int i = 0; i < tris.Length / 3; i++)
                {
                    processedTriangleCount++;

                    // Bounds validation for triangle indices
                    int idx0 = tris[i * 3 + 0];
                    int idx1 = tris[i * 3 + 1];
                    int idx2 = tris[i * 3 + 2];
                    if (idx0 < 0 || idx0 >= verts.Length ||
                        idx1 < 0 || idx1 >= verts.Length ||
                        idx2 < 0 || idx2 >= verts.Length)
                    {
                        continue; // Skip invalid triangle
                    }

                    Vector3 v0_local = verts[idx0];
                    Vector3 v1_local = verts[idx1];
                    Vector3 v2_local = verts[idx2];

                    Vector2 screenV0 = HandleUtility.WorldToGUIPoint(objectTransform.TransformPoint(v0_local));
                    Vector2 screenV1 = HandleUtility.WorldToGUIPoint(objectTransform.TransformPoint(v1_local));
                    Vector2 screenV2 = HandleUtility.WorldToGUIPoint(objectTransform.TransformPoint(v2_local));

                    if (selectionRect.Contains(screenV0) || selectionRect.Contains(screenV1) || selectionRect.Contains(screenV2))
                    {
                        bool isVisible = isXRaySelectionEnabled || (visibleTrianglesCache.ContainsKey(mesh) && visibleTrianglesCache[mesh].Contains(i));

                        if (isVisible)
                        {
                            if (!trianglesToModify.ContainsKey(mesh)) trianglesToModify[mesh] = new HashSet<int>();
                            trianglesToModify[mesh].Add(i);
                        }
                    }

                    if (totalTriangleCount > 0 && processedTriangleCount % trianglesPerFrame == 0)
                    {
                        yield return 0.2f + (0.8f * ((float)processedTriangleCount / totalTriangleCount));
                    }
                }
            }

            if (!isShiftHeld && !isCtrlHeld)
            {
                if (!isolateToHierarchySelection)
                {
                    ClearTriangleSelection();
                }
            }

            foreach (var pair in trianglesToModify)
            {
                if (!selectedTriangles.ContainsKey(pair.Key)) selectedTriangles[pair.Key] = new HashSet<int>();
                if (isCtrlHeld) selectedTriangles[pair.Key].ExceptWith(pair.Value);
                else selectedTriangles[pair.Key].UnionWith(pair.Value);
            }

            if (renderersToProcess.Any() && trianglesToModify.Any())
            {
                GameObject firstObject = FindObjectForMesh(trianglesToModify.First().Key);
                if (firstObject)
                {
                    separatorTargetObject = firstObject;
                    separatorTargetMesh = GetMeshFromObject(firstObject);
                }
            }

            UpdateCachedSelectionCount();
            SceneView.RepaintAll();
            yield return 1f;
        }
        #endregion

        #region Mesh Operations

        /// <summary>
        /// OPTIMIZED: Batches asset database operations for faster saving.
        /// </summary>
        private void SeparateSelectedTriangles()
        {
            if (selectedTriangles.Count == 0)
            {
                EditorUtility.DisplayDialog("Separation Error", "No triangles selected to separate.", "OK");
                return;
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            Undo.SetCurrentGroupName("Separate Mesh");
            string defaultPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(sourceObject));
            List<GameObject> newGroupParents = new List<GameObject>();
            List<(Mesh mesh, string name)> meshesToSave = new List<(Mesh, string)>();
            bool rootWasDestroyed = false;

            foreach (var pair in selectedTriangles.ToList())
            {
                Mesh sourceMesh = pair.Key;
                HashSet<int> selection = pair.Value;
                GameObject originalObject = FindObjectForMesh(sourceMesh);

                if (originalObject == null || sourceMesh == null || selection.Count == 0) continue;

                // Cache all mesh data before separation
                var meshDataCache = CacheMeshDataForSeparation(sourceMesh);
                if (meshDataCache == null) continue;

                var (selectionMesh, remainingMesh) = CreateSeparatedMeshesOptimized(sourceMesh, selection, meshDataCache);

                if (selectionMesh == null || remainingMesh == null)
                {
                    Debug.LogWarning($"Separation failed for {originalObject.name}: Could not create new meshes. Ensure selection is not empty or the entire mesh.");
                    continue;
                }

                var originalRenderer = originalObject.GetComponent<Renderer>();
                Transform originalParent = originalObject.transform.parent;
                int originalSiblingIndex = originalObject.transform.GetSiblingIndex();

                GameObject groupParent = new GameObject(originalObject.name + "_parts");
                Undo.RegisterCreatedObjectUndo(groupParent, "Create group parent");
                groupParent.transform.SetParent(originalParent, false);
                groupParent.transform.SetPositionAndRotation(originalObject.transform.position, originalObject.transform.rotation);
                groupParent.transform.SetSiblingIndex(originalSiblingIndex);
                groupParent.transform.localScale = originalObject.transform.localScale;
                newGroupParents.Add(groupParent);

                GameObject separatedObject = CreateMeshObject(originalObject.name + "_separated", selectionMesh, originalRenderer, groupParent.transform);
                GameObject remainderObject = CreateMeshObject(originalObject.name + "_remainder", remainingMesh, originalRenderer, groupParent.transform);

                // Queue meshes for batch saving
                meshesToSave.Add((selectionMesh, separatedObject.name));
                meshesToSave.Add((remainingMesh, remainderObject.name));

                if (originalObject == activeEditingCopy)
                {
                    rootWasDestroyed = true;
                }
                Undo.DestroyObjectImmediate(originalObject);
            }

            // Batch save all meshes
            SaveMeshAssetsBatched(meshesToSave, defaultPath);

            if (rootWasDestroyed && newGroupParents.Count > 0)
            {
                activeEditingCopy = newGroupParents[0];
                Debug.Log($"Re-assigned active editing copy to {activeEditingCopy.name}");
            }

            stopwatch.Stop();
            Debug.Log($"Successfully separated {selectedTriangles.Count} meshes into new groups in {stopwatch.ElapsedMilliseconds}ms.");
            if (newGroupParents.Any()) Selection.activeObject = newGroupParents.First();
            ClearTriangleSelection();
        }

        /// <summary>
        /// OPTIMIZED: Batches asset database operations for faster saving.
        /// </summary>
        private void ExtractSelectedTriangles()
        {
            if (selectedTriangles.Count == 0) return;

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            Undo.SetCurrentGroupName("Extract Mesh");
            string defaultPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(sourceObject));
            var newObjects = new List<GameObject>();
            List<(Mesh mesh, string name)> meshesToSave = new List<(Mesh, string)>();

            foreach (var pair in selectedTriangles)
            {
                Mesh sourceMesh = pair.Key;
                HashSet<int> selection = pair.Value;
                if (sourceMesh == null || selection.Count == 0) continue;

                GameObject originalObject = FindObjectForMesh(sourceMesh);
                if (originalObject == null) continue;

                // Cache all mesh data before separation
                var meshDataCache = CacheMeshDataForSeparation(sourceMesh);
                if (meshDataCache == null) continue;

                var (selectionMesh, _) = CreateSeparatedMeshesOptimized(sourceMesh, selection, meshDataCache);
                if (selectionMesh == null) continue;

                var originalRenderer = originalObject.GetComponent<Renderer>();

                GameObject extractedObject = CreateMeshObject(originalObject.name + "_extracted", selectionMesh, originalRenderer, activeEditingCopy.transform);

                extractedObject.transform.SetPositionAndRotation(originalObject.transform.position, originalObject.transform.rotation);
                extractedObject.transform.localScale = originalObject.transform.localScale;

                meshesToSave.Add((selectionMesh, extractedObject.name));
                newObjects.Add(extractedObject);
            }

            // Batch save all meshes
            SaveMeshAssetsBatched(meshesToSave, defaultPath);

            stopwatch.Stop();
            Debug.Log($"Successfully extracted selections from {selectedTriangles.Count} meshes into {newObjects.Count} new object(s) in {stopwatch.ElapsedMilliseconds}ms.");

            if (newObjects.Any()) Selection.activeObject = newObjects.First();

            ClearTriangleSelection();
        }

        private void SeparateByLooseParts()
        {
            if (separatorTargetObject == null || separatorTargetMesh == null)
            {
                EditorUtility.DisplayDialog("Separation Error", "No valid mesh object selected.", "OK");
                return;
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            Undo.SetCurrentGroupName("Separate By Loose Parts");

            var adjacency = GetTriangleAdjacency(separatorTargetMesh);
            var parts = FindMeshIslands(separatorTargetMesh, adjacency);

            if (parts.Count <= 1)
            {
                EditorUtility.DisplayDialog("Separation Info", "Could not find any loose parts to separate. The mesh is fully connected.", "OK");
                return;
            }

            Debug.Log($"Found {parts.Count} loose parts. Creating new objects...");

            // Cache mesh data once for all parts
            var meshDataCache = CacheMeshDataForSeparation(separatorTargetMesh);
            if (meshDataCache == null) return;

            var originalRenderer = separatorTargetObject.GetComponent<Renderer>();
            string defaultPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(sourceObject));
            List<(Mesh mesh, string name)> meshesToSave = new List<(Mesh, string)>();

            var (part0Mesh, _) = CreateSeparatedMeshesOptimized(separatorTargetMesh, parts[0], meshDataCache);

            if (part0Mesh == null)
            {
                Debug.LogError("Failed to create mesh for part 0. Aborting separation.");
                return;
            }

            if (separatorTargetObject.TryGetComponent<MeshFilter>(out var mf))
            {
                Undo.RecordObject(mf, "Update Original Mesh to Part 0");
                mf.sharedMesh = part0Mesh;
            }
            else if (separatorTargetObject.TryGetComponent<SkinnedMeshRenderer>(out var smr))
            {
                Undo.RecordObject(smr, "Update Original Mesh to Part 0");
                smr.sharedMesh = part0Mesh;
            }
            meshesToSave.Add((part0Mesh, $"{separatorTargetObject.name}_part_0"));

            for (int i = 1; i < parts.Count; i++)
            {
                var (partMesh, _) = CreateSeparatedMeshesOptimized(separatorTargetMesh, parts[i], meshDataCache);
                if (partMesh == null)
                {
                    Debug.LogWarning($"Failed to create mesh for part {i}. Skipping.");
                    continue;
                }
                GameObject partObject = CreateMeshObject($"{separatorTargetObject.name}_part_{i}", partMesh, originalRenderer, activeEditingCopy.transform);
                partObject.transform.position = separatorTargetObject.transform.position;
                partObject.transform.rotation = separatorTargetObject.transform.rotation;
                partObject.transform.localScale = separatorTargetObject.transform.localScale;
                meshesToSave.Add((partMesh, partObject.name));
            }

            // Batch save all meshes
            SaveMeshAssetsBatched(meshesToSave, defaultPath);

            stopwatch.Stop();
            Debug.Log($"Separated into {parts.Count} parts in {stopwatch.ElapsedMilliseconds}ms.");
            
            ClearTriangleSelection();
        }

        #region Optimized Mesh Separation Helpers

        /// <summary>
        /// Cached mesh data structure to avoid repeated array allocations.
        /// </summary>
        private class SeparationMeshData
        {
            public Vector3[] vertices;
            public Vector3[] normals;
            public Vector4[] tangents;
            public Vector2[] uv;
            public Vector2[] uv2;
            public Vector2[] uv3;
            public Vector2[] uv4;
            public Color[] colors;
            public BoneWeight[] boneWeights;
            public Matrix4x4[] bindposes;
            public int[] triangles;
            public int vertexCount;
            public int subMeshCount;
        }

        /// <summary>
        /// Caches ALL mesh data in one go to avoid repeated allocations during separation.
        /// </summary>
        private SeparationMeshData CacheMeshDataForSeparation(Mesh sourceMesh)
        {
            if (sourceMesh == null) return null;

            return new SeparationMeshData
            {
                vertices = sourceMesh.vertices,
                normals = sourceMesh.normals,
                tangents = sourceMesh.tangents,
                uv = sourceMesh.uv,
                uv2 = sourceMesh.uv2,
                uv3 = sourceMesh.uv3,
                uv4 = sourceMesh.uv4,
                colors = sourceMesh.colors,
                boneWeights = sourceMesh.boneWeights,
                bindposes = sourceMesh.bindposes,
                triangles = sourceMesh.triangles,
                vertexCount = sourceMesh.vertexCount,
                subMeshCount = sourceMesh.subMeshCount
            };
        }

        /// <summary>
        /// OPTIMIZED: Uses pre-cached mesh data instead of accessing mesh properties repeatedly.
        /// </summary>
        private (Mesh selectionMesh, Mesh remainingMesh) CreateSeparatedMeshesOptimized(
            Mesh sourceMesh, HashSet<int> selection, SeparationMeshData cache)
        {
            if (cache.triangles == null) return (null, null);

            if (selection.Count == 0 || selection.Count == cache.triangles.Length / 3)
            {
                return (null, null);
            }

            // Pre-calculate capacity for better memory allocation
            int estimatedSelectionVerts = selection.Count * 3;
            int estimatedRemainingVerts = (cache.triangles.Length / 3 - selection.Count) * 3;

            var selectionSubmeshTriangles = new List<int>[cache.subMeshCount];
            var remainingSubmeshTriangles = new List<int>[cache.subMeshCount];
            for (int i = 0; i < cache.subMeshCount; i++)
            {
                selectionSubmeshTriangles[i] = new List<int>(estimatedSelectionVerts / cache.subMeshCount);
                remainingSubmeshTriangles[i] = new List<int>(estimatedRemainingVerts / cache.subMeshCount);
            }

            for (int i = 0; i < cache.subMeshCount; i++)
            {
                var submesh = sourceMesh.GetSubMesh(i);
                for (int j = 0; j < submesh.indexCount; j += 3)
                {
                    int triangleIndex = (submesh.indexStart + j) / 3;
                    var listToAddTo = selection.Contains(triangleIndex) ? selectionSubmeshTriangles[i] : remainingSubmeshTriangles[i];
                    listToAddTo.Add(cache.triangles[triangleIndex * 3]);
                    listToAddTo.Add(cache.triangles[triangleIndex * 3 + 1]);
                    listToAddTo.Add(cache.triangles[triangleIndex * 3 + 2]);
                }
            }

            Mesh selectionMesh = CreateMeshFromDataOptimized("Selection", sourceMesh, selectionSubmeshTriangles, cache);
            Mesh remainingMesh = CreateMeshFromDataOptimized("Remaining", sourceMesh, remainingSubmeshTriangles, cache);

            return (selectionMesh, remainingMesh);
        }

        /// <summary>
        /// OPTIMIZED: Uses cached mesh data and pre-allocated arrays.
        /// The original version called sourceMesh.vertices, .normals, etc. inside the loop,
        /// which allocates a NEW array every single time!
        /// </summary>
        private Mesh CreateMeshFromDataOptimized(string name, Mesh sourceMesh, List<int>[] submeshTriangles, SeparationMeshData cache)
        {
            // Calculate total vertex indices to process
            int totalIndices = 0;
            foreach (var list in submeshTriangles) totalIndices += list.Count;
            if (totalIndices == 0) return null;

            // Estimate unique vertices (typically 1/3 to 1/2 of indices due to sharing)
            int estimatedVertCount = totalIndices / 2;

            // Pre-allocate with estimated capacity
            var newVerts = new List<Vector3>(estimatedVertCount);
            var newNormals = cache.normals.Length > 0 ? new List<Vector3>(estimatedVertCount) : null;
            var newTangents = cache.tangents.Length > 0 ? new List<Vector4>(estimatedVertCount) : null;
            var newUVs = cache.uv.Length > 0 ? new List<Vector2>(estimatedVertCount) : null;
            var newUV2s = cache.uv2.Length > 0 ? new List<Vector2>(estimatedVertCount) : null;
            var newUV3s = cache.uv3.Length > 0 ? new List<Vector2>(estimatedVertCount) : null;
            var newUV4s = cache.uv4.Length > 0 ? new List<Vector2>(estimatedVertCount) : null;
            var newColors = cache.colors.Length > 0 ? new List<Color>(estimatedVertCount) : null;
            var newBoneWeights = cache.boneWeights.Length > 0 ? new List<BoneWeight>(estimatedVertCount) : null;
            
            var newTrisBySubmesh = new List<int>[submeshTriangles.Length];
            
            // Use array for vertex remapping (faster than Dictionary for dense indices)
            int[] vertMap = new int[cache.vertexCount];
            for (int i = 0; i < vertMap.Length; i++) vertMap[i] = -1;

            for (int i = 0; i < submeshTriangles.Length; i++)
            {
                var sourceList = submeshTriangles[i];
                newTrisBySubmesh[i] = new List<int>(sourceList.Count);
                
                foreach (int oldIndex in sourceList)
                {
                    if (oldIndex < 0 || oldIndex >= cache.vertexCount) continue;

                    int newIndex = vertMap[oldIndex];
                    if (newIndex == -1)
                    {
                        // New vertex - add to all attribute lists
                        newIndex = newVerts.Count;
                        vertMap[oldIndex] = newIndex;
                        
                        newVerts.Add(cache.vertices[oldIndex]);
                        
                        if (newNormals != null && oldIndex < cache.normals.Length)
                            newNormals.Add(cache.normals[oldIndex]);
                        if (newTangents != null && oldIndex < cache.tangents.Length)
                            newTangents.Add(cache.tangents[oldIndex]);
                        if (newUVs != null && oldIndex < cache.uv.Length)
                            newUVs.Add(cache.uv[oldIndex]);
                        if (newUV2s != null && oldIndex < cache.uv2.Length)
                            newUV2s.Add(cache.uv2[oldIndex]);
                        if (newUV3s != null && oldIndex < cache.uv3.Length)
                            newUV3s.Add(cache.uv3[oldIndex]);
                        if (newUV4s != null && oldIndex < cache.uv4.Length)
                            newUV4s.Add(cache.uv4[oldIndex]);
                        if (newColors != null && oldIndex < cache.colors.Length)
                            newColors.Add(cache.colors[oldIndex]);
                        if (newBoneWeights != null && oldIndex < cache.boneWeights.Length)
                            newBoneWeights.Add(cache.boneWeights[oldIndex]);
                    }
                    newTrisBySubmesh[i].Add(newIndex);
                }
            }

            if (newVerts.Count == 0) return null;

            // Create the new mesh
            Mesh newMesh = new Mesh();
            newMesh.name = $"{sourceMesh.name}_{name}";
            
            // Use 32-bit indices if needed
            if (newVerts.Count > 65535)
            {
                newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            newMesh.SetVertices(newVerts);
            if (newNormals != null && newNormals.Count == newVerts.Count) newMesh.SetNormals(newNormals);
            if (newTangents != null && newTangents.Count == newVerts.Count) newMesh.SetTangents(newTangents);
            if (newUVs != null && newUVs.Count == newVerts.Count) newMesh.SetUVs(0, newUVs);
            if (newUV2s != null && newUV2s.Count == newVerts.Count) newMesh.SetUVs(1, newUV2s);
            if (newUV3s != null && newUV3s.Count == newVerts.Count) newMesh.SetUVs(2, newUV3s);
            if (newUV4s != null && newUV4s.Count == newVerts.Count) newMesh.SetUVs(3, newUV4s);
            if (newColors != null && newColors.Count == newVerts.Count) newMesh.SetColors(newColors);

            if (newBoneWeights != null && newBoneWeights.Count == newVerts.Count)
            {
                newMesh.boneWeights = newBoneWeights.ToArray();
                newMesh.bindposes = cache.bindposes;
            }

            newMesh.subMeshCount = submeshTriangles.Length;
            for (int i = 0; i < submeshTriangles.Length; i++)
            {
                newMesh.SetTriangles(newTrisBySubmesh[i], i);
            }

            newMesh.RecalculateBounds();
            return newMesh;
        }

        /// <summary>
        /// Batch saves multiple meshes with a single AssetDatabase refresh.
        /// </summary>
        private void SaveMeshAssetsBatched(List<(Mesh mesh, string name)> meshes, string defaultPath)
        {
            if (meshes.Count == 0) return;

            // Start batching asset operations
            AssetDatabase.StartAssetEditing();
            
            try
            {
                foreach (var (mesh, meshName) in meshes)
                {
                    SaveMeshAsset(mesh, meshName, defaultPath);
                }
            }
            finally
            {
                // Stop batching and refresh once
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }
        }

        #endregion

        private (Mesh selectionMesh, Mesh remainingMesh) CreateSeparatedMeshes(Mesh sourceMesh, HashSet<int> selection)
        {
            // Use the optimized version with cached data
            var cache = CacheMeshDataForSeparation(sourceMesh);
            if (cache == null) return (null, null);
            return CreateSeparatedMeshesOptimized(sourceMesh, selection, cache);
        }

        private GameObject CreateMeshObject(string name, Mesh mesh, Renderer originalRenderer, Transform parent)
        {
            GameObject newObject = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(newObject, "Create Mesh Object");
            newObject.transform.SetParent(parent, false);

            if (originalRenderer is SkinnedMeshRenderer originalSmr)
            {
                var smr = newObject.AddComponent<SkinnedMeshRenderer>();
                smr.sharedMesh = mesh;
                smr.sharedMaterials = originalSmr.sharedMaterials;
                smr.bones = originalSmr.bones;
                smr.rootBone = originalSmr.rootBone;
            }
            else
            {
                newObject.AddComponent<MeshFilter>().sharedMesh = mesh;
                newObject.AddComponent<MeshRenderer>().sharedMaterials = originalRenderer.sharedMaterials;
            }
            return newObject;
        }

        #endregion

        #region Selection & Adjacency Helpers

        /// <summary>
        /// Updates the cached selected triangle count. Call after any selection modification.
        /// </summary>
        private void UpdateCachedSelectionCount()
        {
            int count = 0;
            foreach (var pair in selectedTriangles)
                count += pair.Value.Count;
            cachedSelectedTriangleCount = count;
            selectionVersion++; // Increment to invalidate world vertex drawing cache
        }

        private void ClearTriangleSelection()
        {
            selectedTriangles.Clear();
            separatorTargetObject = null;
            separatorTargetMesh = null;
            meshTriangleAdjacency.Clear();
            vertexToTrianglesMapCache.Clear();
            isVisibilityCacheValid = false;
            isolatedRendererCacheDirty = true;
            ClearMeshCaches();
            cachedWorldVertsForDrawing.Clear();
            UpdateCachedSelectionCount();
            RepaintSceneAndWindow();
        }

        private void SelectAllTriangles()
        {
            if (separatorTargetObject == null) return;
            Mesh mesh = GetMeshFromObject(separatorTargetObject);
            if (mesh == null) return;

            var tris = GetCachedTriangles(mesh);
            if (tris == null) return;

            var allTris = Enumerable.Range(0, tris.Length / 3);
            selectedTriangles[mesh] = new HashSet<int>(allTris);
            UpdateCachedSelectionCount();
            RepaintSceneAndWindow();
        }

        private void InvertSelection()
        {
            if (separatorTargetObject == null) return;
            Mesh mesh = GetMeshFromObject(separatorTargetObject);
            if (mesh == null) return;

            var tris = GetCachedTriangles(mesh);
            if (tris == null) return;

            var allTris = Enumerable.Range(0, tris.Length / 3);
            var currentSelection = selectedTriangles.ContainsKey(mesh) ? selectedTriangles[mesh] : new HashSet<int>();
            var inverted = new HashSet<int>(allTris);
            inverted.ExceptWith(currentSelection);
            selectedTriangles[mesh] = inverted;
            UpdateCachedSelectionCount();
            RepaintSceneAndWindow();
        }

        private void InitiateGrowSelection()
        {
            if (isProcessingAsync || selectedTriangles.Count == 0) return;

            isProcessingAsync = true;
            asyncMessage = "Processing Adjacency Data...";
            asyncProcessor = ProcessGrowSelectionAsync();
        }

        private IEnumerator<float> ProcessGrowSelectionAsync()
        {
            var meshesToProcess = selectedTriangles.Keys.Where(m => m != null && !meshTriangleAdjacency.ContainsKey(m)).ToList();
            float totalProgress = 0f;
            float progressPerMesh = meshesToProcess.Count > 0 ? 0.9f / meshesToProcess.Count : 0f;

            foreach (var mesh in meshesToProcess)
            {
                asyncMessage = $"Building Adjacency for {mesh.name}...";

                var triangles = GetCachedTriangles(mesh);
                if (triangles == null) continue;
                
                int triangleCount = triangles.Length / 3;
                var adjacency = new int[triangleCount][];
                var edgeToTriangleMap = new Dictionary<KeyValuePair<int, int>, List<int>>();
                const int trianglesPerFrame = 4000;

                for (int i = 0; i < triangleCount; i++)
                {
                    int i0 = triangles[i * 3 + 0];
                    int i1 = triangles[i * 3 + 1];
                    int i2 = triangles[i * 3 + 2];

                    var edges = new[] {
                        new KeyValuePair<int, int>(Mathf.Min(i0, i1), Mathf.Max(i0, i1)),
                        new KeyValuePair<int, int>(Mathf.Min(i1, i2), Mathf.Max(i1, i2)),
                        new KeyValuePair<int, int>(Mathf.Min(i2, i0), Mathf.Max(i2, i0))
                    };

                    foreach (var edge in edges)
                    {
                        if (!edgeToTriangleMap.ContainsKey(edge))
                        {
                            edgeToTriangleMap[edge] = new List<int>();
                        }
                        edgeToTriangleMap[edge].Add(i);
                    }

                    if (i > 0 && i % trianglesPerFrame == 0)
                    {
                        yield return totalProgress + (progressPerMesh * ((float)i / triangleCount));
                    }
                }

                for (int i = 0; i < triangleCount; i++)
                {
                    var neighbors = new HashSet<int>();
                    int i0 = triangles[i * 3 + 0];
                    int i1 = triangles[i * 3 + 1];
                    int i2 = triangles[i * 3 + 2];

                    var edges = new[] {
                        new KeyValuePair<int, int>(Mathf.Min(i0, i1), Mathf.Max(i0, i1)),
                        new KeyValuePair<int, int>(Mathf.Min(i1, i2), Mathf.Max(i1, i2)),
                        new KeyValuePair<int, int>(Mathf.Min(i2, i0), Mathf.Max(i2, i0))
                    };

                    foreach (var edge in edges)
                    {
                        foreach (int otherTri in edgeToTriangleMap[edge])
                        {
                            if (otherTri != i)
                            {
                                neighbors.Add(otherTri);
                            }
                        }
                    }
                    adjacency[i] = neighbors.ToArray();
                }

                meshTriangleAdjacency[mesh] = adjacency;
                totalProgress += progressPerMesh;
                yield return totalProgress;
            }

            asyncMessage = "Growing Selection...";

            var growth = new Dictionary<Mesh, HashSet<int>>();
            foreach (var pair in selectedTriangles)
            {
                var mesh = pair.Key;
                var selection = pair.Value;
                if (!meshTriangleAdjacency.TryGetValue(mesh, out var adjacency)) continue;

                var toAdd = new HashSet<int>();

                foreach (int triIndex in selection)
                {
                    if (triIndex >= adjacency.Length) continue;
                    foreach (int neighbor in adjacency[triIndex])
                    {
                        if (neighbor != -1 && !selection.Contains(neighbor))
                        {
                            toAdd.Add(neighbor);
                        }
                    }
                }
                if (toAdd.Any()) growth[mesh] = toAdd;
            }

            foreach (var pair in growth)
            {
                selectedTriangles[pair.Key].UnionWith(pair.Value);
            }

            UpdateCachedSelectionCount();
            RepaintSceneAndWindow();
            yield return 1f;
        }

        private void ShrinkSelection()
        {
            if (selectedTriangles.Count == 0) return;

            foreach (var pair in selectedTriangles.ToList())
            {
                var mesh = pair.Key;
                var selection = pair.Value;
                var adjacency = GetTriangleAdjacency(mesh);
                if (adjacency == null)
                {
                    EditorUtility.DisplayDialog("Adjacency Data Missing", $"Please use 'Grow' once on '{mesh.name}' to build its adjacency data before shrinking.", "OK");
                    continue;
                }
                var toRemove = new HashSet<int>();

                foreach (int triIndex in selection)
                {
                    bool isBorder = false;
                    if (triIndex >= adjacency.Length) continue;
                    foreach (int neighbor in adjacency[triIndex])
                    {
                        if (neighbor == -1 || !selection.Contains(neighbor))
                        {
                            isBorder = true;
                            break;
                        }
                    }
                    if (isBorder)
                    {
                        toRemove.Add(triIndex);
                    }
                }
                selection.ExceptWith(toRemove);
            }
            UpdateCachedSelectionCount();
            RepaintSceneAndWindow();
        }

        private int[][] GetTriangleAdjacency(Mesh mesh)
        {
            if (mesh == null) return null;
            if (meshTriangleAdjacency.TryGetValue(mesh, out var cachedAdjacency))
            {
                return cachedAdjacency;
            }

            Debug.LogWarning($"Adjacency data for {mesh.name} was not pre-cached. Please use the 'Grow' button to generate it first.");
            return null;
        }

        private List<int>[] GetVertexToTrianglesMap(Mesh mesh)
        {
            if (vertexToTrianglesMapCache.TryGetValue(mesh, out var map))
            {
                return map;
            }

            var verts = GetCachedVertices(mesh);
            var triangles = GetCachedTriangles(mesh);
            if (verts == null || triangles == null) return null;

            map = new List<int>[verts.Length];
            for (int i = 0; i < verts.Length; i++)
            {
                map[i] = new List<int>();
            }

            for (int i = 0; i < triangles.Length / 3; i++)
            {
                map[triangles[i * 3 + 0]].Add(i);
                map[triangles[i * 3 + 1]].Add(i);
                map[triangles[i * 3 + 2]].Add(i);
            }

            vertexToTrianglesMapCache[mesh] = map;
            return map;
        }

        private List<HashSet<int>> FindMeshIslands(Mesh mesh, int[][] adjacency)
        {
            var tris = GetCachedTriangles(mesh);
            if (tris == null) return new List<HashSet<int>>();
            
            var visited = new bool[tris.Length / 3];
            var parts = new List<HashSet<int>>();

            for (int i = 0; i < visited.Length; i++)
            {
                if (!visited[i])
                {
                    var newPart = new HashSet<int>();
                    var queue = new Queue<int>();

                    queue.Enqueue(i);
                    visited[i] = true;

                    while (queue.Count > 0)
                    {
                        int current = queue.Dequeue();
                        newPart.Add(current);

                        if (adjacency == null || current >= adjacency.Length) continue;

                        foreach (int neighbor in adjacency[current])
                        {
                            if (neighbor < 0 || neighbor >= visited.Length) continue;
                            if (!visited[neighbor])
                            {
                                visited[neighbor] = true;
                                queue.Enqueue(neighbor);
                            }
                        }
                    }
                    parts.Add(newPart);
                }
            }
            return parts;
        }

        private void RepaintSceneAndWindow()
        {
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.Repaint();
            }
            Repaint();
        }

        private GameObject FindObjectForMesh(Mesh mesh)
        {
            if (mesh == null || activeEditingCopy == null) return null;

            foreach (var r in this.isolatedRendererCache)
            {
                if (r == null) continue;
                var m = GetMeshFromObject(r.gameObject);
                if (m == mesh) return r.gameObject;
            }

            var renderers = activeEditingCopy.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                if (r == null) continue;
                var m = GetMeshFromObject(r.gameObject);
                if (m == mesh) return r.gameObject;
            }
            return null;
        }
        #endregion
    }
}