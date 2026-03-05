// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.11.2025
// =========================================

using AldaEngine.Analytics;

namespace TycoonBuilder
{
	public struct SServiceTechFunnelStep : IFunnelStep
	{
		private readonly EServiceTechnicalFlow _step;
		public string StepName => _step.ToString();
		public int Step => (int) _step;

		public SServiceTechFunnelStep(EServiceTechnicalFlow step)
		{
			_step = step;
		}
	}
}