// =========================================
// AUTHOR: Radek Mikulka
// DATE:   19.11.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using ServerData;
using UnityEngine.SceneManagement;

namespace Pharaoh
{
	public class CLoadedScenes
	{
		private class CLoadedScene : IEquatable<CLoadedScene>
		{
			public readonly CSceneResourceConfig Config;
			public readonly Scene Scene;

			public CLoadedScene(Scene scene, CSceneResourceConfig config)
			{
				Scene = scene;
				Config = config;
			}

			public bool Equals(CLoadedScene other)
			{
				return Scene == other.Scene;
			}

			public override int GetHashCode()
			{
				return Scene.GetHashCode();
			}
		}
        
		private readonly HashSet<CLoadedScene> _loadedScenesSet = new();
		
		private readonly CSceneResourceConfig[] _allSceneConfigs;

		public CLoadedScenes(CSceneResourceConfig[] allSceneConfigs)
		{
			_allSceneConfigs = allSceneConfigs;
		}

		public void AddScene(Scene scene)
		{
			CSceneResourceConfig config = _allSceneConfigs.First(resourceConfig => resourceConfig.SceneName == scene.name);
			CLoadedScene loadedScene = new(scene, config);
			_loadedScenesSet.Add(loadedScene);
		}

		public void Clear()
		{
			_loadedScenesSet.Clear();
		}

		public bool IsSceneLoaded(ESceneId sceneId)
		{
			return _loadedScenesSet.Any(scene => scene.Config.Id == sceneId);
		}

		public void Remove(Scene scene)
		{
			_loadedScenesSet.RemoveWhere(loadedScene => loadedScene.Scene == scene);
		}

		public bool IsBundleUsed(EBundleId sceneBundle)
		{
			foreach (CLoadedScene loadedScene in _loadedScenesSet)
			{
				bool needsBundle = loadedScene.Config.BundleIds.Contains(sceneBundle);
				if (needsBundle)
				{
					return true;
				}
			}
			return false;
		}
	}
}