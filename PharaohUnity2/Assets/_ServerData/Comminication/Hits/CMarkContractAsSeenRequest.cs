// =========================================
// AUTHOR: Radek Mikulka
// DATE:   04.11.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CMarkContractAsSeenRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public string ContractUid { get; set; }
			
		public CMarkContractAsSeenRequest() : base(EHit.MarkContractAsSeenRequest)
		{
		}
			
		public CMarkContractAsSeenRequest(string contractUid) : base(EHit.MarkContractAsSeenRequest)
		{
			ContractUid = contractUid;
		}
	}

	public class CMarkContractAsSeenResponse : CResponseHit
	{
		public CMarkContractAsSeenResponse() : base(EHit.MarkContractAsSeenResponse)
		{
		}
	}
}