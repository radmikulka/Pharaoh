// =========================================
// AUTHOR: Radek Mikulka
// DATE:   27.06.2025
// =========================================

using System.Collections.Generic;

namespace ServerData
{
	public class CVehicleConfig
	{
		public readonly EVehicle Id;
		public readonly ERegion Region;
		public readonly IValuable Price;
		public readonly EOriginFlag Origin;
		public readonly ELiveEvent LiveEvent;
		public readonly EBundleId OverrideBundleId;
		public readonly ETransportType TransportType;
		public readonly EMovementType MovementType;
		public readonly IUnlockRequirement UnlockRequirement;
		public readonly EVehicleRarity Rarity;

		private readonly Dictionary<EVehicleStat, int[]> _stats = new();
		public IReadOnlyDictionary<EVehicleStat, int[]> Stats => _stats;

		public CVehicleConfig(
			EVehicle id, 
			ERegion region,
			ETransportType transportType, 
			EMovementType movementType, 
			Dictionary<EVehicleStat, List<int>> stats,
			IValuable price,
			IUnlockRequirement unlockRequirement,
			EBundleId overrideBundleId,
			ELiveEvent liveEvent,
			EOriginFlag origin,
			EVehicleRarity rarity
			)
		{
			Id = id;
			Price = price;
			Region = region;
			Origin = origin;
			LiveEvent = liveEvent;
			MovementType = movementType;
			TransportType = transportType;
			UnlockRequirement = unlockRequirement;
			OverrideBundleId = overrideBundleId;
			Rarity = rarity;

			foreach (var stat in stats)
			{
				_stats.Add(stat.Key, stat.Value.ToArray());
			}
		}

		public int GetStat(EVehicleStat stat, int statIndex)
		{
			return _stats[stat][statIndex];
		}

		public int GetMaxStatLevel(EVehicleStat stat)
		{
			int maxLevel = _stats[stat].Length;
			if (stat == EVehicleStat.AdvancedCapacity)
			{
				maxLevel--;
			}
			return maxLevel;
		}
	}
}