// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.11.2025
// =========================================

using AldaEngine;
using AldaEngine.Analytics;

namespace Pharaoh
{
	public class CServiceFunnelTracker
	{
		private readonly CBaseAnalyticsFunnel _funnel;

		public CServiceFunnelTracker(IAnalytics analytics)
		{
			_funnel = new CBaseAnalyticsFunnel("TechFlowService", analytics);
		}

		public void Send(EServiceFunnelStep step)
		{
			_funnel.Send(new SServiceFunnelStep(step));
		}
	}
}