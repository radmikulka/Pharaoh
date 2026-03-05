// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.12.2024
// =========================================

using System;
using ServerData;

namespace TycoonBuilder
{
	public class COwnedValuableFactory
	{
		public COwnedValuable Create(EValuable id)
		{
			switch (id)
			{
				case EValuable.SoftCurrency:
				case EValuable.HardCurrency:
				case EValuable.Fuel:
				case EValuable.CityBlueprint:
				case EValuable.CityPlan:
				case EValuable.FuelPart:
				case EValuable.DurabilityPart:
				case EValuable.CapacityPart:
				case EValuable.AdvancedCapacityPart:
				case EValuable.MachineOil:
				case EValuable.Wrenche:
				case EValuable.EventCoin:
					return new CConsumableOwnedValuable(id, 0);
				default:
					throw new NotImplementedException(id.ToString());
			}
		}
	}
}