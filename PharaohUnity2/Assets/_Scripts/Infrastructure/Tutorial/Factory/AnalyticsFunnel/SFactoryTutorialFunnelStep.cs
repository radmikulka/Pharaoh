// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.11.2025
// =========================================

using AldaEngine.Analytics;

namespace TycoonBuilder
{
	public struct SFactoryTutorialFunnelStep : IFunnelStep
	{
		private readonly EFactoryTutorialFunnelStep _step;
		public string StepName => _step.ToString();
		public int Step => (int) _step;

		public SFactoryTutorialFunnelStep(EFactoryTutorialFunnelStep step)
		{
			_step = step;
		}
	}
}