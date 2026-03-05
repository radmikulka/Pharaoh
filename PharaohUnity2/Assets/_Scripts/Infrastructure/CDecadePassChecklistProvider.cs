// =========================================
// AUTHOR: Marek Karaba
// DATE:   13.02.2026
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using ServerData;
using ServerData.Design;

namespace TycoonBuilder
{
	public class CDecadePassChecklistProvider
	{
		private readonly CDesignRegionConfigs _regionConfigs;
		private readonly CUser _user;

		public CDecadePassChecklistProvider(CDesignRegionConfigs regionConfigs, CUser user)
		{
			_regionConfigs = regionConfigs;
			_user = user;
		}

		public int GetTotalTaskAmount()
		{
			return GetTasks().Count;
		}

		public IReadOnlyList<CRetroactiveTaskConfig> GetTasks()
		{
			CRegionConfig config = _regionConfigs.GetRegionConfig(_user.Progress.Region);
			return config.DecadeChecklistTasks;
		}
		
		public int GetCompletedTaskAmount()
		{
			CRegionConfig config = _regionConfigs.GetRegionConfig(_user.Progress.Region);
			IReadOnlyList<CRetroactiveTaskConfig> tasks = config.DecadeChecklistTasks;
			
			int completedTasks = 0;
			foreach (CRetroactiveTaskConfig task in tasks)
			{
				bool isCompleted = IsTaskCompleted(task);
				if (isCompleted)
				{
					completedTasks++;
				}
			}
			return completedTasks;
		}

		public bool IsCompleted()
		{
			int completedTasks = GetCompletedTaskAmount();
			int totalTasks = GetTotalTaskAmount();
			return completedTasks >= totalTasks;
		}

		private bool IsTaskCompleted(CRetroactiveTaskConfig task)
		{
			return task.Requirements.Select(requirement => IsRequirementCompleted(_user, requirement)).All(isCompleted => isCompleted);
		}
		
		private bool IsRequirementCompleted(CUser user, ITaskRequirement requirement)
		{
			if (requirement is CCountableTaskRequirementConfig countable)
			{
				return IsRequirementCompleted(countable);
			}
		
			switch (requirement.Id)
			{
				case ETaskRequirement.ClaimAllFreeDecadePassRewards:
					return user.DecadePass.AllFreeRewardsClaimed();
				
				default: throw new ArgumentOutOfRangeException(requirement.Id.ToString());
			}
		}
		
		private bool IsRequirementCompleted(CCountableTaskRequirementConfig requirementConfig)
		{
			return RequirementCompletedCount(requirementConfig) >= requirementConfig.TargetCount;
		}

		public int RequirementCompletedCount(ITaskRequirement requirementConfig)
		{
			switch (requirementConfig.Id)
			{
				case ETaskRequirement.HaveFactoryLevelsSum:
					return _user.Factories.FactoryLevelSum();
				case ETaskRequirement.HaveWarehouseLevel:
					return _user.Warehouse.LevelData.Level;
				case ETaskRequirement.HaveCityLevel:
					return _user.City.GetOrCreateCityData().LevelData.Level;
				case ETaskRequirement.OwnVehiclesCount:
					return _user.Vehicles.GetAllOwnedVehicles().Length;
				case ETaskRequirement.OwnedBuildingPlotsCount:
					return _user.City.GetOrCreateCityData().GetUnlockedBuildingPlotsCount();
				case ETaskRequirement.HaveFullyUpgradedVehiclesCount:
					return _user.Vehicles.GetFullyUpgradedVehiclesCount();
				case ETaskRequirement.ClaimAllFreeDecadePassRewards:
					return _user.DecadePass.AllFreeRewardsClaimed() ? 1 : 0;
				default: throw new ArgumentOutOfRangeException(requirementConfig.Id.ToString());
			}
		}
	}
}