// =========================================
// AUTHOR: Radek Mikulka
// DATE:   23.05.2024
// =========================================

using System;
using System.Reflection;
using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TycoonBuilder;
using ShadowResolution = UnityEngine.Rendering.Universal.ShadowResolution;

#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace TycoonBuilder.Infrastructure
{
	public class CGraphicsQualityProvider : IInitializable, IGraphicsQualityProvider
	{
		public EGraphicsQuality Quality => (EGraphicsQuality)_settingsData.Graphics.Value;

		private readonly CSettingsData _settingsData;
		private readonly IEventBus _eventBus;
		private bool _deferred;
		private bool _hasPendingChange;

		public CGraphicsQualityProvider(CSettingsData settingsData, IEventBus eventBus)
		{
			_settingsData = settingsData;
			_eventBus = eventBus;
			_settingsData.Graphics.OnValueChanged += OnSettingsChanged;
		}

		public void Initialize()
		{
			OnSettingsChanged(_settingsData.Graphics.Value);
		}

		public void DeferChanges()
		{
			_deferred = true;
		}

		public void ApplyDeferredChanges()
		{
			_deferred = false;
			if (_hasPendingChange)
			{
				_hasPendingChange = false;
				_eventBus.Send(new CGraphicsQualityChangedSignal(Quality));
			}
		}

		private void OnSettingsChanged(int _)
		{
			if (_deferred)
			{
				_hasPendingChange = true;
				return;
			}
			_eventBus.Send(new CGraphicsQualityChangedSignal(Quality));
		}
	}
}