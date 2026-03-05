// =========================================
// AUTHOR: Radek Mikulka
// DATE:   26.06.2025
// =========================================

using System.Collections.Generic;

namespace ServerData
{
	public class CResourceIndustryConfig
	{
		public readonly IUnlockRequirement UnlockRequirement;
		public readonly CTripPrice TripPrice;
		public readonly ELiveEvent LiveEvent;
		public readonly EResource Resource;
		public readonly ERegion Region;
		public readonly EIndustry Id;

		public CResourceIndustryConfig(
			EIndustry id,
			IUnlockRequirement unlockRequirement,
			EResource resource, 
			ERegion region,
			CTripPrice tripPrice,
			ELiveEvent liveEvent
			)
		{
			UnlockRequirement = unlockRequirement;
			TripPrice = tripPrice;
			LiveEvent = liveEvent;
			Resource = resource;
			Region = region;
			Id = id;
		}
	}
}