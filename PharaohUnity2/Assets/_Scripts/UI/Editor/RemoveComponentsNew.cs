#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AldaEngine;
using TycoonBuilder.Ui;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RemoveUnusedCUiComponentImageSafe
{
    [MenuItem("Tools/Remove Unused CUiComponentImage (Safe)")]
    public static void RemoveUnused()
    {
        try
        {
            // --- Phase 1: Collect all CUiComponentImage instances ---
            EditorUtility.DisplayProgressBar("Phase 1/3: Collecting CUiComponentImage", "Scanning prefabs and scenes...", 0f);
            var allTextComponents = new Dictionary<string, (string assetPath, Component component)>();
            var allPrefabGuids = AssetDatabase.FindAssets("t:Prefab");
            var allSceneGuids = AssetDatabase.FindAssets("t:Scene");

            string GetFileId(Component comp)
            {
                if (EditorUtility.IsPersistent(comp))
                {
                    ulong fileId = Unsupported.GetLocalIdentifierInFileForPersistentObject(comp);
                    return $"{AssetDatabase.GetAssetPath(comp)}:{fileId}";
                }
                else
                {
                    // For scene objects, use scene path and instance ID
                    var go = comp.gameObject;
                    var scene = go.scene;
                    return $"{scene.path}:{comp.GetInstanceID()}";
                }
            }

            // Prefabs
            for (int i = 0; i < allPrefabGuids.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Phase 1/3: Collecting CUiComponentImage", $"Scanning prefab {i + 1}/{allPrefabGuids.Length}", (float)i / allPrefabGuids.Length);
                string path = AssetDatabase.GUIDToAssetPath(allPrefabGuids[i]);
                var prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefabRoot == null) continue;
                foreach (var text in prefabRoot.GetComponentsInChildren<CUiComponentImage>(true))
                {
                    string id = GetFileId(text);
                    if (!allTextComponents.ContainsKey(id))
                        allTextComponents.Add(id, (path, text));
                }
            }

            // Scenes
            for (int i = 0; i < allSceneGuids.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Phase 1/3: Collecting CUiComponentImage", $"Scanning scene {i + 1}/{allSceneGuids.Length}", (float)i / allSceneGuids.Length);
                string path = AssetDatabase.GUIDToAssetPath(allSceneGuids[i]);
                
                if(Path.GetFileName(path) != "_UI.unity")
                    continue;
                
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                foreach (var text in UnityEngine.Object.FindObjectsByType<CUiComponentImage>(FindObjectsSortMode.None))
                {
                    string id = GetFileId(text);
                    if (!allTextComponents.ContainsKey(id))
                        allTextComponents.Add(id, (path, text));
                }
            }

            // --- Phase 2: Collect all references to CUiComponentImage ---
            var referencedIds = new HashSet<string>();
            
            for (int i = 0; i < allPrefabGuids.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Phase 2/3: Collecting References", $"Scanning prefab {i + 1}/{allPrefabGuids.Length}", (float)i / allPrefabGuids.Length);
                string path = AssetDatabase.GUIDToAssetPath(allPrefabGuids[i]);
            
                // Scan prefab contents (instance hierarchy)
                var prefabContents = PrefabUtility.LoadPrefabContents(path);
                var allComponents = prefabContents.GetComponentsInChildren<MonoBehaviour>(true);
                foreach (var comp in allComponents)
                {
                    if (comp == null) continue;
                    var so = new SerializedObject(comp);
                    var prop = so.GetIterator();
                    while (prop.NextVisible(true))
                    {
                        if (prop.propertyType == SerializedPropertyType.ObjectReference && prop.objectReferenceValue is CUiComponentImage refText)
                        {
                            string id = GetFileId(refText);
                            referencedIds.Add(id);
                        }
                    }
                }
                PrefabUtility.UnloadPrefabContents(prefabContents);
            
                // Scan prefab asset for overrides
                var assets = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var asset in assets)
                {
                    if (asset is GameObject go)
                    {
                        var comps = go.GetComponentsInChildren<MonoBehaviour>(true);
                        foreach (var comp in comps)
                        {
                            if (comp == null) continue;
                            var so = new SerializedObject(comp);
                            var prop = so.GetIterator();
                            while (prop.NextVisible(true))
                            {
                                if (prop.propertyType == SerializedPropertyType.ObjectReference && prop.objectReferenceValue is CUiComponentImage refText)
                                {
                                    string id = GetFileId(refText);
                                    referencedIds.Add(id);
                                }
                            }
                        }
                    }
                }
            }

            // --- Phase 3: Remove unused CUiComponentImage components ---
            int removedCount = 0;
            var allToRemove = allTextComponents.Where(kvp =>
                !referencedIds.Contains(kvp.Key) &&
                !HasRequiredParent(kvp.Value.component.transform)).ToList();

            for (int i = 0; i < allToRemove.Count; i++)
            {
                EditorUtility.DisplayProgressBar("Phase 3/3: Removing Unused", $"Removing {i + 1}/{allToRemove.Count}", (float)i / allToRemove.Count);
                var (id, (assetPath, comp)) = (allToRemove[i].Key, allToRemove[i].Value);

                if (assetPath.EndsWith(".prefab"))
                {
                    var prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    if (prefabRoot == null) continue;
                    UnityEngine.Object.DestroyImmediate(comp, true);
                    EditorUtility.SetDirty(prefabRoot);
                    removedCount++;
                }
                else if (assetPath.EndsWith(".unity"))
                {
                    var scene = EditorSceneManager.GetSceneByPath(assetPath);
                    if (!scene.isLoaded)
                        scene = EditorSceneManager.OpenScene(assetPath, OpenSceneMode.Single);
                    UnityEngine.Object.DestroyImmediate(comp);
                    EditorSceneManager.MarkSceneDirty(scene);
                    removedCount++;
                }
            }

            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"Removed {removedCount} unused CUiComponentImage components.");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }
    }

    private static bool HasRequiredParent(Transform t)
    {
        while (t != null)
        {
            if (t.GetComponent<CUiComponentGraphicGroup>() != null) return true;
            if (t.GetComponent<CUiComponentMultiTint>() != null) return true;
            if (t.GetComponent<CTabButtonColorSwapper>() != null) return true;
            if (t.GetComponent<CUiImageAlphaAnimator>() != null) return true;
            t = t.parent;
        }
        return false;
    }
}
#endif