// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.10.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CYearSeenSignal : IEventBusSignal
	{
		public readonly EYearMilestone YearMilestone;

		public CYearSeenSignal(EYearMilestone yearMilestone)
		{
			YearMilestone = yearMilestone;
		}
	}
}