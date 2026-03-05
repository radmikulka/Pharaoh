// =========================================
// AUTHOR: Marek Karaba
// DATE:   19.02.2026
// =========================================

using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Coffee.UIExtensions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using KBCore.Refs;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CEventPointParticleTargetPulser : ValidatedMonoBehaviour, IParticleTargetPart, IAldaFrameworkComponent
	{
		[SerializeField] private UIParticle _fxParticles;
		[SerializeField, Self] private RectTransform _rectTransform;
		
		private ICtsProvider _ctsProvider;
		
		private bool _pulseInProgress;
		private CancellationTokenSource _pulseCts;
		private ELiveEvent _liveEvent;
		
		[Inject]
		private void Inject(ICtsProvider ctsProvider)
		{
			_ctsProvider = ctsProvider;
		}

		public void SetLiveEvent(ELiveEvent liveEvent)
		{
			_liveEvent = liveEvent;
		}
		
		public void DisableSync()
		{
			
		}

		public void EnableSync()
		{
			
		}

		public void ParticleStepFinished(IValuable valuable)
		{
			if (valuable is not CEventPointValuable eventPointValuable)
				return;
			
			if (eventPointValuable.LiveEvent != _liveEvent)
				return;
			
			_pulseCts?.Cancel();
			_pulseCts = _ctsProvider.GetNewLinkedCts();
			ParticleStepFinishedAsync(_pulseCts.Token).Forget();
		}
		
		private async UniTaskVoid ParticleStepFinishedAsync(CancellationToken ct)
		{
			if(_pulseInProgress)
				return;
			
			_pulseInProgress = true;
			_rectTransform.DOPunchScale(Vector3.one * 0.2f, 0.2f)
				.OnComplete(() => _pulseInProgress = false)
				.WithCancellation(_ctsProvider.Token)
				.Forget()
				;
			
			_fxParticles.gameObject.SetActiveObject(true);
			foreach (ParticleSystem part in _fxParticles.particles)
			{
				int count = (int)part.emission.GetBurst(0).count.constant;
				part.Emit(count);
			}

			await UniTask.WaitForSeconds(2f, cancellationToken: ct);
			
			_fxParticles.gameObject.SetActiveObject(false);
		}
	}
}