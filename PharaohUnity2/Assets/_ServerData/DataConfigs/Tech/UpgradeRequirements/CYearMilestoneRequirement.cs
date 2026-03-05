// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

namespace ServerData
{
	public class CYearMilestoneRequirement : IUpgradeRequirement
	{
		public readonly EYearMilestone Year;

		public CYearMilestoneRequirement(EYearMilestone year)
		{
			Year = year;
		}
	}
}