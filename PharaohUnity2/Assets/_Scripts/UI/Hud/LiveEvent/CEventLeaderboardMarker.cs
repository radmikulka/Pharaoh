// =========================================
// AUTHOR: Marek Karaba
// DATE:   22.01.2026
// =========================================

using AldaEngine.AldaFramework;
using KBCore.Refs;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CEventLeaderboardMarker : ValidatedMonoBehaviour, IAldaFrameworkComponent
	{
		[SerializeField, Self] private RectTransform _markerHolder;
		
		private CUiMarkerProvider _markerProvider;
		private IServerTime _serverTime;
		private CUser _user;
		
		[Inject]
		private void Inject(CUiMarkerProvider markerProvider, IServerTime serverTime, CUser user)
		{
			_markerProvider = markerProvider;
			_serverTime = serverTime;
			_user = user;
		}

		public void SetMarkerState(ELiveEvent liveEventId, bool isMaskable = true)
		{
			IEventWithLeaderboard content = _user.LiveEvents.GetEventContent<IEventWithLeaderboard>(liveEventId);
			SetMarkerState(content, isMaskable);
		}
		
		private void SetMarkerState(IEventWithLeaderboard content, bool isMaskable)
		{
			if (content?.Leaderboard == null)
			{
				_markerProvider.DisableMarker(_markerHolder);
				return;
			}
			bool isFinished = content.Leaderboard.IsFinished(_serverTime.GetTimestampInMs());
			if (!isFinished)
			{
				_markerProvider.DisableMarker(_markerHolder);
				return;
			}
			_markerProvider.SetMarker(1, _markerHolder, EMarkerType.ExclamationMark, EMarkerColor.Red,true, isMaskable);
		}
	}
}