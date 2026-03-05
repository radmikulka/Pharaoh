// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.09.2025
// =========================================

using AldaEngine;
using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData
{
	public class CContractDto : IMapAble
	{
		[JsonProperty] public EContractType Type { get; set; }

		// Common fields
		[JsonProperty] public SResource Requirement { get; set; }
		[JsonProperty] public int FleetTotalPower { get; set; }
		[JsonProperty] public CFleetSlotDto[] FleetSlots { get; set; }
		[JsonProperty] public int DeliveredAmount { get; set; }
		[JsonProperty] public bool IsSeen { get; set; }
		[JsonProperty] public CTripPriceDto TripPrice { get; set; }
		[JsonProperty] public string Uid { get; set; }
		[JsonProperty] public CValuableDto[] Rewards { get; set; }
		[JsonProperty] public ECustomer Customer { get; set; }
		[JsonProperty] public EMovementType MovementType { get; set; }
		[JsonProperty] public ERegion[] Regions { get; set; }

		// Static contract fields (null for Passenger)
		[JsonProperty] public CStaticContractMetaDataDto MetaData { get; set; }

		// Event fields (default for Story/Passenger)
		[JsonProperty] public ELiveEvent EventId { get; set; }
		[JsonProperty] public bool IsInfinity { get; set; }

		// Passenger fields (default for Story/Event)
		[JsonProperty] public ECity CityId { get; set; }

		public CContractDto()
		{
		}

		public CContractDto(
			EContractType type,
			CTripPriceDto tripPrice,
			CValuableDto[] rewards,
			SResource requirement,
			int fleetTotalPower,
			CFleetSlotDto[] fleetSlots,
			int deliveredAmount,
			ECustomer customer,
			bool isSeen,
			string uid,
			ERegion[] regions,
			EMovementType movementType,
			CStaticContractMetaDataDto metaData,
			ELiveEvent eventId,
			bool isInfinity,
			ECity cityId
		)
		{
			DeliveredAmount = deliveredAmount;
			FleetTotalPower = fleetTotalPower;
			FleetSlots = fleetSlots;
			MovementType = movementType;
			Requirement = requirement;
			IsInfinity = isInfinity;
			TripPrice = tripPrice;
			Customer = customer;
			MetaData = metaData;
			EventId = eventId;
			Rewards = rewards;
			Regions = regions;
			IsSeen = isSeen;
			CityId = cityId;
			Type = type;
			Uid = uid;
		}
	}
}
