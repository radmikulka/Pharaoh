// =========================================
// AUTHOR: Marek Karaba
// DATE:   22.07.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CYearIncreasedSignal : IEventBusSignal
	{
		public readonly EYearMilestone YearMilestone;

		public CYearIncreasedSignal(EYearMilestone yearMilestone)
		{
			YearMilestone = yearMilestone;
		}
	}
}