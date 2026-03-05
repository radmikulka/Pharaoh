// =========================================
// AUTHOR: Marek Karaba
// DATE:   15.01.2026
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class COpenLiveEventOverviewButton : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField, Self] private CUiButton _button;
		
		private ICtsProvider _ctsProvider;
		private IEventBus _eventBus;
		private CUser _user;
		
		[Inject]
		private void Inject(
			ICtsProvider ctsProvider,
			IEventBus eventBus,
			CUser user
		)
		{
			_ctsProvider = ctsProvider;
			_eventBus = eventBus;
			_user = user;
		}
		
		public void Initialize()
		{
			_button.AddClickListener(LoadEvent);
		}
		
		private void LoadEvent()
		{
			ILiveEvent runningEvent = _user.LiveEvents.GetRunningEventOrDefault();
			if (runningEvent == null)
				return;
			
			_eventBus.ProcessTaskAsync(new COpenEventOverviewTask(runningEvent.Id), _ctsProvider.Token);
		}
	}
}