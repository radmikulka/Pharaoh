// =========================================
// AUTHOR: Radek Mikulka
// DATE:   30.01.2026
// =========================================

using AldaEngine;
using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData
{
	public class CWeeklyTaskDto : IMapAble
	{
		[JsonProperty] public CValuableDto[] Rewards { get; set; }
		[JsonProperty] public int RequiredPoints { get; set; }
		[JsonProperty] public bool IsClaimed { get; set; }
		[JsonProperty] public string Uid { get; set; }

		public CWeeklyTaskDto()
		{
		}

		public CWeeklyTaskDto(CValuableDto[] rewards, int requiredPoints, bool isClaimed, string uid)
		{
			Rewards = rewards;
			RequiredPoints = requiredPoints;
			IsClaimed = isClaimed;
			Uid = uid;
		}
	}
}