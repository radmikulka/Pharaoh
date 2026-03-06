// =========================================
// AUTHOR: Radek Mikulka
// DATE:   07.09.2023
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CDebugAuthDataDto
	{
		[JsonProperty] public string OverrideUid { get; set; }
		[JsonProperty] public string PresetName { get; set; }
		[JsonProperty] public bool LikeABoss { get; set; }

		public CDebugAuthDataDto()
		{
		}

		public CDebugAuthDataDto(
			string overrideUid,
			string presetName,
			bool likeABoss
			)
		{
			OverrideUid = overrideUid;
			PresetName = presetName;
			LikeABoss = likeABoss;
		}
	}
}

