// =========================================
// AUTHOR: Radek Mikulka
// DATE:   20.10.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CSetGetMoreMaterialTutorialStepRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public EGetMoreMaterialTutorialStep StepId { get; set; }
		
		public CSetGetMoreMaterialTutorialStepRequest() : base(EHit.SetGetMoreMaterialTutorialStepRequest)
		{
		}
		
		public CSetGetMoreMaterialTutorialStepRequest(EGetMoreMaterialTutorialStep stepId) : base(EHit.SetGetMoreMaterialTutorialStepRequest)
		{
			StepId = stepId;
		}
	}
	
	public class CSetGetMoreMaterialTutorialStepResponse : CResponseHit
	{
		public CSetGetMoreMaterialTutorialStepResponse() : base(EHit.SetGetMoreMaterialTutorialStepResponse)
		{
		}
	}
}