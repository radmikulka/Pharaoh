// =========================================
// AUTHOR: Radek Mikulka
// DATE:   11.07.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CDispatchForResourceRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public string DispatchUid { get; set; }
		[JsonProperty] public EVehicle Vehicle { get; set; }
		[JsonProperty] public EResource Resource { get; set; }
		[JsonProperty] public long StartTime { get; set; }
		[JsonProperty] public long TravelToTime { get; set; }
		[JsonProperty] public long WaitTime { get; set; }
		[JsonProperty] public long TravelFromTime { get; set; }
		
		public CDispatchForResourceRequest() : base(EHit.DispatchForResourceRequest)
		{
		}
		
		public CDispatchForResourceRequest(
			string dispatchUid, 
			EVehicle vehicle, 
			EResource resource,
			long startTime,
			long travelToTime,
			long waitTime,
			long travelFromTime
			) : base(EHit.DispatchForResourceRequest)
		{
			TravelFromTime = travelFromTime;
			TravelToTime = travelToTime;
			DispatchUid = dispatchUid;
			StartTime = startTime;
			WaitTime = waitTime;
			Resource = resource;
			Vehicle = vehicle;
		}
	}
	
	public class CDispatchForResourceResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CDispatchDto Dispatch { get; set; }
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }

		public CDispatchForResourceResponse() : base(EHit.DispatchForResourceResponse)
		{
		}

		public CDispatchForResourceResponse(CDispatchDto dispatch, CModifiedUserDataDto modifiedData) : base(EHit.DispatchForResourceResponse)
		{
			ModifiedData = modifiedData;
			Dispatch = dispatch;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}