using ServerData;
using UnityEngine;

namespace Pharaoh
{
    public class CRunWorldConsumableParticleTask
    {
        public readonly Vector3 StartWorldPosition;
        public readonly CConsumableValuable Currency;

        public CRunWorldConsumableParticleTask(Vector3 startWorldPosition, CConsumableValuable currency)
        {
            StartWorldPosition = startWorldPosition;
            Currency = currency;
        }
    }
}
