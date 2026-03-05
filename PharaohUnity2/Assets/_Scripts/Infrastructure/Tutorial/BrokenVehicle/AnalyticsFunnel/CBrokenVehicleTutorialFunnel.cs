// =========================================
// AUTHOR: Radek Mikulka
// DATE:   27.11.2025
// =========================================

using AldaEngine;

namespace TycoonBuilder
{
	public class CBrokenVehicleTutorialFunnel
	{
		private readonly CTutorialAnalyticsFunnel _funnel;

		public CBrokenVehicleTutorialFunnel(IAnalytics analytics)
		{
			_funnel = new CTutorialAnalyticsFunnel("BrokenVehicleTutorial", analytics);
		}
		
		public void Send(EBrokenVehicleFunnelStep step)
		{
			_funnel.Send(new SBrokenVehicleFunnelStep(step));
		}
	}
}