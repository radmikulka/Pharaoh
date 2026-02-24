using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;

namespace Pharaoh.Building
{
	public class CBuildingTickSystem : ITickable
	{
		private const float TickInterval = 1f;

		private readonly CBuildingManager _buildingManager;
		private readonly COwnedResources _ownedResources;
		private readonly IMissionController _missionController;
		private readonly CResourceConfigs _resourceConfigs;

		private float _timer;

		public CBuildingTickSystem(
			CBuildingManager buildingManager,
			COwnedResources ownedResources,
			IMissionController missionController,
			CResourceConfigs resourceConfigs)
		{
			_buildingManager = buildingManager;
			_ownedResources = ownedResources;
			_missionController = missionController;
			_resourceConfigs = resourceConfigs;
		}

		public void Tick()
		{
			_timer += Time.deltaTime;

			if (_timer < TickInterval)
				return;

			_timer -= TickInterval;
			ProcessTick();
		}

		private void ProcessTick()
		{
			var mission = _missionController.ActiveMissionId;
			var buildings = _buildingManager.Buildings;
			var context = new CBuildingTickContext(_ownedResources, mission);

			for (int i = 0; i < buildings.Count; i++)
			{
				CBuilding building = buildings[i];

				if (!building.IsActive)
					continue;

				CBuildingResourceConfig config = _resourceConfigs.Buildings.GetConfig(building.Id);

				if (config == null)
					continue;

				SBuildingLevelData levelData = config.GetLevelData(building.Level);
				context.ProcessBuilding(levelData);
			}

			context.Apply(_ownedResources, mission);
		}
	}
}
