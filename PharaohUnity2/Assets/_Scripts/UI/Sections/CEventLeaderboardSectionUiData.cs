// =========================================
// AUTHOR: Marek Karaba
// DATE:   22.01.2026
// =========================================

using System;
using ServerData;
using UnityEngine;

namespace TycoonBuilder.Ui
{
	public class CEventLeaderboardSectionUiData : CSectionUiData
	{
		public readonly ELiveEvent LiveEventId;

		public event Action OnLeaderboardFinished;

		public CEventLeaderboardSectionUiData(
			ELiveEvent liveEventId,
			string title,
			CSectionContentBase sectionContentBase,
			CSectionHandler sectionHandler,
			Sprite icon = null) : 
			base(title, sectionContentBase, sectionHandler, icon)
		{
			LiveEventId = liveEventId;
		}
		
		public void InvokeLeaderboardFinished()
		{
			OnLeaderboardFinished?.Invoke();
		}
	}
}