// =========================================
// AUTHOR: Marek Karaba
// DATE:   01.02.2026
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public interface IVehicleSortingRule
	{
		int Compare(SVehicleSortContext a, SVehicleSortContext b);
	}
}