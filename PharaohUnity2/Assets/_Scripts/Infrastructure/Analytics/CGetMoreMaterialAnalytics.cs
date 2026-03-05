// =========================================
// AUTHOR: Juraj Joscak
// DATE:   21.11.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;

namespace TycoonBuilder
{
	public class CGetMoreMaterialAnalytics
	{
		private readonly IAnalytics _analytics;
		
		private readonly Dictionary<string, object> _cachedParams = new();
		
		public CGetMoreMaterialAnalytics(IAnalytics analytics, IEventBus eventBus)
		{
			_analytics = analytics;
		}

		public void GetMoreMenuOpen(SResource requiredResource, string eventSource)
		{
			_cachedParams.Clear();
			_cachedParams.Add("Resource", requiredResource.Id);
			_cachedParams.Add("EventSource", eventSource);
			_analytics.SendData("GetMoreMenuOpen", _cachedParams);
		}

		public void GetMoreConversion(EResource requiredResource, string eventSource, string destination)
		{
			_cachedParams.Clear();
			_cachedParams.Add("Resource", requiredResource);
			_cachedParams.Add("EventSource", eventSource);
			_cachedParams.Add("Destination", destination);
			_analytics.SendData("GetMoreConversion", _cachedParams);
		}
	}
}