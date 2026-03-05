// =========================================
// AUTHOR: Radek Mikulka
// DATE:   30.09.2025
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CRegionLiveEventGameGameModeData : IGameModeData
	{
		public EGameModeId GameModeId => EGameModeId.RegionLiveEvent;

		public readonly ELiveEvent LiveEventId;

		public CRegionLiveEventGameGameModeData(ELiveEvent liveEventId)
		{
			LiveEventId = liveEventId;
		}
	}
}