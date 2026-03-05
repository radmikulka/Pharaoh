// =========================================
// AUTHOR: Radek Mikulka
// DATE:   02.07.2025
// =========================================

using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TycoonBuilder
{
	public class CStartupQueue : IInitializable
	{
		private readonly CDispatchCenterTutorial _dispatchCenterTutorial;
		private readonly CContractsMenuTutorial _contractsMenuTutorial;
		private readonly CLazyActionQueue _lazyActionQueue;
		private readonly CIntroTutorial _introTutorial;
		private readonly ICtsProvider _ctsProvider;
		private readonly IEventBus _eventBus;
		
		public CStartupQueue(
			CDispatchCenterTutorial dispatchCenterTutorial, 
			CContractsMenuTutorial contractsMenuTutorial, 
			CLazyActionQueue lazyActionQueue, 
			CIntroTutorial introTutorial, 
			ICtsProvider ctsProvider, 
			IEventBus eventBus
			)
		{
			_dispatchCenterTutorial = dispatchCenterTutorial;
			_contractsMenuTutorial = contractsMenuTutorial;
			_lazyActionQueue = lazyActionQueue;
			_introTutorial = introTutorial;
			_ctsProvider = ctsProvider;
			_eventBus = eventBus;
		}
		
		public void Initialize()
		{
			_eventBus.Subscribe<CCoreGameLoadedSignal>(OnCoreGameLoaded);
		}

		private void OnCoreGameLoaded(CCoreGameLoadedSignal signal)
		{
			RunAsync(_ctsProvider.Token).Forget();
		}

		private async UniTask RunAsync(CancellationToken ct)
		{
			await UniTask.WaitForSeconds(0.5f, cancellationToken: ct);

			await RunTutorials(ct);

			_lazyActionQueue.Activate();

			_eventBus.Send(new CCoreGameUnlockedSignal());
		}

		private async UniTask RunTutorials(CancellationToken ct)
		{
			await _introTutorial.TryRunAsync(ct);
			_dispatchCenterTutorial.TryStartOnGameLoad();
			_contractsMenuTutorial.TryStartOnGameLoad();
		}
	}
}