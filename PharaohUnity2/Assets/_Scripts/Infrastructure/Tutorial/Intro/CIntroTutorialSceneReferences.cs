// =========================================
// AUTHOR: Radek Mikulka
// DATE:   29.09.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CIntroTutorialSceneReferences : MonoBehaviour, IInitializable
	{
		[SerializeField] private CStonesOnRoadAnimation _stonesOnRoadAnimation;
		[SerializeField] private CinemachineCamera _stonesOnRoadCenterViewCamera;
		
		private IEventBus _eventBus;
		
		public CStonesOnRoadAnimation StonesOnRoadAnimation => _stonesOnRoadAnimation;
		public CinemachineCamera StonesOnRoadCenterViewCamera => _stonesOnRoadCenterViewCamera;
		
		[Inject]
		private void Inject(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			_eventBus.AddTaskHandler<CGetIntroTutorialSceneReferencesRequest, CGetIntroTutorialSceneReferencesResponse>(OnGetIntroTutorialSceneReferences);
		}

		private CGetIntroTutorialSceneReferencesResponse OnGetIntroTutorialSceneReferences(CGetIntroTutorialSceneReferencesRequest arg)
		{
			return new CGetIntroTutorialSceneReferencesResponse(this);
		}
	}
}