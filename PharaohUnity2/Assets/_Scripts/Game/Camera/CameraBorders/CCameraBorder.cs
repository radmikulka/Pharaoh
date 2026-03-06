// =========================================
// AUTHOR: Radek Mikulka
// DATE:   23.10.2024
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using Pharaoh;
using Zenject;

namespace Pharaoh
{
	public class CCameraBorder : MonoBehaviour, IInitializable
	{
		[SerializeField] private Bounds _bounds;
		[SerializeField] private bool _drawGizmos = true;

		private CMissionController _missionController;
		private CCameraBorders _cameraBorders;
		private IEventBus _eventBus;

		[Inject]
		private void Inject(IEventBus eventBus, CCameraBorders cameraBorders, CMissionController missionController)
		{
			_missionController = missionController;
			_cameraBorders = cameraBorders;
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CMissionActivatedSignal>(OnRegionLoaded);
		}

		private void OnRegionLoaded(CMissionActivatedSignal signal)
		{
			if(signal.Mission != _missionController.MissionId)
				return;
			_cameraBorders.SetBounds(_bounds);
		}

		private void OnDrawGizmos()
		{
			if(!_drawGizmos)
				return;
			
			OnDrawGizmosSelected();
		}

		private void OnDrawGizmosSelected()
		{
			CGizmos.DrawWireCube(_bounds.center, Color.green, _bounds.size);
		}
	}
}