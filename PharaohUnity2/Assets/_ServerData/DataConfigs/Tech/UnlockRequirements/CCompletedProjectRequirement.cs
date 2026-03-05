// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.02.2026
// =========================================

namespace ServerData
{
	public class CCompletedProjectRequirement : IUnlockRequirement
	{
		public readonly EProjectId ProjectId;

		public CCompletedProjectRequirement(EProjectId projectId)
		{
			ProjectId = projectId;
		}
	}
}