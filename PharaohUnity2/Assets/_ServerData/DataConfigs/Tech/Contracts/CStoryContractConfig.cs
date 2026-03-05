// =========================================
// AUTHOR: Radek Mikulka
// DATE:   27.06.2025
// =========================================

using System.Collections.Generic;

namespace ServerData
{
	public class CStoryContractConfig : CContractConfig
	{
		public readonly IReadOnlyList<IUnlockRequirement> UnlockRequirements;
		public readonly EMovementType OverrideMovementType;
		public readonly EDialogueId DispatchDialogue;
		public readonly EDialogueId StoryDialogue;
		public readonly EStaticContractId Id;
		public readonly ECustomer Customer;
		public readonly ERegion Region;

		public CStoryContractConfig(
			ERegion region, 
			IReadOnlyList<IUnlockRequirement> unlockRequirements, 
			ECustomer customer, 
			CContractTask[] tasks, 
			EStaticContractId id, 
			CTripPrice tripPrice,
			EDialogueId storyDialogue,
			EDialogueId dispatchDialogue, 
			EMovementType overrideMovementType
			) 
			: base(tasks, tripPrice)
		{
			Id = id;
			OverrideMovementType = overrideMovementType;
			UnlockRequirements = unlockRequirements;
			DispatchDialogue = dispatchDialogue;
			StoryDialogue = storyDialogue;
			Customer = customer;
			Region = region;
		}

		public bool IsUnlockedInYear(EYearMilestone yearMilestone)
		{
			foreach (IUnlockRequirement requirement in UnlockRequirements)
			{
				if(requirement is not CYearUnlockRequirement year)
					continue;
				return year.Year <= yearMilestone;
			}

			return true;
		}

		public IEnumerable<EStaticContractId> GetBlockingContracts()
		{
			foreach (IUnlockRequirement unlockRequirement in UnlockRequirements)
			{
				if(unlockRequirement is CContractUnlockRequirement contractUnlockRequirement)
					yield return contractUnlockRequirement.ContractId;
			}
		}
	}
}