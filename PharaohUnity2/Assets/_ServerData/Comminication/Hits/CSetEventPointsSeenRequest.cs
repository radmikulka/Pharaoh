// =========================================
// AUTHOR: Marek Karaba
// DATE:   05.01.2026
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CSetEventPointsSeenRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public ELiveEvent LiveEvent { get; set; }
		[JsonProperty] public int PointsSeen { get; set; }
		
		public CSetEventPointsSeenRequest() : base(EHit.SetEventPointSeenRequest)
		{
		}
		
		public CSetEventPointsSeenRequest(ELiveEvent liveEvent, int pointsSeen) : base(EHit.SetEventPointSeenRequest)
		{
			PointsSeen = pointsSeen;
			LiveEvent = liveEvent;
		}
	}
	
	public class CSetEventPointsSeenResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
	
		public CSetEventPointsSeenResponse() : base(EHit.SetEventPointSeenResponse)
		{
		}
	
		public CSetEventPointsSeenResponse(CModifiedUserDataDto modifiedData) : base(EHit.SetEventPointSeenResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}