using UnityEngine;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;
using System.IO;
using System.Reflection; // Required for Gizmo toggling
using System.Collections.Generic; // Required for Async Processor

namespace FBXEditor.Editor
{
    public partial class FBXEditor : EditorWindow
{
    // --- Core State Variables ---
    private GameObject sourceObject;
    private GameObject activeEditingCopy;
    private bool shouldOverwrite;
    private int currentTab = 0;

    // --- Scroll View Positions for Each Tab ---
    private Vector2 hierarchyEditorScrollPos;
    private Vector2 meshCombinerScrollPos;
    private Vector2 pivotEditorScrollPos;
    private Vector2 textureLinkerScrollPos;
    private Vector2 meshSeparatorScrollPos;

    // --- Editor Resources ---
    private GUIStyle watermarkStyle;
    // private Texture2D undoIcon;
    private bool originalShowSelectionWire;
    private PropertyInfo showSelectionWireProperty;

    private bool modelHasSkinnedMeshes = false;

    // --- Static UI Content (avoid per-frame allocations) ---
    private static readonly string[] TabNames = { "Hierarchy", "Combine", "Pivot", "Textures", "Separate" };

    // --- Asynchronous Processing Canvas ---
    private bool isProcessingAsync = false;
    private float asyncProgress = 0f;
    private string asyncMessage = "";
    private IEnumerator<float> asyncProcessor;


    [MenuItem("Tools/FBX Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<FBXEditor>("FBX Editor");
        // Set a more elegant default window size
        window.minSize = new Vector2(400, 400);
        if (window.position.width < 450 || window.position.height < 500)
        {
            window.position = new Rect(window.position.x, window.position.y, 500, 600);
        }
    }

    #region Unity Events

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        EditorApplication.update += EditorUpdate; // Hook into the editor's update loop

        // --- Get Gizmo Property via Reflection ---
        var annotationUtilityType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.AnnotationUtility");
        if (annotationUtilityType != null)
        {
            showSelectionWireProperty = annotationUtilityType.GetProperty("showSelectionOutline", BindingFlags.Static | BindingFlags.NonPublic);
        }

        // --- Initialize GUI Styles ---
        InitializeStyles();

        // --- Initialize Feature-Specific Data from Partial Classes ---
        InitializeMeshCombiner();
        InitializeTextureLinker();
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        EditorApplication.update -= EditorUpdate; // Unhook from the editor's update loop

        TrySetSelectionWireProperty(originalShowSelectionWire);
        CleanUpCopy();
    }
    
    /// <summary>
    /// Main update loop for the editor window, drives asynchronous processes.
    /// </summary>
    void EditorUpdate()
    {
        if (isProcessingAsync && asyncProcessor != null)
        {
            if (asyncProcessor.MoveNext())
            {
                asyncProgress = asyncProcessor.Current;
                Repaint(); // Redraw the window to update the progress bar
            }
            else
            {
                // Process finished
                isProcessingAsync = false;
                asyncProcessor = null;
                Repaint();
            }
        }
    }

    void OnGUI()
    {
        // Disable the entire UI if an async process is running
        using (new EditorGUI.DisabledScope(isProcessingAsync))
        {
            DrawToolbar();

            int previousTab = currentTab;
            currentTab = GUILayout.Toolbar(currentTab, TabNames);

            HandleTabSwitch(previousTab, currentTab);

            // Draw the content for the currently selected tab with scroll views.
            switch (currentTab)
            {
                case 0: 
                    hierarchyEditorScrollPos = EditorGUILayout.BeginScrollView(hierarchyEditorScrollPos);
                    DrawHierarchyEditorTab();
                    EditorGUILayout.EndScrollView();
                    break;
                case 1: 
                    meshCombinerScrollPos = EditorGUILayout.BeginScrollView(meshCombinerScrollPos);
                    DrawMeshCombinerTab();
                    EditorGUILayout.EndScrollView();
                    break;
                case 2: 
                    pivotEditorScrollPos = EditorGUILayout.BeginScrollView(pivotEditorScrollPos);
                    DrawPivotEditorTab();
                    EditorGUILayout.EndScrollView();
                    break;
                case 3: 
                    textureLinkerScrollPos = EditorGUILayout.BeginScrollView(textureLinkerScrollPos);
                    DrawTextureLinkerTab();
                    EditorGUILayout.EndScrollView();
                    break;
                case 4: 
                    meshSeparatorScrollPos = EditorGUILayout.BeginScrollView(meshSeparatorScrollPos);
                    DrawMeshSeparatorTab();
                    EditorGUILayout.EndScrollView();
                    break;
            }
        }
        
        // Draw the async progress bar canvas if a process is running
        if (isProcessingAsync)
        {
            DrawAsyncProgressCanvas();
        }
    }
    
    void OnSceneGUI(SceneView sceneView)
    {
        if (activeEditingCopy == null) return;

        DrawSceneWatermark(sceneView);

        // Don't allow scene interaction while processing
        if (isProcessingAsync)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            return;
        }

        switch(currentTab)
        {
            case 0: DrawBoneReorientPreview(sceneView); break;
            case 2: HandlePivotEditorSceneGUI(sceneView); break;
            case 4: HandleMeshSeparatorSceneGUI(sceneView); break;
        }
    }
    
    void OnInspectorUpdate() { Repaint(); }

    #endregion

    #region Core Logic

    private void HandleTabSwitch(int previousTab, int newTab)
    {
        if(previousTab == newTab) return;

        // Cleanup when switching AWAY from a tab
        if(previousTab == 2 && pivotHandle != null) DestroyImmediate(pivotHandle);
        if(previousTab == 4)
        {
            ClearTriangleSelection(); // From MeshSeparator partial class
            TrySetSelectionWireProperty(originalShowSelectionWire);
        }

        // Setup when switching TO a tab
        if (newTab == 4)
        {
            originalShowSelectionWire = TryGetSelectionWireProperty() ?? true;
            TrySetSelectionWireProperty(true);
        }
    }

    /// <summary>
    /// Safely gets the selection wire property value using reflection.
    /// Returns null if the property is not available.
    /// </summary>
    private bool? TryGetSelectionWireProperty()
    {
        try
        {
            if (showSelectionWireProperty != null)
            {
                return (bool)showSelectionWireProperty.GetValue(null);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to get selection wire property: {e.Message}");
        }
        return null;
    }

    /// <summary>
    /// Safely sets the selection wire property value using reflection.
    /// </summary>
    private void TrySetSelectionWireProperty(bool value)
    {
        try
        {
            if (showSelectionWireProperty != null)
            {
                showSelectionWireProperty.SetValue(null, value);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to set selection wire property: {e.Message}");
        }
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        EditorGUI.BeginChangeCheck();
        sourceObject = (GameObject)EditorGUILayout.ObjectField(sourceObject, typeof(GameObject), true, GUILayout.Width(250));
        if(EditorGUI.EndChangeCheck())
        {
            CleanUpCopy();
        }

        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Undo", EditorStyles.toolbarButton)) { Undo.PerformUndo(); }

        if (sourceObject != null)
        {
            if (GUILayout.Button("Restore Original", EditorStyles.toolbarButton)) { RestoreOriginal(); }
            shouldOverwrite = GUILayout.Toggle(shouldOverwrite, "Overwrite Original", EditorStyles.toolbarButton);
            if (shouldOverwrite)
            {
                if (GUILayout.Button("Save", EditorStyles.toolbarButton)) { SaveOverwrite(); }
            }
            else
            {
                if (GUILayout.Button("Save a Copy", EditorStyles.toolbarButton)) { SaveAsCopy(); }
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Draws the progress bar and message at the bottom of the window.
    /// </summary>
    private void DrawAsyncProgressCanvas()
    {
        Rect progressRect = new Rect(0, position.height - 20, position.width, 20);
        EditorGUI.ProgressBar(progressRect, asyncProgress, asyncMessage);
    }

    private void CleanUpCopy()
    {
        if (activeEditingCopy != null) DestroyImmediate(activeEditingCopy);
        if (pivotHandle != null) DestroyImmediate(pivotHandle);
        originalTextureCache?.Clear();
        ClearTriangleSelection();
        modelHasSkinnedMeshes = false;
    }

    private void RestoreOriginal()
    {
        CleanUpCopy();
        CreateEditableCopy();
    }
    
    private void CreateEditableCopy()
    {
        if (sourceObject == null) return;
        activeEditingCopy = (GameObject)PrefabUtility.InstantiatePrefab(sourceObject);
        activeEditingCopy.name = sourceObject.name + " (Editable Copy)";
        PrefabUtility.UnpackPrefabInstance(activeEditingCopy, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

        modelHasSkinnedMeshes = activeEditingCopy.GetComponentInChildren<SkinnedMeshRenderer>(true) != null;
        
        bakeScale = 1.0f;
        bakeRotation = Vector3.zero;
        ApplyLiveTransformPreview();

        CacheOriginalTextures(); 
        
        FrameObject(activeEditingCopy);
    }
    
    private void SaveOverwrite()
    {
        if (activeEditingCopy == null) return;
        string originalPath = AssetDatabase.GetAssetPath(sourceObject);
        if (string.IsNullOrEmpty(originalPath) || !originalPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogError("Could not find the original FBX file path. Please use 'Save a Copy' instead.");
            return;
        }
        if (EditorUtility.DisplayDialog("Overwrite Original FBX?", $"You are about to overwrite:\n\n{originalPath}\n\nThis action cannot be undone.", "Overwrite", "Cancel"))
        {
            // The user is expected to bake manually before saving.
            // The ModelExporter will handle baking the root object's transform on export.
            ModelExporter.ExportObject(originalPath, activeEditingCopy);
            RestoreMaterialTexturesAfterExport(originalPath);
            Debug.Log($"Successfully saved and overwrote: {originalPath}");
        }
    }

    private void SaveAsCopy()
    {
        if (activeEditingCopy == null) return;
        
        string originalPath = AssetDatabase.GetAssetPath(sourceObject);
        string defaultDirectory = System.IO.Path.GetDirectoryName(originalPath);
        if (!string.IsNullOrEmpty(defaultDirectory) && !System.IO.Path.IsPathRooted(defaultDirectory))
        {
            defaultDirectory = System.IO.Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length), defaultDirectory);
        }
        
        string defaultName = sourceObject.name + "_Edited.fbx";
        string path = EditorUtility.SaveFilePanel("Save Edited FBX As...", defaultDirectory, defaultName, "fbx");
        if (string.IsNullOrEmpty(path)) return;
        
        // The user is expected to bake manually before saving.
        // The ModelExporter will handle baking the root object's transform on export.
        ModelExporter.ExportObject(path, activeEditingCopy);

        // Convert absolute path to relative asset path for AssetDatabase APIs
        if (path.StartsWith(Application.dataPath))
        {
            string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
            RestoreMaterialTexturesAfterExport(relativePath);
        }
        else
        {
            AssetDatabase.Refresh();
        }

        Debug.Log($"Successfully saved a copy to: {path}");
    }

    /// <summary>
    /// After exporting an FBX, restores material texture assignments on the reimported asset.
    /// Unity's FBX Exporter may not properly embed texture references, causing materials
    /// to lose their textures after reimport.
    /// </summary>
    private void RestoreMaterialTexturesAfterExport(string savedAssetPath)
    {
        // 1. Build name-based texture cache from the editing copy's current materials
        var texturesByMaterialName = new Dictionary<string, Dictionary<string, Texture>>();
        var allRenderers = activeEditingCopy.GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in allRenderers)
        {
            foreach (var mat in renderer.sharedMaterials)
            {
                if (mat == null || texturesByMaterialName.ContainsKey(mat.name)) continue;
                var textures = new Dictionary<string, Texture>();
                var shader = mat.shader;
                for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
                {
                    if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        string propName = ShaderUtil.GetPropertyName(shader, i);
                        Texture tex = mat.GetTexture(propName);
                        if (tex != null)
                            textures[propName] = tex;
                    }
                }
                if (textures.Count > 0)
                    texturesByMaterialName[mat.name] = textures;
            }
        }

        AssetDatabase.Refresh();

        // 2. Copy source FBX's material remapping to saved FBX (for external .mat files)
        string sourcePath = AssetDatabase.GetAssetPath(sourceObject);
        var sourceImporter = AssetImporter.GetAtPath(sourcePath) as ModelImporter;
        var savedImporter = AssetImporter.GetAtPath(savedAssetPath) as ModelImporter;
        if (sourceImporter != null && savedImporter != null)
        {
            var sourceMap = sourceImporter.GetExternalObjectMap();
            bool hasRemaps = false;
            foreach (var entry in sourceMap)
            {
                if (entry.Value is Material)
                {
                    savedImporter.AddRemap(entry.Key, entry.Value);
                    hasRemaps = true;
                }
            }
            if (hasRemaps)
                savedImporter.SaveAndReimport();
        }

        // 3. Restore textures on embedded materials in the saved FBX
        var savedAssets = AssetDatabase.LoadAllAssetsAtPath(savedAssetPath);
        bool anyRestored = false;
        foreach (var asset in savedAssets)
        {
            if (asset is Material savedMat && texturesByMaterialName.TryGetValue(savedMat.name, out var textures))
            {
                foreach (var kvp in textures)
                {
                    if (savedMat.HasProperty(kvp.Key))
                    {
                        savedMat.SetTexture(kvp.Key, kvp.Value);
                        anyRestored = true;
                    }
                }
            }
        }

        if (anyRestored)
            AssetDatabase.SaveAssets();
    }

    #endregion
    }
}
