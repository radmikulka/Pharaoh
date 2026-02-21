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

namespace Pharaoh
{
	public class CUserPropertyAnalytics : IInitializable
	{
		private readonly IDeviceIdProvider _deviceIdProvider;
		private readonly IAnalytics _analytics;
		private readonly IEventBus _eventBus;
		private readonly CUser _user;

		public CUserPropertyAnalytics(
			IDeviceIdProvider deviceIdProvider, 
			IAnalytics analytics, 
			IEventBus eventBus, 
			CUser user
			)
		{
			_deviceIdProvider = deviceIdProvider;
			_analytics = analytics;
			_eventBus = eventBus;
			_user = user;
		}

		public void Initialize()
		{
			RefreshResolution();
			RefreshSystemMemory();
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