// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.12.2025
// =========================================

using AldaEngine;
using Newtonsoft.Json;
using ServerData.Dto;

namespace ServerData
{
	public class CBattlePassRewardTemplateDto : IMapAble
	{
		[JsonProperty] public CValuableDto Reward { get; private set; }
		[JsonProperty] public bool CanBeDoubled { get; private set; }
		[JsonProperty] public bool IsPropagated { get; private set; }

		public CBattlePassRewardTemplateDto()
		{
		}

		public CBattlePassRewardTemplateDto(CValuableDto reward, bool canBeDoubled, bool isPropagated)
		{
			IsPropagated = isPropagated;
			CanBeDoubled = canBeDoubled;
			Reward = reward;
		}
	}
}