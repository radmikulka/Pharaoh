// =========================================
// AUTHOR: Radek Mikulka
// DATE:   19.08.2025
// =========================================

using System.Collections.Generic;

namespace ServerData
{
	public class CTripPriceBuilder
	{
		private int _fuelPrice;
		private int _durabilityPrice;

		public CTripPriceBuilder SetFuelPrice(int price)
		{
			_fuelPrice = price;
			return this;
		}
		
		public CTripPriceBuilder SetDurabilityPrice(int price)
		{
			_durabilityPrice = price;
			return this;
		}
		
		public CTripPrice Build()
		{
			return new CTripPrice(_fuelPrice, _durabilityPrice);
		}
	}
}