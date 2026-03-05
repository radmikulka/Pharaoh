// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.07.2025
// =========================================

using Newtonsoft.Json;

namespace ServerData
{
	public class CVehicleValuable : IValuable, IOfferAnalyticsValueProvider
	{
		public EValuable Id => EValuable.Vehicle;
		
		[JsonProperty] public EVehicle Vehicle { get; private set; }

		public CVehicleValuable()
		{
		}

		public CVehicleValuable(EVehicle vehicle)
		{
			Vehicle = vehicle;
		}

		public string GetAnalyticsValue()
		{
			return Vehicle.ToString();
		}

		public string GetOfferRewardAnalyticsValue()
		{
			return $"ve{Vehicle.ToString()}";
		}

		public override string ToString()
		{
			return $"{nameof(Id)}: {Id}, {nameof(Vehicle)}: {Vehicle}";
		}
	}
}