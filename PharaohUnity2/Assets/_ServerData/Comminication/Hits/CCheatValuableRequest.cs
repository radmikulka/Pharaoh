// =========================================
// AUTHOR: Radek Mikulka
// DATE:   07.10.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CCheatValuableRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public CValuableDto Valuable { get; set; }
		
		public CCheatValuableRequest() : base(EHit.CheatValuableRequest)
		{
		}
		
		public CCheatValuableRequest(CValuableDto valuable) : base(EHit.CheatValuableRequest)
		{
			Valuable = valuable;
		}
	}

	public class CCheatValuableResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CCheatValuableResponse() : base(EHit.CheatValuableResponse)
		{
		}
		
		public CCheatValuableResponse(CModifiedUserDataDto modifiedData) : base(EHit.CheatValuableResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}