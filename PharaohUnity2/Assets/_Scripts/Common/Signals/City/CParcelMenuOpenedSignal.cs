// =========================================
// AUTHOR: Radek Mikulka
// DATE:   15.10.2025
// =========================================

using AldaEngine;

namespace TycoonBuilder
{
	public class CParcelMenuOpenedSignal : IEventBusSignal
	{
		public readonly int Index;

		public CParcelMenuOpenedSignal(int index)
		{
			Index = index;
		}
	}
}