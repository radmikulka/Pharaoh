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

namespace Pharaoh
{
	public class CStartupQueue : IInitializable
	{
		private readonly CDeferredActionQueue _lazyActionQueue;
		private readonly ICtsProvider _ctsProvider;
		private readonly IEventBus _eventBus;
		
		public CStartupQueue(
			CDeferredActionQueue lazyActionQueue, 
			ICtsProvider ctsProvider, 
			IEventBus eventBus
			)
		{
			_lazyActionQueue = lazyActionQueue;
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

			_lazyActionQueue.Activate();

			_eventBus.Send(new CCoreGameUnlockedSignal());
		}
	}
}