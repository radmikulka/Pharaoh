// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.01.2026
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CSetSeenCompletedPiggyContractsRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public string OfferId { get; set; }
		[JsonProperty] public int SeenContracts { get; set; }
		
		public CSetSeenCompletedPiggyContractsRequest() : base(EHit.SetSeenCompletedPiggyContractsRequest)
		{
		}
		
		public CSetSeenCompletedPiggyContractsRequest(string offerId, int seenContracts) : base(EHit.SetSeenCompletedPiggyContractsRequest)
		{
			SeenContracts = seenContracts;
			OfferId = offerId;
		}
	}

	public class CSetSeenCompletedPiggyContractsResponse : CResponseHit
	{
		public CSetSeenCompletedPiggyContractsResponse() : base(EHit.SetSeenCompletedPiggyContractsResponse)
		{
		}
	}
}