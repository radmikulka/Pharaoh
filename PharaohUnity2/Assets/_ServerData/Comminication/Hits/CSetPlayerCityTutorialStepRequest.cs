// =========================================
// AUTHOR: Radek Mikulka
// DATE:   21.10.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CSetPlayerCityTutorialStepRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public EPlayerCityTutorialStep StepId { get; set; }
		
		public CSetPlayerCityTutorialStepRequest() : base(EHit.SetPlayerCityTutorialStepRequest)
		{
		}
		
		public CSetPlayerCityTutorialStepRequest(EPlayerCityTutorialStep stepId) : base(EHit.SetPlayerCityTutorialStepRequest)
		{
			StepId = stepId;
		}
	}
	
	public class CSetPlayerCityTutorialStepResponse : CResponseHit
	{
		public CSetPlayerCityTutorialStepResponse() : base(EHit.SetPlayerCityTutorialStepResponse)
		{
		}
	}
}