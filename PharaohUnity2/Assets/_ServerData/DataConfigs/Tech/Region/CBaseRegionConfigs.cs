// =========================================
// AUTHOR: Radek Mikulka
// DATE:   26.06.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerData
{
	public class CBaseRegionConfigs
	{
		private readonly Dictionary<ERegion, CRegionConfig> _regionConfigs = new();
		private readonly List<CRegionConfig> _orderedRegions = new();
		
		public IReadOnlyList<CRegionConfig> OrderedRegions => _orderedRegions;
		
		public CRegionConfig GetRegionConfig(ERegion region)
		{
			return _regionConfigs[region];
		}
		
		public static ERegionPoint GetRegionExitPoint(EMovementType movementType)
		{
			return movementType switch
			{
				EMovementType.Road => ERegionPoint.TruckRegionExit,
				EMovementType.Air => ERegionPoint.PlaneRegionExit,
				EMovementType.Rail => ERegionPoint.TrainRegionExit,
				EMovementType.Water => ERegionPoint.ShipRegionExit,
				_ => throw new ArgumentOutOfRangeException(nameof(movementType), movementType, null)
			};
		}

		public ERegion GetRegionFromYear(EYearMilestone year)
		{
			int decadeIndex = (year - EYearMilestone._1930) / 10;
			return _orderedRegions[decadeIndex].Id;
		}

		public EYearMilestone GetYearFromRegion(ERegion region)
		{
			for (int i = 0; i < _orderedRegions.Count - 1; i++)
			{
				bool isCurrentRegion = _orderedRegions[i].Id == region;
				if(!isCurrentRegion)
					continue;
				int yearIndex = i * 10;
				return EYearMilestone._1930 + yearIndex;
			}

			return EYearMilestone.None;
		}

		public EYearMilestone GetMaxRegionYear(ERegion region)
		{
			EYearMilestone year = GetYearFromRegion(region);
			return year + 10;
		}

		public CRegionConfig GetNextRegion(ERegion region)
		{
			for (int i = 0; i < _orderedRegions.Count - 1; i++)
			{
				bool isCurrentRegion = _orderedRegions[i].Id == region;
				if(!isCurrentRegion)
					continue;
				ERegion nextRegion = _orderedRegions[i + 1].Id;
				return _regionConfigs[nextRegion];
			}
			
			throw new Exception($"Region {region} is the last region, no next region available.");
		}
		
		protected void AddConfig(CRegionConfig config)
		{
			_regionConfigs.Add(config.Id, config);
			_orderedRegions.Add(config);
		}
		
		protected void AddConfig(CRegionConfigBuilder builder)
		{
			CRegionConfig config = builder.Build();
			AddConfig(config);
		}
		
		protected IEnumerable<CRegionConfig> GetAllRegionConfigs()
		{
			foreach (CRegionConfig regionConfig in _regionConfigs.Values)
			{
				yield return regionConfig;
			}
		}
	}
}