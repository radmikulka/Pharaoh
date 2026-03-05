// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.12.2024
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CInitialCameraPositionProvider : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField, Self] private Transform _defaultPosition;

		private CRegionController _regionController;
		private CCameraMover _cameraMover;
		private IEventBus _eventBus;

		[Inject]
		private void Inject(CCameraMover cameraMover, IEventBus eventBus, CRegionController regionController)
		{
			_regionController = regionController;
			_cameraMover = cameraMover;
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			WarpCamera();

			_eventBus.Subscribe<CRegionActivatedSignal>(OnRegionActivated);
		}

		private void OnRegionActivated(CRegionActivatedSignal signal)
		{
			if(!_regionController.IsActive)
				return;
			
			WarpCamera();
		}

		private void WarpCamera()
		{
			_cameraMover.Warp(_defaultPosition.position);
		}
	}
}