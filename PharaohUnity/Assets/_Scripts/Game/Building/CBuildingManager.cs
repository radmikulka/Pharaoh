using System;
using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;
using AldaEngine.UnityObjectPool;
using Pharaoh.Map;
using ServerData;
using UnityEngine;
using Zenject;

namespace Pharaoh.Building
{
	public class CBuildingManager : IInitializable
	{
		private readonly CBuildingPlacementValidator _validator;
		private readonly IMissionController _missionController;
		private readonly CResourceConfigs _resourceConfigs;
		private readonly IBundleManager _bundleManager;
		private readonly CMapInstance _mapInstance;
		private readonly IEventBus _eventBus;

		private readonly List<CBuilding> _buildings = new();
		private readonly Dictionary<CMapCell, CBuildingView> _views = new();

		public IReadOnlyList<CBuilding> Buildings => _buildings;

		public CBuildingManager(
			CBuildingPlacementValidator validator,
			IMissionController missionController,
			CResourceConfigs resourceConfigs,
			IBundleManager bundleManager,
			CMapInstance mapInstance,
			IEventBus eventBus)
		{
			_validator = validator;
			_missionController = missionController;
			_resourceConfigs = resourceConfigs;
			_bundleManager = bundleManager;
			_mapInstance = mapInstance;
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			CMapClickDetector.SetEventBus(_eventBus);

			_eventBus.Subscribe<CCellClickedSignal>(OnCellClicked);
			_eventBus.Subscribe<CBuildingPlacementRequestSignal>(OnBuildingPlacementRequested);
		}

		private void OnCellClicked(CCellClickedSignal signal)
		{
			CMapCell cell = _mapInstance.GetCell(signal.Cell.X, signal.Cell.Y);
			if (cell == null || cell.HasBuilding)
				return;

			_eventBus.Send(new COpenBuildingMenuSignal(signal.Cell));
		}

		private void OnBuildingPlacementRequested(CBuildingPlacementRequestSignal signal)
		{
			EBuildingId buildingId = signal.BuildingId;
			CMapCell cell = _mapInstance.GetCell(signal.Cell.X, signal.Cell.Y);

			if (cell == null || !_validator.CanPlace(buildingId, cell))
				return;

			CBuildingResourceConfig config = _resourceConfigs.Buildings.GetConfig(buildingId);
			GameObject prefab = _bundleManager.LoadItem<GameObject>(config.Prefab, EBundleCacheType.Persistent);

			var position = new Vector3(cell.X, 0f, cell.Y);
			GameObject buildingGO = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity, _mapInstance.transform);
			buildingGO.name = $"Building_{buildingId}_{cell.X}_{cell.Y}";

			var building = new CBuilding(buildingId, cell);

			CBuildingView view = buildingGO.AddComponent<CBuildingView>();
			view.Initialize(building);

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
	}
}
