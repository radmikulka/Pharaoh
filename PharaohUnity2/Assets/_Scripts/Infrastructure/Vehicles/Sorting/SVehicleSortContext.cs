// =========================================
// AUTHOR: Marek Karaba
// DATE:   01.02.2026
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public struct SVehicleSortContext
	{
		public readonly EVehicle VehicleId;
		public readonly EVehicleState State;
		public readonly bool IsOwned;
		public readonly int MaxCapacity;
		public readonly long RemainingDispatchTime;
		public readonly int Price;
		public readonly EYearMilestone Year;

		public SVehicleSortContext(
			EVehicle vehicleId,
			EVehicleState state,
			bool isOwned,
			int maxCapacity,
			long remainingDispatchTime,
			int price,
			EYearMilestone year)
		{
			VehicleId = vehicleId;
			State = state;
			IsOwned = isOwned;
			MaxCapacity = maxCapacity;
			RemainingDispatchTime = remainingDispatchTime;
			Price = price;
			Year = year;
		}
	}
}