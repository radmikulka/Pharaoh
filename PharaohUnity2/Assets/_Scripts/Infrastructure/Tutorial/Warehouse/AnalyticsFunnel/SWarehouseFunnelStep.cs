// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.11.2025
// =========================================

using AldaEngine.Analytics;

namespace TycoonBuilder
{
	public struct SWarehouseFunnelStep : IFunnelStep
	{
		private readonly EWarehouseFunnelStep _step;
		public string StepName => _step.ToString();
		public int Step => (int) _step;

		public SWarehouseFunnelStep(EWarehouseFunnelStep step)
		{
			_step = step;
		}
	}
}