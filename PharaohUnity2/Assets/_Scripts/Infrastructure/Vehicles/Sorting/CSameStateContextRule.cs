// =========================================
// AUTHOR: Marek Karaba
// DATE:   01.02.2026
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CSameStateContextRule : IVehicleSortingRule
	{
		public int Compare(SVehicleSortContext x, SVehicleSortContext y)
		{
			if (x.State != y.State)
				return 0;

			switch (x.State)
			{
				case EVehicleState.ReadyToCollect:
				case EVehicleState.ReadyToSend:
				case EVehicleState.LowDurability:
					return y.MaxCapacity.CompareTo(x.MaxCapacity);

				case EVehicleState.DispatchedCurrent:
				case EVehicleState.DispatchedOther:
				case EVehicleState.DispatchedTransportFleet:
					int timeResult = x.RemainingDispatchTime.CompareTo(y.RemainingDispatchTime);
					return timeResult != 0 ? timeResult : y.MaxCapacity.CompareTo(x.MaxCapacity);

				case EVehicleState.NotOwned:
					return x.Price.CompareTo(y.Price); 

				case EVehicleState.Locked:
					int yearResult = x.Year.CompareTo(y.Year);
					return yearResult != 0 ? yearResult : x.Price.CompareTo(y.Price);

				default:
					return 0;
			}
		}
	}
}