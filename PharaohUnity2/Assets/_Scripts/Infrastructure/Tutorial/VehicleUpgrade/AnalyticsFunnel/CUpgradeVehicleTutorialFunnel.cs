// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.12.2025
// =========================================

using AldaEngine;

namespace TycoonBuilder
{
	public class CUpgradeVehicleTutorialFunnel
	{
		private readonly CTutorialAnalyticsFunnel _funnel;

		public CUpgradeVehicleTutorialFunnel(IAnalytics analytics)
		{
			_funnel = new CTutorialAnalyticsFunnel("VehicleUpgradeTutorial", analytics);
		}
		
		public void Send(EUpgradeVehicleFunnelStep step)
		{
			_funnel.Send(new SUpgradeVehicleFunnelStep(step));
		}
	}
}