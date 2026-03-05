// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.01.2026
// =========================================

using System.Collections.Generic;

namespace TycoonBuilder
{
	public class CSceneLightingHandler
	{
		private readonly HashSet<CSceneLightingLock> _sceneLightingLocks = new();
		private readonly ISceneManager _sceneManager;

		public CSceneLightingHandler(ISceneManager sceneManager)
		{
			_sceneManager = sceneManager;
		}

		public void AddSceneLightingLock(CSceneLightingLock lightingLock)
		{
			_sceneLightingLocks.Add(lightingLock);
			Refresh();
		}
		
		public void RemoveSceneLightingLock(CSceneLightingLock lightingLock)
		{
			_sceneLightingLocks.Remove(lightingLock);
			Refresh();
		}

		private void Refresh()
		{
			CSceneLightingLock topPriorityLock = GetTopPriorityLock();
			if(topPriorityLock == null)
				return;
			_sceneManager.SetActiveScene(topPriorityLock.SceneId);
		}
		
		private CSceneLightingLock GetTopPriorityLock()
		{
			CSceneLightingLock topPriorityLock = null;
			foreach (CSceneLightingLock lightingLock in _sceneLightingLocks)
			{
				if (topPriorityLock == null || lightingLock.Priority > topPriorityLock.Priority)
				{
					topPriorityLock = lightingLock;
				}
			}

			return topPriorityLock;
		}
	}
}