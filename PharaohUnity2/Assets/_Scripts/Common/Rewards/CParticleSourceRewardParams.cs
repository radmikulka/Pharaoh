using UnityEngine;

namespace Pharaoh
{
    public class CParticleSourceRewardParams : IRewardParams
    {
        public readonly Vector3 WorldPosition;

        public CParticleSourceRewardParams(Vector3 worldPosition)
        {
            WorldPosition = worldPosition;
        }
    }
}
