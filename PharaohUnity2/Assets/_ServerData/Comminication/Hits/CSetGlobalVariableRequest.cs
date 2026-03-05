// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.07.2024
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CSetGlobalVariableRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public CGlobalVariableDto Variable { get; set; }
	
		public CSetGlobalVariableRequest() : base(EHit.SetGlobalVariableRequest)
		{
		}
	
		public CSetGlobalVariableRequest(CGlobalVariableDto variable) : base(EHit.SetGlobalVariableRequest)
		{
			Variable = variable;
		}
	}

	public class CSetGlobalVariableResponse : CResponseHit
	{
		public CSetGlobalVariableResponse() : base(EHit.SetGlobalVariableResponse)
		{
		}
	}
}

