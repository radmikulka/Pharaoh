#if UNITY_EDITOR

using System.IO;
using AldaEngine;
using KBCore.Refs;
using TycoonBuilder.Ui;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RemoveComponentsFromPrefabsAndScenes
{
	[MenuItem("Tools/Remove unneeded UI components from prefabs and scenes")]
	public static void RunCleanup()
	{
		RemoveComponent<CUiComponentText>();
		RemoveComponent<CUiComponentImage>();
	}

	public static void RemoveComponent<T>() where T : MonoBehaviour
	{
		int counter = 0;
		// Remove from prefabs
		string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
		long totalGuids = prefabGuids.Length;
		int processedGuids = 0;
		
		foreach (string guid in prefabGuids)
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			
			if (EditorUtility.DisplayCancelableProgressBar("Removing components", $"Loading prefab... {processedGuids}/{totalGuids} ({Path.GetFileName(path)})", (float)processedGuids / totalGuids))
				break;
			
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			
			if (prefab.transform is not RectTransform)
			{
				processedGuids++;
				continue;
			}
			
			if(EditorUtility.DisplayCancelableProgressBar("Removing components", $"Processing prefabs... {processedGuids}/{totalGuids} ({prefab.name})", (float)processedGuids / totalGuids))
				break;
			
			if (prefab == null)
				continue;

			var components = prefab.GetComponentsInChildren<T>(true);
			bool changed = false;
			foreach (var comp in components)
			{
				if (ShouldBeRemoved(comp, prefab))
				{
					Object.DestroyImmediate(comp, true);
					counter++;
					changed = true;
				}
			}
			if (changed)
			{
				EditorUtility.SetDirty(prefab);
				AssetDatabase.SaveAssets();
			}
			processedGuids++;
		}

		// Remove from scenes
		/*string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
		totalGuids = sceneGuids.Length;
		processedGuids = 0;
		foreach (string guid in sceneGuids)
		{
			EditorUtility.DisplayProgressBar("Removing components", $"Loading scene... {processedGuids}/{totalGuids}", (float)processedGuids / totalGuids);
			
			string path = AssetDatabase.GUIDToAssetPath(guid);
			var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
			
			EditorUtility.DisplayProgressBar("Removing components", $"Processing scenes... {processedGuids}/{totalGuids} ({scene.name})", (float)processedGuids / totalGuids);
			
			bool changed = false;
			foreach (var comp in Object.FindObjectsByType<T>(FindObjectsSortMode.None))
			{
				if (ShouldBeRemoved(comp))
				{
					Object.DestroyImmediate(comp);
					counter++;
					changed = true;
				}
			}
			if (changed)
			{
				EditorSceneManager.MarkSceneDirty(scene);
				EditorSceneManager.SaveScene(scene);
			}
			processedGuids++;
		}*/
		
		EditorUtility.ClearProgressBar();

		AssetDatabase.Refresh();
		Debug.LogWarning($"Removed {counter} instances of {typeof(T)}");
	}

	private static bool ShouldBeRemoved(MonoBehaviour component, GameObject prefab)
	{
		if (IsComponentReferenced(component, prefab))
			return false;
		
		if(IsGetComponentCalled(component))
			return false;
		
		return true;
	}
	
	private static bool IsComponentReferenced(Object target, Object prefab)
	{
		//string targetPath = AssetDatabase.GetAssetPath(target);
		string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();

		foreach (string path in allAssetPaths)
		{
			/*if (path == targetPath)
				continue;*/
			
			Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
			foreach (Object asset in assets)
			{
				if (asset == null)
					continue;

				if (asset is GameObject go)
				{
					if(CheckGameObject(go))
						return true;
				}
				else if (asset is SceneAsset scene)
				{
					if (CheckScene(scene))
						return true;
				}
			}
		}
		return false;

		bool CheckGameObject(GameObject gameObject)
		{
			if(gameObject.transform is not RectTransform)
				return false;
			
			var allComponents = gameObject.GetComponentsInChildren<MonoBehaviour>(true);
			foreach (MonoBehaviour comp in allComponents)
			{
				if(IsObjectReferencingComponent(comp, target))
					return true;
			}
			
			// Check parent prefabs if component is part of a nested prefab
			var parent = PrefabUtility.GetCorrespondingObjectFromSource(target);
			while (parent != null)
			{
				if (IsComponentReferenced(parent, null))
					return true;
				parent = PrefabUtility.GetCorrespondingObjectFromSource(parent);
			}

			return false;
		}
		
		bool CheckScene(SceneAsset sceneAsset)
		{
			string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
			var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
			var allComponents = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
			foreach (MonoBehaviour comp in allComponents)
			{
				SerializedObject so = new SerializedObject(comp);
				SerializedProperty prop = so.GetIterator();
			
				while (prop.NextVisible(true))
				{
					if (prop.propertyType == SerializedPropertyType.ObjectReference && prop.objectReferenceValue == target)
					{
						EditorSceneManager.CloseScene(scene, true);
						return true;
					}
				}
			}

			EditorSceneManager.CloseScene(scene, true);
			return false;
		}
		
		/*var children = prefab.GetComponentsInChildren<MonoBehaviour>(true);
		foreach (MonoBehaviour child in children)
		{
			SerializedObject so = new SerializedObject(child);
			SerializedProperty prop = so.GetIterator();
			
			while (prop.NextVisible(true))
			{
				if (prop.propertyType == SerializedPropertyType.ObjectReference && prop.objectReferenceValue == target)
					return true;
			}
		}

		return false;*/
	}
	
	private static bool IsObjectReferencingComponent(Object asset, Object target)
	{
		SerializedObject so = new SerializedObject(asset);
		SerializedProperty prop = so.GetIterator();

		while (prop.NextVisible(true))
		{
			if (prop.propertyType == SerializedPropertyType.ObjectReference && prop.objectReferenceValue == target)
				return true;
		}
		return false;
	}
	
	private static bool IsGetComponentCalled(MonoBehaviour component)
	{
		if(component.transform.GetComponentInParent<CUiComponentGraphicGroup>(true) != null)
			return true;
		
		if(component.transform.GetComponentInParent<CUiComponentMultiTint>(true) != null)
			return true;
		
		if(component.GetComponent<CTabButtonColorSwapper>() != null)
			return true;
		
		if(component is CUiComponentImage && component.GetComponent<CUiImageAlphaAnimator>() != null)
			return true;
		
		return false;
	}
}
#endif