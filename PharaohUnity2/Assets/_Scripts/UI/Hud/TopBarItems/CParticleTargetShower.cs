// =========================================
// AUTHOR: Juraj Joscak
// DATE:   07.10.2025
// =========================================

using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using KBCore.Refs;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CParticleTargetShower : ValidatedMonoBehaviour, IParticleTargetPart, IConstructable
	{
		[SerializeField, Self] private CUiComponentCanvasGroup _canvasGroup;
		
		private CTopBarItemsUpperHolder _topBarItemsUpperHolder;
		private IEventBus _eventBus;
		private ICtsProvider _ctsProvider;
		
		private RectTransform _transform;
		private Transform _originalParent;
		
		[Inject]
		private void Inject(CTopBarItemsUpperHolder topBarItemsUpperHolder, IEventBus eventBus, ICtsProvider ctsProvider)
		{
			_topBarItemsUpperHolder = topBarItemsUpperHolder;
			_eventBus = eventBus;
			_ctsProvider = ctsProvider;
		}

		public void Construct()
		{
			_transform = (RectTransform)transform;
			_originalParent = _transform.parent;
		}

		public void DisableSync()
		{
			CUiComponentCanvasGroup cg = _eventBus.ProcessTask<CGetHudCanvasGroup, CUiComponentCanvasGroup>(new CGetHudCanvasGroup());
			
			_canvasGroup.SetAlpha(cg.Alpha);
			_transform.SetParent(_topBarItemsUpperHolder.Transform, true);
			_canvasGroup.DOFade(1f, 0.2f);
		}

		public void EnableSync()
		{
			FadeOut(0.2f, _ctsProvider.Token).Forget();
		}

		private async UniTaskVoid FadeOut(float duration, CancellationToken ct)
		{
			CUiComponentCanvasGroup targetCg = _eventBus.ProcessTask<CGetHudCanvasGroup, CUiComponentCanvasGroup>(new CGetHudCanvasGroup());
			float progress = 0;
			while (progress < 1f)
			{
				await UniTask.Yield(ct);
				progress += Time.deltaTime/duration;
				_canvasGroup.SetAlpha(Mathf.Lerp(1f, targetCg.Alpha, progress));
			}
			
			_transform.SetParent(_originalParent, true);
			_canvasGroup.SetAlpha(1);
		}

		public void ParticleStepFinished(IValuable _)
		{
			
		}
	}
}