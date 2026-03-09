// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.03.2026
// =========================================

using ServerData;

namespace Pharaoh
{
    public interface IMissionStatLimitsProvider
    {
        int GetMaxLevel(EMissionStatId stat);
    }
}
