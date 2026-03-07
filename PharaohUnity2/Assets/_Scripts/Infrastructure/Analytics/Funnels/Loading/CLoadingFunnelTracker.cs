// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.11.2025
// =========================================

using AldaEngine;
using AldaEngine.Analytics;

namespace Pharaoh
{
	public class CLoadingFunnelTracker
	{
		private readonly CBaseAnalyticsFunnel _funnel;

		public CLoadingFunnelTracker(IAnalytics analytics)
		{
			_funnel = new CBaseAnalyticsFunnel("TechFlowLoading", analytics);
		}

		public void Send(ELoadingFunnelStep step)
		{
			_funnel.Send(new SLoadingFunnelStep(step));
		}
	}
}