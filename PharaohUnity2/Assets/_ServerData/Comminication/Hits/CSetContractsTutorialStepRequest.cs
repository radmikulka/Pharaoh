// =========================================
// AUTHOR: Radek Mikulka
// DATE:   23.10.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CSetContractsTutorialStepRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public EContractsTutorialStep StepId { get; set; }
		
		public CSetContractsTutorialStepRequest() : base(EHit.SetContractsTutorialStepRequest)
		{
		}
		
		public CSetContractsTutorialStepRequest(EContractsTutorialStep stepId) : base(EHit.SetContractsTutorialStepRequest)
		{
			StepId = stepId;
		}
	}
	
	public class CSetContractsTutorialStepResponse : CResponseHit
	{
		public CSetContractsTutorialStepResponse() : base(EHit.SetContractsTutorialStepResponse)
		{
		}
	}
}