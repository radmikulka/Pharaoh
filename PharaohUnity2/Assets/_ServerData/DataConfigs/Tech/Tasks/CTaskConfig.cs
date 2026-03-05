// =========================================
// AUTHOR: Radek Mikulka
// DATE:   30.01.2026
// =========================================

namespace ServerData
{
	public abstract class CTaskConfig
	{
		public readonly ITaskRequirement[] Requirements;
		public readonly ETaskId Id;

		protected CTaskConfig(ETaskId id, ITaskRequirement[] requirements)
		{
			Requirements = requirements;
			Id = id;
		}
	}
}