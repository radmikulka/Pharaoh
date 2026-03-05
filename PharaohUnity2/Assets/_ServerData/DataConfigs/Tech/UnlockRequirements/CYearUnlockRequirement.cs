// =========================================
// AUTHOR: Radek Mikulka
// DATE:   30.05.2024
// =========================================

namespace ServerData
{
	public class CYearUnlockRequirement : IUnlockRequirement
	{
		public readonly EYearMilestone Year;

		public CYearUnlockRequirement(EYearMilestone year)
		{
			Year = year;
		}
	}

	public static class CYearUnlockRequirementExtensions
	{
		public static T GetRequirementOfType<T>(this IUnlockRequirement requirement) where T : IUnlockRequirement
		{
			if(requirement is T typedRequirement)
				return typedRequirement;

			if (requirement is not CCompositeUnlockRequirement composite)
				return default;

			foreach (IUnlockRequirement r in composite.Requirements)
			{
				T found = r.GetRequirementOfType<T>();
				if(found != null)
					return found;
			}
			
			return default;
		}
	}
}