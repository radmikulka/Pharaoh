// =========================================
// AUTHOR: Marek Karaba
// DATE:   22.07.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using ServerData.Design;
using Zenject;

namespace TycoonBuilder.MenuTriggers
{
	public class CMenuTriggersHandler : IInitializable
	{
		private CMenuTriggersConfig _menuTriggersConfig;
		private CLazyActionQueue _lazyActionQueue;
		private IEventBus _eventBus;

		[Inject]
		private void Inject(
			CMenuTriggersConfig menuTriggersConfig,
			CLazyActionQueue lazyActionQueue,
			IEventBus eventBus)
		{
			_menuTriggersConfig = menuTriggersConfig;
			_lazyActionQueue = lazyActionQueue;
			_eventBus = eventBus;
		}
		
		public void Initialize()
		{
			_eventBus.Subscribe<CYearIncreasedSignal>(OnYearIncreased);
			_eventBus.Subscribe<CStoryContractRewardsScreenShownSignal>(OnStoryContractRewardScreenShown);
		}

		private void OnYearIncreased(CYearIncreasedSignal signal)
		{
			IUnlockRequirement requirement = IUnlockRequirement.Year(signal.YearMilestone);
			AddTriggersToQueue(requirement);
		}

		private void OnStoryContractRewardScreenShown(CStoryContractRewardsScreenShownSignal signal)
		{
			if (!signal.LastStageCompleted)
				return;
			
			IUnlockRequirement requirement = IUnlockRequirement.Contract(signal.ContractId);
			AddTriggersToQueue(requirement);
		}

		private void AddTriggersToQueue(IUnlockRequirement requirement)
		{
			List<EMenuTrigger> triggers = _menuTriggersConfig.GetActiveTriggers(requirement);
			foreach (EMenuTrigger trigger in triggers)
			{
				CMenuTriggerLazyAction lazyAction = new (trigger, _eventBus);
				_lazyActionQueue.AddAction(lazyAction);
			}
		}
	}
}