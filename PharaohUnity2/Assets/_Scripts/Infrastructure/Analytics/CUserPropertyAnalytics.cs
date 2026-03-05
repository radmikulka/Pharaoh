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
		private readonly CSettingsData _settingsData;
		private readonly IAnalytics _analytics;
		private readonly IEventBus _eventBus;
		private readonly CUser _user;

		public CUserPropertyAnalytics(
			IDeviceIdProvider deviceIdProvider, 
			CSettingsData settingsData, 
			IAnalytics analytics, 
			IEventBus eventBus, 
			CUser user
			)
		{
			_deviceIdProvider = deviceIdProvider;
			_settingsData = settingsData;
			_analytics = analytics;
			_eventBus = eventBus;
			_user = user;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CGraphicsQualityChangedSignal>(OnQualityChanged);
			_eventBus.Subscribe<CXpIncreasedSignal>(OnXpIncreased);
			_eventBus.Subscribe<CYearSeenSignal>(OnYearSeen);
			

			SetYear(_user.Progress.Year);
			SetQuality((EGraphicsQuality)_settingsData.Graphics.Value);
			RefreshResolution();
			RefreshDecadePassTier();
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

		private void OnXpIncreased(CXpIncreasedSignal signal)
		{
			RefreshDecadePassTier();
		}

		private void OnYearSeen(CYearSeenSignal signal)
		{
			SetYear(signal.YearMilestone);
		}

		private void SetYear(EYearMilestone year)
		{
			_analytics.SetUserProperty("Year", (int)year);
		}

		private void RefreshDecadePassTier()
		{
			int maxClaimableIndex = _user.DecadePass.GetMaxClaimableIndex();
			_analytics.SetUserProperty("DecadePassTier", maxClaimableIndex);
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