// =========================================
// AUTHOR: Radek Mikulka
// DATE:   27.06.2025
// =========================================

using System.Collections.Generic;
using ServerData.Design;

namespace ServerData
{
	public class CVehicleBuilder
	{
		private EVehicle _id;
		private ETransportType _transportType;
		private EMovementType _movementType;
		private EOriginFlag _origin;
		private EBundleId _bundleId;
		private ELiveEvent _liveEvent;
		private ERegion _region;
		private IValuable _price;
		private IUnlockRequirement _unlockRequirement = IUnlockRequirement.Null();
		private EVehicleRarity _rarity = EVehicleRarity.Standard;

		private readonly Dictionary<EVehicleStat, List<int>> _stats = new();
		private readonly CDesignRegionConfigs _regionConfigs;

		public CVehicleBuilder(CDesignRegionConfigs regionConfigs)
		{
			_regionConfigs = regionConfigs;
		}

		public CVehicleBuilder SetVehicle(EVehicle vehicle)
		{
			_id = vehicle;
			return this;
		}
		
		public CVehicleBuilder SetStandardOrigin()
		{
			_origin = EOriginFlag.Standard;
			return this;
		}
		
		public CVehicleBuilder SetRarity(EVehicleRarity rarity)
		{
			_rarity = rarity;
			return this;
		}
		
		public CVehicleBuilder SetTransportType(ETransportType transportType)
		{
			_transportType = transportType;
			return this;
		}
		
		public CVehicleBuilder SetLiveEvent(ELiveEvent liveEvent)
		{
			_liveEvent = liveEvent;
			_origin = EOriginFlag.LiveEvent;
			return this;
		}
		
		public CVehicleBuilder MarkAsPremium(EBundleId bundleId)
		{
			_bundleId = bundleId;
			_origin = EOriginFlag.Premium;
			return this;
		}
		
		public CVehicleBuilder SetPrice(IValuable price)
		{
			_price = price;
			return this;
		}
		
		public CVehicleBuilder SetMovementType(EMovementType movementType)
		{
			_movementType = movementType;
			return this;
		}
		
		public CVehicleBuilder SetUnlockYear(EYearMilestone year)
		{
			_unlockRequirement = IUnlockRequirement.Year(year);
			_region = _regionConfigs.GetRegionFromYear(year);
			return this;
		}
		
		public CVehicleBuilder AddStat(EVehicleStat stat, params int[] value)
		{
			if (!_stats.TryGetValue(stat, out List<int> levels))
			{
				levels = new List<int>();
				_stats[stat] = levels;
			}
			
			levels.AddRange(value);

			return this;
		}
		
		public CVehicleConfig Build()
		{
			return new CVehicleConfig(
				_id, 
				_region, 
				_transportType, 
				_movementType, 
				_stats,
				_price,
				_unlockRequirement,
				_bundleId,
				_liveEvent,
				_origin,
				_rarity
				);
		}
	}
}