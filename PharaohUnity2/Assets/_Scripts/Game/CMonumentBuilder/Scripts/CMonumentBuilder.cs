using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CMonumentBuilder
{
    public class CMonumentBuilder : MonoBehaviour
    {
        public GameObject sourceAsset;

        /// <summary>
        /// Main entry point: slices source meshes through the CCutVolume hierarchy
        /// and creates generated GameObjects with the resulting geometry.
        /// </summary>
        public void Generate()
        {
            try
            {
                GenerateInternal();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"CMonumentBuilder: Generate failed with exception: {e}");
            }
        }

        private void GenerateInternal()
        {
            // 1. Clean previous output
            CleanGenerated();

            // 2. Validate
            if (sourceAsset == null)
            {
                Debug.LogError("CMonumentBuilder: sourceAsset is not assigned. Drag a GameObject with meshes into the Source Asset field.");
                return;
            }

            MeshFilter[] meshFilters = sourceAsset.GetComponentsInChildren<MeshFilter>();
            if (meshFilters.Length == 0)
            {
                Debug.LogError($"CMonumentBuilder: sourceAsset '{sourceAsset.name}' has no MeshFilter components (checked children too).");
                return;
            }

            // 3. Collect root CCutVolumes (direct children of this GameObject)
            var rootVolumes = new List<CCutVolume>();
            int childCount = 0;
            foreach (Transform child in transform)
            {
                childCount++;
                var vol = child.GetComponent<CCutVolume>();
                if (vol != null)
                    rootVolumes.Add(vol);
            }

            if (rootVolumes.Count == 0)
            {
                Debug.LogError($"CMonumentBuilder: No CCutVolume children found! " +
                    $"Found {childCount} direct child(ren) of '{gameObject.name}', but none have a CCutVolume component. " +
                    $"Add the CCutVolume script to each volume GameObject (alongside the Collider).");
                return;
            }

            Debug.Log($"CMonumentBuilder: Found {rootVolumes.Count} root volume(s).");

            // 4-5. Extract all triangles from source meshes (world space)
            var allTriangles = new List<SliceTriangle>();
            foreach (var mf in meshFilters)
            {
                MeshRenderer mr = mf.GetComponent<MeshRenderer>();
                var tris = MeshSlicerUtils.ExtractTriangles(mf, mr);
                allTriangles.AddRange(tris);
                Debug.Log($"  Mesh '{mf.sharedMesh?.name}': {tris.Count} triangles (readable={mf.sharedMesh?.isReadable})");
            }

            if (allTriangles.Count == 0)
            {
                Debug.LogError("CMonumentBuilder: No triangles extracted from sourceAsset. Check that meshes have Read/Write enabled in import settings.");
                return;
            }

            Debug.Log($"CMonumentBuilder: Extracted {allTriangles.Count} total triangles from {meshFilters.Length} mesh(es).");

            // 6. Process through volume hierarchy
            var results = MeshSlicer.SliceAll(allTriangles, rootVolumes);

            Debug.Log($"CMonumentBuilder: Slicing produced {results.Count} volume result(s).");
            foreach (var kvp in results)
            {
                Debug.Log($"  Volume '{kvp.Key.gameObject.name}': {kvp.Value.fragments.Count} fragments, {kvp.Value.caps.Count} caps");
            }

            // 7. Create GameObjects for each volume with assigned fragments
            int createdCount = 0;
            foreach (var kvp in results)
            {
                CCutVolume volume = kvp.Key;
                VolumeResult result = kvp.Value;

                if (result.fragments.Count == 0 && result.caps.Count == 0)
                {
                    Debug.Log($"  Skipping volume '{volume.gameObject.name}': no geometry.");
                    continue;
                }

                var (mesh, materials) = MeshSlicer.BuildMesh(result);
                if (mesh == null)
                {
                    Debug.LogWarning($"  BuildMesh returned null for volume '{volume.gameObject.name}'.");
                    continue;
                }

                mesh.name = $"Generated_{volume.gameObject.name}_Mesh";

                GameObject go = new GameObject($"Generated_{volume.gameObject.name}");
                // Set world-space identity transform first (mesh data is in world space)
                go.transform.position = Vector3.zero;
                go.transform.rotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                // Parent with worldPositionStays=true to preserve world-space transform
                go.transform.SetParent(volume.transform, true);

                MeshFilter goMF = go.AddComponent<MeshFilter>();
                goMF.sharedMesh = mesh;

                MeshRenderer goMR = go.AddComponent<MeshRenderer>();
                goMR.sharedMaterials = materials;

#if UNITY_EDITOR
                Undo.RegisterCreatedObjectUndo(go, "CMonumentBuilder Generate");
#endif

                createdCount++;
                Debug.Log($"  Created '{go.name}' with {mesh.vertexCount} vertices, {materials.Length} material(s).");
            }

            // 8. Deactivate source
            if (sourceAsset.activeSelf)
            {
#if UNITY_EDITOR
                Undo.RecordObject(sourceAsset, "CMonumentBuilder Deactivate Source");
#endif
                sourceAsset.SetActive(false);
            }

            // 9. Log summary
            Debug.Log($"CMonumentBuilder: Done. Generated {createdCount} piece(s) from {rootVolumes.Count} root volume(s).");
        }

        /// <summary>
        /// Removes all previously generated children (GameObjects named "Generated_*")
        /// from all CCutVolume descendants.
        /// </summary>
        public void CleanGenerated()
        {
            var toDestroy = new List<GameObject>();

            // Search all descendants for Generated_ objects
            var allVolumes = GetComponentsInChildren<CCutVolume>(true);
            foreach (var vol in allVolumes)
            {
                foreach (Transform child in vol.transform)
                {
                    if (child.gameObject.name.StartsWith("Generated_"))
                    {
                        toDestroy.Add(child.gameObject);
                    }
                }
            }

            foreach (var go in toDestroy)
            {
#if UNITY_EDITOR
                Undo.DestroyObjectImmediate(go);
#else
                DestroyImmediate(go);
#endif
            }

            // Reactivate source if it was deactivated
            if (sourceAsset != null && !sourceAsset.activeSelf)
            {
#if UNITY_EDITOR
                Undo.RecordObject(sourceAsset, "CMonumentBuilder Reactivate Source");
#endif
                sourceAsset.SetActive(true);
            }
        }
    }
}
