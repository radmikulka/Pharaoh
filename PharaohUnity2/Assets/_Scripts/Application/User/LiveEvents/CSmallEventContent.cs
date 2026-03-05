// =========================================
// AUTHOR: Radek Mikulka
// DATE:   26.01.2026
// =========================================

using AldaEngine;
using ServerData;
using ServerData.Dto;

namespace TycoonBuilder
{
	public class CSmallEventContent : CStandardEventContent
	{
		public EMovementType ResourceIndustryMovementType { get; private set; }
		
		public CSmallEventContent(
			IRewardQueue rewardQueue, 
			IServerTime serverTime, 
			CHitBuilder hitBuilder, 
			ELiveEvent liveEventId, 
			IEventBus eventBus, 
			IMapper mapper, 
			CUser user
		) : base(rewardQueue, serverTime, hitBuilder, liveEventId, eventBus, mapper, user)
		{
		}

		public override void InitialSync(CStandardEventContentDto dto)
		{
			base.InitialSync(dto);

			CSmallEventContentDto smallDto = (CSmallEventContentDto)dto;
			ResourceIndustryMovementType = smallDto.ResourceIndustryMovementType;
		}
	}
}