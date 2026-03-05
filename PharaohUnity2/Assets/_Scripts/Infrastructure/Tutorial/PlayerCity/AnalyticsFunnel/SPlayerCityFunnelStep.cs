// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.11.2025
// =========================================

using AldaEngine.Analytics;

namespace TycoonBuilder
{
	public struct SPlayerCityFunnelStep : IFunnelStep
	{
		private readonly EPlayerCityFunnelStep _step;
		public string StepName => _step.ToString();
		public int Step => (int) _step;

		public SPlayerCityFunnelStep(EPlayerCityFunnelStep step)
		{
			_step = step;
		}
	}
}