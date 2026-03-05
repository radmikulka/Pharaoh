// =========================================
// AUTHOR: Radek Mikulka
// DATE:   18.10.2023
// =========================================

using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServiceEngine.Purchasing;

namespace TycoonBuilder
{
	public class CFailedPurchaseResolver : IInitializable
	{
		private readonly IRestartGameHandler _restartGameHandler;
		private readonly CLazyActionQueue _lazyStartupQueue;
		private readonly IEventBus _eventBus;

		public CFailedPurchaseResolver(
			IRestartGameHandler restartGameHandler,
			CLazyActionQueue lazyStartupQueue, 
			IEventBus eventBus
			)
		{
			_restartGameHandler = restartGameHandler;
			_lazyStartupQueue = lazyStartupQueue;
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CFailedPurchaseResolvedSignal>(OnFailedPurchaseResolved);
		}

		private void OnFailedPurchaseResolved(CFailedPurchaseResolvedSignal signal)
		{
			_lazyStartupQueue.AddAction(new CShowFailedPurchaseDialogAction(_eventBus, _restartGameHandler));
		}
		
		private class CShowFailedPurchaseDialogAction : ILazyAction
		{
			private readonly IRestartGameHandler _restartGameHandler;
			private readonly IEventBus _eventBus;

			public int Priority => int.MaxValue;

			public CShowFailedPurchaseDialogAction(IEventBus eventBus, IRestartGameHandler restartGameHandler)
			{
				_restartGameHandler = restartGameHandler;
				_eventBus = eventBus;
			}

			public async UniTask Execute(CancellationToken ct)
			{
				CShowDialogTask task = new CShowDialogTask()
					.SetHeaderLocalized("Dialog.PurchaseResolved.Title")
					.SetContentLocalized("Dialog.PurchaseResolved.Content")
					.SetOverlay()
					.SetCanBeClosed(false)
					.SetAnalyticsId("PurchaseResolved")
					.SetOneButton(new CDialogButtonData("Generic.Restart", () =>
					{
						_restartGameHandler.RestartGame(null);
					}, EDialogButtonColor.Blue, true));
				
				_eventBus.ProcessTask(task);
				
				// wait infinity wait - will be breaked by game restart
				await UniTask.WaitUntil(() => !ct.IsCancellationRequested, cancellationToken: ct);
			}
		}
	}
}