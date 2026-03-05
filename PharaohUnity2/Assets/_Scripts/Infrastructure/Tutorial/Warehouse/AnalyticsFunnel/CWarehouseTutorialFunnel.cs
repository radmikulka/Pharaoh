// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.11.2025
// =========================================

using AldaEngine;
using AldaEngine.Analytics;

namespace TycoonBuilder
{
	public class CWarehouseTutorialFunnel
	{
		private readonly CTutorialAnalyticsFunnel _funnel;

		public CWarehouseTutorialFunnel(IAnalytics analytics)
		{
			_funnel = new CTutorialAnalyticsFunnel("WarehouseTutorial", analytics);
		}
		
		public void Send(EWarehouseFunnelStep step)
		{
			_funnel.Send(new SWarehouseFunnelStep(step));
		}
	}
}