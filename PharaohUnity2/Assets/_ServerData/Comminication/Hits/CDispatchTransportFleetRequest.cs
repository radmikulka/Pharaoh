// =========================================
// AUTHOR: Radek Mikulka
// DATE:   23.02.2026
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CDispatchTransportFleetRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public string DispatchUid { get; set; }
		[JsonProperty] public string ContractUid { get; set; }
		[JsonProperty] public EVehicle[] Vehicles { get; set; }
		[JsonProperty] public long StartTime { get; set; }
		[JsonProperty] public long[] TravelToTimes { get; set; }
		[JsonProperty] public long[] TravelFromTimes { get; set; }

		public CDispatchTransportFleetRequest() : base(EHit.DispatchTransportFleetRequest)
		{
		}

		public CDispatchTransportFleetRequest(
			string dispatchUid,
			string contractUid,
			EVehicle[] vehicles,
			long startTime,
			long[] travelToTimes,
			long[] travelFromTimes
		) : base(EHit.DispatchTransportFleetRequest)
		{
			DispatchUid = dispatchUid;
			ContractUid = contractUid;
			Vehicles = vehicles;
			StartTime = startTime;
			TravelToTimes = travelToTimes;
			TravelFromTimes = travelFromTimes;
		}
	}

	public class CDispatchTransportFleetResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CDispatchDto Dispatch { get; set; }
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }

		public CDispatchTransportFleetResponse() : base(EHit.DispatchTransportFleetResponse)
		{
		}

		public CDispatchTransportFleetResponse(CModifiedUserDataDto modifiedData, CDispatchDto dispatch)
			: base(EHit.DispatchTransportFleetResponse)
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
