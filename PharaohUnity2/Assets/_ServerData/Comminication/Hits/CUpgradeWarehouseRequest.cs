// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CUpgradeWarehouseRequest : CCommTokenBasedRequest
	{
		public CUpgradeWarehouseRequest() : base(EHit.UpgradeWarehouseRequest)
		{
		}
	}
	
	public class CUpgradeWarehouseResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CUpgradeWarehouseResponse() : base(EHit.UpgradeWarehouseResponse)
		{
		}
		
		public CUpgradeWarehouseResponse(CModifiedUserDataDto modifiedData) : base(EHit.UpgradeWarehouseResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}