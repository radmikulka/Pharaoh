using ServerData;

namespace Pharaoh
{
    public readonly struct SMilestoneConfig
    {
        public readonly float Threshold;
        public readonly IValuable Reward;

        public SMilestoneConfig(float threshold, IValuable reward)
        {
            Threshold = threshold;
            Reward    = reward;
        }
    }
}
