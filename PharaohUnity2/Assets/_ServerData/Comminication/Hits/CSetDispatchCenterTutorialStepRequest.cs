// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.10.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CSetDispatchCenterTutorialStepRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public EDispatchCenterTutorialStep StepId { get; set; }
		
		public CSetDispatchCenterTutorialStepRequest() : base(EHit.SetDispatchCenterTutorialStepRequest)
		{
		}
		
		public CSetDispatchCenterTutorialStepRequest(EDispatchCenterTutorialStep stepId) : base(EHit.SetDispatchCenterTutorialStepRequest)
		{
			StepId = stepId;
		}
	}
	
	public class CSetDispatchCenterTutorialStepResponse : CResponseHit
	{
		public CSetDispatchCenterTutorialStepResponse() : base(EHit.SetDispatchCenterTutorialStepResponse)
		{
		}
	}
}