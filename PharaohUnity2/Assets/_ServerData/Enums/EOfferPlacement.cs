// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.10.2023
// =========================================

using System;

namespace ServerData
{
	[Flags]
	public enum EOfferPlacement
	{
		None = 0,
		Popup = 1 << 0,
		Shop = 1 << 1,
		FuelStation = 1 << 2,
		City = 1 << 3,
		SpecialBuildings = 1 << 4,
		All = ~0
	}
}

