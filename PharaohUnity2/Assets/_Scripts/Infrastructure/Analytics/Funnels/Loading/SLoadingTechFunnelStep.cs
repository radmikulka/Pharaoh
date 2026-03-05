// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.11.2025
// =========================================

using AldaEngine.Analytics;

namespace TycoonBuilder
{
	public struct SLoadingTechFunnelStep : IFunnelStep
	{
		private readonly ELoadingTechFlow _step;
		public string StepName => _step.ToString();
		public int Step => (int) _step;

		public SLoadingTechFunnelStep(ELoadingTechFlow step)
		{
			_step = step;
		}
	}
}