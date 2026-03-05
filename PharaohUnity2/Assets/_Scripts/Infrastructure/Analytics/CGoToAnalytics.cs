// =========================================
// AUTHOR: Juraj Joscak
// DATE:   10.12.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;

namespace TycoonBuilder
{
	public class CGoToAnalytics : IAldaFrameworkComponent
	{
		private readonly IAnalytics _analytics;
		
		private readonly Dictionary<string, object> _cachedParams = new();
		
		public CGoToAnalytics(IAnalytics analytics)
		{
			_analytics = analytics;
		}

		public void UXCityFindClick()
		{
			_cachedParams.Clear();
			_analytics.SendData("UXCityFindClick", _cachedParams);
		}
	}
}