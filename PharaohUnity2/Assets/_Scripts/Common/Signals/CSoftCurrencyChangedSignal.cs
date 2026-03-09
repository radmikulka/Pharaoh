using AldaEngine;
using AldaEngine.AldaFramework;

namespace Pharaoh
{
    public class CSoftCurrencyChangedSignal : IEventBusSignal
    {
        public readonly int NewValue;
        public readonly int Delta;

        public CSoftCurrencyChangedSignal(int newValue, int delta)
        {
            NewValue = newValue;
            Delta    = delta;
        }
    }
}
