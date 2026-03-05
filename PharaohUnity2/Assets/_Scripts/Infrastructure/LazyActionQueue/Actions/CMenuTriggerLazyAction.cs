// =========================================
// AUTHOR: Marek Karaba
// DATE:   22.07.2025
// =========================================

using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TycoonBuilder
{
	public class CMenuTriggerLazyAction : ILazyAction
	{
		private readonly EMenuTrigger _trigger;
		private readonly IEventBus _eventBus;

		public CMenuTriggerLazyAction(EMenuTrigger trigger, IEventBus eventBus)
		{
			_trigger = trigger;
			_eventBus = eventBus;
		}

		public int Priority => int.MaxValue;
		
		public UniTask Execute(CancellationToken ct)
		{
			SendTrigger(_trigger);
			return UniTask.CompletedTask;
		}

		private void SendTrigger(EMenuTrigger trigger)
		{
			switch (trigger)
			{
				case EMenuTrigger.RateUs:
					_eventBus.ProcessTask(new CShowScreenTask(EScreenId.RateUs));
					break;
				case EMenuTrigger.NativeRateUs:
					if (CPlatform.IsEditor)
					{
						_eventBus.ProcessTask(new CShowNativeRateUsEditor());
						break;
					}
					_eventBus.ProcessTask(new CShowNativeRateUs());
					break;
				case EMenuTrigger.SaveProgress:
					_eventBus.ProcessTask(new CShowSaveProgressPopUpTask());
					break;
				case EMenuTrigger.FacebookLike:
					_eventBus.ProcessTask(new CShowScreenTask(EScreenId.FacebookLikePopUp));
					break;
				case EMenuTrigger.FacebookConnect:
					_eventBus.ProcessTask(new CShowScreenTask(EScreenId.FacebookConnectPopUp));
					break;
			}
		}
	}
}