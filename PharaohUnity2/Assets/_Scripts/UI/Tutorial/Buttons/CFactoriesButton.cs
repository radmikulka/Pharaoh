// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.10.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using ServerData.Design;
using TycoonBuilder.Ui;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CFactoriesButton : CTutorialButton, IInitializable, IUpgradeMarkerValueSource
	{
		private IEventBus _eventBus;
		private	CUser _user;
		private CDesignFactoryConfigs _factoryConfigs;

		private int _finishedProductsCount;

		[Inject]
		private void Inject(IEventBus eventBus, CUser user, CDesignFactoryConfigs factoryConfigs)
		{
			_eventBus = eventBus;
			_user = user;
			_factoryConfigs = factoryConfigs;
		}

		private void Update()
		{
			SetProductMarker();
		}

		public void Initialize()
		{
			LoadInitialState();
			_eventBus.AddTaskHandler<CActivateFactoriesButtonTask>(ActivateFactoryButton);
		}

		private void ActivateFactoryButton(CActivateFactoriesButtonTask task)
		{
			ActivateAnimated();
		}

		private void LoadInitialState()
		{
			SetActive(_user.Tutorials.IsTutorialCompleted(EFactoryTutorialStep.Completed));
		}
		
		private void SetProductMarker()
		{
			int productsCount = _user.Factories.GetTotalFinishedProductsCount();
			if(_finishedProductsCount == productsCount)
				return;
			
			_finishedProductsCount = productsCount;
			_eventBus.Send(new CFactoryProductMarkerUpdatedSignal());
		}

		public EUpgradeMarkerState GetUpgradeMarkerState()
		{
			if (_user.Factories.IsAnyFactoryUpgradeCompleted())
				return EUpgradeMarkerState.Completed;
			
			if(_user.Factories.AreAllFactoryUpgradesRunning())
				return EUpgradeMarkerState.Running;

			EUpgradeMarkerState result = EUpgradeMarkerState.Locked;
			EFactory[] factoryIds = _factoryConfigs.GetAllFactoryIds();
			foreach (EFactory factory in factoryIds)
			{
				CFactoryConfig factoryConfig = _factoryConfigs.GetFactoryConfig(factory);
				if(factoryConfig.LiveEvent != ELiveEvent.None && _user.LiveEvents.GetActiveEventOrDefault(factoryConfig.LiveEvent) == null)
					continue;
				
				if(!_user.IsUnlockRequirementMet(factoryConfig.UnlockRequirement))
					continue;
				
				CFactory factoryData = _user.Factories.GetOrCreateFactory(factory);
				int maxLevel = _factoryConfigs.GetFactoryConfig(factory).GetMaxLevel();
				if (factoryData.LevelData.Level >= maxLevel)
					continue;
				
				IReadOnlyList<IUpgradeRequirement> requirements = _factoryConfigs.GetUpgradeRequirements(factory, factoryData.LevelData.Level);
				if (CLevelData.CanAffordRequirements(requirements, _user) && !_user.Factories.IsUpgradeRunning(factory))
					return EUpgradeMarkerState.Available;
				
				CYearMilestoneRequirement yearRequirement = requirements.FirstOrDefault(r => r is CYearMilestoneRequirement) as CYearMilestoneRequirement;
				if (yearRequirement == null || CLevelData.CanAffordRequirements(new[] { yearRequirement }, _user))
					result = EUpgradeMarkerState.Unlocked;
			}

			return result;
		}
	}
}