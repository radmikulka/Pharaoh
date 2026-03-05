// =========================================
// AUTHOR: Radek Mikulka
// DATE:   22.09.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CSetIntroTutorialStepRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public EIntroTutorialStep StepId { get; set; }
		
		public CSetIntroTutorialStepRequest() : base(EHit.SetIntroTutorialStepRequest)
		{
		}
		
		public CSetIntroTutorialStepRequest(EIntroTutorialStep stepId) : base(EHit.SetIntroTutorialStepRequest)
		{
			StepId = stepId;
		}
	}
	
	public class CSetIntroTutorialStepResponse : CResponseHit
	{
		public CSetIntroTutorialStepResponse() : base(EHit.SetIntroTutorialStepResponse)
		{
		}
	}
}