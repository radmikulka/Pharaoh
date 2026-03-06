// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.12.2024
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
	public class CInitialCameraPositionProvider : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField, Self] private Transform _defaultPosition;

		private CMissionController _missionController;
		private CCameraMover _cameraMover;
		private IEventBus _eventBus;

		[Inject]
		private void Inject(CCameraMover cameraMover, IEventBus eventBus, CMissionController missionController)
		{
			_missionController = missionController;
			_cameraMover = cameraMover;
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			WarpCamera();

			_eventBus.Subscribe<CMissionActivatedSignal>(OnRegionActivated);
		}

		private void OnRegionActivated(CMissionActivatedSignal signal)
		{
			if(!_missionController.IsActive)
				return;
			
			WarpCamera();
		}

		private void WarpCamera()
		{
			_cameraMover.Warp(_defaultPosition.position);
		}
	}
}