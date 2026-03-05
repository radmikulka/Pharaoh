// =========================================
// AUTHOR: Marek Karaba
// DATE:   11.11.2025
// =========================================

using System;
using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData;

namespace TycoonBuilder
{
	public class CContractAnimationHandler
	{
		public async UniTask Play(
			IContract contract,
			EStaticContractId contractId,
			CAnimation anim,
			Func<CancellationToken, UniTask> action,
			IEventBus eventBus,
			CancellationToken ct,
			bool canShowSkipButton = true,
			float skipButtonDelay = 1.1f
		)
		{
			eventBus.Send(new CContractAnimationStartedSignal(contract, canShowSkipButton, anim));
			await CSkipAbleUniTask.Play(action, eventBus, ct, canShowSkipButton, skipButtonDelay);
			eventBus.Send(new CContractAnimationCompletedSignal(contractId));
		}
	}
}