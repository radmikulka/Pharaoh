// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.11.2025
// =========================================

using AldaEngine;
using AldaEngine.Analytics;

namespace TycoonBuilder
{
	public class COpenCityPlotTutorialFunnel
	{
		private readonly CTutorialAnalyticsFunnel _funnel;

		public COpenCityPlotTutorialFunnel(IAnalytics analytics)
		{
			_funnel = new CTutorialAnalyticsFunnel("OpenCityPlotTutorial", analytics);
		}
		
		public void Send(EOpenCityPlotFunnelStep step)
		{
			_funnel.Send(new SOpenCityPlotFunnelStep(step));
		}
	}
}