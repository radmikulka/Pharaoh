// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.11.2025
// =========================================

using AldaEngine;
using AldaEngine.Analytics;

namespace TycoonBuilder
{
	public class CDispatchCenterTutorialFunnel
	{
		private readonly CTutorialAnalyticsFunnel _funnel;

		public CDispatchCenterTutorialFunnel(IAnalytics analytics)
		{
			_funnel = new CTutorialAnalyticsFunnel("DispatchCenterTutorial", analytics);
		}
		
		public void Send(EDispatchCenterFunnelStep step)
		{
			_funnel.Send(new SDispatchCenterFunnelStep(step));
		}
	}
}