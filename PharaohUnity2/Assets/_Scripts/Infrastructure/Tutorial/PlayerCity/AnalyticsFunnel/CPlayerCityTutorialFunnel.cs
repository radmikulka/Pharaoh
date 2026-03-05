// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.11.2025
// =========================================

using AldaEngine;
using AldaEngine.Analytics;

namespace TycoonBuilder
{
	public class CPlayerCityTutorialFunnel
	{
		private readonly CTutorialAnalyticsFunnel _funnel;

		public CPlayerCityTutorialFunnel(IAnalytics analytics)
		{
			_funnel = new CTutorialAnalyticsFunnel("PlayerCityTutorial", analytics);
		}
		
		public void Send(EPlayerCityFunnelStep step)
		{
			_funnel.Send(new SPlayerCityFunnelStep(step));
		}
	}
}