// =========================================
// NAME: Marek Karaba
// DATE: 11.02.2026
// =========================================

using System.Linq;
using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using ServerData;
using TycoonBuilder.Ui.DispatchMenu;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CLiveEventButtonsHandler : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField] private CLiveEventButton _mainButton;
		[SerializeField] private CLiveEventButton _secondaryButton;
		
		private IEventBus _eventBus;
		private CUser _user;
		
		[Inject]
		private void Inject(IEventBus eventBus, CUser user) 
		{
			_eventBus = eventBus;
			_user = user;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CLiveEventsSyncedSignal>(OnEventsSynced);
			_eventBus.Subscribe<CYearSeenSignal>(OnYearSeen);
			_eventBus.Subscribe<CStoryContractTaskCompleted>(OnStoryContractCompleted);
			_eventBus.Subscribe<CLiveEventFinishedSignal>(OnLiveEventFinished);
			_eventBus.Subscribe<CEventLeaderboardFinishedSignal>(OnLiveEventLeaderboardFinished);
			RefreshActivity();
		}

		private void OnEventsSynced(CLiveEventsSyncedSignal signal)
		{
			RefreshActivity();
		}

		private void OnYearSeen(CYearSeenSignal signal)
		{
			RefreshActivity();
		}

		private void OnStoryContractCompleted(CStoryContractTaskCompleted signal)
		{
			RefreshActivity();
		}

		private void OnLiveEventLeaderboardFinished(CEventLeaderboardFinishedSignal signal)
		{
			RefreshActivity();
		}

		private void OnLiveEventFinished(CLiveEventFinishedSignal signal)
		{
			RefreshActivity();
		}

		private void RefreshActivity()
		{
			bool isUnlocked = _user.IsUnlockRequirementMet(CDesignGlobalConfig.LiveEventUnlockRequirement);
			if (!isUnlocked)
			{
				_mainButton.Hide();
				_secondaryButton.Hide();
				return;
			}
			
			ILiveEvent[] activeEvents = _user.LiveEvents.GetRunningEventsOrDefault();
			bool anyEventAvailable = activeEvents.Length > 0;
			if (!anyEventAvailable)
			{
				_mainButton.Hide();
				_secondaryButton.Hide();
				return;
			}

			ShowActiveEvents(activeEvents);
		}

		private void ShowActiveEvents(ILiveEvent[] activeEvents)
		{
			if (activeEvents.Length == 1)
			{
				_mainButton.SetEvent(activeEvents[0]);
				_secondaryButton.Hide();
				return;
			}
			
			ILiveEvent[] sortedByEndTime = activeEvents.OrderByDescending(liveEvent => liveEvent.EndTimeInMs).ToArray();
			_mainButton.SetEvent(sortedByEndTime[0]);
			_secondaryButton.SetEvent(sortedByEndTime[1]);
		}
	}
}