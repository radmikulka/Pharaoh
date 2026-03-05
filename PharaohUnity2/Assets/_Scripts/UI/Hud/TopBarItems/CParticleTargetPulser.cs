// =========================================
// AUTHOR: Juraj Joscak
// DATE:   14.10.2025
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
	public class CParticleTargetPulser : ValidatedMonoBehaviour, IParticleTargetPart, IConstructable
	{
		[SerializeField] private UIParticle _fxParticles;
		
		private ICtsProvider _ctsProvider;
		
		private RectTransform _transform;
		private bool _pulseInProgress;
		private CancellationTokenSource _pulseCts;
		
		[Inject]
		private void Inject(ICtsProvider ctsProvider)
		{
			_ctsProvider = ctsProvider;
		}
		
		public void Construct()
		{
			_transform = (RectTransform)transform;
		}
		
		public void DisableSync()
		{
			
		}

		public void EnableSync()
		{
			
		}

		public void ParticleStepFinished(IValuable _)
		{
			_pulseCts?.Cancel();
			_pulseCts = _ctsProvider.GetNewLinkedCts();
			ParticleStepFinishedAsync(_pulseCts.Token).Forget();
		}
		
		private async UniTaskVoid ParticleStepFinishedAsync(CancellationToken ct)
		{
			if(_pulseInProgress)
				return;
			
			_pulseInProgress = true;
			_transform.DOPunchScale(Vector3.one * 0.2f, 0.2f)
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