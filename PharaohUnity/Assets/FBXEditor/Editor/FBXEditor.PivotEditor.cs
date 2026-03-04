// FBXEditor.PivotEditor.cs
// This partial class contains all logic for the "Pivot Editor" tab.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace FBXEditor.Editor
{
    public partial class FBXEditor : EditorWindow
{
    // --- Pivot Editor Variables --- //
    private GameObject pivotHandle;
    private Vector3 customPivotPosition = Vector3.zero;
    private Vector3 customPivotRotation = Vector3.zero;

    /// <summary>
    /// Draws the UI for the Pivot Editor tab.
    /// </summary>
    private void DrawPivotEditorTab()
    {
        if (modelHasSkinnedMeshes)
        {
            EditorGUILayout.HelpBox("Warning: Functionality for FBX files with skinned meshes is still a WIP & may result in unexpected behavior.", MessageType.Warning);
        }
        
        EditorGUILayout.HelpBox("Select a single object in the hierarchy to modify its pivot point.", MessageType.Info);
        
        GameObject selection = Selection.activeGameObject;
        bool isValidSelection = Selection.gameObjects.Length == 1 &&
                                    selection != null && 
                                    activeEditingCopy != null && 
                                    selection.transform.IsChildOf(activeEditingCopy.transform) &&
                                    selection != pivotHandle; 

        using (new EditorGUI.DisabledScope(!isValidSelection))
        {
            EditorGUILayout.LabelField("Selected Object", isValidSelection ? selection.name : "None", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            // --- Preset Pivots ---
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Preset Pivots", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Set to Center"))
            {
                SetPivot(selection, GetObjectBounds(selection).center, selection.transform.rotation);
            }
            if (GUILayout.Button("Set to Bottom Center"))
            {
                Bounds bounds = GetObjectBounds(selection);
                Vector3 bottomCenter = bounds.center - new Vector3(0, bounds.extents.y, 0);
                SetPivot(selection, bottomCenter, selection.transform.rotation);
            }
            if (GUILayout.Button("Restore Original Pivot"))
            {
                RestoreOriginalPivot(selection);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10);

            // --- Custom Pivot ---
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Custom Pivot", EditorStyles.boldLabel);

            if (GUILayout.Button("Create Visual Pivot Handle"))
            {
                CreatePivotHandle(selection);
            }

            if (pivotHandle != null)
            {
                EditorGUI.BeginChangeCheck();
                customPivotPosition = EditorGUILayout.Vector3Field("Pivot Position", pivotHandle.transform.position);
                customPivotRotation = EditorGUILayout.Vector3Field("Pivot Rotation", pivotHandle.transform.rotation.eulerAngles);
                if(EditorGUI.EndChangeCheck())
                {
                    pivotHandle.transform.position = customPivotPosition;
                    pivotHandle.transform.rotation = Quaternion.Euler(customPivotRotation);
                }
                
                EditorGUILayout.Space(5);
                GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
                if (GUILayout.Button("Apply Custom Pivot from Handle"))
                {
                    SetPivot(selection, pivotHandle.transform.position, pivotHandle.transform.rotation);
                }
                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndVertical();
        }
        
        if (!isValidSelection)
        {
            EditorGUILayout.HelpBox("Please select a single, valid child object (not the handle itself).", MessageType.Warning);
        }
    }

    /// <summary>
    /// Handles drawing and interaction for the pivot handle in the Scene View.
    /// </summary>
    private void HandlePivotEditorSceneGUI(SceneView sceneView)
    {
        if (pivotHandle == null) return;
        
        EditorGUI.BeginChangeCheck();
        
        // Use standard Unity handles for moving and rotating the pivot object.
        Quaternion newRotation = Handles.RotationHandle(pivotHandle.transform.rotation, pivotHandle.transform.position);
        Vector3 newPosition = Handles.PositionHandle(pivotHandle.transform.position, newRotation);
        
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(pivotHandle.transform, "Move Pivot Handle");
            pivotHandle.transform.position = newPosition;
            pivotHandle.transform.rotation = newRotation;
            Repaint(); // Update the Vector3 fields in the editor window
        }
    }

    /// <summary>
    /// Core logic to change an object's pivot by modifying its mesh vertices.
    /// </summary>
    private void SetPivot(GameObject target, Vector3 pivotWorldPosition, Quaternion pivotWorldRotation)
    {
        if (target == null) return;
        
        if (pivotHandle != null) DestroyImmediate(pivotHandle);

        Undo.SetCurrentGroupName("Set Pivot");
        
        var allRenderers = target.GetComponentsInChildren<Renderer>(true);
        var meshMap = new Dictionary<Mesh, Mesh>();
        var originalMatrices = new Dictionary<Renderer, Matrix4x4>();

        // Store the original world matrix of each renderer.
        foreach (var r in allRenderers) { originalMatrices[r] = r.transform.localToWorldMatrix; }
        
        // Move the GameObject's transform to the new pivot location first.
        Undo.RecordObject(target.transform, "Set New Pivot Transform");
        target.transform.position = pivotWorldPosition;
        target.transform.rotation = pivotWorldRotation;

        // Now, iterate through meshes and move their vertices by the inverse amount.
        foreach(var renderer in allRenderers)
        {
            Mesh sourceMesh = GetMeshFromObject(renderer.gameObject);
            if (sourceMesh == null) continue;

            // Use a map to handle objects that share the same mesh.
            if (!meshMap.TryGetValue(sourceMesh, out Mesh newMesh))
            {
                newMesh = Instantiate(sourceMesh);
                meshMap[sourceMesh] = newMesh;
            }

            var vertices = newMesh.vertices;
            var normals = newMesh.normals; // <<<--- [FIX] Get normals
            var tangents = newMesh.tangents; // <<<--- [FIX] Get tangents

            bool hasNormals = normals != null && normals.Length > 0;
            bool hasTangents = tangents != null && tangents.Length > 0;

            Matrix4x4 newWorldToLocal = renderer.transform.worldToLocalMatrix;
            Matrix4x4 originalLocalToWorld = originalMatrices[renderer];
            
            // This matrix transforms from the *original* local space to the *new* local space
            Matrix4x4 combinedMatrix = newWorldToLocal * originalLocalToWorld;

            for (int i = 0; i < vertices.Length; i++)
            {
                // The vertex position is transformed from its original local space to world space,
                // and then back into the *new* local space of the moved object.
                vertices[i] = combinedMatrix.MultiplyPoint3x4(vertices[i]);
                
                // <<<--- [FIX] We must also transform normals and tangents
                if (hasNormals && i < normals.Length)
                {
                    normals[i] = combinedMatrix.MultiplyVector(normals[i]).normalized;
                }
                
                if (hasTangents && i < tangents.Length)
                {
                    Vector3 tanDir = new Vector3(tangents[i].x, tangents[i].y, tangents[i].z);
                    tanDir = combinedMatrix.MultiplyVector(tanDir).normalized;
                    tangents[i] = new Vector4(tanDir.x, tanDir.y, tanDir.z, tangents[i].w);
                }
            }
            newMesh.vertices = vertices;
            
            // <<<--- [FIX] Write normals and tangents back
            if(hasNormals) newMesh.normals = normals;
            if(hasTangents) newMesh.tangents = tangents;
            
            newMesh.RecalculateBounds();

            if (renderer.TryGetComponent<MeshFilter>(out var mf)) { Undo.RecordObject(mf, "Assign new mesh"); mf.sharedMesh = newMesh; }
            if (renderer.TryGetComponent<SkinnedMeshRenderer>(out var smr)) { Undo.RecordObject(smr, "Assign new mesh"); smr.sharedMesh = newMesh; }
        }
        
        Debug.Log($"New pivot set for {target.name}");
    }

    /// <summary>
    /// Restores an object's original pivot by replacing it with a fresh copy from the source prefab.
    /// </summary>
    private void RestoreOriginalPivot(GameObject target)
    {
        if (target == null || sourceObject == null) return;
        
        if (pivotHandle != null) DestroyImmediate(pivotHandle);
        
        // Find the corresponding object in the original source asset.
        string path = AnimationUtility.CalculateTransformPath(target.transform, activeEditingCopy.transform);
        Transform sourceChild = sourceObject.transform.Find(path);

        if (sourceChild == null)
        {
            Debug.LogError($"Could not find original object for '{target.name}' to restore pivot.");
            return;
        }
        
        GameObject freshCopy = Instantiate(sourceChild.gameObject);
        freshCopy.name = sourceChild.name;
        
        Undo.SetCurrentGroupName("Restore Original Pivot");

        // Place the new object exactly where the old one was in the hierarchy.
        Transform parent = target.transform.parent;
        int siblingIndex = target.transform.GetSiblingIndex();
        Undo.SetTransformParent(freshCopy.transform, parent, "Reparent Restored Object");
        freshCopy.transform.SetSiblingIndex(siblingIndex);
        
        // Copy the original local transforms.
        freshCopy.transform.localPosition = sourceChild.localPosition;
        freshCopy.transform.localRotation = sourceChild.localRotation;
        freshCopy.transform.localScale = sourceChild.localScale;

        Undo.DestroyObjectImmediate(target);
        Selection.activeObject = freshCopy;
        Debug.Log($"Restored original pivot and mesh for {freshCopy.name}");
    }
    
    /// <summary>
    /// Creates a temporary GameObject in the scene to serve as a visual pivot handle.
    /// </summary>
    private void CreatePivotHandle(GameObject target)
    {
        if (target == null) return;
        
        if (pivotHandle != null)
        {
            DestroyImmediate(pivotHandle);
        }

        pivotHandle = new GameObject(target.name + "_PivotHandle");
        pivotHandle.transform.position = target.transform.position;
        pivotHandle.transform.rotation = target.transform.rotation;
        
        Undo.RegisterCreatedObjectUndo(pivotHandle, "Create Pivot Handle");
        Selection.activeObject = pivotHandle;
    }
    }
}
