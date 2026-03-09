using ServerData;

namespace Pharaoh
{
    public interface IRewardQueue
    {
        void EnqueueReward(IValuable reward, IRewardParams[] data);
    }
}