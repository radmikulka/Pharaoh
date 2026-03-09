using AldaEngine;

namespace Pharaoh
{
    public class CMilestoneRewardClaimedSignal : IEventBusSignal
    {
        public readonly float MilestoneThreshold;

        public CMilestoneRewardClaimedSignal(float milestoneThreshold)
        {
            MilestoneThreshold = milestoneThreshold;
        }
    }
}
