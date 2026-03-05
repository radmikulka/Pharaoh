// =========================================
// AUTHOR: Marek Karaba
// DATE:   03.11.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CLiveEventButton : ValidatedMonoBehaviour, IAldaFrameworkComponent
	{
		[SerializeField, Child] private CActiveLiveEventVisual _activeVisual;
		[SerializeField, Child] private CFinishedLiveEventVisual _finishedVisual;
		[SerializeField, Child] private CUiEventPointParticlePoint _eventParticlePoint;
		[SerializeField, Child] private CEventPointParticleTargetPulser _particlePulser;
		[SerializeField, Child] private CLeaderboardChangeShower _leaderboardChangeShower;
		
		private IServerTime _serverTime;
		private CUser _user;

		[Inject]
		private void Inject(IServerTime serverTime, CUser user)
		{
			_serverTime = serverTime;
			_user = user;
		}
		
		public void SetEvent(ILiveEvent liveEvent)
		{
			bool eventWithRegion = _user.LiveEvents.IsEventWithRegion(liveEvent);
			if (!eventWithRegion)
			{
				_eventParticlePoint.Register(liveEvent.Id);
				_particlePulser.SetLiveEvent(liveEvent.Id);
			}
			
			gameObject.SetActive(true);
			bool isFinished = liveEvent.IsFinished(_serverTime.GetTimestampInMs());
			if (isFinished)
			{
				_finishedVisual.Show(liveEvent);
				_activeVisual.Hide();
			}
			else
			{
				_activeVisual.Show(liveEvent);
				_finishedVisual.Hide();
			}

			SetLeaderboard(liveEvent);
		}

		private void SetLeaderboard(ILiveEvent liveEvent)
		{
			if (liveEvent.BaseContent is not CStandardEventContent standardEventContent)
				return;

			_leaderboardChangeShower.SetContent(standardEventContent);
		}

		public void Hide()
		{
			gameObject.SetActive(false);
		}
	}
}