// =========================================
// AUTHOR: Radek Mikulka
// DATE:   27.11.2025
// =========================================

using AldaEngine.Analytics;

namespace TycoonBuilder
{
	public struct SBrokenVehicleFunnelStep : IFunnelStep
	{
		private readonly EBrokenVehicleFunnelStep _step;
		public string StepName => _step.ToString();
		public int Step => (int) _step;

		public SBrokenVehicleFunnelStep(EBrokenVehicleFunnelStep step)
		{
			_step = step;
		}
	}
}