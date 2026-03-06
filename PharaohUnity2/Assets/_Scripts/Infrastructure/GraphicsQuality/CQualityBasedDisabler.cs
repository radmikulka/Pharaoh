// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.09.2024
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using Pharaoh.Infrastructure;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
	public class CQualityBasedDisabler : MonoBehaviour, IConstructable, IInitializable
	{
		[SerializeField] private EGraphicsQuality _minQuality;
        [SerializeField] private EGraphicsQuality _maxQuality = EGraphicsQuality.High;
        [SerializeField] private GameObject[] _objects;
		
		private IGraphicsQualityProvider _graphicsQualityProvider;
		private IEventBus _eventBus;
		
		[Inject]
		private void Inject(IGraphicsQualityProvider graphicsQualityProvider, IEventBus eventBus)
		{
			_graphicsQualityProvider = graphicsQualityProvider;
			_eventBus = eventBus;
		}

		public void Construct()
		{
			Refresh();
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CGraphicsQualityChangedSignal>(OnGraphicsQualityChanged);
		}

		private void OnGraphicsQualityChanged(CGraphicsQualityChangedSignal signal)
		{
			Refresh();
		}

		private void Refresh()
		{
            var q = _graphicsQualityProvider.Quality;
            bool isInRange = q >= _minQuality && q <= _maxQuality;

            foreach (GameObject o in _objects)
			{
				o.SetActiveObject(isInRange);
			}
		}
	}
}