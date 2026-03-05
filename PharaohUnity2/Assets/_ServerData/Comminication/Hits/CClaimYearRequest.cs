// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.07.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CClaimYearRequest : CCommTokenBasedRequest
	{
		public CClaimYearRequest() : base(EHit.ClaimYearRequest)
		{
		}
	}

	public class CClaimYearResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CLiveEventDto[] LiveEventsDto { get; set; }
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CClaimYearResponse() : base(EHit.ClaimYearResponse)
		{
		}
		
		public CClaimYearResponse(CLiveEventDto[] liveEvents, CModifiedUserDataDto modifiedData) : base(EHit.ClaimYearResponse)
		{
			ModifiedData = modifiedData;
			LiveEventsDto = liveEvents;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}