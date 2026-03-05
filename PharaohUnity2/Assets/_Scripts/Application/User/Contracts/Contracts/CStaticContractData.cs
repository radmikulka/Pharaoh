// =========================================
// AUTHOR: Radek Mikulka
// DATE:   19.11.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CStaticContractData
	{
		public readonly EStaticContractId ContractId;
		public readonly bool AllowGoTo;
		public readonly int TotalTasksCount;
		public readonly int Task;
		public readonly EDialogueId StoryDialogue;

		public bool IsActivated { get; private set; }
		public bool IsLastTask => Task == TotalTasksCount;

		public CStaticContractData(
			EStaticContractId contractId,
			bool isActivated,
			int task,
			int totalTasksCount,
			bool allowGoTo,
			EDialogueId storyDialogue
			)
		{
			TotalTasksCount = totalTasksCount;
			IsActivated = isActivated;
			ContractId = contractId;
			AllowGoTo = allowGoTo;
			StoryDialogue = storyDialogue;
			Task = task;
		}

		public void Activate()
		{
			IsActivated = true;
		}
	}
}
