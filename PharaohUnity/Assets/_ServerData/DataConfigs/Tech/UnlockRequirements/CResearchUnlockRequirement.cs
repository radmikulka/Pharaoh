// =========================================
// DATE:   02.03.2026
// =========================================

namespace ServerData
{
	public class CResearchUnlockRequirement : IUnlockRequirement
	{
		public readonly EResearchId RequiredResearch;

		public CResearchUnlockRequirement(EResearchId research)
		{
			RequiredResearch = research;
		}
	}
}
