// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.10.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CSetWarehouseTutorialStepRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public EWarehouseTutorialStep StepId { get; set; }
		
		public CSetWarehouseTutorialStepRequest() : base(EHit.SetWarehouseTutorialStepRequest)
		{
		}
		
		public CSetWarehouseTutorialStepRequest(EWarehouseTutorialStep stepId) : base(EHit.SetWarehouseTutorialStepRequest)
		{
			StepId = stepId;
		}
	}
	
	public class CSetWarehouseTutorialStepResponse : CResponseHit
	{
		public CSetWarehouseTutorialStepResponse() : base(EHit.SetWarehouseTutorialStepResponse)
		{
		}
	}
}