// =========================================
// AUTHOR: Radek Mikulka
// DATE:   22.12.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CFactoryProductSelectedSignal : IEventBusSignal
	{
		public readonly EResource Resource;

		public CFactoryProductSelectedSignal(EResource resource)
		{
			Resource = resource; 
		}
	}
}