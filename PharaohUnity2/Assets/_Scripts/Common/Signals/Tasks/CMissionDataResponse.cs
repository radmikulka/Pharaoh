using System.Collections.Generic;

namespace Pharaoh
{
    public class CMissionDataResponse
    {
        public readonly int SoftCurrency;
        public readonly float MonumentProgress;
        public readonly HashSet<float> ClaimedMilestones;

        public CMissionDataResponse(int softCurrency, float monumentProgress, HashSet<float> claimedMilestones)
        {
            SoftCurrency      = softCurrency;
            MonumentProgress  = monumentProgress;
            ClaimedMilestones = claimedMilestones;
        }
    }
}
