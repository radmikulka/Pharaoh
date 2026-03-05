// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.11.2025
// =========================================

using AldaEngine.Analytics;

namespace TycoonBuilder
{
	public struct SDispatchCenterFunnelStep : IFunnelStep
	{
		private readonly EDispatchCenterFunnelStep _step;
		public string StepName => _step.ToString();
		public int Step => (int) _step;

		public SDispatchCenterFunnelStep(EDispatchCenterFunnelStep step)
		{
			_step = step;
		}
	}
}