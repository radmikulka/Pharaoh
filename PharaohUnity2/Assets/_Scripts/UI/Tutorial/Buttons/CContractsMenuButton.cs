// =========================================
// AUTHOR: Radek Mikulka
// DATE:   23.10.2025
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using TycoonBuilder.Ui;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CContractsMenuButton : CTutorialButton, IInitializable
	{
		[SerializeField] private RectTransform _markerHolder;
		
		private IEventBus _eventBus;
		private	CUser _user;
		private CUiMarkerProvider _uiMarkerProvider;

		[Inject]
		private void Inject(IEventBus eventBus, CUser user, CUiMarkerProvider uiMarkerProvider)
		{
			_eventBus = eventBus;
			_user = user;
			_uiMarkerProvider = uiMarkerProvider;
		}

		public void Initialize()
		{
			LoadInitialState();
			_eventBus.AddTaskHandler<CActivateContractsButtonTask>(ActivateButton);
			SetMarkers();

			_eventBus.Subscribe<CStoryContractStateChangedSignal>(RefreshMarkers);
			_eventBus.Subscribe<CStoryContractsMarkedAsSeenSignal>(RefreshMarkers);
			_eventBus.Subscribe<CStaticContractCompletedSignal>(RefreshMarkers);
			_eventBus.Subscribe<CDispatchCompletedSignal>(RefreshMarkers);
			_eventBus.Subscribe<CStoryContractRewardsClaimedSignal>(RefreshMarkers);
			_eventBus.Subscribe<CContractsSyncedSignal>(RefreshMarkers);
			_eventBus.Subscribe<CLiveEventsSyncedSignal>(RefreshMarkers);
			_eventBus.Subscribe<CYearSeenSignal>(RefreshMarkers);
		}

		private void ActivateButton(CActivateContractsButtonTask task)
		{
			ActivateAnimated();
		}

		private void LoadInitialState()
		{
			SetActive(_user.Tutorials.IsTutorialCompleted(EContractsTutorialStep.Completed));
		}

		private void SetMarkers()
		{
			int unseenContracts = _user.Contracts.UnseenStoryContractsCount() + _user.Contracts.UnseenEventContractsCount();
			if (unseenContracts > 0)
			{
				_uiMarkerProvider.SetMarker(1, _markerHolder, EMarkerType.New, EMarkerColor.Blue);
				return;
			}
			
			int contractsToClaim = _user.Contracts.StoryContractsToClaimCount() + _user.Contracts.EventContractsToClaimCount();
			if (contractsToClaim > 0)
			{
				_uiMarkerProvider.SetMarker(1, _markerHolder, EMarkerType.CheckMark, EMarkerColor.Green);
				return;
			}
			
			_uiMarkerProvider.DisableMarker(_markerHolder);
		}
		
		private void RefreshMarkers(IEventBusSignal signal)
		{
			SetMarkers();
		}
	}
}