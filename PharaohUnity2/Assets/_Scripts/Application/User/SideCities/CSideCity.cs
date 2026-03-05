// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.11.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CSideCity
	{
		public readonly ECity CityId;
		public int CompletedContractsCount { get; private set; }

		public CSideCity(ECity cityId, int completedContractsCount)
		{
			CityId = cityId;
			CompletedContractsCount = completedContractsCount;
		}

		public void IncreaseCompletedContractsCount()
		{
			++CompletedContractsCount;
		}
	}
}