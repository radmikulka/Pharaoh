// FBXEditor.MeshCombiner.cs
// This partial class contains all logic for the "Mesh Combiner" tab.
// FIXED: Corrected skinned mesh combining to work correctly with FBX export.

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace FBXEditor.Editor
{
    public partial class FBXEditor : EditorWindow
    {
        // --- Mesh Combiner Variables ---
        private List<GameObject> meshesToCombine = new List<GameObject>();
        private ReorderableList meshCombineList;

        /// <summary>
        /// Sets up the ReorderableList used for the UI.
        /// </summary>
        private void InitializeMeshCombiner()
        {
            meshCombineList = new ReorderableList(meshesToCombine, typeof(GameObject), true, true, true, true);
            
            meshCombineList.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "Objects to Combine");
            };

            meshCombineList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                rect.y += 2;
                rect.height = EditorGUIUtility.singleLineHeight;
                var element = (GameObject)meshCombineList.list[index];
                meshesToCombine[index] = (GameObject)EditorGUI.ObjectField(rect, element, typeof(GameObject), true);
            };
        }

        /// <summary>
        /// Draws the UI for the Mesh Combiner tab.
        /// </summary>
        private void DrawMeshCombinerTab()
        {
            if (modelHasSkinnedMeshes)
            {
                EditorGUILayout.HelpBox("Skinned Mesh Combining is supported. Meshes with different local transforms are now handled correctly.", MessageType.Info);
            }
            if (activeEditingCopy == null)
            {
                EditorGUILayout.HelpBox("Load a model in the 'Hierarchy Editor' tab first.", MessageType.Warning);
                return;
            }

            EditorGUILayout.HelpBox("Add two or more mesh objects from your active editing copy below. They will be combined into a single new mesh.", MessageType.Info);
            
            // Draw drag & drop area
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drag & Drop Meshes Here", EditorStyles.helpBox);
            
            // Handle drag & drop events
            Event evt = Event.current;
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(evt.mousePosition))
                        break;
                    
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    
                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        
                        foreach (Object draggedObject in DragAndDrop.objectReferences)
                        {
                            GameObject go = draggedObject as GameObject;
                            if (go != null)
                            {
                                // If it's already in the scene, add it directly
                                if (!EditorUtility.IsPersistent(go))
                                {
                                    if (GetMeshFromObject(go) != null && !meshesToCombine.Contains(go))
                                    {
                                        meshesToCombine.Add(go);
                                    }
                                }
                                // If it's a prefab or model asset, instantiate it first
                                else if (activeEditingCopy != null)
                                {
                                    GameObject instance = PrefabUtility.InstantiatePrefab(go) as GameObject;
                                    if (instance != null)
                                    {
                                        instance.transform.SetParent(activeEditingCopy.transform, false);
                                        if (GetMeshFromObject(instance) != null)
                                        {
                                            meshesToCombine.Add(instance);
                                        }
                                        else
                                        {
                                            DestroyImmediate(instance);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Try to handle direct Mesh assets
                                Mesh meshAsset = draggedObject as Mesh;
                                if (meshAsset != null && activeEditingCopy != null)
                                {
                                    // Create a new GameObject with this mesh
                                    GameObject newMeshObj = new GameObject(meshAsset.name);
                                    newMeshObj.transform.SetParent(activeEditingCopy.transform, false);
                                    
                                    // Add mesh components
                                    MeshFilter mf = newMeshObj.AddComponent<MeshFilter>();
                                    mf.sharedMesh = meshAsset;
                                    newMeshObj.AddComponent<MeshRenderer>().sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
                                    
                                    meshesToCombine.Add(newMeshObj);
                                }
                            }
                        }
                        
                        GUI.changed = true;
                        evt.Use();
                        Repaint(); // Force immediate UI update
                    }
                    break;
            }
            
            if (meshCombineList != null)
            {
                meshCombineList.DoLayoutList();
            }

            EditorGUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear List"))
            {
                meshesToCombine.Clear();
            }
            
            bool canCombine = activeEditingCopy != null &&
                                  meshesToCombine.Count >= 2 &&
                                  meshesToCombine.All(obj => obj != null && obj.transform.IsChildOf(activeEditingCopy.transform) && GetMeshFromObject(obj) != null);

            using (new EditorGUI.DisabledScope(!canCombine))
            {
                if (GUILayout.Button("Combine Meshes", GUILayout.Height(30)))
                {
                    CombineMeshes();
                }
            }
            EditorGUILayout.EndHorizontal();

            if (meshesToCombine.Count > 0 && !canCombine)
            {
                EditorGUILayout.HelpBox("Ensure all objects in the list are valid, non-null children of the model being edited and contain a mesh.", MessageType.Warning);
            }
        }

        /// <summary>
        /// The core logic for combining meshes. It handles multiple materials by creating sub-meshes.
        /// This function detects if skinned meshes or static meshes are being combined and branches logic.
        /// FIXED: Properly handles skinned meshes for FBX export.
        /// </summary>
        private void CombineMeshes()
        {
            var validMeshes = meshesToCombine.Where(o => o != null && GetMeshFromObject(o) != null).ToList();
            if (validMeshes.Count < 2)
            {
                Debug.LogError("At least two valid meshes are required to combine.");
                return;
            }

            var firstRenderer = validMeshes[0].GetComponent<Renderer>();
            bool isSkinned = firstRenderer is SkinnedMeshRenderer;

            // Ensure we are not mixing skinned and static meshes
            if (validMeshes.Any(go => (go.GetComponent<SkinnedMeshRenderer>() != null) != isSkinned))
            {
                EditorUtility.DisplayDialog("Combination Error", "Cannot combine SkinnedMeshRenderers and static MeshRenderers. Please select only one type of object to combine.", "OK");
                return;
            }

            Undo.SetCurrentGroupName("Combine Meshes");

            // Check total vertex count and warn user if it's high
            int totalVertexCount = 0;
            foreach (var go in validMeshes)
            {
                var mesh = GetMeshFromObject(go);
                if (mesh != null) totalVertexCount += mesh.vertexCount;
            }
            
            if (totalVertexCount > 65535)
            {
                Debug.LogWarning($"Combining meshes with {totalVertexCount} total vertices. This exceeds the UInt16 limit (65535) but will work with UInt32 index format.");
            }
            else
            {
                Debug.Log($"Combining meshes with {totalVertexCount} total vertices.");
            }

            // 1. Gather all unique materials from the source objects.
            var allSourceMaterials = new List<Material>();
            foreach (var go in validMeshes)
            {
                var renderer = go.GetComponent<Renderer>();
                if (renderer != null) allSourceMaterials.AddRange(renderer.sharedMaterials);
            }
            List<Material> uniqueMaterials = allSourceMaterials.Distinct().ToList();
            
            GameObject combinedObject = null;
            Mesh finalMesh = null;

            if (isSkinned)
            {
                // --- FIXED LOGIC FOR COMBINING SKINNED MESHES ---
                CombineSkinnedMeshes(validMeshes, uniqueMaterials, out combinedObject, out finalMesh);
            }
            else
            {
                // --- LOGIC FOR COMBINING STATIC MESHES (Original Logic) ---
                CombineStaticMeshes(validMeshes, uniqueMaterials, out combinedObject, out finalMesh);
            }

            if (combinedObject == null || finalMesh == null)
            {
                Debug.LogError("Mesh combination failed for an unknown reason.");
                return;
            }

            // --- COMMON POST-PROCESSING ---
            
            // Center the pivot of the new combined mesh (ONLY for static meshes).
            if (!isSkinned)
            {
                Vector3 center = finalMesh.bounds.center;
                var vertices = finalMesh.vertices;
                for (int i = 0; i < vertices.Length; i++) { vertices[i] -= center; }
                finalMesh.vertices = vertices;
                finalMesh.RecalculateBounds();
                combinedObject.transform.position = activeEditingCopy.transform.TransformPoint(center);
            }

            // Save the new mesh asset
            string defaultPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(sourceObject));
            SaveMeshAsset(finalMesh, finalMesh.name, defaultPath); // from Helpers partial
            
            // Disable the original objects.
            foreach (var go in validMeshes)
            {
                Undo.RecordObject(go, "Disable Original Mesh");
                go.SetActive(false);
            }
            
            Selection.activeObject = combinedObject;
            meshesToCombine.Clear();
            Debug.Log($"Successfully combined {validMeshes.Count} meshes into '{combinedObject.name}'.");
        }

        /// <summary>
        /// Combines skinned meshes with proper handling for FBX export.
        /// 
        /// KEY FIXES:
        /// 1. Transform vertices to root-local space
        /// 2. DEDUPLICATE bones by NAME (not Transform reference) to match FBX Exporter behavior
        ///    - FBX Exporter skips bones with duplicate names, shifting indices
        ///    - If we don't match this, bone indices become invalid after export
        /// 3. Use a universal bindpose formula that's independent of source SMR:
        ///    newBindpose = bone.worldToLocalMatrix * root.localToWorldMatrix
        /// 
        /// This works because:
        /// - Original skinning: worldPos = bone.L2W * origBindpose * smrLocalPos
        /// - We transform vertices: rootLocalPos = root.W2L * smr.L2W * smrLocalPos  
        /// - New skinning: worldPos = bone.L2W * newBindpose * rootLocalPos
        /// - With newBindpose = bone.W2L * root.L2W, this correctly produces worldPos
        /// </summary>
        private void CombineSkinnedMeshes(List<GameObject> validMeshes, List<Material> uniqueMaterials, 
            out GameObject combinedObject, out Mesh finalMesh)
        {
            combinedObject = null;
            finalMesh = null;
            
            var smrs = validMeshes.Select(go => go.GetComponent<SkinnedMeshRenderer>()).ToList();
            
            // Validate and find root bone
            Transform newRootBone = FindCommonRootBone(smrs);
            if (newRootBone == null)
            {
                EditorUtility.DisplayDialog("Skinned Combination Error", 
                    "Could not find a valid root bone for the selected meshes. Please ensure the meshes have bones assigned.", "OK");
                return;
            }

            // Get the root transform (the activeEditingCopy)
            Transform rootTransform = activeEditingCopy.transform;
            Matrix4x4 rootWorldToLocal = rootTransform.worldToLocalMatrix;
            Matrix4x4 rootLocalToWorld = rootTransform.localToWorldMatrix;

            // Prepare lists for new mesh data
            var newVertices = new List<Vector3>();
            var newNormals = new List<Vector3>();
            var newTangents = new List<Vector4>();
            var newUVs = new List<Vector2>();
            var newUV2s = new List<Vector2>();
            var newUV3s = new List<Vector2>();
            var newUV4s = new List<Vector2>();
            var newColors = new List<Color>();
            var newBoneWeights = new List<BoneWeight>();
            var newTrianglesBySubmesh = new List<int>[uniqueMaterials.Count];
            for (int i = 0; i < newTrianglesBySubmesh.Length; i++) 
                newTrianglesBySubmesh[i] = new List<int>();

            // DEDUPLICATED bone list - maps bone NAME to index in master list
            // CRITICAL: FBX Exporter deduplicates by NAME, not Transform reference!
            // If we have multiple skeleton instances, they'll have different Transform objects
            // but identical bone names. We must match FBX Exporter's behavior.
            var masterBones = new List<Transform>();
            var masterBindPoses = new List<Matrix4x4>();
            var boneNameToIndexMap = new Dictionary<string, int>();

            // Process each SkinnedMeshRenderer
            foreach (var smr in smrs)
            {
                Mesh mesh = smr.sharedMesh;
                int vertexOffset = newVertices.Count;
                
                // Calculate the matrix to transform from SMR-local space to root-local space
                Matrix4x4 smrLocalToWorld = smr.transform.localToWorldMatrix;
                Matrix4x4 smrToRoot = rootWorldToLocal * smrLocalToWorld;
                
                // Transform vertices, normals, tangents to root-local space
                var verts = mesh.vertices;
                var norms = mesh.normals;
                var tangs = mesh.tangents;
                
                for (int i = 0; i < verts.Length; i++)
                {
                    verts[i] = smrToRoot.MultiplyPoint3x4(verts[i]);
                    if (i < norms.Length) 
                        norms[i] = smrToRoot.MultiplyVector(norms[i]).normalized;
                    if (i < tangs.Length)
                    {
                        Vector3 tanDir = smrToRoot.MultiplyVector(new Vector3(tangs[i].x, tangs[i].y, tangs[i].z)).normalized;
                        tangs[i] = new Vector4(tanDir.x, tanDir.y, tanDir.z, tangs[i].w);
                    }
                }
                
                newVertices.AddRange(verts);
                if (norms.Length == verts.Length) newNormals.AddRange(norms);
                if (tangs.Length == verts.Length) newTangents.AddRange(tangs);
                
                // Copy UVs (these don't need transformation)
                if (mesh.uv.Length == verts.Length) newUVs.AddRange(mesh.uv);
                else newUVs.AddRange(new Vector2[verts.Length]);
                
                if (mesh.uv2.Length == verts.Length) newUV2s.AddRange(mesh.uv2);
                else if (newUV2s.Count > 0) newUV2s.AddRange(new Vector2[verts.Length]);
                
                if (mesh.uv3.Length == verts.Length) newUV3s.AddRange(mesh.uv3);
                else if (newUV3s.Count > 0) newUV3s.AddRange(new Vector2[verts.Length]);
                
                if (mesh.uv4.Length == verts.Length) newUV4s.AddRange(mesh.uv4);
                else if (newUV4s.Count > 0) newUV4s.AddRange(new Vector2[verts.Length]);
                
                // Copy vertex colors
                if (mesh.colors.Length == verts.Length) newColors.AddRange(mesh.colors);
                else if (newColors.Count > 0) newColors.AddRange(new Color[verts.Length]);

                // Build the bone index remap array for this SMR
                // CRITICAL: Deduplicate by bone NAME to match FBX Exporter behavior
                Transform[] smrBones = smr.bones;
                int[] boneRemapArray = new int[smrBones.Length];
                
                for (int i = 0; i < smrBones.Length; i++)
                {
                    Transform bone = smrBones[i];
                    
                    if (bone == null)
                    {
                        // Handle null bones - map to index 0
                        boneRemapArray[i] = 0;
                        continue;
                    }
                    
                    string boneName = bone.name;
                    
                    if (boneNameToIndexMap.TryGetValue(boneName, out int existingIndex))
                    {
                        // Bone with this name already exists - reuse its index
                        // This matches what FBX Exporter does when it "skips duplicate" bones
                        boneRemapArray[i] = existingIndex;
                    }
                    else
                    {
                        // New bone name - add it to the master list
                        int newIndex = masterBones.Count;
                        boneNameToIndexMap[boneName] = newIndex;
                        boneRemapArray[i] = newIndex;
                        
                        masterBones.Add(bone);
                        
                        // Calculate the UNIVERSAL bindpose for this bone
                        // This formula is independent of the source SMR!
                        // newBindpose transforms from root-local space to bone-local space
                        Matrix4x4 newBindpose = bone.worldToLocalMatrix * rootLocalToWorld;
                        masterBindPoses.Add(newBindpose);
                    }
                }

                // Remap bone weights using the remap array
                BoneWeight[] meshBoneWeights = mesh.boneWeights;
                foreach (var bw in meshBoneWeights)
                {
                    BoneWeight newBw = new BoneWeight
                    {
                        boneIndex0 = boneRemapArray[bw.boneIndex0],
                        boneIndex1 = boneRemapArray[bw.boneIndex1],
                        boneIndex2 = boneRemapArray[bw.boneIndex2],
                        boneIndex3 = boneRemapArray[bw.boneIndex3],
                        weight0 = bw.weight0,
                        weight1 = bw.weight1,
                        weight2 = bw.weight2,
                        weight3 = bw.weight3
                    };
                    newBoneWeights.Add(newBw);
                }

                // Add triangles to the correct submesh based on material
                for (int submeshIndex = 0; submeshIndex < mesh.subMeshCount; submeshIndex++)
                {
                    if (submeshIndex >= smr.sharedMaterials.Length) continue;
                    
                    Material mat = smr.sharedMaterials[submeshIndex];
                    int materialIndex = uniqueMaterials.IndexOf(mat);
                    if (materialIndex == -1) continue;

                    var tris = mesh.GetTriangles(submeshIndex);
                    for (int i = 0; i < tris.Length; i++)
                    {
                        newTrianglesBySubmesh[materialIndex].Add(tris[i] + vertexOffset);
                    }
                }
            }
            
            // Create new GameObject - place it at the root's local origin
            combinedObject = new GameObject("Combined_Skinned_Mesh_" + smrs[0].name);
            Undo.RegisterCreatedObjectUndo(combinedObject, "Create Combined Skinned Object");
            combinedObject.transform.SetParent(activeEditingCopy.transform, false);
            combinedObject.transform.localPosition = Vector3.zero;
            combinedObject.transform.localRotation = Quaternion.identity;
            combinedObject.transform.localScale = Vector3.one;
            
            // Create the mesh
            finalMesh = new Mesh();
            finalMesh.name = combinedObject.name;
            finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            
            finalMesh.SetVertices(newVertices);
            if (newNormals.Count == newVertices.Count) finalMesh.SetNormals(newNormals);
            if (newTangents.Count == newVertices.Count) finalMesh.SetTangents(newTangents);
            if (newUVs.Count == newVertices.Count) finalMesh.SetUVs(0, newUVs);
            if (newUV2s.Count == newVertices.Count) finalMesh.SetUVs(1, newUV2s);
            if (newUV3s.Count == newVertices.Count) finalMesh.SetUVs(2, newUV3s);
            if (newUV4s.Count == newVertices.Count) finalMesh.SetUVs(3, newUV4s);
            if (newColors.Count == newVertices.Count) finalMesh.SetColors(newColors);
            
            finalMesh.boneWeights = newBoneWeights.ToArray();
            finalMesh.bindposes = masterBindPoses.ToArray();

            finalMesh.subMeshCount = newTrianglesBySubmesh.Length;
            for (int i = 0; i < newTrianglesBySubmesh.Length; i++)
            {
                finalMesh.SetTriangles(newTrianglesBySubmesh[i], i);
            }
            finalMesh.RecalculateBounds();
            
            // Add SkinnedMeshRenderer component and assign data
            var newSmr = combinedObject.AddComponent<SkinnedMeshRenderer>();
            newSmr.sharedMesh = finalMesh;
            newSmr.sharedMaterials = uniqueMaterials.ToArray();
            newSmr.bones = masterBones.ToArray();
            newSmr.rootBone = newRootBone;
            
            // Copy rendering settings from the first SMR
            newSmr.updateWhenOffscreen = smrs[0].updateWhenOffscreen;
            newSmr.skinnedMotionVectors = smrs[0].skinnedMotionVectors;
            
            Debug.Log($"Combined {smrs.Count} skinned meshes. Unique bone names: {masterBones.Count}, Vertices: {newVertices.Count}");
        }

        /// <summary>
        /// Combines static meshes (original logic, unchanged).
        /// </summary>
        private void CombineStaticMeshes(List<GameObject> validMeshes, List<Material> uniqueMaterials,
            out GameObject combinedObject, out Mesh finalMesh)
        {
            // Create a list of CombineInstance arrays, one for each unique material.
            var combineInstancesByMaterial = new List<CombineInstance>[uniqueMaterials.Count];
            for (int i = 0; i < uniqueMaterials.Count; i++) { combineInstancesByMaterial[i] = new List<CombineInstance>(); }
            
            foreach (var go in validMeshes)
            {
                var mf = go.GetComponent<MeshFilter>();
                var mr = go.GetComponent<Renderer>();
                if (mf == null || mr == null) continue;

                for (int i = 0; i < mf.sharedMesh.subMeshCount; i++)
                {
                    if (i >= mr.sharedMaterials.Length) continue;
                    
                    int materialIndex = uniqueMaterials.IndexOf(mr.sharedMaterials[i]);
                    if (materialIndex != -1)
                    {
                        // The transform matrix converts the mesh from local to world space.
                        combineInstancesByMaterial[materialIndex].Add(new CombineInstance { 
                            mesh = mf.sharedMesh, 
                            subMeshIndex = i, 
                            transform = mf.transform.localToWorldMatrix 
                        });
                    }
                }
            }

            // For each material, combine its associated meshes into a temporary mesh.
            var finalCombineInstances = new List<CombineInstance>();
            foreach (var materialGroup in combineInstancesByMaterial)
            {
                if (materialGroup.Count > 0)
                {
                    var combinedMeshForMaterial = new Mesh();
                    // Set index format to UInt32 to support more than 65535 vertices
                    combinedMeshForMaterial.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                    combinedMeshForMaterial.CombineMeshes(materialGroup.ToArray(), true, true);
                    finalCombineInstances.Add(new CombineInstance { mesh = combinedMeshForMaterial });
                }
            }
            
            // Create the new GameObject and combine all temporary meshes into the final mesh.
            combinedObject = new GameObject("Combined_Mesh_" + validMeshes.First().name);
            Undo.RegisterCreatedObjectUndo(combinedObject, "Create Combined Object");
            combinedObject.transform.SetParent(activeEditingCopy.transform, false); // Parent to the root copy
            
            finalMesh = new Mesh();
            finalMesh.name = combinedObject.name;
            
            // Set index format to UInt32 to support more than 65535 vertices
            finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            
            // The second CombineMeshes call merges the submeshes into one, preserving material order.
            finalMesh.CombineMeshes(finalCombineInstances.ToArray(), false, false);
            
            // Add components and assign the new mesh and materials.
            combinedObject.AddComponent<MeshFilter>().sharedMesh = finalMesh;
            combinedObject.AddComponent<MeshRenderer>().sharedMaterials = uniqueMaterials.ToArray();
        }

        /// <summary>
        /// Attempts to find a common root bone among all SkinnedMeshRenderers.
        /// Returns the most common root bone, or finds one from the bone hierarchy.
        /// </summary>
        private Transform FindCommonRootBone(List<SkinnedMeshRenderer> smrs)
        {
            // First, try to find explicit root bones
            var rootBoneCounts = new Dictionary<Transform, int>();
            
            foreach (var smr in smrs)
            {
                if (smr.rootBone != null)
                {
                    if (rootBoneCounts.ContainsKey(smr.rootBone))
                        rootBoneCounts[smr.rootBone]++;
                    else
                        rootBoneCounts[smr.rootBone] = 1;
                }
            }
            
            // If we found explicit root bones, return the most common one
            if (rootBoneCounts.Count > 0)
            {
                return rootBoneCounts.OrderByDescending(kvp => kvp.Value).First().Key;
            }
            
            // No explicit root bones found, try to find from bone hierarchy
            var allBones = new HashSet<Transform>();
            foreach (var smr in smrs)
            {
                if (smr.bones != null)
                {
                    foreach (var bone in smr.bones)
                    {
                        if (bone != null) allBones.Add(bone);
                    }
                }
            }
            
            if (allBones.Count == 0)
                return null;
            
            // Find the highest bone in the hierarchy (closest to root)
            Transform highestBone = allBones.First();
            int highestDepth = GetTransformDepth(highestBone);
            
            foreach (var bone in allBones)
            {
                int depth = GetTransformDepth(bone);
                if (depth < highestDepth)
                {
                    highestBone = bone;
                    highestDepth = depth;
                }
            }
            
            return highestBone;
        }
        
        /// <summary>
        /// Gets the depth of a transform in the hierarchy (distance from scene root).
        /// </summary>
        private int GetTransformDepth(Transform t)
        {
            int depth = 0;
            while (t.parent != null)
            {
                depth++;
                t = t.parent;
            }
            return depth;
        }
    }
}