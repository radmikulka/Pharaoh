// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.10.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using DG.Tweening;
using ServerData;
using TycoonBuilder.Ui.DispatchMenu;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CDispatchCenterButton : CTutorialButton, IInitializable
	{
		[SerializeField] private GameObject _claimVisual;
		[SerializeField] private CTweener _iconTweener;
		
		private IEventBus _eventBus;
		private	CUser _user;

		[Inject]
		private void Inject(IEventBus eventBus, CUser user)
		{
			_eventBus = eventBus;
			_user = user;
		}

		public void Initialize()
		{
			LoadInitialState();
			_eventBus.AddTaskHandler<CActivateDispatchCenterButtonTask>(ActivateDispatchCenterButton);
			
			_eventBus.Subscribe<CResourceDispatchCollectedSignal>(OnResourceDispatchCollected);
			_eventBus.Subscribe<CResourceDispatchCompletionStateChangedSignal>(OnResourceDispatchCompletionStateChanged);
			_eventBus.Subscribe<CVehicleSendSignal>(OnVehicleSend);
		}

		private void OnResourceDispatchCollected(CResourceDispatchCollectedSignal signal)
		{
			SetCorrectState();
		}

		private void OnResourceDispatchCompletionStateChanged(CResourceDispatchCompletionStateChangedSignal signal)
		{
			SetCorrectState();
		}
		
		private void OnVehicleSend(CVehicleSendSignal signal)
		{
			SetCorrectState();
		}

		private void ActivateDispatchCenterButton(CActivateDispatchCenterButtonTask task)
		{
			ActivateAnimated();
		}

		private void LoadInitialState()
		{
			SetActive(_user.Tutorials.IsTutorialCompleted(EDispatchCenterTutorialStep.Completed));
			SetCorrectState();
		}

		private void SetCorrectState()
		{
			CDispatch[] dispatches = _user.Dispatches.GetCompletedResourceDispatches();
			bool somethingToClaim = dispatches.Length > 0;
			_claimVisual.SetActive(somethingToClaim);
			SetTweener(somethingToClaim);
		}

		private void SetTweener(bool somethingToClaim)
		{
			if (somethingToClaim)
			{
				_iconTweener.Enable();
			}
			else
			{
				_iconTweener.Disable();
				_iconTweener.transform.localScale = Vector3.one;
			}
		}
	}
}