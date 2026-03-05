// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.2.2024
// =========================================

using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;
using NaughtyAttributes;
using TycoonBuilder.Signal;
using ServiceEngine.Ads;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CDailyRefresh : MonoBehaviour, IAldaFrameworkComponent
	{
		private IRestartGameHandler _restartGameHandler;
		private IServerTime _serverTime;
		private IAdsManager _adsManager;
		private IPurchasing _purchasing;
		private IEventBus _eventBus;

		private bool _refreshTriggered;
		private readonly HashSet<CLockObject> _refreshLockers = new();

		[Inject]
		public void Inject(
			IRestartGameHandler restartGameHandler, 
			IServerTime serverTime, 
			IAdsManager adsManager, 
			IPurchasing purchasing, 
			IEventBus eventBus
			)
		{
			_restartGameHandler = restartGameHandler;
			_serverTime = serverTime;
			_adsManager = adsManager;
			_purchasing = purchasing;
			_eventBus = eventBus;
		}

		public void LateUpdate()
		{
			TryPerformAutomaticDailyRefresh();
		}

		private bool ShouldPerformDailyRefresh()
		{
			bool isDailyRefreshTime = IsDailyRefreshTime();
			bool canPerformDailyRefresh = CanPerformDailyRefresh();
			return isDailyRefreshTime && canPerformDailyRefresh;
		}

		private void TryPerformAutomaticDailyRefresh()
		{
			bool locked = _refreshLockers.Count > 0;
			if (locked)
				return;
			
			bool shouldPerformDailyRefresh = ShouldPerformDailyRefresh();
			if(!shouldPerformDailyRefresh)
				return;
			
			bool canPerformAutomaticDailyRefresh = CanPerformAutomaticDailyRefresh();
			if(!canPerformAutomaticDailyRefresh)
				return;
			
			PerformAutomaticDailyRefresh();
		}

		private void PerformAutomaticDailyRefresh()
		{
			_refreshTriggered = true;
			ShowDailyRefreshDialog();
		}

		[Button]
		private void ShowDailyRefreshDialog()
		{
			CShowDialogTask task = new CShowDialogTask()
				.SetHeaderLocalized("Dialog.DailyRestart.Title")
				.SetOneButton(
					new CDialogButtonData("Generic.Restart", () =>
					{
						_restartGameHandler.RestartGame(null);
					}, EDialogButtonColor.Green, true))
				.SetContentLocalized("Dialog.DailyRestart.Content");
			_eventBus.ProcessTask(task);
		}

		private bool IsDailyRefreshTime()
		{
			long dayRefreshTime = _serverTime.GetDayRefreshTimeInMs();
			long currentTime = _serverTime.GetTimestampInMs();
			return dayRefreshTime > 0 && dayRefreshTime <= currentTime;
		}

		private bool CanPerformDailyRefresh()
		{
			if (_refreshTriggered)
				return false;
			
			bool isLockedByWatchingAd = _adsManager.LastAdFinishTime + 1f > Time.time;
			if (isLockedByWatchingAd)
				return false;

			bool purchaseIsRunning = _purchasing.PurchaseIsRunning;
			if (purchaseIsRunning)
				return false;

			return true;
		}

		private bool CanPerformAutomaticDailyRefresh()
		{
			return true;
		}
	}
}