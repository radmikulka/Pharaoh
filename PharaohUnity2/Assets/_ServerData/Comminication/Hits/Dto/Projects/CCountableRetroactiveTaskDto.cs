// =========================================
// AUTHOR: Radek Mikulka
// DATE:   24.02.2026
// =========================================

using Newtonsoft.Json;

namespace ServerData
{
	public class CCountableRetroactiveTaskDto : IRetroactiveTaskDto
	{
		[JsonProperty] public ETaskId TaskId { get; set; }
		[JsonProperty] public int TargetCount { get; set; }

		public CCountableRetroactiveTaskDto()
		{
		}

		public CCountableRetroactiveTaskDto(ETaskId taskId, int targetCount)
		{
			TaskId = taskId;
			TargetCount = targetCount;
		}
	}
}
