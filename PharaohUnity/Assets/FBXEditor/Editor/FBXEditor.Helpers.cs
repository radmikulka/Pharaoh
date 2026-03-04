// FBXEditor.Helpers.cs
// This partial class contains various helper and utility methods used by other
// parts of the FBX Editor to reduce code duplication.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace FBXEditor.Editor
{
    public partial class FBXEditor : EditorWindow
{
    /// <summary>
    /// Draws a label and a selectable value field, for use in stat displays.
    /// </summary>
    private void DrawStat(string label, string value)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(120));
        EditorGUILayout.SelectableLabel(value, EditorStyles.boldLabel, GUILayout.Height(18));
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Initializes GUI styles. Call this from OnEnable to avoid creating styles during OnGUI.
    /// </summary>
    private void InitializeStyles()
    {
        // Only create styles if they haven't been created yet
        // Note: EditorStyles is only valid during certain callbacks, so we still need a null check
        if (watermarkStyle == null && EditorStyles.largeLabel != null)
        {
            watermarkStyle = new GUIStyle(EditorStyles.largeLabel)
            {
                fontSize = 48,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.LowerRight,
                normal = { textColor = new Color(1, 1, 1, 0.1f) }
            };
        }
    }

    /// <summary>
    /// Draws a watermark in the scene view to indicate that editing is active.
    /// </summary>
    private void DrawSceneWatermark(SceneView sceneView)
    {
        // Ensure style is initialized (fallback if OnEnable didn't run or EditorStyles wasn't ready)
        if (watermarkStyle == null)
        {
            InitializeStyles();
        }

        if (watermarkStyle == null) return; // Still null, EditorStyles not ready

        Handles.BeginGUI();
        GUI.Label(new Rect(0, 0, sceneView.position.width - 10, sceneView.position.height - 30), "FBX EDITING", watermarkStyle);
        Handles.EndGUI();
    }

    /// <summary>
    /// Calculates the total bounding box of a GameObject and all its children.
    /// </summary>
    private Bounds GetObjectBounds(GameObject obj)
    {
        var renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(obj.transform.position, Vector3.zero);

        var bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds;
    }

    /// <summary>
    /// Displays the vertex, triangle, and mesh count for a given GameObject hierarchy.
    /// </summary>
    private void DisplayFullModelStats(GameObject root)
    {
        if (root == null) return;
        int totalVertices = 0, totalTriangles = 0, meshCount = 0;
        var processedMeshes = new HashSet<Mesh>();

        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            Mesh mesh = GetMeshFromObject(renderer.gameObject);
            if(mesh != null && processedMeshes.Add(mesh))
            {
                totalVertices += mesh.vertexCount;
                // Use GetIndexCount to avoid allocating a new triangles array every frame
                for (int i = 0; i < mesh.subMeshCount; i++)
                {
                    totalTriangles += (int)mesh.GetIndexCount(i) / 3;
                }
                meshCount++;
            }
        }

        DrawStat("Total Vertices", totalVertices.ToString("N0"));
        DrawStat("Total Triangles", totalTriangles.ToString("N0"));
        DrawStat("Unique Meshes", meshCount.ToString());
    }

    /// <summary>
    /// Frames the selected object in the last active Scene View.
    /// </summary>
    private void FrameObject(GameObject obj)
    {
        if (obj == null) return;
        Selection.activeObject = obj;
        if (SceneView.lastActiveSceneView != null) { SceneView.lastActiveSceneView.FrameSelected(); }
    }
    
    /// <summary>
    /// Safely gets a mesh from a GameObject, checking for both MeshFilter and SkinnedMeshRenderer.
    /// </summary>
    private Mesh GetMeshFromObject(GameObject selection)
    {
        if (selection == null) return null;
        if (selection.TryGetComponent<MeshFilter>(out var mf) && mf.sharedMesh != null) return mf.sharedMesh;
        if (selection.TryGetComponent<SkinnedMeshRenderer>(out var smr) && smr.sharedMesh != null) return smr.sharedMesh;
        return null;
    }
    
    /// <summary>
    /// Gets a list of selected GameObjects that are valid children of the root editing copy.
    /// </summary>
    private List<GameObject> GetValidSelectedChildren(GameObject root)
    {
        if (root == null) return new List<GameObject>();
        return Selection.gameObjects.Where(obj => obj != null && obj != root && obj.transform.IsChildOf(root.transform)).ToList();
    }
    
    private void RecalculateNormalsForSingleMesh(Mesh mesh)
    {
        Undo.RecordObject(mesh, "Recalculate Normals");
        mesh.RecalculateNormals();
        Debug.Log($"Recalculated normals for mesh: {mesh.name}");
    }

    private void RecalculateAllNormals(GameObject root)
    {
        if (root == null) return;
        Undo.SetCurrentGroupName("Recalculate All Normals");
        var processedMeshes = new HashSet<Mesh>();
        int recalculatedCount = 0;
        
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            Mesh mesh = GetMeshFromObject(renderer.gameObject);
            if (mesh != null && processedMeshes.Add(mesh))
            {
                Undo.RecordObject(mesh, "Recalculate Normals");
                mesh.RecalculateNormals();
                recalculatedCount++;
            }
        }
        Debug.Log($"Recalculated normals for {recalculatedCount} unique mesh(es).");
    }

    private void DeleteSelectedObjects(GameObject[] objectsToDelete)
    {
        if (objectsToDelete.Length == 0) return;
        Undo.SetCurrentGroupName($"Delete {objectsToDelete.Length} objects");
        foreach (var obj in objectsToDelete) { Undo.DestroyObjectImmediate(obj); }
    }

    /// <summary>
    /// Saves a mesh asset to the project folder.
    /// </summary>
    private string SaveMeshAsset(Mesh mesh, string name, string folderPath)
    {
        if (mesh == null || mesh.vertexCount == 0) return null;

        // Sanitize the name to ensure it's a valid filename
        string sanitizedName = string.Join("_", name.Split(Path.GetInvalidFileNameChars()));
        string path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folderPath, sanitizedName + ".asset"));
        
        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();
        return path;
    }

    /// <summary>
    /// Helper method for ray-triangle intersection using the Möller-Trumbore algorithm.
    /// This is required for the paint selection to work accurately.
    /// </summary>
    private bool RayIntersectsTriangle(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2, out float distance)
    {
        distance = 0;
        
        Vector3 e1 = v1 - v0;
        Vector3 e2 = v2 - v0;
        Vector3 h = Vector3.Cross(ray.direction, e2);
        float a = Vector3.Dot(e1, h);
        
        // If a is too close to 0, ray is parallel to the triangle
        if (a > -Mathf.Epsilon && a < Mathf.Epsilon)
            return false;
            
        float f = 1.0f / a;
        Vector3 s = ray.origin - v0;
        float u = f * Vector3.Dot(s, h);
        
        if (u < 0.0f || u > 1.0f)
            return false;
            
        Vector3 q = Vector3.Cross(s, e1);
        float v = f * Vector3.Dot(ray.direction, q);
        
        if (v < 0.0f || u + v > 1.0f)
            return false;
            
        // Calculate the distance from the ray origin to the intersection point
        float t = f * Vector3.Dot(e2, q);
        
        // Check if the triangle is behind the ray
        if (t < Mathf.Epsilon)
            return false;
            
        distance = t;
        return true;
    }
    }
}