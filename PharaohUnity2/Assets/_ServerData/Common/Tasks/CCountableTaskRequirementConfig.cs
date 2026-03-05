// =========================================
// AUTHOR: Radek Mikulka
// DATE:   30.01.2026
// =========================================

namespace ServerData
{
	public class CCountableTaskRequirementConfig : CTaskRequirementConfig
	{
		public int TargetCount { get; private set; }

		public CCountableTaskRequirementConfig(ETaskRequirement id, ETaskRequirementType taskRequirementType, int targetCount) : base(id, taskRequirementType)
		{
			TargetCount = targetCount;
		}
	}
}