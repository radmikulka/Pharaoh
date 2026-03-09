// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.03.2026
// =========================================

using ServerData;

namespace Pharaoh
{
    public class CDesignMissionConfigs : CBaseMissionConfigs
    {
        public CDesignMissionConfigs()
        {
            AddMission(EMissionId.Mission1_1, () => new CMissionConfig(
                maxWorkerCount: 20
            ));
        }
    }
}
