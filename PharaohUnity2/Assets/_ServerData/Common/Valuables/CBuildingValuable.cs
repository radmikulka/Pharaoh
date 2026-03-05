// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.12.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData
{
	public class CBuildingValuable : IValuable, IOfferAnalyticsValueProvider
	{
		public EValuable Id => EValuable.Building;
		
		[JsonProperty] public ESpecialBuilding Building { get; private set; }

		public CBuildingValuable()
		{
		}

		public CBuildingValuable(ESpecialBuilding building)
		{
			Building = building;
		}

		public string GetAnalyticsValue()
		{
			return Building.ToString();
		}

		public string GetOfferRewardAnalyticsValue()
		{
			return $"bu{Building.ToString()}";
		}

		public override string ToString()
		{
			return $"{nameof(Id)}: {Id}, {nameof(Building)}: {Building}";
		}
	}
}