// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.12.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CCreateStoryContractRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public EStaticContractId ContractId { get; set; }
		[JsonProperty] public string ContractUid { get; set; }

		public CCreateStoryContractRequest() : base(EHit.CreateStoryContractRequest)
		{
		}
		
		public CCreateStoryContractRequest(EStaticContractId contractId, string contractUid) : base(EHit.CreateStoryContractRequest)
		{
			ContractUid = contractUid;
			ContractId = contractId;
		}
	}

	public class CCreateStoryContractResponse : CResponseHit
	{
		public CCreateStoryContractResponse() : base(EHit.CreateStoryContractResponse)
		{
		}
	}
}