// =========================================
// DATE:   02.03.2026
// =========================================

using System;
using System.Collections.Generic;
using AldaEngine;
using ServerData;
using UnityEngine;
using Zenject;

namespace Pharaoh.Ui
{
	public class CResearchScreen : CTycoonBuilderScreen
	{
		[SerializeField] private Transform _itemsContainer;
		[SerializeField] private CResearchItemView _itemPrefab;

		private IMissionController _missionController;
		private CDesignMissionsConfigs _missionConfigs;
		private COwnedResearches _ownedResearches;
		private COwnedResources _ownedResources;
		private IEventBus _eventBus;

		private readonly List<CResearchItemView> _spawnedItems = new();
		private Guid _researchPurchasedSub;

		[Inject]
		private void Inject(
			IMissionController missionController,
			CDesignMissionsConfigs missionConfigs,
			COwnedResearches ownedResearches,
			COwnedResources ownedResources,
			IEventBus eventBus)
		{
			_missionController = missionController;
			_missionConfigs = missionConfigs;
			_ownedResearches = ownedResearches;
			_ownedResources = ownedResources;
			_eventBus = eventBus;
		}

		public override void OnScreenOpenStart()
		{
			base.OnScreenOpenStart();
			Populate();
			_researchPurchasedSub = _eventBus.Subscribe<CResearchPurchasedSignal>(OnResearchPurchased);
		}

		public override int Id { get; }
		public override string GetScreenName()
		{
			throw new NotImplementedException();
		}

		public override void OnScreenCloseEnd()
		{
			base.OnScreenCloseEnd();
			ClearItems();
		}

		private void Populate()
		{
			ClearItems();

			EMissionId missionId = _missionController.ActiveMissionId;
			CResearchConfig[] researches = _missionConfigs.GetMission(missionId)?.AvailableResearches;

			if (researches == null)
				return;

			foreach (CResearchConfig config in researches)
			{
				CResearchItemView item = Instantiate(_itemPrefab, _itemsContainer);
				EResearchState state = GetResearchState(missionId, config);
				item.Setup(config, state, OnBuyClicked);
				_spawnedItems.Add(item);
			}
		}

		private EResearchState GetResearchState(EMissionId missionId, CResearchConfig config)
		{
			if (_ownedResearches.HasResearch(missionId, config.Id))
				return EResearchState.Purchased;

			foreach (EResearchId prereq in config.Prerequisites)
			{
				if (!_ownedResearches.HasResearch(missionId, prereq))
					return EResearchState.Locked;
			}

			return EResearchState.Available;
		}

		private void OnBuyClicked(EResearchId researchId)
		{
			EMissionId missionId = _missionController.ActiveMissionId;
			CResearchConfig config = GetConfig(missionId, researchId);
			if (config == null)
				return;

			EResearchState state = GetResearchState(missionId, config);
			if (state != EResearchState.Available)
				return;

			if (!_ownedResources.HasEnough(missionId, config.Cost))
				return;

			foreach (SResource cost in config.Cost)
				_ownedResources.Remove(missionId, cost.Id, cost.Amount);

			_ownedResearches.Purchase(missionId, researchId);
		}

		private CResearchConfig GetConfig(EMissionId missionId, EResearchId researchId)
		{
			CResearchConfig[] researches = _missionConfigs.GetMission(missionId)?.AvailableResearches;
			if (researches == null)
				return null;

			foreach (CResearchConfig config in researches)
			{
				if (config.Id == researchId)
					return config;
			}
			return null;
		}

		private void OnResearchPurchased(CResearchPurchasedSignal signal)
		{
			Populate();
		}

		private void ClearItems()
		{
			foreach (CResearchItemView item in _spawnedItems)
				Destroy(item.gameObject);
			_spawnedItems.Clear();
		}
	}
}
