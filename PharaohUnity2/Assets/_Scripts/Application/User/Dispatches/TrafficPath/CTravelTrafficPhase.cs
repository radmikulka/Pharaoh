// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.01.2026
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CTravelTrafficPhase : CTrafficPhase
	{
		public readonly STrafficLineId LineId;
		public readonly bool LeadingToTarget;

		public ERegion Region => LineId.Start.Region;

		public CTravelTrafficPhase(
			STrafficLineId trafficLine, 
			bool leadingToTarget, 
			long duration
			) : base(duration)
		{
			LeadingToTarget = leadingToTarget;
			LineId = trafficLine;
		}
	}
}