// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.10.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class COpenInspectVehicleMenuTask
	{
		public readonly EVehicle Vehicle;
		public readonly bool ShowStats;
		
		public COpenInspectVehicleMenuTask(EVehicle vehicle, bool showStats)
		{
			Vehicle = vehicle;
			ShowStats = showStats;
		}
	}
}