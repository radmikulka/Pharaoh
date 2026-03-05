// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.11.2025
// =========================================

using AldaEngine.Analytics;

namespace TycoonBuilder
{
	public struct SFtueFunnelStep : IFunnelStep
	{
		private readonly EFtueFunnelStep _step;
		public string StepName => _step.ToString();
		public int Step => (int) _step;

		public SFtueFunnelStep(EFtueFunnelStep step)
		{
			_step = step;
		}
	}
}