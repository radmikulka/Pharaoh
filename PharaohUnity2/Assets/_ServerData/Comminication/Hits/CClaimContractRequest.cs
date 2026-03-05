// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.08.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CClaimContractRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public string ContractUid { get; set; }

		public CClaimContractRequest() : base(EHit.ClaimContractRequest)
		{
		}

		public CClaimContractRequest(string contractUid) : base(EHit.ClaimContractRequest)
		{
			ContractUid = contractUid;
		}
	}

	public class CClaimContractResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CContractDto[] GeneratedContracts { get; set; }
		[JsonProperty] public CLiveEventDto[] LiveEventsDto { get; set; }
		[JsonProperty] public CContractDto[] NewEventContracts { get; set; }
		[JsonProperty] public CModifiedUserDataDto ModifiedUserData { get; set; }

		public CClaimContractResponse() : base(EHit.ClaimContractResponse)
		{
		}

		public CClaimContractResponse(
			CModifiedUserDataDto modifiedUserData,
			CContractDto[] generatedContracts,
			CContractDto[] newEventContracts,
			CLiveEventDto[] liveEventsDto
			)
			: base(EHit.ClaimContractResponse)
		{
			GeneratedContracts = generatedContracts;
			NewEventContracts = newEventContracts;
			ModifiedUserData = modifiedUserData;
			LiveEventsDto = liveEventsDto;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedUserData;
		}
	}
}
