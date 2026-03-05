// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.02.2026
// =========================================

using System;

namespace ServerData
{
	public class CRetroactiveTaskConfig : CTaskConfig
	{
		public CRetroactiveTaskConfig(ETaskId id, ITaskRequirement requirement) : base(id, new []{ requirement })
		{
		}
	}
}