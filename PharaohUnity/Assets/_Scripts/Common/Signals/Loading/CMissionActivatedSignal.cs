// =========================================
// AUTHOR: Radek Mikulka
// DATE:   30.10.2025
// =========================================

using AldaEngine;
using ServerData;

namespace Pharaoh
{
	public class CMissionActivatedSignal : IEventBusSignal
	{
		public readonly EMissionId Mission;

		public CMissionActivatedSignal(EMissionId mission)
		{
			Mission = mission;
		}
	}
}