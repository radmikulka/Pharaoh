// =========================================
// AUTHOR: Marek Karaba
// DATE:   25.07.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public interface IDecadeProvider
	{
		EDecadeMilestone GetDecade(EYearMilestone yearMilestone);
		EYearMilestone GetNextDecadeYearMilestone(ERegion regionId);
		EYearMilestone GetCurrentDecadeYearMilestone(ERegion regionId);
		EDecadeMilestone GetNextDecade(EYearMilestone yearMilestone);
	}
}