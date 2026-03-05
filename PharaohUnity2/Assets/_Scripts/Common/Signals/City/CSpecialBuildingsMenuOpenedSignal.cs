// =========================================
// AUTHOR: Radek Mikulka
// DATE:   15.10.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CSpecialBuildingsMenuOpenedSignal : IEventBusSignal
	{
		public readonly int Index;

		public CSpecialBuildingsMenuOpenedSignal(int index)
		{
			Index = index;
		}
	}
}