// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.03.2026
// =========================================

using AldaEngine;
using ServerData;

namespace Pharaoh
{
    public class CMissionStatUpgradedSignal : IEventBusSignal
    {
        public readonly EMissionStatId Stat;

        public CMissionStatUpgradedSignal(EMissionStatId stat)
        {
            Stat = stat;
        }
    }
}
