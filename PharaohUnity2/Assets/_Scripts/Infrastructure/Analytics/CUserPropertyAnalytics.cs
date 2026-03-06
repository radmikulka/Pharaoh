// =========================================
// AUTHOR: Radek Mikulka
// DATE:   05.11.2025
// =========================================

using System.Globalization;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;
using UnityEngine.Analytics;
using UnityEngine.Device;

namespace TycoonBuilder
{
	public class CUserPropertyAnalytics : IInitializable
	{
		private readonly IDeviceIdProvider _deviceIdProvider;
		private readonly IAnalytics _analytics;
		private readonly IEventBus _eventBus;

		public CUserPropertyAnalytics(
			IDeviceIdProvider deviceIdProvider, 
			IAnalytics analytics, 
			IEventBus eventBus
			)
		{
			_deviceIdProvider = deviceIdProvider;
			_analytics = analytics;
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CGraphicsQualityChangedSignal>(OnQualityChanged);
			
			RefreshResolution();
			RefreshSystemMemory();
			RefreshAsid().Forget();
		}

		private async UniTaskVoid RefreshAsid()
		{
			CDeviceIdResult id = await _deviceIdProvider.GetIdsAsync();
			_analytics.SetUserProperty("asid", id.AdvertisingId);
		}

		private void OnQualityChanged(CGraphicsQualityChangedSignal signal)
		{
			SetQuality(signal.Quality);
		}
		
		private void SetQuality(EGraphicsQuality quality)
		{
			_analytics.SetUserProperty("GraphicsSetting", (int)quality);
		}

		private void RefreshResolution()
		{
			_analytics.SetUserProperty("ResolutionWidth", Screen.width);
			_analytics.SetUserProperty("ResolutionHeight", Screen.height);
		}

		private void RefreshSystemMemory()
		{
			_analytics.SetUserProperty("SystemMemorySize", SystemInfo.systemMemorySize);
		}
	}
}