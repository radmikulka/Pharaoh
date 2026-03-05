// =========================================
// AUTHOR: Radek Mikulka
// DATE:   15.10.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CPreviewSpecialBuildingSelectedSignal : IEventBusSignal
	{
		public readonly ESpecialBuilding BuildingId;
		public readonly ERegion Region;
		public readonly int Index;
		public readonly bool IsOwned;
		public readonly IValuable Price;
		public readonly string OfferGuid;

		public CPreviewSpecialBuildingSelectedSignal(ESpecialBuilding buildingId, ERegion region, int index, bool isOwned, IValuable price, string offerGuid)
		{
			BuildingId = buildingId;
			Region = region;
			Index = index;
			IsOwned = isOwned;
			Price = price;
			OfferGuid = offerGuid;
		}
	}
}