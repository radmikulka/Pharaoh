// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.12.2025
// =========================================

using AldaEngine;
using ServerData;

namespace Pharaoh
{
	public class CMissionUnloadedStartedSignal : IEventBusSignal
	{
		public readonly EMissionId Mission;

		public CMissionUnloadedStartedSignal(EMissionId mission)
		{
			Mission = mission;
		}
	}
}