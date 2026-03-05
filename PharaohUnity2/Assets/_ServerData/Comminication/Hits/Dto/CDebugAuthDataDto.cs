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
		[JsonProperty] public ETutorialSkip TutorialSkip { get; set; }
		[JsonProperty] public EYearMilestone OverrideYear { get; set; }
		[JsonProperty] public EStaticContractId OverrideContract { get; set; }
		[JsonProperty] public bool LikeABossVehicles { get; set; }

		public CDebugAuthDataDto()
		{
		}

		public CDebugAuthDataDto(
			string overrideUid,
			string presetName,
			bool likeABoss,
			EYearMilestone overrideYear,
			ETutorialSkip tutorialSkip,
			EStaticContractId overrideContract,
			bool likeABossVehicles
			)
		{
			OverrideYear = overrideYear;
			TutorialSkip = tutorialSkip;
			OverrideContract = overrideContract;
			OverrideUid = overrideUid;
			PresetName = presetName;
			LikeABoss = likeABoss;
			LikeABossVehicles = likeABossVehicles;
		}
	}
}

