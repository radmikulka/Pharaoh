// =========================================
// AUTHOR: Marek Karaba
// DATE:   01.02.2026
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class COwnershipRule : IVehicleSortingRule
	{
		public int Compare(SVehicleSortContext x, SVehicleSortContext y)
		{
			bool xOwned = x.IsOwned;
			bool yOwned = y.IsOwned;

			return xOwned switch
			{
				true when !yOwned => -1,
				false when yOwned => 1,
				_ => 0
			};
		}
	}
}