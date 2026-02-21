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