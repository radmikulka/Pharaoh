// =========================================
// AUTHOR: Marek Karaba
// DATE:   20.02.2026
// =========================================

using AldaEngine.AldaFramework;
using KBCore.Refs;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CEventPassMarker : ValidatedMonoBehaviour, IAldaFrameworkComponent
	{
		[SerializeField, Self] private RectTransform _markerHolder;
		
		private CUiMarkerProvider _markerProvider;
		private CUser _user;
		
		[Inject]
		private void Inject(CUiMarkerProvider markerProvider, CUser user)
		{
			_markerProvider = markerProvider;
			_user = user;
		}

		public void SetMarkerState(ELiveEvent liveEventId, bool isMaskable = true)
		{
			bool isRewardClaimable = IsRewardClaimable(liveEventId);
			SetMarkerState(isRewardClaimable, isMaskable);
		}
		
		private bool IsRewardClaimable(ELiveEvent liveEventId)
		{
			IEventWithBattlePass content = _user.LiveEvents.GetEventContent<IEventWithBattlePass>(liveEventId);
			if (content == null)
				return false;
			
			bool isAnyRewardClaimable = content.IsAnyRewardClaimable();
			return isAnyRewardClaimable;
		}
		
		private void SetMarkerState(bool isActive, bool isMaskable)
		{
			if (isActive)
			{
				_markerProvider.SetMarker(1, _markerHolder, EMarkerType.ExclamationMark, EMarkerColor.Red,true, isMaskable);
				return;
			}
			_markerProvider.DisableMarker(_markerHolder);
		}
	}
}