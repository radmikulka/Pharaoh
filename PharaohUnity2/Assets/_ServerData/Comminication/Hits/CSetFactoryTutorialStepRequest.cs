// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.10.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CSetFactoryTutorialStepRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public EFactoryTutorialStep StepId { get; set; }
		
		public CSetFactoryTutorialStepRequest() : base(EHit.SetFactoryTutorialStepRequest)
		{
		}
		
		public CSetFactoryTutorialStepRequest(EFactoryTutorialStep stepId) : base(EHit.SetFactoryTutorialStepRequest)
		{
			StepId = stepId;
		}
	}
	
	public class CSetFactoryTutorialStepResponse : CResponseHit
	{
		public CSetFactoryTutorialStepResponse() : base(EHit.SetFactoryTutorialStepResponse)
		{
		}
	}
}