// =========================================
// AUTHOR: Radek Mikulka
// DATE:   24.02.2026
// =========================================

using Newtonsoft.Json;

namespace ServerData
{
	public class CUpgradeFactoryToLevelRetroactiveTaskDto : CCountableRetroactiveTaskDto
	{
		[JsonProperty] public EFactory Factory { get; set; }

		public CUpgradeFactoryToLevelRetroactiveTaskDto()
		{
		}

		public CUpgradeFactoryToLevelRetroactiveTaskDto(ETaskId taskId, int targetCount, EFactory factory)
			: base(taskId, targetCount)
		{
			Factory = factory;
		}
	}
}
