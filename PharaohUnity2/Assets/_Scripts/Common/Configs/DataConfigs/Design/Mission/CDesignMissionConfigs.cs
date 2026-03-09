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
                maxWorkerCount: 20,
                milestones: new[]
                {
                    new SMilestoneConfig(0.25f, CValuableFactory.HardCurrency(10)),
                    new SMilestoneConfig(0.50f, CValuableFactory.HardCurrency(25)),
                    new SMilestoneConfig(0.75f, CValuableFactory.HardCurrency(50)),
                    new SMilestoneConfig(1.00f, CValuableFactory.HardCurrency(100)),
                }
            ));
        }
    }
}
