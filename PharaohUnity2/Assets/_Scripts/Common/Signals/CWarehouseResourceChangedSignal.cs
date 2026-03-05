// =========================================
// AUTHOR: Radek Mikulka
// DATE:   24.09.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CWarehouseResourceChangedSignal : IEventBusSignal
	{
		public readonly SResource NewValue;

		public CWarehouseResourceChangedSignal(SResource newValue)
		{
			NewValue = newValue;
		}
	}
}