// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.11.2025
// =========================================

using AldaEngine.Analytics;

namespace TycoonBuilder
{
	public struct SOpenCityPlotFunnelStep : IFunnelStep
	{
		private readonly EOpenCityPlotFunnelStep _step;
		public string StepName => _step.ToString();
		public int Step => (int) _step;

		public SOpenCityPlotFunnelStep(EOpenCityPlotFunnelStep step)
		{
			_step = step;
		}
	}
}