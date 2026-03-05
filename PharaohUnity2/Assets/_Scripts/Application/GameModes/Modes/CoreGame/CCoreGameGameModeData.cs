// =========================================
// AUTHOR: Radek Mikulka
// DATE:   30.05.2025
// =========================================

using ServerData;
using TycoonBuilder;

namespace TycoonBuilder
{
	public class CCoreGameGameModeData : IGameModeData
	{
		public EGameModeId GameModeId => EGameModeId.CoreGame;

		public readonly ERegion Region;

		public CCoreGameGameModeData(ERegion region)
		{
			Region = region;
		}
	}
}