// =========================================
// AUTHOR: Radek Mikulka
// DATE:   11.07.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CDispatchToStoryContractRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public string DispatchUid { get; set; }
		[JsonProperty] public EVehicle Vehicle { get; set; }
		[JsonProperty] public string ContractUid { get; set; }
		[JsonProperty] public int MaterialAmount { get; set; }
		[JsonProperty] public long StartTime { get; set; }
		
		public CDispatchToStoryContractRequest() : base(EHit.DispatchToStoryContractRequest)
		{
		}
		
		public CDispatchToStoryContractRequest(
			string dispatchUid,
			EVehicle vehicle, 
			string contractUid, 
			long startTime,
			int materialAmount
			) : base(EHit.DispatchToStoryContractRequest)
		{
			MaterialAmount = materialAmount;
			DispatchUid = dispatchUid;
			ContractUid = contractUid;
			StartTime = startTime;
			Vehicle = vehicle;
		}
	}

	public class CDispatchToStoryContractResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CDispatchDto Dispatch { get; set; }
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }

		public CDispatchToStoryContractResponse() : base(EHit.DispatchToStoryContractResponse)
		{
		}

		public CDispatchToStoryContractResponse(CModifiedUserDataDto modifiedData, CDispatchDto dispatch) : base(EHit.DispatchToStoryContractResponse)
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