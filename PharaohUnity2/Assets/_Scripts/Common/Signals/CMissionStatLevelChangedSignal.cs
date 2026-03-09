// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.03.2026
// =========================================

using AldaEngine;
using ServerData;

namespace Pharaoh
{
    public class CMissionStatLevelChangedSignal : IEventBusSignal
    {
        public readonly EMissionStatId Stat;
        public readonly int            NewLevel;

        public CMissionStatLevelChangedSignal(EMissionStatId stat, int newLevel)
        {
            Stat     = stat;
            NewLevel = newLevel;
        }
    }
}
