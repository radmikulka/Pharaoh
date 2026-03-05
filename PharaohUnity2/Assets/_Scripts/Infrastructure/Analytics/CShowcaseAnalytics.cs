// =========================================
// AUTHOR: Marek Karaba
// DATE:   23.02.2026
// =========================================

using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;
using Zenject;

namespace TycoonBuilder
{
	public class CShowcaseAnalytics : IInitializable
	{
		private readonly IAnalytics _analytics;
		private readonly IEventBus _eventBus;
		
		private readonly Dictionary<string, object> _cachedParams = new();

		public CShowcaseAnalytics(IAnalytics analytics, IEventBus eventBus)
		{
			_analytics = analytics;
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CVehicleShownSignal>(OnVehicleShown);
		}

		private void OnVehicleShown(CVehicleShownSignal signal)
		{
			_cachedParams.Clear();
			
			_cachedParams.Add("Type", signal.VehicleShownType);
			_cachedParams.Add("VehicleId", signal.VehicleId);
			
			_analytics.SendData("ShowcaseOpen", _cachedParams);
		}
	}
}