// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.11.2025
// =========================================

using AldaEngine;
using AldaEngine.Analytics;

namespace TycoonBuilder
{
	public class CVehicleDepotTutorialFunnel
	{
		private readonly CTutorialAnalyticsFunnel _funnel;

		public CVehicleDepotTutorialFunnel(IAnalytics analytics)
		{
			_funnel = new CTutorialAnalyticsFunnel("VehicleDepotTutorial", analytics);
		}
		
		public void Send(EVehicleDepotFunnelStep step)
		{
			_funnel.Send(new SVehicleDepotFunnelStep(step));
		}
	}
}