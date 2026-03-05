// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.09.2025
// =========================================

using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData.Hits
{
	public class CBuyFactorySlotRequest : CCommTokenBasedRequest
	{
		[JsonProperty] public EFactory Factory { get; set; }
		[JsonProperty] public int SlotIndex { get; set; }
		
		public CBuyFactorySlotRequest() : base(EHit.BuyFactorySlotRequest)
		{
		}
	
		public CBuyFactorySlotRequest(EFactory factory, int slotIndex) : base(EHit.BuyFactorySlotRequest)
		{
			SlotIndex = slotIndex;
			Factory = factory;
		}
	}
	
	public class CBuyFactorySlotResponse : CResponseHit, IIHaveModifiedData
	{
		[JsonProperty] public CModifiedUserDataDto ModifiedData { get; set; }
		
		public CBuyFactorySlotResponse() : base(EHit.BuyFactorySlotResponse)
		{
		}
		
		public CBuyFactorySlotResponse(CModifiedUserDataDto modifiedData) : base(EHit.BuyFactorySlotResponse)
		{
			ModifiedData = modifiedData;
		}

		public CModifiedUserDataDto GetModifiedData()
		{
			return ModifiedData;
		}
	}
}