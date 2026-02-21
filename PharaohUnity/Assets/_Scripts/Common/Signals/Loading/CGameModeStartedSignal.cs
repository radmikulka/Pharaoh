// =========================================
// AUTHOR: Radek Mikulka
// DATE:   30.05.2025
// =========================================

using AldaEngine;

namespace Pharaoh
{
	public class CGameModeStartedSignal : IEventBusSignal
	{
		public readonly IGameModeData Data;

		public CGameModeStartedSignal(IGameModeData data)
		{
			Data = data;
		}
	}
}