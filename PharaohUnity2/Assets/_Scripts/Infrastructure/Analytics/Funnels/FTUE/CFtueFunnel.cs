// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.11.2025
// =========================================

using AldaEngine;
using AldaEngine.Analytics;

namespace TycoonBuilder
{
	public class CFtueFunnel : ICFtueFunnel
	{
		private readonly CTutorialAnalyticsFunnel _funnel;

		public EFtueFunnelStep CurrentStep => (EFtueFunnelStep)_funnel.CurrentValue.Step;

		public CFtueFunnel(IAnalytics analytics)
		{
			_funnel = new CTutorialAnalyticsFunnel("FtueFunnel", analytics);
		}
		
		public void Send(EFtueFunnelStep step)
		{
			_funnel.Send(new SFtueFunnelStep(step));
		}
	}
}