using System;
using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;
using AldaEngine.UnityObjectPool;
using Pharaoh.Map;
using ServerData;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Pharaoh.Building
{
	public class CBuildingManager : IInitializable
	{
		private readonly CBuildingPlacementValidator _validator;
		private readonly IMissionController _missionController;
		private readonly CResourceConfigs _resourceConfigs;
		private readonly CDesignBuildingsConfigs _buildingConfigs;
		private readonly COwnedResources _ownedResources;
		private readonly IBundleManager _bundleManager;
		private readonly CMapInstance _mapInstance;
		private readonly IEventBus _eventBus;
		private readonly CUser _user;

		private readonly List<CBuilding> _buildings = new();
		private readonly Dictionary<CMapCell, CBuildingView> _views = new();

		public IReadOnlyList<CBuilding> Buildings => _buildings;

		public CBuildingManager(
			CBuildingPlacementValidator validator,
			IMissionController missionController,
			CResourceConfigs resourceConfigs,
			CDesignBuildingsConfigs buildingConfigs,
			COwnedResources ownedResources,
			IBundleManager bundleManager,
			CMapInstance mapInstance,
			IEventBus eventBus,
			CUser user)
		{
			_validator = validator;
			_missionController = missionController;
			_resourceConfigs = resourceConfigs;
			_buildingConfigs = buildingConfigs;
			_ownedResources = ownedResources;
			_bundleManager = bundleManager;
			_mapInstance = mapInstance;
			_eventBus = eventBus;
			_user = user;
		}

		public void Initialize()
		{
			CMapClickDetector.SetEventBus(_eventBus);

			_eventBus.Subscribe<CCellClickedSignal>(OnCellClicked);
			_eventBus.Subscribe<CBuildingPlacementRequestSignal>(OnBuildingPlacementRequested);
			_eventBus.Subscribe<CBuildingUpgradeRequestSignal>(OnBuildingUpgradeRequested);
			_eventBus.Subscribe<CBuildingRemovalRequestSignal>(OnBuildingRemovalRequested);
		}

		public int GetBuildingCount(EBuildingId id)
		{
			int count = 0;
			foreach (CBuilding building in _buildings)
			{
				if (building.Id == id)
					count++;
			}
			return count;
		}

		public bool TryGetView(CMapCell cell, out CBuildingView view) => _views.TryGetValue(cell, out view);

		public CBuilding GetBuildingAtCell(SCellCoord coord)
		{
			CMapCell cell = _mapInstance.GetCell(coord.X, coord.Y);
			if (cell == null)
				return null;

			foreach (CBuilding building in _buildings)
			{
				if (building.Cell == cell)
					return building;
			}
			return null;
		}

		private void OnCellClicked(CCellClickedSignal signal)
		{
			CMapCell cell = _mapInstance.GetCell(signal.Cell.X, signal.Cell.Y);
			if (cell == null)
				return;

			if (cell.HasBuilding)
				_eventBus.Send(new COpenBuildingDetailSignal(signal.Cell));
			else
				_eventBus.Send(new COpenBuildingMenuSignal(signal.Cell));
		}

		private void OnBuildingPlacementRequested(CBuildingPlacementRequestSignal signal)
		{
			EBuildingId buildingId = signal.BuildingId;
			CMapCell cell = _mapInstance.GetCell(signal.Cell.X, signal.Cell.Y);

			if (cell == null || !_validator.CanPlace(buildingId, cell))
				return;

			CBuildingConfig buildingConfig = _buildingConfigs.GetBuilding(buildingId);
			SResource[] cost = buildingConfig.GetBuildCost(GetBuildingCount(buildingId));
			EMissionId missionId = _missionController.ActiveMissionId;

			if (!_ownedResources.HasEnough(missionId, cost))
				return;

			foreach (SResource c in cost)
				_ownedResources.Remove(missionId, c.Id, c.Amount);

			CBuildingResourceConfig config = _resourceConfigs.Buildings.GetConfig(buildingId);
			GameObject prefab = _bundleManager.LoadItem<GameObject>(config.Prefab, EBundleCacheType.Persistent);

			var position = new Vector3(cell.X, 0f, cell.Y);
			GameObject buildingGO = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity, _mapInstance.transform);
			buildingGO.name = $"Building_{buildingId}_{cell.X}_{cell.Y}";

			var building = new CBuilding(buildingId, cell);

			CBuildingView view = buildingGO.AddComponent<CBuildingView>();
			view.Initialize(building);

			if (buildingId == EBuildingId.Road)
			{
				var roadView = buildingGO.AddComponent<CRoadView>();
				roadView.Initialize(
					_bundleManager.LoadItem<GameObject>(config.RoadVariantDeadEnd,   EBundleCacheType.Persistent),
					_bundleManager.LoadItem<GameObject>(config.RoadVariantCorner,    EBundleCacheType.Persistent),
					_bundleManager.LoadItem<GameObject>(config.RoadVariantStraight,  EBundleCacheType.Persistent),
					_bundleManager.LoadItem<GameObject>(config.RoadVariantTJunction, EBundleCacheType.Persistent),
					_bundleManager.LoadItem<GameObject>(config.RoadVariantCross,     EBundleCacheType.Persistent)
				);
			}

			cell.BuildingId = buildingId;

			if (cell.ObstacleObject != null)
			{
				UnityEngine.Object.Destroy(cell.ObstacleObject);
				cell.ObstacleObject = null;
			}

			_buildings.Add(building);
			_views[cell] = view;

			_eventBus.Send(new CBuildingPlacedSignal(buildingId, new SCellCoord(cell.X, cell.Y)));
		}

		private void OnBuildingUpgradeRequested(CBuildingUpgradeRequestSignal signal)
		{
			CBuilding building = GetBuildingAtCell(signal.Cell);
			if (building == null)
				return;

			CBuildingConfig config = _buildingConfigs.GetBuilding(building.Id);
			if (config == null || building.Level >= config.Levels.Length)
				return;

			SBuildingLevelData levelData = config.Levels[building.Level];

			if (levelData.LevelUpRequirement != null && !_user.IsUnlockRequirementMet(levelData.LevelUpRequirement))
				return;

			SResource[] upgradeCost = levelData.LevelCost;
			if (upgradeCost == null || upgradeCost.Length == 0)
				return;

			EMissionId missionId = _missionController.ActiveMissionId;
			if (!_ownedResources.HasEnough(missionId, upgradeCost))
				return;

			foreach (SResource c in upgradeCost)
				_ownedResources.Remove(missionId, c.Id, c.Amount);

			building.Level++;

			_eventBus.Send(new CBuildingUpgradedSignal(building.Id, signal.Cell, building.Level));
		}

		private void OnBuildingRemovalRequested(CBuildingRemovalRequestSignal signal)
		{
			CMapCell cell = _mapInstance.GetCell(signal.Cell.X, signal.Cell.Y);
			if (cell == null || !cell.HasBuilding)
				return;

			CBuilding building = GetBuildingAtCell(new SCellCoord(cell.X, cell.Y));
			if (building == null)
				return;

			if (_views.TryGetValue(cell, out CBuildingView view))
			{
				Object.Destroy(view.gameObject);
				_views.Remove(cell);
			}

			_buildings.Remove(building);
			cell.BuildingId = EBuildingId.None;

			_eventBus.Send(new CBuildingRemovedSignal(building.Id, new SCellCoord(cell.X, cell.Y)));
		}
	}
}
