// =========================================
// AUTHOR: Radek Mikulka
// DATE:   01.10.2025
// =========================================

using System.Collections.Generic;
using ServerData;

namespace Pharaoh
{
	public static class CUnlockRequirementExtensions
	{
		public static bool IsUnlockRequirementMet(this CUser user, IUnlockRequirement requirement)
		{
			switch (requirement)
			{
				case CNullUnlockRequirement:
					return true;
				case CCompositeUnlockRequirement composite:
				{
					foreach (IUnlockRequirement unlockRequirement in composite.Requirements)
					{
						bool isMet = user.IsUnlockRequirementMet(unlockRequirement);
						if (!isMet)
						{
							return false;
						}
					}

					return true;
				}
			}

			return false;
		}
		
		public static bool IsUnlockRequirementMet(this CUser user, IReadOnlyList<IUnlockRequirement> requirements)
		{
			for (int i = 0; i < requirements.Count; i++)
			{
				if(!user.IsUnlockRequirementMet(requirements[i]))
					return false;
			}

			return true;
		}
	}
}