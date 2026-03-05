// =========================================
// AUTHOR: Radek Mikulka
// DATE:   17.09.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CDispatchToPassengerContractRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public string DispatchUid { get; set; }
		[JsonProperty] public EVehicle Vehicle { get; set; }
		[JsonProperty] public string ContractUid { get; set; }
		[JsonProperty] public int MaterialAmount { get; set; }
		[JsonProperty] public long StartTime { get; set; }
		
		public CDispatchToPassengerContractRequest() : base(EHit.DispatchToPassengerContractRequest)
		{
		}
		
		public CDispatchToPassengerContractRequest(
			string dispatchUid,
			EVehicle vehicle, 
			string contractUid, 
			long startTime,
			int materialAmount
		) : base(EHit.DispatchToPassengerContractRequest)
		{
			MaterialAmount = materialAmount;
			DispatchUid = dispatchUid;
			ContractUid = contractUid;
			StartTime = startTime;
			Vehicle = vehicle;
		}
	}

	public class CDispatchToPassengerContractResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		[JsonProperty] public CDispatchDto Dispatch { get; set; }

		public CDispatchToPassengerContractResponse() : base(EHit.DispatchToPassengerContractResponse)
		{
		}

		public CDispatchToPassengerContractResponse(CModifiedUserDataDto modifiedData, CDispatchDto dispatch) : base(EHit.DispatchToPassengerContractResponse)
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