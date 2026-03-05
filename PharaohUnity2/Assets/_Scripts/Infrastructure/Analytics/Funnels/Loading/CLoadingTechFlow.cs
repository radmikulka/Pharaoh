// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.11.2025
// =========================================

using AldaEngine;
using AldaEngine.Analytics;

namespace TycoonBuilder
{
	public class CLoadingTechFlow
	{
		private readonly CBaseAnalyticsFunnel _funnel;

		public CLoadingTechFlow(IAnalytics analytics)
		{
			_funnel = new CBaseAnalyticsFunnel("TechFlowLoading", analytics);
		}
		
		public void Send(ELoadingTechFlow step)
		{
			_funnel.Send(new SLoadingTechFunnelStep(step));
		}
	}
}