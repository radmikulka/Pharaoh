// =========================================
// AUTHOR: Radek Mikulka
// DATE:   30.01.2026
// =========================================

using AldaEngine;
using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData
{
	public class CActiveTaskDto : IMapAble
	{
		[JsonProperty] public CCountableTaskRequirementDto[] Requirements { get; set; }
		[JsonProperty] public CValuableDto[] Rewards { get; set; }
		[JsonProperty] public ETaskId TaskId { get; set; }
		[JsonProperty] public string Uid { get; set; }
		[JsonProperty] public int FrontendOrder { get; set; }
		[JsonProperty] public bool IsClaimed { get; set; }

		public CActiveTaskDto()
		{
		}

		public CActiveTaskDto(string uid, ETaskId taskId, CCountableTaskRequirementDto[] requirements, CValuableDto[] rewards, bool isClaimed, int frontendOrder)
		{
			FrontendOrder = frontendOrder;
			Requirements = requirements;
			IsClaimed = isClaimed;
			Rewards = rewards;
			TaskId = taskId;
			Uid = uid;
		}
	}
}