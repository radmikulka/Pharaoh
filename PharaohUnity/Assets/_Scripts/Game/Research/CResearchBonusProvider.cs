// =========================================
// DATE:   02.03.2026
// =========================================

using System;
using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;

namespace Pharaoh
{
	public class CResearchBonusProvider : IInitializable
	{
		private readonly IMissionController _missionController;
		private readonly CDesignMissionsConfigs _missionConfigs;
		private readonly COwnedResearches _ownedResearches;
		private readonly IEventBus _eventBus;

		private readonly Dictionary<EBuildingId, float> _productionMultipliers = new();
		private readonly Dictionary<EBuildingId, float> _speedMultipliers = new();

		private Guid _researchPurchasedSub;

		public CResearchBonusProvider(
			IMissionController missionController,
			CDesignMissionsConfigs missionConfigs,
			COwnedResearches ownedResearches,
			IEventBus eventBus)
		{
			_missionController = missionController;
			_missionConfigs = missionConfigs;
			_ownedResearches = ownedResearches;
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			RebuildCache();
			_researchPurchasedSub = _eventBus.Subscribe<CResearchPurchasedSignal>(OnResearchPurchased);
		}

		public float GetProductionMultiplier(EBuildingId building)
		{
			return _productionMultipliers.TryGetValue(building, out float mult) ? mult : 1f;
		}

		public float GetSpeedMultiplier(EBuildingId building)
		{
			return _speedMultipliers.TryGetValue(building, out float mult) ? mult : 1f;
		}

		private void RebuildCache()
		{
			_productionMultipliers.Clear();
			_speedMultipliers.Clear();

			EMissionId missionId = _missionController.ActiveMissionId;
			CResearchConfig[] researches = _missionConfigs.GetMission(missionId)?.AvailableResearches;

			if (researches == null)
				return;

			foreach (CResearchConfig research in researches)
			{
				if (!_ownedResearches.HasResearch(missionId, research.Id))
					continue;

				foreach (IResearchEffect effect in research.Effects)
				{
					if (effect is CProductionBonusResearchEffect bonus)
					{
						if (!_productionMultipliers.ContainsKey(bonus.BuildingId))
							_productionMultipliers[bonus.BuildingId] = 1f;
						_productionMultipliers[bonus.BuildingId] *= bonus.ProductionMultiplier;

						if (!_speedMultipliers.ContainsKey(bonus.BuildingId))
							_speedMultipliers[bonus.BuildingId] = 1f;
						_speedMultipliers[bonus.BuildingId] *= bonus.SpeedMultiplier;
					}
				}
			}
		}

		private void OnResearchPurchased(CResearchPurchasedSignal signal)
		{
			if (signal.Mission == _missionController.ActiveMissionId)
				RebuildCache();
		}
	}
}
