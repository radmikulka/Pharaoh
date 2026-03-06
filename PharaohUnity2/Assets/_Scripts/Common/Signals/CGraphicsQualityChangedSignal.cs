// =========================================
// AUTHOR: Radek Mikulka
// DATE:   05.11.2025
// =========================================

using AldaEngine;

namespace Pharaoh
{
	public class CGraphicsQualityChangedSignal : IEventBusSignal
	{
		public readonly EGraphicsQuality Quality;

		public CGraphicsQualityChangedSignal(EGraphicsQuality quality)
		{
			Quality = quality;
		}
	}
}