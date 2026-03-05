// =========================================
// AUTHOR: Radek Mikulka
// DATE:   30.01.2026
// =========================================

namespace ServerData
{
	public class CTaskRequirementConfig : ITaskRequirement
	{
		public ETaskRequirement Id { get; }
		public ETaskRequirementType TaskRequirementType { get; }

		public CTaskRequirementConfig(ETaskRequirement id, ETaskRequirementType taskRequirementType)
		{
			TaskRequirementType = taskRequirementType;
			Id = id;
		}
	}
}