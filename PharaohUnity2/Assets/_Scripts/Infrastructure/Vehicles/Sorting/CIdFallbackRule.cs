// =========================================
// AUTHOR: Marek Karaba
// DATE:   01.02.2026
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CIdFallbackRule : IVehicleSortingRule
	{
		public int Compare(SVehicleSortContext a, SVehicleSortContext b)
		{
			return a.VehicleId.CompareTo(b.VehicleId);
		}
	}
}