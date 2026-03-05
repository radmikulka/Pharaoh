// =========================================
// AUTHOR: Radek Mikulka
// DATE:   30.01.2026
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData
{
	public class CCountableTaskRequirementDto : IMapAble
	{
		[JsonProperty] public ETaskRequirement Id { get; set; }
		[JsonProperty] public ETaskRequirementType TaskRequirementType { get; set; }
		[JsonProperty] public int CurrentCount { get; set; }
		[JsonProperty] public int TargetCount { get; set; }

		public CCountableTaskRequirementDto()
		{
		}

		public CCountableTaskRequirementDto(ETaskRequirement id, ETaskRequirementType taskRequirementType, int currentCount, int targetCount)
		{
			Id = id;
			TargetCount = targetCount;
			TaskRequirementType = taskRequirementType;
			CurrentCount = currentCount;
		}
	}
}