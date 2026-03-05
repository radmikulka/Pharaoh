// =========================================
// AUTHOR: Marek Karaba
// DATE:   01.02.2026
// =========================================

using System.Collections.Generic;
using System.Linq;
using ServerData;

namespace TycoonBuilder
{
	public class CVehicleComparer : IComparer<SVehicleSortContext>
	{
		private readonly List<IVehicleSortingRule> _rules;

		public CVehicleComparer()
		{
			_rules = new List<IVehicleSortingRule>
			{
				new COwnershipRule(),
				new CStateHierarchyRule(),
				new CSameStateContextRule(),
				new CIdFallbackRule()
			};
		}

		public int Compare(SVehicleSortContext a, SVehicleSortContext b)
		{
			// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
			foreach (IVehicleSortingRule rule in _rules)
			{
				int result = rule.Compare(a, b);
				if (result != 0)
				{
					return result;
				}
			}
			return 0;
		}
	}
}