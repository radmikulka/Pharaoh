// =========================================
// AUTHOR: Radek Mikulka
// DATE:   23.10.2024
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using TycoonBuilder;
using Zenject;

namespace TycoonBuilder
{
	public class CCameraBorder : MonoBehaviour, IInitializable
	{
		[SerializeField] private Bounds _bounds;
		[SerializeField] private bool _drawGizmos = true;

		private CRegionController _regionController;
		private CCameraBorders _cameraBorders;
		private IEventBus _eventBus;

		[Inject]
		private void Inject(IEventBus eventBus, CCameraBorders cameraBorders, CRegionController regionController)
		{
			_regionController = regionController;
			_cameraBorders = cameraBorders;
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CRegionActivatedSignal>(OnRegionLoaded);
		}

		private void OnRegionLoaded(CRegionActivatedSignal signal)
		{
			if(signal.Region != _regionController.RegionId)
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