// =========================================
// AUTHOR: Radek Mikulka
// DATE:   05.01.2026
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CClaimEventLeaderboardRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public ELiveEvent LiveEvent { get; set; }
		[JsonProperty] public int Rank { get; set; }
		[JsonProperty] public int PointsOnRank { get; set; }
		
		public CClaimEventLeaderboardRequest() : base(EHit.ClaimEventLeaderboardRequest)
		{
		}
		
		public CClaimEventLeaderboardRequest(ELiveEvent liveEvent, int rank, int pointsOnRank) : base(EHit.ClaimEventLeaderboardRequest)
		{
			PointsOnRank = pointsOnRank;
			LiveEvent = liveEvent;
			Rank = rank;
		}
	}

	public class CClaimEventLeaderboardResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CClaimEventLeaderboardResponse() : base(EHit.ClaimEventLeaderboardResponse)
		{
		}
		
		public CClaimEventLeaderboardResponse(CModifiedUserDataDto modifiedData) : base(EHit.ClaimEventLeaderboardResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}