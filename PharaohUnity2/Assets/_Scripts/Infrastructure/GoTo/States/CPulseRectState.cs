// =========================================
// AUTHOR: Juraj Joscak
// DATE:   16.02.2026
// =========================================

using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace TycoonBuilder.GoToStates
{
	public class CPulseRectState : CAwaitableState
	{
		public CPulseRectState(IEventBus eventBus) : base(eventBus)
		{
			
		}

		protected override async UniTask Run(CancellationToken ct)
		{
			EUiRect rect = Context.GetEntry<EUiRect>(EGoToContextKey.UiRect);
			await UniTask.WaitForSeconds(0.25f, cancellationToken: ct);
			
			CGetUiRectResponse rectResponse = EventBus.ProcessTask<CGetUiRectRequest, CGetUiRectResponse>(new CGetUiRectRequest(rect));
			if(rectResponse.RectTransform == null)
			{
				IsCompleted = true;
				return;
			}
			
			rectResponse.RectTransform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.4f, 0, 0)
				.OnComplete(() => IsCompleted = true);
		}
	}
}