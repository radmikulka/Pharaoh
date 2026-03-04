using UnityEngine;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;
using System.Collections.Generic;
using System.Linq;

namespace FBXEditor.Editor
{
    public partial class FBXEditor : EditorWindow
{
    // --- Transform Baking Variables ---
    private enum BakeMode { Static, Armature }
    private BakeMode bakeMode = BakeMode.Static;

    private float bakeScale = 1.0f;
    private Vector3 bakeRotation = Vector3.zero;
    // <<<--- [FIX] Default rotation set to 0 ---
    private Vector3 armatureBakeRotation = Vector3.zero;

    // --- Bone Reorientation Variables ---
    private static readonly string[] boneReorientPresetNames = { "Custom", "Y-Forward", "Z-Forward", "X-Forward" };
    private static readonly Vector3[] boneReorientPresetValues = {
        Vector3.zero,
        new Vector3(-90f, 0f, 0f),   // Y-Forward to Y-Up
        new Vector3(0f, 0f, 90f),    // Z-Forward to Y-Up
        new Vector3(0f, 0f, -90f),   // X-Forward to Y-Up
    };
    private int boneReorientPreset = 0;
    private Vector3 boneReorientRotation = Vector3.zero;


    /// <summary>
    /// Draws the UI for the Hierarchy Editor tab.
    /// </summary>
    private void DrawHierarchyEditorTab()
    {
        if (sourceObject == null)
        {
            EditorGUILayout.HelpBox("Select a Model Root to begin editing.", MessageType.Info);
            return;
        }

        if (activeEditingCopy == null) { CreateEditableCopy(); }

        // --- NEW Bake Mode Selection UI ---
        EditorGUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Bake Transforms", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();
        bakeMode = (BakeMode)GUILayout.Toolbar((int)bakeMode, new string[] { "Static Mesh", "Armature" });
        if (EditorGUI.EndChangeCheck())
        {
            // When we change modes, reset or apply the live preview
            ApplyLiveTransformPreview(); 
        }
        
        EditorGUILayout.Space(5);

        if (bakeMode == BakeMode.Static)
        {
            // --- Static Mesh Transform Baking UI ---
            EditorGUILayout.HelpBox("Use this for simple props and environment models without skeletons. This bakes the root object's transform directly into the mesh vertices.", MessageType.Info);
            
            // Add safety warning
            if (activeEditingCopy.GetComponentInChildren<SkinnedMeshRenderer>(true) != null)
            {
                EditorGUILayout.HelpBox("Warning: This model appears to be rigged (it contains SkinnedMeshRenderers). Static Bake will ignore rigged meshes. Use 'Armature' mode instead.", MessageType.Warning);
            }
            
            EditorGUI.BeginChangeCheck();
            bakeScale = EditorGUILayout.FloatField("Bake Scale", bakeScale);
            bakeRotation = EditorGUILayout.Vector3Field("Bake Rotation", bakeRotation);
            if (EditorGUI.EndChangeCheck())
            {
                ApplyLiveTransformPreview();
            }

            if (GUILayout.Button("Bake Static Mesh Transforms"))
            {
                 if (EditorUtility.DisplayDialog("Bake Static Transforms?",
                    "This will permanently modify the mesh vertices for all non-skinned meshes in the model. This action can be undone with Ctrl+Z.",
                    "Yes, Bake", "Cancel"))
                {
                    BakeTransforms(activeEditingCopy, false);
                    // Reset preview after baking
                    bakeScale = 1.0f;
                    bakeRotation = Vector3.zero;
                    ApplyLiveTransformPreview();
                }
            }
        }
        else if (bakeMode == BakeMode.Armature)
        {
            // --- Armature Baking UI ---
            // <<<--- [FIX] Updated help text ---
            EditorGUILayout.HelpBox("Use this for rigged characters (Skinned Meshes) that are imported with the wrong orientation (e.g., -90 on X-axis for Blender imports).", MessageType.Info);

            // Add safety warning
            if (activeEditingCopy.GetComponentInChildren<SkinnedMeshRenderer>(true) == null && activeEditingCopy.GetComponentInChildren<MeshRenderer>(true) == null)
            {
                EditorGUILayout.HelpBox("Warning: This model contains no meshes. Armature Bake will have no effect.", MessageType.Warning);
            }
            
            EditorGUI.BeginChangeCheck();
            // <<<--- [FIX] Add Bake Scale field ---
            // bakeScale = EditorGUILayout.FloatField("Bake Scale", bakeScale); // <-- REMOVED
            armatureBakeRotation = EditorGUILayout.Vector3Field("Correction Rotation", armatureBakeRotation);
            if(EditorGUI.EndChangeCheck())
            {
                ApplyLiveTransformPreview();
            }

            GUI.backgroundColor = new Color(1f, 0.8f, 0.6f); // A distinct color
            if (GUILayout.Button("Bake Armature Transform"))
            {
                if (EditorUtility.DisplayDialog("Bake Armature Transform?",
                    "This will permanently modify the mesh vertices, bone transforms, and bind poses for ALL meshes in the model (Skinned and Static). This action can be undone with Ctrl+Z.\n\nAre you sure you want to continue?",
                    "Yes, Bake Armature", "Cancel"))
                {
                    // <<<--- [FIX] Function now implicitly uses class-level bakeScale ---
                    BakeArmatureRotation(activeEditingCopy, Quaternion.Euler(armatureBakeRotation));
                    
                    // <<<--- [FIX] Reset fields after baking ---
                    armatureBakeRotation = Vector3.zero;
                    // bakeScale = 1.0f; // <-- REMOVED
                    ApplyLiveTransformPreview(); // Reset the preview
                }
            }
            GUI.backgroundColor = Color.white;

            // --- Bone Reorientation UI ---
            EditorGUILayout.Space(10);
            GUILayout.Label("Reorient Bone Axes", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Changes the local axis convention of all bones in the skeleton without affecting the visual pose. " +
                "Useful when IK or procedural animation code assumes a specific bone-local axis direction (e.g., Y-Up).\n\n" +
                "Warning: This will invalidate any animations baked for this skeleton.",
                MessageType.Info);

            EditorGUI.BeginChangeCheck();
            int newPreset = EditorGUILayout.Popup("Preset", boneReorientPreset, boneReorientPresetNames);
            if (EditorGUI.EndChangeCheck())
            {
                boneReorientPreset = newPreset;
                if (newPreset > 0)
                    boneReorientRotation = boneReorientPresetValues[newPreset];
            }

            EditorGUI.BeginChangeCheck();
            boneReorientRotation = EditorGUILayout.Vector3Field("Reorientation", boneReorientRotation);
            if (EditorGUI.EndChangeCheck())
            {
                // If user manually edits the vector, switch to Custom preset
                boneReorientPreset = 0;
            }

            GUI.backgroundColor = new Color(0.7f, 0.9f, 0.7f);
            using (new EditorGUI.DisabledScope(boneReorientRotation == Vector3.zero))
            {
                if (GUILayout.Button("Reorient Bone Axes"))
                {
                    if (EditorUtility.DisplayDialog("Reorient Bone Axes?",
                        "This will change the local axis convention of every bone in the skeleton. " +
                        "The visual pose and mesh will remain identical, but bone local rotations will change.\n\n" +
                        "Any baked animations for this skeleton will be invalidated.\n\n" +
                        "This action can be undone with Ctrl+Z.",
                        "Yes, Reorient", "Cancel"))
                    {
                        ReorientBoneAxes(activeEditingCopy, Quaternion.Euler(boneReorientRotation));
                        boneReorientRotation = Vector3.zero;
                        boneReorientPreset = 0;
                    }
                }
            }
            GUI.backgroundColor = Color.white;
        }
        EditorGUILayout.EndVertical();

        // --- Model Stats UI ---
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Full Model Statistics", EditorStyles.boldLabel);
        DisplayFullModelStats(activeEditingCopy); // from Helpers partial
        EditorGUILayout.Space(5);
        
        // --- [NEW] Add warning for custom normals ---
        EditorGUILayout.HelpBox("Warning: Recalculating normals will overwrite any custom/edited normals on the model. This is recommended after using mesh separation or combination.", MessageType.Warning);
        
        if (GUILayout.Button("Recalculate All Normals"))
        {
            RecalculateAllNormals(activeEditingCopy);
        }
        EditorGUILayout.EndVertical();

        // --- Selected Object Actions UI ---
        EditorGUILayout.Space(5);
        List<GameObject> validSelectedChildren = GetValidSelectedChildren(activeEditingCopy); // from Helpers partial
        if (validSelectedChildren.Count > 0)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Selected Object Actions", EditorStyles.boldLabel);
            DrawSelectedObjectEditor(validSelectedChildren);
            EditorGUILayout.EndVertical();
        }
    }

    /// <summary>
    /// Draws the UI for actions related to the currently selected child objects.
    /// </summary>
    private void DrawSelectedObjectEditor(List<GameObject> selectedChildren)
    {
        GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
        if (GUILayout.Button($"Delete {selectedChildren.Count} Selected Object(s)"))
        {
            DeleteSelectedObjects(selectedChildren.ToArray());
            return; // Exit to avoid errors after deletion
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(5);
        GUI.backgroundColor = new Color(0.7f, 0.85f, 1f);
        if (GUILayout.Button($"Export {selectedChildren.Count} Selected as New FBX"))
        {
            ExportSelection(selectedChildren);
        }
        GUI.backgroundColor = Color.white;
        
        // Display stats for a single selected object
        if (selectedChildren.Count == 1)
        {
            GameObject singleSelection = selectedChildren[0];
            Mesh selectedMesh = GetMeshFromObject(singleSelection); // from Helpers partial
            
            EditorGUILayout.Space(10);
            
            if (selectedMesh != null)
            {
                DrawStat("Vertex Count", selectedMesh.vertexCount.ToString("N0"));
                // Use GetIndexCount to avoid allocating a new triangles array
                int triCount = 0;
                for (int i = 0; i < selectedMesh.subMeshCount; i++)
                    triCount += (int)selectedMesh.GetIndexCount(i) / 3;
                DrawStat("Triangle Count", triCount.ToString("N0"));
                EditorGUILayout.Space(5);
                if (GUILayout.Button("Recalculate Normals (Selected)"))
                {
                    RecalculateNormalsForSingleMesh(selectedMesh);
                }
            }
        }
    }
    
    /// <summary>
    /// Applies the current bake settings to the active copy for a live preview.
    /// This is now mode-aware.
    /// </summary>
    private void ApplyLiveTransformPreview()
    {
        if (activeEditingCopy == null) return;
        
        if (bakeMode == BakeMode.Static)
        {
            // In Static mode, apply the preview transforms
            activeEditingCopy.transform.localScale = new Vector3(bakeScale, bakeScale, bakeScale);
            activeEditingCopy.transform.rotation = Quaternion.Euler(bakeRotation);
        }
        else
        {
            // <<<--- [FIX] In Armature mode, apply both scale and rotation as preview ---
            activeEditingCopy.transform.localScale = Vector3.one; // <-- MODIFIED
            activeEditingCopy.transform.rotation = Quaternion.Euler(armatureBakeRotation);
        }
    }
    
    /// <summary>
    /// Bakes the specified scale and rotation into the mesh vertices OF STATIC MESHES.
    /// </summary>
    /// <param name="revert">If true, applies the inverse transform to undo a bake.</param>
    private void BakeTransforms(GameObject root, bool revert)
    {
        if (root == null || (bakeScale == 1.0f && bakeRotation == Vector3.zero)) return;
        
        // Note: bakeScale is the class-level variable
        Matrix4x4 bakeMatrix = revert
            ? Matrix4x4.TRS(Vector3.zero, Quaternion.Inverse(Quaternion.Euler(bakeRotation)), Vector3.one / bakeScale)
            : Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(bakeRotation), Vector3.one * bakeScale);
        
        var allRenderers = root.GetComponentsInChildren<Renderer>(true);
        var originalToBakedMeshMap = new Dictionary<Mesh, Mesh>();

        foreach (var renderer in allRenderers)
        {
            if (renderer is SkinnedMeshRenderer) continue; // This function is for static meshes only

            Mesh sourceMesh = GetMeshFromObject(renderer.gameObject);
            if (sourceMesh == null) continue;

            if (!originalToBakedMeshMap.TryGetValue(sourceMesh, out Mesh bakedMesh))
            {
                bakedMesh = Instantiate(sourceMesh);
                bakedMesh.name = sourceMesh.name + "_Baked"; // Give it a new name
                Undo.RegisterCreatedObjectUndo(bakedMesh, "Create Baked Mesh");
                originalToBakedMeshMap[sourceMesh] = bakedMesh;

                Vector3[] vertices = bakedMesh.vertices;
                Vector3[] normals = bakedMesh.normals;
                Vector4[] tangents = bakedMesh.tangents; 

                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = bakeMatrix.MultiplyPoint3x4(vertices[i]);
                    
                    if (i < normals.Length)
                    {
                        normals[i] = bakeMatrix.MultiplyVector(normals[i]).normalized;
                    }

                    if (i < tangents.Length) 
                    {
                        Vector3 tanDir = bakeMatrix.MultiplyVector(new Vector3(tangents[i].x, tangents[i].y, tangents[i].z)).normalized;
                        tangents[i] = new Vector4(tanDir.x, tanDir.y, tanDir.z, tangents[i].w);
                    }
                }
                bakedMesh.vertices = vertices;
                // Validate array lengths before assigning to avoid mesh corruption
                if (normals.Length == vertices.Length) bakedMesh.normals = normals;
                if (tangents.Length == vertices.Length) bakedMesh.tangents = tangents;
                bakedMesh.RecalculateBounds();
            }

            if (renderer is MeshRenderer)
            {
                var mf = renderer.GetComponent<MeshFilter>();
                Undo.RecordObject(mf, "Assign Baked Mesh");
                mf.sharedMesh = bakedMesh;
            }
        }
        
        if (!revert)
        {
            root.transform.rotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// Bakes a rotation into a rigged model's SkinnedMeshRenderers, fixing the mesh, bones, and bindposes.
    /// </summary>
    private void BakeArmatureRotation(GameObject root, Quaternion bakeRotation)
    {
        if (root == null) return;

        Undo.SetCurrentGroupName("Bake Armature Transform");
        
        // --- [FIX] Get ALL renderers, not just skinned ones ---
        var allRenderers = root.GetComponentsInChildren<Renderer>(true);
        if (allRenderers.Length == 0)
        {
            Debug.LogWarning("Bake Armature: No renderers found on the model.");
            return;
        }

        var processedMeshes = new Dictionary<Mesh, Mesh>();
        
        // <<<--- [FIX] Create TRS matrix using class-level bakeScale ---
        Matrix4x4 bakeMatrix = Matrix4x4.TRS(Vector3.zero, bakeRotation, Vector3.one); // <-- MODIFIED
        Matrix4x4 bakeMatrixInv = bakeMatrix.inverse;

        // --- [FIX] Loop over ALL renderers ---
        foreach (var renderer in allRenderers)
        {
            if (renderer is SkinnedMeshRenderer smr)
            {
                // --- 1a. Process Skinned Meshes ---
                if (smr.sharedMesh == null) continue;

                Mesh newMesh;
                // Only process each unique mesh once.
                if (!processedMeshes.TryGetValue(smr.sharedMesh, out newMesh))
                {
                    Mesh oldMesh = smr.sharedMesh;
                    newMesh = Instantiate(oldMesh);
                    newMesh.name = oldMesh.name + "_Corrected";
                    Undo.RegisterCreatedObjectUndo(newMesh, "Create Corrected Mesh");
                    processedMeshes[oldMesh] = newMesh;

                    // Bake transform into vertices, normals, and tangents
                    var vertices = newMesh.vertices;
                    var normals = newMesh.normals;
                    var tangents = newMesh.tangents; 
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] = bakeMatrix.MultiplyPoint3x4(vertices[i]); 
                        if(i < normals.Length) normals[i] = bakeMatrix.MultiplyVector(normals[i]).normalized; 
                        
                        if (i < tangents.Length) 
                        {
                            Vector3 tanDir = bakeMatrix.MultiplyVector(new Vector3(tangents[i].x, tangents[i].y, tangents[i].z)).normalized;
                            tangents[i] = new Vector4(tanDir.x, tanDir.y, tanDir.z, tangents[i].w);
                        }
                    }
                    newMesh.vertices = vertices;
                    // Validate array lengths before assigning to avoid mesh corruption
                    if (normals.Length == vertices.Length) newMesh.normals = normals;
                    if (tangents.Length == vertices.Length) newMesh.tangents = tangents;

                    // Bake transform into the bindposes
                    var bindposes = newMesh.bindposes;
                    for (int i = 0; i < bindposes.Length; i++)
                    {
                        bindposes[i] = bindposes[i] * bakeMatrixInv;
                    }
                    newMesh.bindposes = bindposes;
                    
                    newMesh.RecalculateBounds();
                }
                
                Undo.RecordObject(smr, "Assign Corrected Skinned Mesh");
                smr.sharedMesh = newMesh;
            }
            else if (renderer is MeshRenderer mr)
            {
                // --- 1b. Process Static Meshes ---
                var mf = renderer.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null) continue;

                Mesh newMesh;
                if (!processedMeshes.TryGetValue(mf.sharedMesh, out newMesh))
                {
                    Mesh oldMesh = mf.sharedMesh;
                    newMesh = Instantiate(oldMesh);
                    newMesh.name = oldMesh.name + "_Corrected";
                    Undo.RegisterCreatedObjectUndo(newMesh, "Create Corrected Mesh");
                    processedMeshes[oldMesh] = newMesh;
                    
                    // Bake transform into vertices, normals, and tangents
                    var vertices = newMesh.vertices;
                    var normals = newMesh.normals;
                    var tangents = newMesh.tangents;
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] = bakeMatrix.MultiplyPoint3x4(vertices[i]);
                        if (i < normals.Length) normals[i] = bakeMatrix.MultiplyVector(normals[i]).normalized;
                        if (i < tangents.Length)
                        {
                            Vector3 tanDir = bakeMatrix.MultiplyVector(new Vector3(tangents[i].x, tangents[i].y, tangents[i].z)).normalized;
                            tangents[i] = new Vector4(tanDir.x, tanDir.y, tanDir.z, tangents[i].w);
                        }
                    }
                    newMesh.vertices = vertices;
                    // Validate array lengths before assigning to avoid mesh corruption
                    if (normals.Length == vertices.Length) newMesh.normals = normals;
                    if (tangents.Length == vertices.Length) newMesh.tangents = tangents;

                    // (No bindposes to bake for static mesh)
                    newMesh.RecalculateBounds();
                }

                Undo.RecordObject(mf, "Assign Corrected Static Mesh");
                mf.sharedMesh = newMesh;
            }
        }

        // --- 2. Bake transform into the actual bone transforms ---
        
        // Find all SkinnedMeshRenderers to locate skeleton roots
        var allSmrs = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
    
        // Find all unique "root-level ancestors" of the skeletons
        var skeletonRootsToBake = new HashSet<Transform>();
        foreach (var smr in allSmrs)
        {
            if (smr.rootBone != null && smr.rootBone.IsChildOf(root.transform))
            {
                Transform rootLevelChild = GetRootLevelChild(root.transform, smr.rootBone);
                if (rootLevelChild != null)
                {
                    skeletonRootsToBake.Add(rootLevelChild);
                }
            }
        }
        
        // Now, bake the transform for these unique skeleton root objects
        foreach (var skeletonRoot in skeletonRootsToBake)
        {
            Undo.RecordObject(skeletonRoot, "Bake Root Bone Transform");
            skeletonRoot.localPosition = bakeRotation * skeletonRoot.localPosition; // <-- MODIFIED
            skeletonRoot.localRotation = bakeRotation * skeletonRoot.localRotation;
        }

        Debug.Log($"Successfully baked armature transform for {allRenderers.Length} renderers ({processedMeshes.Count} unique meshes) and {skeletonRootsToBake.Count} skeleton root(s).");
        
        // Reset the root transform after baking
        root.transform.rotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;
    }

    /// <summary>
    /// Draws a preview of reoriented bone axes in the scene view.
    /// Shows colored lines (R=X, G=Y, B=Z) representing what each bone's
    /// local axes will look like after reorientation.
    /// </summary>
    private void DrawBoneReorientPreview(SceneView sceneView)
    {
        if (activeEditingCopy == null || boneReorientRotation == Vector3.zero) return;

        Quaternion correction = Quaternion.Euler(boneReorientRotation);

        var allSmrs = activeEditingCopy.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        if (allSmrs.Length == 0) return;

        // Collect all unique bones
        var allBones = new HashSet<Transform>();
        foreach (var smr in allSmrs)
        {
            if (smr.bones == null) continue;
            foreach (var bone in smr.bones)
            {
                if (bone != null)
                    allBones.Add(bone);
            }
        }

        // Calculate axis line length based on scene view distance
        float lineLength = HandleUtility.GetHandleSize(activeEditingCopy.transform.position) * 0.15f;

        foreach (var bone in allBones)
        {
            // The new world rotation after reorientation: original_world * correction
            Quaternion previewRot = bone.rotation * correction;
            Vector3 pos = bone.position;

            // Draw new axes as solid lines
            Handles.color = new Color(1f, 0.2f, 0.2f, 0.9f); // Red = X
            Handles.DrawLine(pos, pos + previewRot * Vector3.right * lineLength, 2f);
            Handles.color = new Color(0.2f, 1f, 0.2f, 0.9f); // Green = Y
            Handles.DrawLine(pos, pos + previewRot * Vector3.up * lineLength, 2f);
            Handles.color = new Color(0.3f, 0.5f, 1f, 0.9f); // Blue = Z
            Handles.DrawLine(pos, pos + previewRot * Vector3.forward * lineLength, 2f);
        }

        // Label in scene view
        Handles.BeginGUI();
        GUIStyle style = new GUIStyle(EditorStyles.helpBox);
        style.fontSize = 11;
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(10, sceneView.position.height - 60, 220, 22), "  Bone Reorient Preview Active", style);
        Handles.EndGUI();

        sceneView.Repaint();
    }

    /// <summary>
    /// Reorients the local axes of all bones in the skeleton by applying a correction rotation
    /// to each bone's local coordinate frame. The visual pose and mesh remain identical;
    /// only bone local rotations and bindposes change.
    /// </summary>
    private void ReorientBoneAxes(GameObject root, Quaternion correction)
    {
        if (root == null) return;

        Undo.SetCurrentGroupName("Reorient Bone Axes");

        var allSmrs = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        if (allSmrs.Length == 0)
        {
            Debug.LogWarning("Reorient Bone Axes: No SkinnedMeshRenderers found.");
            return;
        }

        Quaternion correctionInv = Quaternion.Inverse(correction);
        Matrix4x4 correctionInvMatrix = Matrix4x4.Rotate(correctionInv);

        // --- 1. Collect all unique bones across all SkinnedMeshRenderers ---
        var allBones = new HashSet<Transform>();
        foreach (var smr in allSmrs)
        {
            if (smr.bones == null) continue;
            foreach (var bone in smr.bones)
            {
                if (bone != null)
                    allBones.Add(bone);
            }
        }

        // --- 2. Find skeleton roots (top-most bones in the hierarchy) ---
        var skeletonRoots = new HashSet<Transform>();
        foreach (var bone in allBones)
        {
            // Walk up until we find a bone whose parent is NOT in the bone set
            Transform current = bone;
            while (current.parent != null && allBones.Contains(current.parent))
            {
                current = current.parent;
            }
            skeletonRoots.Add(current);
        }

        // --- 3. Apply reorientation to each bone ---
        foreach (var bone in allBones)
        {
            Undo.RecordObject(bone, "Reorient Bone");

            if (skeletonRoots.Contains(bone))
            {
                // Root bones: post-multiply correction (no parent compensation needed)
                bone.localRotation = bone.localRotation * correction;
            }
            else
            {
                // Non-root bones: conjugate rotation and counter-rotate position
                bone.localPosition = correctionInv * bone.localPosition;
                bone.localRotation = correctionInv * bone.localRotation * correction;
            }
        }

        // --- 4. Update bindposes on all skinned meshes ---
        var processedMeshes = new Dictionary<Mesh, Mesh>();
        foreach (var smr in allSmrs)
        {
            if (smr.sharedMesh == null) continue;

            Mesh newMesh;
            if (!processedMeshes.TryGetValue(smr.sharedMesh, out newMesh))
            {
                Mesh oldMesh = smr.sharedMesh;
                newMesh = Instantiate(oldMesh);
                newMesh.name = oldMesh.name + "_Reoriented";
                Undo.RegisterCreatedObjectUndo(newMesh, "Create Reoriented Mesh");
                processedMeshes[oldMesh] = newMesh;

                var bindposes = newMesh.bindposes;
                for (int i = 0; i < bindposes.Length; i++)
                {
                    bindposes[i] = correctionInvMatrix * bindposes[i];
                }
                newMesh.bindposes = bindposes;
            }

            Undo.RecordObject(smr, "Assign Reoriented Mesh");
            smr.sharedMesh = newMesh;
        }

        Debug.Log($"Successfully reoriented bone axes for {allBones.Count} bone(s) across {processedMeshes.Count} unique mesh(es).");
    }

    /// <summary>
    /// Helper function to find the ancestor transform that is a direct child of the root.
    /// e.g. for hierarchy root -> A -> B -> C, GetRootLevelChild(root, C) will return A.
    /// </summary>
    private Transform GetRootLevelChild(Transform root, Transform descendant)
    {
        if (descendant == null || root == null || !descendant.IsChildOf(root)) return null;

        Transform current = descendant;
        while (current.parent != null)
        {
            if (current.parent == root)
            {
                return current; // Found the ancestor that is a direct child
            }
            current = current.parent;
        }
        return null; // Should be unreachable if IsChildOf check passed
    }

    /// <summary>
    /// Exports the selected GameObjects to a new FBX file.
    /// </summary>
    private void ExportSelection(List<GameObject> selection)
    {
        if(selection == null || selection.Count == 0) return;

        string defaultName = selection[0].name + "_Exported.fbx";
        
        string originalPath = AssetDatabase.GetAssetPath(sourceObject);
        string defaultDirectory = System.IO.Path.GetDirectoryName(originalPath);
        if (!string.IsNullOrEmpty(defaultDirectory) && !System.IO.Path.IsPathRooted(defaultDirectory))
        {
            defaultDirectory = System.IO.Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length), defaultDirectory);
        }
        
        string path = EditorUtility.SaveFilePanel("Export Selection As...", defaultDirectory, defaultName, "fbx");
        if(string.IsNullOrEmpty(path)) return;
        
        // The ModelExporter is smart enough to handle parent transforms.
        ModelExporter.ExportObjects(path, selection.ToArray());
        
        Debug.Log($"Successfully exported {selection.Count} object(s) to {path}");
        AssetDatabase.Refresh();
    }
    }
}

