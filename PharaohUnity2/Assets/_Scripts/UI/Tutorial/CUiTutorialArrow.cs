// =========================================
// AUTHOR: Radek Mikulka
// DATE:   19.12.2023
// =========================================

using System;
using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CUiTutorialArrow : CUiBouncingArrow, IInitializable
	{
		private CancellationTokenSource _cts;
		private CEventSystem _eventSystem;
		private ICtsProvider _ctsProvider;
		private IEventBus _eventBus;

		[Inject]
		private void Inject(IEventBus eventBus, ICtsProvider ctsProvider, CEventSystem eventSystem)
		{
			_eventSystem = eventSystem;
			_ctsProvider = ctsProvider;
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			_eventBus.AddTaskHandler<CShowTutorialArrowTask>(OnShowTutorialArrow);
			_eventBus.AddTaskHandler<CHideTutorialArrowTask>(OnHideTutorialArrow);
			_eventBus.AddTaskHandler<CHideTutorialArrowInstantTask>(OnHideTutorialArrowInstant);
			_eventBus.AddTaskHandler<CIsTutorialArrowActiveRequest, CIsTutorialArrowActiveResponse>(HandleIsTutorialArrowActive);
		}

		private CIsTutorialArrowActiveResponse HandleIsTutorialArrowActive(CIsTutorialArrowActiveRequest request)
		{
			return new CIsTutorialArrowActiveResponse(IsActive);
		}

		private void OnHideTutorialArrowInstant(CHideTutorialArrowInstantTask task)
		{
			HideInstant();
		}

		private void OnHideTutorialArrow(CHideTutorialArrowTask task)
		{
			Hide();
			_cts?.Cancel();
		}

		private void OnShowTutorialArrow(CShowTutorialArrowTask task)
		{
			_cts?.Cancel();
			_cts = _ctsProvider.GetNewLinkedCts();
			
			ShowAt(task.Target, 
				task.ClockwiseArrowRotation, 
				task.AnchoredOffset, 
				task.DelayInSecsBeforeShow,
				_cts.Token
				)
				.Forget();
		}

		private async UniTaskVoid ShowAt(ITutorialGraphicsTarget target, float clockwiseRotation, Vector2 anchoredOffset, float delayInSecs, CancellationToken ct)
		{
			CInputLock inputLock = new("TutorialArrow", EInputLockLayer.Tutorial);
			_eventSystem.AddInputLocker(inputLock);
			try
			{
				await UniTask.WaitForSeconds(delayInSecs, cancellationToken: ct);
				await base.ShowAt(target, clockwiseRotation, anchoredOffset, ct);
			}
			finally
			{
				_eventSystem.RemoveInputLocker(inputLock);
			}
		}
	}
}