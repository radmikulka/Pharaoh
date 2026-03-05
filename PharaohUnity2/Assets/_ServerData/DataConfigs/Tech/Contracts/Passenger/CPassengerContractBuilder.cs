// =========================================
// AUTHOR: Radek Mikulka
// DATE:   11.09.2025
// =========================================

namespace ServerData
{
	public class CPassengerContractBuilder
	{
		private readonly CContractTaskBuilder _taskBuilder = new();
		private readonly CTripPriceBuilder _priceBuilder = new();
		private readonly ECity _city;

		public CPassengerContractBuilder(ECity city)
		{
			_city = city;
		}

		public CPassengerContractBuilder SetPassengerCount(int count)
		{
			_taskBuilder.SetRequirement(EResource.Passenger, count);
			return this;
		}
		
		public CPassengerContractBuilder SetFuelPrice(int price)
		{
			_priceBuilder.SetFuelPrice(price);
			return this;
		}
		
		public CPassengerContractBuilder SetDurationPrice(int price)
		{
			_priceBuilder.SetDurabilityPrice(price);
			return this;
		}
	
		public CPassengerContractBuilder AddReward(IValuable reward)
		{
			_taskBuilder.AddReward(reward);
			return this;
		}
		
		public CPassengerContractConfig Build()
		{
			return new CPassengerContractConfig(
				_city,
				_taskBuilder.Build(),
				_priceBuilder.Build()
				);
		}
	}
}