using AldaEngine;

namespace Pharaoh
{
    public class CMonumentProgressChangedSignal : IEventBusSignal
    {
        public readonly float Progress;

        public CMonumentProgressChangedSignal(float progress)
        {
            Progress = progress;
        }
    }
}
