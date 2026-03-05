// =========================================
// AUTHOR: Radek Mikulka
// DATE:   19.08.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using AldaEngine;

namespace ServerData
{
	public class CTripPrice : IMapAble
	{
		public readonly int FuelPriceValue;
		public readonly int DurabilityPrice;

		public CConsumableValuable FuelPrice => CValuableFactory.Fuel(FuelPriceValue);
		
		public CTripPrice(int fuelPriceValue, int durabilityPrice)
		{
			FuelPriceValue = fuelPriceValue;
			DurabilityPrice = durabilityPrice;
		}
	}
}