// =========================================
// AUTHOR: Radek Mikulka
// DATE:   30.05.2025
// =========================================

using ServerData;
using Pharaoh;

namespace Pharaoh
{
	public class CCoreGameModeData : IGameModeData
	{
		public EGameModeId GameModeId => EGameModeId.CoreGame;

		public readonly EMissionId Mission;

		public CCoreGameModeData(EMissionId mission)
		{
			Mission = mission;
		}
	}
}