// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.03.2026
// =========================================

using Newtonsoft.Json;

namespace ServerData.Hits
{
	public class CSetEventLastSeenRankRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public ELiveEvent LiveEvent { get; set; }
		[JsonProperty] public int Rank { get; set; }

		public CSetEventLastSeenRankRequest() : base(EHit.SetEventLastSeenRankRequest)
		{
		}

		public CSetEventLastSeenRankRequest(ELiveEvent liveEvent, int rank) : base(EHit.SetEventLastSeenRankRequest)
		{
			LiveEvent = liveEvent;
			Rank = rank;
		}
	}

	public class CSetEventLastSeenRankResponse : CResponseHit
	{
		public CSetEventLastSeenRankResponse() : base(EHit.SetEventLastSeenRankResponse)
		{
		}
	}
}
