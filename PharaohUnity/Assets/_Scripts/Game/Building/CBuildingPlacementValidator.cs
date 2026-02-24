using System.Collections.Generic;
using Pharaoh.Map;
using Pharaoh.MapGenerator;
using ServerData;

namespace Pharaoh.Building
{
	public class CBuildingPlacementValidator
	{
		private readonly CResourceConfigs _resourceConfigs;
		private readonly CMapInstance _mapInstance;

		public CBuildingPlacementValidator(CResourceConfigs resourceConfigs, CMapInstance mapInstance)
		{
			_resourceConfigs = resourceConfigs;
			_mapInstance = mapInstance;
		}

		public bool CanPlace(EBuildingId buildingId, CMapCell cell)
		{
			if (cell == null)
				return false;

			if (!cell.TileType.IsBuildable())
				return false;

			if (cell.HasBuilding)
				return false;

			CBuildingResourceConfig config = _resourceConfigs.Buildings.GetConfig(buildingId);
			if (config.RequiredTags != ECellTag.None && (cell.Tags & config.RequiredTags) != config.RequiredTags)
				return false;

			return true;
		}

		public List<EBuildingId> GetPlaceableBuildings(CMapCell cell, EBuildingId[] availableBuildings)
		{
			var result = new List<EBuildingId>();

			if (availableBuildings == null)
				return result;

			foreach (EBuildingId buildingId in availableBuildings)
			{
				if (CanPlace(buildingId, cell))
					result.Add(buildingId);
			}

			return result;
		}
	}
}
