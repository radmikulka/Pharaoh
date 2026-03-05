// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.11.2025
// =========================================

using AldaEngine.Analytics;

namespace TycoonBuilder
{
	public struct SGetMoreMaterialFunnelStep : IFunnelStep
	{
		private readonly EGetMoreMaterialFunnelStep _step;
		public string StepName => _step.ToString();
		public int Step => (int) _step;

		public SGetMoreMaterialFunnelStep(EGetMoreMaterialFunnelStep step)
		{
			_step = step;
		}
	}
}