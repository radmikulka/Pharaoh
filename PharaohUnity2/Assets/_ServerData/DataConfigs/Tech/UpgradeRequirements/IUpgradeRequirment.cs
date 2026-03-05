// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

namespace ServerData
{
	public interface IUpgradeRequirement
	{
		public static CValuableRequirement Valuable(IValuable valuable)
		{
			return new CValuableRequirement(valuable);
		}

		public static CYearMilestoneRequirement Year(EYearMilestone year)
		{
			return new CYearMilestoneRequirement(year);
		}
	}
}