// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.09.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CRepairFactoryRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public EFactory Factory { get; set; }
		
		public CRepairFactoryRequest() : base(EHit.RepairFactoryRequest)
		{
		}
	
		public CRepairFactoryRequest(EFactory factory) : base(EHit.RepairFactoryRequest)
		{
			Factory = factory;
		}
	}
	
	public class CRepairFactoryResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CRepairFactoryResponse() : base(EHit.RepairFactoryResponse)
		{
		}
		
		public CRepairFactoryResponse(CModifiedUserDataDto modifiedData) : base(EHit.RepairFactoryResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}