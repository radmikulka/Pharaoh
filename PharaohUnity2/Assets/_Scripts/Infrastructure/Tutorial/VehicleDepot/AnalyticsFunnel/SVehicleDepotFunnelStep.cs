// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.11.2025
// =========================================

using AldaEngine.Analytics;

namespace TycoonBuilder
{
	public struct SVehicleDepotFunnelStep : IFunnelStep
	{
		private readonly EVehicleDepotFunnelStep _step;
		public string StepName => _step.ToString();
		public int Step => (int) _step;

		public SVehicleDepotFunnelStep(EVehicleDepotFunnelStep step)
		{
			_step = step;
		}
	}
}