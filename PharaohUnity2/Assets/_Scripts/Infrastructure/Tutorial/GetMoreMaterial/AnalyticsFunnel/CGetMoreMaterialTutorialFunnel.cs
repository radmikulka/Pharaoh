// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.11.2025
// =========================================

using AldaEngine;
using AldaEngine.Analytics;

namespace TycoonBuilder
{
	public class CGetMoreMaterialTutorialFunnel
	{
		private readonly CTutorialAnalyticsFunnel _funnel;

		public CGetMoreMaterialTutorialFunnel(IAnalytics analytics)
		{
			_funnel = new CTutorialAnalyticsFunnel("GetMoreMaterialTutorial", analytics);
		}
		
		public void Send(EGetMoreMaterialFunnelStep step)
		{
			_funnel.Send(new SGetMoreMaterialFunnelStep(step));
		}
	}
}