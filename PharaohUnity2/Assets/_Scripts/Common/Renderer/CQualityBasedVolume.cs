// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.11.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using Zenject;

namespace TycoonBuilder
{
	public class CQualityBasedVolume : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField, Self] private Volume _volume;

		private bool _vignetteInitiallyActive;
		private bool _bloomInitiallyActive;
		private CRenderer _renderer;
		private IEventBus _eventBus;
		
		[Inject]
		private void Inject(IEventBus eventBus, CRenderer rend)
		{
			_eventBus = eventBus;
			_renderer = rend;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CGraphicsQualityChangedSignal>(OnQualityChanged);
			
			CacheInitialBloomState();
			CacheInitialVignetteState();
			
			RefreshBloomState();
			RefreshVignetteState();
		}
		
		private void CacheInitialBloomState()
		{
			if (!_volume.profile.TryGet(out Bloom bloom))
				return;
			_bloomInitiallyActive = bloom.active;
		}
		
		private void CacheInitialVignetteState()
		{
			if (!_volume.profile.TryGet(out Vignette vignette))
				return;
			_vignetteInitiallyActive = vignette.active;
		}

		private void OnQualityChanged(CGraphicsQualityChangedSignal signal)
		{
			RefreshBloomState();
			RefreshVignetteState();
		}
		
		private void RefreshBloomState()
		{
			if(!_bloomInitiallyActive)
				return;
			
			if (!_volume.profile.TryGet(out Bloom bloom)) 
				return;
			
			bool shouldBeActive = _renderer.GraphicsQuality != (int)EGraphicsQuality.Low;
			bloom.active = shouldBeActive;
		}

		private void RefreshVignetteState()
		{
			if (!_vignetteInitiallyActive)
				return;
			
			if (!_volume.profile.TryGet(out Vignette vignette))
				return;
			
			bool shouldBeActive = _renderer.GraphicsQuality != (int)EGraphicsQuality.Low;
			vignette.active = shouldBeActive;
		}
	}
}