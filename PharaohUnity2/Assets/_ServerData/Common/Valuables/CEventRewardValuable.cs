// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.02.2026
// =========================================

using Newtonsoft.Json;

namespace ServerData
{
	public class CEventRewardValuable : IValuable
	{
		public EValuable Id => EValuable.EventReward;

		public readonly EEventRewardPlacement Placement;

		public CEventRewardValuable(EEventRewardPlacement placement)
		{
			Placement = placement;
		}
	}
}