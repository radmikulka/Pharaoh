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

		private CCameraMover _cameraMover;
		private IEventBus _eventBus;
		private CMission _mission;

		[Inject]
		private void Inject(CCameraMover cameraMover, IEventBus eventBus, CMission regionController)
		{
			_mission = regionController;
			_cameraMover = cameraMover;
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			WarpCamera();

			_eventBus.Subscribe<CMissionActivatedSignal>(OnMssionActivated);
		}

		private void OnMssionActivated(CMissionActivatedSignal signal)
		{
			if(!_mission.IsActive)
				return;
			
			WarpCamera();
		}

		private void WarpCamera()
		{
			_cameraMover.Warp(_defaultPosition.position);
		}
	}
}