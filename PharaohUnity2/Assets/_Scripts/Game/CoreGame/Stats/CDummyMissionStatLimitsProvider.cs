// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.03.2026
// =========================================

using ServerData;

namespace Pharaoh
{
    public sealed class CDummyMissionStatLimitsProvider : IMissionStatLimitsProvider
    {
        public int GetMaxLevel(EMissionStatId stat) => 10;
    }
}
