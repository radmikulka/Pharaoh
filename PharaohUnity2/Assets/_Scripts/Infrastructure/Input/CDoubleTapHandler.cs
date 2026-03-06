// =========================================
// AUTHOR: Marek Karaba
// DATE:   20.11.2025
// =========================================

using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;

namespace Pharaoh
{
	public class CDoubleTapHandler : IInitializable
	{
		private const float DoubleTapDuration = 0.2f;
		
		private readonly ICtsProvider _ctsProvider;
		private readonly IEventBus _eventBus;

		private bool _waitingForSecondTap;
		private CancellationTokenSource _cts;

		public CDoubleTapHandler(ICtsProvider ctsProvider, IEventBus eventBus)
		{
			_ctsProvider = ctsProvider;
			_eventBus = eventBus;
		}
		
		public void Initialize()
		{
			_eventBus.Subscribe<CTapSignal>(OnTap);
		}
		
		private void OnTap(CTapSignal signal)
		{
			if (!_waitingForSecondTap)
			{
				_waitingForSecondTap = true;
				_cts?.Cancel();
				_cts = CancellationTokenSource.CreateLinkedTokenSource(_ctsProvider.Token);
				TapsExpirationCountdownAsync(_cts.Token).Forget();
				return;
			}
			
			_cts?.Cancel();
			_cts = null;
			_waitingForSecondTap = false;
			_eventBus.Send(new CDoubleTapSignal());
		}
		
		private async UniTask TapsExpirationCountdownAsync(CancellationToken cancellationToken)
		{
			await UniTask.WaitForSeconds(DoubleTapDuration, cancellationToken: cancellationToken);
			_waitingForSecondTap = false;
		}
	}
}