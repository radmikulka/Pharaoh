using System;
using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;
using Pharaoh.Map;
using ServerData;
using Zenject;

namespace Pharaoh.Building
{
	public class CRoadNetworkService : IInitializable, IDisposable
	{
		private readonly CBuildingManager _buildingManager;
		private readonly CDesignBuildingsConfigs _buildingConfigs;
		private readonly CMapInstance _mapInstance;
		private readonly IEventBus _eventBus;

		private readonly HashSet<CMapCell> _connectedRoadCells = new();

		private Guid _placedSub;
		private Guid _removedSub;

		// Cardinal directions matching CMapInstance.CardinalOffsets: N S E W
		// Bitmask: bit3=N, bit2=S, bit1=E, bit0=W
		private static readonly (int dx, int dy, int bit)[] CardinalBits =
		{
			( 0,  1, 8),  // N → bit3
			( 0, -1, 4),  // S → bit2
			( 1,  0, 2),  // E → bit1
			(-1,  0, 1),  // W → bit0
		};

		public CRoadNetworkService(
			CBuildingManager buildingManager,
			CDesignBuildingsConfigs buildingConfigs,
			CMapInstance mapInstance,
			IEventBus eventBus)
		{
			_buildingManager = buildingManager;
			_buildingConfigs = buildingConfigs;
			_mapInstance     = mapInstance;
			_eventBus        = eventBus;
		}

		public void Initialize()
		{
			_placedSub  = _eventBus.Subscribe<CBuildingPlacedSignal>(OnBuildingPlaced);
			_removedSub = _eventBus.Subscribe<CBuildingRemovedSignal>(OnBuildingRemoved);
		}

		public void Dispose()
		{
			_eventBus.Unsubscribe(_placedSub);
			_eventBus.Unsubscribe(_removedSub);
		}

		private void OnBuildingPlaced(CBuildingPlacedSignal signal)
		{
			if (signal.BuildingId == EBuildingId.Road)
				OnRoadChanged(signal.Cell);
			else
				RefreshSingleBuilding(signal.Cell);
		}

		private void OnBuildingRemoved(CBuildingRemovedSignal signal)
		{
			if (signal.BuildingId == EBuildingId.Road)
				OnRoadChanged(signal.Cell);
			else
				RefreshSingleBuilding(signal.Cell);
		}

		private void OnRoadChanged(SCellCoord changedCell)
		{
			RecomputeConnectedNetwork();
			RefreshRoadVisuals(changedCell);
			RefreshAllBuildingConnectivity();
		}

		private void RecomputeConnectedNetwork()
		{
			_connectedRoadCells.Clear();

			// Find Townhall
			CMapCell hubCell = null;
			foreach (CBuilding building in _buildingManager.Buildings)
			{
				if (building.Id == EBuildingId.Townhall)
				{
					hubCell = building.Cell;
					break;
				}
			}

			if (hubCell == null)
				return;

			// BFS: seed from road cells adjacent to hub
			var queue = new Queue<CMapCell>();
			foreach ((int dx, int dy, int _) in CardinalBits)
			{
				int nx = hubCell.X + dx;
				int ny = hubCell.Y + dy;
				if (!_mapInstance.IsValid(nx, ny))
					continue;

				CMapCell neighbor = _mapInstance.GetCell(nx, ny);
				if (neighbor.BuildingId == EBuildingId.Road && _connectedRoadCells.Add(neighbor))
					queue.Enqueue(neighbor);
			}

			// BFS propagation
			while (queue.Count > 0)
			{
				CMapCell current = queue.Dequeue();
				foreach ((int dx, int dy, int _) in CardinalBits)
				{
					int nx = current.X + dx;
					int ny = current.Y + dy;
					if (!_mapInstance.IsValid(nx, ny))
						continue;

					CMapCell neighbor = _mapInstance.GetCell(nx, ny);
					if (neighbor.BuildingId == EBuildingId.Road && _connectedRoadCells.Add(neighbor))
						queue.Enqueue(neighbor);
				}
			}
		}

		private void RefreshRoadVisuals(SCellCoord changedCell)
		{
			RefreshRoadVisualAtCoord(changedCell.X, changedCell.Y);
			foreach ((int dx, int dy, int _) in CardinalBits)
				RefreshRoadVisualAtCoord(changedCell.X + dx, changedCell.Y + dy);
		}

		private void RefreshRoadVisualAtCoord(int x, int y)
		{
			if (!_mapInstance.IsValid(x, y))
				return;

			CMapCell cell = _mapInstance.GetCell(x, y);
			if (cell.BuildingId != EBuildingId.Road)
				return;

			if (!_buildingManager.TryGetView(cell, out CBuildingView view))
				return;

			CRoadView roadView = view.GetComponent<CRoadView>();
			if (roadView == null)
				return;

			int mask = 0;
			foreach ((int dx, int dy, int bit) in CardinalBits)
			{
				int nx = x + dx;
				int ny = y + dy;
				if (!_mapInstance.IsValid(nx, ny))
					continue;

				if (_mapInstance.GetCell(nx, ny).BuildingId == EBuildingId.Road)
					mask |= bit;
			}

			roadView.RefreshVisual(mask);
		}

		private void RefreshAllBuildingConnectivity()
		{
			foreach (CBuilding building in _buildingManager.Buildings)
			{
				CBuildingConfig config = _buildingConfigs.GetBuilding(building.Id);
				if (config == null || !config.RequiresRoadAccess)
					continue;

				building.IsActive = HasConnectedRoadNeighbor(building.Cell);
			}
		}

		private void RefreshSingleBuilding(SCellCoord coord)
		{
			CBuilding building = _buildingManager.GetBuildingAtCell(coord);
			if (building == null)
				return;

			CBuildingConfig config = _buildingConfigs.GetBuilding(building.Id);
			if (config == null || !config.RequiresRoadAccess)
				return;

			building.IsActive = HasConnectedRoadNeighbor(building.Cell);
		}

		private bool HasConnectedRoadNeighbor(CMapCell cell)
		{
			foreach ((int dx, int dy, int _) in CardinalBits)
			{
				int nx = cell.X + dx;
				int ny = cell.Y + dy;
				if (!_mapInstance.IsValid(nx, ny))
					continue;

				if (_connectedRoadCells.Contains(_mapInstance.GetCell(nx, ny)))
					return true;
			}
			return false;
		}
	}
}
