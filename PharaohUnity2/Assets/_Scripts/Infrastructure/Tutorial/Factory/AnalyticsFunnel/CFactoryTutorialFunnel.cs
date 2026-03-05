// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.11.2025
// =========================================

using AldaEngine;
using AldaEngine.Analytics;

namespace TycoonBuilder
{
	public class CFactoryTutorialFunnel
	{
		private readonly CTutorialAnalyticsFunnel _funnel;

		public CFactoryTutorialFunnel(IAnalytics analytics)
		{
			_funnel = new CTutorialAnalyticsFunnel("FactoryTutorial", analytics);
		}
		
		public void Send(EFactoryTutorialFunnelStep step)
		{
			_funnel.Send(new SFactoryTutorialFunnelStep(step));
		}
	}
}