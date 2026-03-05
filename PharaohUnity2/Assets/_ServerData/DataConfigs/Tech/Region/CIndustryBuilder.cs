// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System.Collections.Generic;

namespace ServerData
{
	public class CIndustryBuilder
	{
		private EIndustry _id;
		private ERegion _region;
		private EResource _resource;
		private ELiveEvent _liveEvent;
		private IUnlockRequirement _unlockRequirement;

		public readonly CTripPriceBuilder TripPrice = new();
		
		public CIndustryBuilder SetId(EIndustry id)
		{
			_id = id;
			return this;
		}
		
		public CIndustryBuilder SetRegion(ERegion region)
		{
			_region = region;
			return this;
		}

		public CIndustryBuilder SetLiveEvent(ELiveEvent liveEvent)
		{
			_liveEvent = liveEvent;
			return this;
		}

		public CIndustryBuilder SetResource(EResource resource)
		{
			_resource = resource;
			return this;
		}
		
		public CIndustryBuilder SetUnlockYear(EYearMilestone unlockYear)
		{
			_unlockRequirement = IUnlockRequirement.Year(unlockYear);
			return this;
		}
		
		public CIndustryBuilder SetUnlockContract(EStaticContractId contract)
		{
			_unlockRequirement = IUnlockRequirement.Contract(contract);
			return this;
		}

		public CResourceIndustryConfig Build()
		{
			CTripPrice tripPrice = TripPrice.Build();
			CResourceIndustryConfig industry = new(
				_id, 
				_unlockRequirement ?? IUnlockRequirement.Null(), 
				_resource, 
				_region,
				tripPrice,
				_liveEvent
				);
			return industry;
		}
	}
}