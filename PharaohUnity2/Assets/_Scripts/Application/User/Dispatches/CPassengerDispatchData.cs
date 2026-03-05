// =========================================
// AUTHOR: Radek Mikulka
// DATE:   23.02.2026
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CPassengerDispatchData
	{
		public readonly ECity City;
		public readonly int ResourceAmount;

		public CPassengerDispatchData(ECity city, int resourceAmount)
		{
			City = city;
			ResourceAmount = resourceAmount;
		}
	}
}
