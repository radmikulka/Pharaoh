// =========================================
// AUTHOR: Juraj Joscak
// DATE:   08.08.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder.Ui
{
	public class CWarehouseResourceSelectedSignal : IEventBusSignal
	{
		public readonly EResource Resource;
		
		public CWarehouseResourceSelectedSignal(EResource resource)
		{
			Resource = resource;
		}
	}
}