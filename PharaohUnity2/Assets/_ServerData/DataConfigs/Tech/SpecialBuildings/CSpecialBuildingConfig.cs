// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.09.2025
// =========================================

namespace ServerData
{
	public class CSpecialBuildingConfig
	{
		public readonly IUnlockRequirement UnlockRequirement;
		public readonly ESpecialBuilding Id;
		public readonly int MaxPassengersBonus;
		public readonly ELiveEvent LiveEvent;
		public readonly EBundleId BundleId;
		public readonly IValuable Price;
		public readonly bool IsPremium;
		
		public bool IsEventBuilding => LiveEvent != ELiveEvent.None;

		public CSpecialBuildingConfig(
			ESpecialBuilding id, 
			int maxPassengersBonus, 
			IUnlockRequirement unlockRequirement, 
			IValuable price,
			ELiveEvent liveEvent,
			EBundleId bundleId,
			bool isPremium
			)
		{
			Id = id;
			MaxPassengersBonus = maxPassengersBonus;
			UnlockRequirement = unlockRequirement;
			LiveEvent = liveEvent;
			IsPremium = isPremium;
			BundleId = bundleId;
			Price = price;
		}

		public EYearMilestone GetUnlockYear()
		{
			if (UnlockRequirement is CYearUnlockRequirement year)
				return year.Year;
			return EYearMilestone._1930;
		}
	}
}