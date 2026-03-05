// =========================================
// AUTHOR: Radek Mikulka
// DATE:   20.10.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CStoryContractFullyLoadedSignal : IEventBusSignal
	{
		public readonly SStaticContractPointer Pointer;

		public CStoryContractFullyLoadedSignal(SStaticContractPointer pointer)
		{
			Pointer = pointer;
		}
	}
}