// =========================================
// DATE:   02.03.2026
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using TMPro;
using UnityEngine;
using Zenject;

namespace Pharaoh.Ui
{
	public class CKnowledgePointsHudElement : MonoBehaviour, IInitializable
	{
		[SerializeField] private TextMeshProUGUI _amountLabel;
		[SerializeField] private TextMeshProUGUI _timerLabel;

		private IMissionController _missionController;
		private COwnedResources _ownedResources;
		private CResourceConfigs _resourceConfigs;
		private IEventBus _eventBus;
		private ICtsProvider _ctsProvider;

		private Guid _resourceChangedSub;

		[Inject]
		private void Inject(
			IMissionController missionController,
			COwnedResources ownedResources,
			CResourceConfigs resourceConfigs,
			IEventBus eventBus,
			ICtsProvider ctsProvider)
		{
			_missionController = missionController;
			_ownedResources = ownedResources;
			_resourceConfigs = resourceConfigs;
			_eventBus = eventBus;
			_ctsProvider = ctsProvider;
		}

		public void Initialize()
		{
			_resourceChangedSub = _eventBus.Subscribe<COwnedResourceChangedSignal>(OnResourceChanged);
			RefreshDisplay();
		}

		private void OnResourceChanged(COwnedResourceChangedSignal signal)
		{
			if (signal.Resource.Id == EResource.KnowledgePoints)
				RefreshDisplay();
		}

		private void RefreshDisplay()
		{
			EMissionId missionId = _missionController.ActiveMissionId;
			int current = _ownedResources.GetAmount(missionId, EResource.KnowledgePoints);
			int max = _resourceConfigs.Gameplay != null ? _resourceConfigs.Gameplay.MaxKnowledgePoints : 0;

			_amountLabel.text = $"{current}/{max}";

			if (current >= max)
				_timerLabel.text = string.Empty;
		}
	}
}
