// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData
{
	public class CLevelDataDto : IMapAble
	{
		[JsonProperty] public int Level { get; set; }
		[JsonProperty] public long? UpgradeStartTime { get; set; }

		public CLevelDataDto()
		{
		}

		public CLevelDataDto(int level, long? upgradeStartTime)
		{
			Level = level;
			UpgradeStartTime = upgradeStartTime;
		}
	}
}