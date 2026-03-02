using System.Collections.Generic;
using Pharaoh.Map;
using Pharaoh.MapGenerator;
using ServerData;

namespace Pharaoh.Building
{
	public class CBuildingPlacementValidator
	{
		private readonly CDesignBuildingsConfigs _buildingConfigs;
		private readonly CMapInstance _mapInstance;
		private readonly CUser _user;

		public CBuildingPlacementValidator(CDesignBuildingsConfigs buildingConfigs, CMapInstance mapInstance, CUser user)
		{
			_buildingConfigs = buildingConfigs;
			_mapInstance = mapInstance;
			_user = user;
		}

		public bool CanPlace(EBuildingId buildingId, CMapCell cell)
		{
			if (cell == null)
				return false;

			if (!cell.TileType.IsBuildable())
				return false;

			if (cell.HasBuilding)
				return false;

			CBuildingConfig config = _buildingConfigs.GetBuilding(buildingId);
			if (config.RequiredTags != ECellTag.None && (cell.Tags & config.RequiredTags) != config.RequiredTags)
				return false;

			if (!_user.IsUnlockRequirementMet(config.PlacementRequirement))
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
