// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.10.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CSetVehicleDepotTutorialStepRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public EVehicleDepotTutorialStep StepId { get; set; }
		
		public CSetVehicleDepotTutorialStepRequest() : base(EHit.SetVehicleDepotTutorialStepRequest)
		{
		}
		
		public CSetVehicleDepotTutorialStepRequest(EVehicleDepotTutorialStep stepId) : base(EHit.SetVehicleDepotTutorialStepRequest)
		{
			StepId = stepId;
		}
	}
	
	public class CSetVehicleDepotTutorialStepResponse : CResponseHit
	{
		public CSetVehicleDepotTutorialStepResponse() : base(EHit.SetVehicleDepotTutorialStepResponse)
		{
		}
	}
}