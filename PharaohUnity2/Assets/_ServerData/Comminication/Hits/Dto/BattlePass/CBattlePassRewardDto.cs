// =========================================
// AUTHOR: Radek Mikulka
// DATE:   05.12.2025
// =========================================

using AldaEngine;
using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData
{
	public class CBattlePassRewardDto : IMapAble
	{
		[JsonProperty] public CValuableDto Reward { get; private set; }
		[JsonProperty] public bool IsClaimed { get; private set; }

		public CBattlePassRewardDto()
		{
		}

		public CBattlePassRewardDto(CValuableDto reward, bool isClaimed)
		{
			Reward = reward;
			IsClaimed = isClaimed;
		}
	}
}