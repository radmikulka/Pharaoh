// =========================================
// AUTHOR: Radek Mikulka
// DATE:   01.11.2023
// =========================================

using System;
using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using KBCore.Refs;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
	public class CFullScreenOverlay : ValidatedMonoBehaviour, IConstructable, IInitializable
	{
		private const float BlendDuration = 0.3f;
		
		[SerializeField, Child] private CanvasGroup _canvasGroup;
		[SerializeField, Child] private Canvas _canvas;

		private readonly CInputLock _lockObject = new("FullScreenOverlay", EInputLockLayer.FullscreenOverlay);

		private CEventSystem _eventSystem;
		private IEventBus _eventBus;

		[Inject]
		private void Inject(IEventBus eventBus, CEventSystem eventSystem)
		{
			_eventSystem = eventSystem;
			_eventBus = eventBus;
		}

		public void Construct()
		{
			_canvas.SetActiveBehaviour(false);
		}

		public void Initialize()
		{
			_eventBus.AddAsyncTaskHandler<CPingPongFullScreenOverlayTask>(OnPingPongFullScreenOverlay);
		}

		private async Task OnPingPongFullScreenOverlay(CPingPongFullScreenOverlayTask task, CancellationToken ct)
		{
			await PingPong(task.OnComplete, task.OverrideBlendInDuration ?? BlendDuration, task.OverrideBlendOutDuration ?? BlendDuration, ct);
		}

		private async UniTask PingPong(Func<CancellationToken, UniTask> onComplete, float blendInDuration, float blendOutDuration, CancellationToken ct)
		{
			_eventSystem.AddInputLocker(_lockObject);
			_canvas.SetActiveBehaviour(true);
			await BlendIn(blendInDuration, ct);
			if(onComplete != null)
			{
				await onComplete(ct);
			}
			await BlendOut(blendOutDuration, ct);
			_canvas.SetActiveBehaviour(false);
			_eventSystem.RemoveInputLocker(_lockObject);
		}

		private async UniTask BlendIn(float duration, CancellationToken ct)
		{
			_canvasGroup.alpha = 0f;
			await _canvasGroup.DOFade(1f, duration).WithCancellation(ct);
		}
		
		private async UniTask BlendOut(float duration, CancellationToken ct)
		{
			await _canvasGroup.DOFade(0f, duration).WithCancellation(ct);
		}
	}
}