// =========================================
// AUTHOR: Radek Mikulka
// DATE:   30.05.2024
// =========================================

using ServerData;

namespace ServerData
{
	public interface IUnlockRequirement
	{
		private static readonly CNullUnlockRequirement NullUnlockRequirement = new();

		public static CNullUnlockRequirement Null() => NullUnlockRequirement;

		public static CYearUnlockRequirement Year(EYearMilestone yearMilestone)
		{
			return new CYearUnlockRequirement(yearMilestone);
		}

		public static CContractUnlockRequirement Contract(EStaticContractId contract)
		{
			return new CContractUnlockRequirement(contract);
		}
		
		public static CContractFullyLoadedRequirement ContractFullyLoaded(EStaticContractId contract)
		{
			return new CContractFullyLoadedRequirement(contract);
		}

		public static CRewardUnlockRequirement Reward()
		{
			return new CRewardUnlockRequirement();
		}
		
		public static CCompositeUnlockRequirement Composite(params IUnlockRequirement[] requirements)
		{
			return new CCompositeUnlockRequirement(requirements);
		}
		
		public static CIntroTutorialUnlockRequirement IntroTutorial(EIntroTutorialStep step)
		{
			return new CIntroTutorialUnlockRequirement(step);
		}

		public static CCompletedProjectRequirement CompletedProject(EProjectId projectId)
		{
			return new CCompletedProjectRequirement(projectId);
		}
		
		public static COwnedValuableUnlockRequirement OwnedValuable(IValuable valuable)
		{
			return new COwnedValuableUnlockRequirement(valuable);
		}
	}
}