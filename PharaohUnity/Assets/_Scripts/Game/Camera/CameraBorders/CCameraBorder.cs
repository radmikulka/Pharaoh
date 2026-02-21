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

		private CCameraBorders _cameraBorders;
		private CMission mission;
		private IEventBus _eventBus;

		[Inject]
		private void Inject(IEventBus eventBus, CCameraBorders cameraBorders, CMission regionController)
		{
			_cameraBorders = cameraBorders;
			mission = regionController;
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CMissionActivatedSignal>(OnMissionLoaded);
		}

		private void OnMissionLoaded(CMissionActivatedSignal signal)
		{
			if(signal.Mission != mission.MissionId)
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