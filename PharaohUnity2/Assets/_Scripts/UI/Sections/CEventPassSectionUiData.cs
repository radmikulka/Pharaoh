// =========================================
// AUTHOR: Marek Karaba
// DATE:   20.02.2026
// =========================================

using System;
using ServerData;
using UnityEngine;

namespace TycoonBuilder.Ui
{
	public class CEventPassSectionUiData : CSectionUiData
	{
		public readonly ELiveEvent LiveEventId;

		public event Action OnRewardsStateChanged;

		public CEventPassSectionUiData(
			ELiveEvent liveEventId,
			string title,
			CSectionContentBase sectionContentBase,
			CSectionHandler sectionHandler,
			Sprite icon = null) : 
			base(title, sectionContentBase, sectionHandler, icon)
		{
			LiveEventId = liveEventId;
		}
		
		public void InvokedRewardsStateChanged()
		{
			OnRewardsStateChanged?.Invoke();
		}
	}
}