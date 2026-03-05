// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.10.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CVehicleBoughtSignal : IEventBusSignal
	{
		public readonly EVehicle Id;

		public CVehicleBoughtSignal(EVehicle id)
		{
			Id = id;
		}
	}
}