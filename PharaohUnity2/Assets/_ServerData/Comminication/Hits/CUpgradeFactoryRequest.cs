// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.09.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CUpgradeFactoryRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public EFactory Factory { get; set; }
		
		public CUpgradeFactoryRequest() : base(EHit.UpgradeFactoryRequest)
		{
		}
	
		public CUpgradeFactoryRequest(EFactory factory) : base(EHit.UpgradeFactoryRequest)
		{
			Factory = factory;
		}
	}

	public class CUpgradeFactoryResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CUpgradeFactoryResponse() : base(EHit.UpgradeFactoryResponse)
		{
		}
		
		public CUpgradeFactoryResponse(CModifiedUserDataDto modifiedData) : base(EHit.UpgradeFactoryResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}