// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.09.2025
// =========================================

using System;
using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;

namespace TycoonBuilder
{
	public static class CSkipAbleUniTask
	{
		public static async UniTask Play(
			Func<CancellationToken, UniTask> action, 
			IEventBus eventBus, 
			CancellationToken ct, 
			bool canShowSkipButton = true,
			float skipButtonDelay = 1.1f
			)
		{
			CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

			if (canShowSkipButton)
			{
				ShowSkipButtonAsync().Forget();
			}
			
			Guid subscriptionGuid = eventBus.Subscribe<CAnimationSkipRequestedSignal>(OnSkipRequested);
			
			bool cancelled = await action(cts.Token).SuppressCancellationThrow();
			if (cancelled)
			{
				ct.ThrowIfCancellationRequested();
			}
			
			eventBus.Unsubscribe(subscriptionGuid);
			eventBus.ProcessTask(new CHideSkipButtonRequest());
			cts.Cancel();
			return;

			void OnSkipRequested(CAnimationSkipRequestedSignal signal)
			{
				cts.Cancel();
			}
			
			async UniTask ShowSkipButtonAsync()
			{
				await UniTask.WaitForSeconds(skipButtonDelay, cancellationToken: cts.Token);
				eventBus.ProcessTask(new CShowSkipButtonRequest());
			}
		}
	}
}