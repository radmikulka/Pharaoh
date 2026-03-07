// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.11.2025
// =========================================

using AldaEngine.Analytics;

namespace Pharaoh
{
	public struct SLoadingFunnelStep : IFunnelStep
	{
		private readonly ELoadingFunnelStep _step;
		public string StepName => _step.ToString();
		public int Step => (int) _step;

		public SLoadingFunnelStep(ELoadingFunnelStep step)
		{
			_step = step;
		}
	}
}