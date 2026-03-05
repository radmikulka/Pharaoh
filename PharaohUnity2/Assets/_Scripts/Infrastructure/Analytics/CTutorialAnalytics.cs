// =========================================
// AUTHOR: Juraj Joscak
// DATE:   10.12.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;

namespace TycoonBuilder
{
	public class CTutorialAnalytics : IAldaFrameworkComponent
	{
		private readonly IAnalytics _analytics;
		
		private readonly Dictionary<string, object> _cachedParams = new();
		
		public CTutorialAnalytics(IAnalytics analytics)
		{
			_analytics = analytics;
		}

		public void UXInfoShow(EScreenInfoId name)
		{
			_cachedParams.Clear();
			
			_cachedParams.Add("Name", name);
			_analytics.SendData("UXInfoShow", _cachedParams);
		}
	}
}