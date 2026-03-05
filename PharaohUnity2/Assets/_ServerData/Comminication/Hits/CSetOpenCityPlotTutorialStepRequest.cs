// =========================================
// AUTHOR: Radek Mikulka
// DATE:   31.10.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CSetOpenCityPlotTutorialStepRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public EOpenCityPlotStep StepId { get; set; }
		
		public CSetOpenCityPlotTutorialStepRequest() : base(EHit.SetOpenCityPlotTutorialStepRequest)
		{
		}
		
		public CSetOpenCityPlotTutorialStepRequest(EOpenCityPlotStep stepId) : base(EHit.SetOpenCityPlotTutorialStepRequest)
		{
			StepId = stepId;
		}
	}
	
	public class CSetOpenCityPlotTutorialStepResponse : CResponseHit
	{
		public CSetOpenCityPlotTutorialStepResponse() : base(EHit.SetOpenCityPlotTutorialStepResponse)
		{
		}
	}
}