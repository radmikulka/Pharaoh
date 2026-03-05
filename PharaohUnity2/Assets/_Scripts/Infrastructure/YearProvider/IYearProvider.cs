// =========================================
// AUTHOR: Marek Karaba
// DATE:   25.07.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public interface IYearProvider
	{
		int GetCurrentYear();
		int GetYear(EYearMilestone yearMilestone);
		EYearMilestone GetNextYear(EYearMilestone currentYear);
	}
}