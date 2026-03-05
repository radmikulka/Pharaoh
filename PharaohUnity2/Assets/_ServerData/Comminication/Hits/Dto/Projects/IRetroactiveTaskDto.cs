// =========================================
// AUTHOR: Radek Mikulka
// DATE:   24.02.2026
// =========================================

using AldaEngine;

namespace ServerData
{
	public interface IRetroactiveTaskDto : IMapAble
	{
		ETaskId TaskId { get; }
		int TargetCount { get; }
	}
}
