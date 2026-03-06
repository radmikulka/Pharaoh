// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.11.2025
// =========================================

using AldaEngine;
using AldaEngine.Analytics;

namespace Pharaoh
{
	public class CServicesTechFlow
	{
		private readonly CBaseAnalyticsFunnel _funnel;

		public CServicesTechFlow(IAnalytics analytics)
		{
			_funnel = new CBaseAnalyticsFunnel("TechFlowService", analytics);
		}
		
		public void Send(EServiceTechnicalFlow step)
		{
			_funnel.Send(new SServiceTechFunnelStep(step));
		}
	}
}