// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.02.2026
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CCountableTaskRequirement
	{
		public readonly ETaskRequirementType TaskRequirementType;
		public readonly ETaskRequirement Id;
		public readonly int TargetCount;
		public int CurrentCount { get; private set; }

		public CCountableTaskRequirement(ETaskRequirementType taskRequirementType, ETaskRequirement id, int targetCount, int currentCount)
		{
			TaskRequirementType = taskRequirementType;
			Id = id;
			TargetCount = targetCount;
			CurrentCount = currentCount;
		}

		public void IncreaseCount(int amount)
		{
			CurrentCount += amount;
		}

		public bool IsCompleted()
		{
			return CurrentCount >= TargetCount;
		}
	}
}