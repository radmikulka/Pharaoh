using AldaEngine;

namespace Pharaoh
{
    public class CMonumentPartActivatedSignal : IEventBusSignal
    {
        public readonly int PartIndex;
        public readonly CMonumentPart Part;

        public CMonumentPartActivatedSignal(int partIndex, CMonumentPart part)
        {
            PartIndex = partIndex;
            Part = part;
        }
    }
}
