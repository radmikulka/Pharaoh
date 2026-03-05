// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.09.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CCraftInFactoryRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public EFactory Factory { get; set; }
		[JsonProperty] public EResource Resource { get; set; }
		[JsonProperty] public int SlotIndex { get; set; }
		
		public CCraftInFactoryRequest() : base(EHit.CraftInFactoryRequest)
		{
		}
	
		public CCraftInFactoryRequest(EFactory factory, EResource resource, int slotIndex) : base(EHit.CraftInFactoryRequest)
		{
			SlotIndex = slotIndex;
			Resource = resource;
			Factory = factory;
		}
	}
	
	public class CCraftInFactoryResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CCraftInFactoryResponse() : base(EHit.CraftInFactoryResponse)
		{
		}
		
		public CCraftInFactoryResponse(CModifiedUserDataDto modifiedData) : base(EHit.CraftInFactoryResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}