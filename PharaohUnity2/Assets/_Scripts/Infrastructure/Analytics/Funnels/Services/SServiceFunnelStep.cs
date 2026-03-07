// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.11.2025
// =========================================

using AldaEngine.Analytics;

namespace Pharaoh
{
	public struct SServiceFunnelStep : IFunnelStep
	{
		private readonly EServiceFunnelStep _step;
		public string StepName => _step.ToString();
		public int Step => (int) _step;

		public SServiceFunnelStep(EServiceFunnelStep step)
		{
			_step = step;
		}
	}
}