// =========================================
// AUTHOR: Radek Mikulka
// DATE:   03.09.2025
// =========================================

using System.Collections.Generic;

namespace ServerData
{
	public static class CUpgradeRequirementExtensions
	{
		public static IReadOnlyList<IValuable> ExtractValuables(this IEnumerable<IUpgradeRequirement> requirement)
		{
			List<IValuable>	result = new();
			foreach (IUpgradeRequirement req in requirement)
			{
				if(req is not CValuableRequirement valuable)
					continue;
				result.Add(valuable.Valuable);
			}

			return result;
		}
	}
}