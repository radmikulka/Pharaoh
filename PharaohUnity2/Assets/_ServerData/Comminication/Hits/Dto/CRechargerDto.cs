// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.09.2025
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData
{
	public class CRechargerDto : IMapAble
	{
		[JsonProperty] public long LastTickTime { get; set; }
		[JsonProperty] public int CurrentAmount { get; set; }

		public CRechargerDto()
		{
		}

		public CRechargerDto(long lastTickTime, int currentAmount)
		{
			LastTickTime = lastTickTime;
			CurrentAmount = currentAmount;
		}

		public override string ToString()
		{
			return $"{nameof(LastTickTime)}: {LastTickTime} ({CUnixTime.GetDate(LastTickTime)}), {nameof(CurrentAmount)}: {CurrentAmount}";
		}
	}
}