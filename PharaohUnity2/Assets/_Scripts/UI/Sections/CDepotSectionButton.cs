// =========================================
// AUTHOR:
// DATE:   01.10.2025
// =========================================

using AldaEngine;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CDepotSectionButton : CSectionButton
	{
		[SerializeField] private CDepotSectionUiItem _item;
		
		private IEventBus _eventBus;
		
		[Inject]
		private void Inject(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}
		
		protected override void OnClick()
		{
			bool isLocked = _item.IsLocked();
			if (isLocked)
			{
				int year = _item.GetUnlockYear();
				_eventBus.ProcessTask(new CShowTooltipTask("DepotMenu.YearNotUnlockedYet", true, year));
				return;
			}
			
			base.OnClick();
		}
	}
}