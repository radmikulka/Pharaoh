// =========================================
// AUTHOR: Radek Mikulka
// DATE:   24.02.2026
// =========================================

namespace ServerData
{
	public class CProjectActiveTaskConfig : CTaskConfig
	{
		public CProjectActiveTaskConfig(ETaskId id, ITaskRequirement[] requirements)
			: base(id, requirements) { }
	}
}
