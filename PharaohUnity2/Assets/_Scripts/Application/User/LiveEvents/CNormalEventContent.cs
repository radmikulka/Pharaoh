// =========================================
// AUTHOR: Radek Mikulka
// DATE:   29.10.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using ServerData;
using ServerData.Dto;
using ServerData.Hits;
using TycoonBuilder.Offers;

namespace TycoonBuilder
{
	public class CNormalEventContent : CStandardEventContent
	{
		public CNormalEventContent(
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

			CNormalEventContentDto normalDto = (CNormalEventContentDto)dto;
			Region = normalDto.Region;
		}
	}
}