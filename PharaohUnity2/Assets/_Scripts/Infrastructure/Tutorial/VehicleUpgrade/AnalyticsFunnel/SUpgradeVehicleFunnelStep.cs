// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.12.2025
// =========================================

using AldaEngine.Analytics;

namespace TycoonBuilder
{
	public struct SUpgradeVehicleFunnelStep : IFunnelStep
	{
		private readonly EUpgradeVehicleFunnelStep _step;
		public string StepName => _step.ToString();
		public int Step => (int) _step;

		public SUpgradeVehicleFunnelStep(EUpgradeVehicleFunnelStep step)
		{
			_step = step;
		}
	}
}