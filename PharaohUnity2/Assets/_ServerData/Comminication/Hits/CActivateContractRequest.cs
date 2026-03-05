// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.08.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CActivateContractRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public string ContractUid { get; set; }

		public CActivateContractRequest() : base(EHit.ActivateContractRequest)
		{
		}
		
		public CActivateContractRequest(string contractUid) : base(EHit.ActivateContractRequest)
		{
			ContractUid = contractUid;
		}
	}

	public class CActivateContractResponse : CResponseHit
	{
		public CActivateContractResponse() : base(EHit.ActivateContractResponse)
		{
		}
	}
}