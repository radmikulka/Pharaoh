// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.09.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CCollectCraftedFactoryProductRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public EModificationSource OverrideSource { get; set; }
		[JsonProperty] public EFactory Factory { get; set; }
		[JsonProperty] public int SlotIndex { get; set; }
		
		public CCollectCraftedFactoryProductRequest() : base(EHit.CollectCraftedFactoryProductRequest)
		{
		}
	
		public CCollectCraftedFactoryProductRequest(EFactory factory, int slotIndex, EModificationSource overrideSource) : base(EHit.CollectCraftedFactoryProductRequest)
		{
			OverrideSource = overrideSource;
			SlotIndex = slotIndex;
			Factory = factory;
		}
	}
	
	public class CCollectCraftedFactoryProductResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CCollectCraftedFactoryProductResponse() : base(EHit.CollectCraftedFactoryProductResponse)
		{
		}
		
		public CCollectCraftedFactoryProductResponse(CModifiedUserDataDto modifiedData) : base(EHit.CollectCraftedFactoryProductResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}