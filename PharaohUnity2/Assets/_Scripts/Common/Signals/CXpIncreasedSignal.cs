// =========================================
// AUTHOR: Marek Karaba
// DATE:   25.07.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CXpIncreasedSignal : IEventBusSignal
	{
		public readonly int CurrentXp;
		public readonly EYearMilestone Year;

		public CXpIncreasedSignal(int currentXp, EYearMilestone year)
		{
			CurrentXp = currentXp;
			Year = year;
		}
	}
}