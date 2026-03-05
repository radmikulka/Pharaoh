// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.09.2025
// =========================================

using System;

namespace ServerData
{
	[Flags]
	public enum ETutorialSkip
	{
		None = 0,
		IntroTutorial = 1 << 0,
		Warehouse = 1 << 1,
		DispatchCenter = 1 << 2,
		VehicleDepot = 1 << 3,
		OpenFirstFactory = 1 << 4,
		GetMoreMaterial = 1 << 5,
		PlayerCity = 1 << 6,
		ContractsMenu = 1 << 7,
		OpenCityPlot = 1 << 8,
		BrokenVehicle = 1 << 9,
		VehicleUpgrade = 1 << 10,
		All = ~0
	}
}