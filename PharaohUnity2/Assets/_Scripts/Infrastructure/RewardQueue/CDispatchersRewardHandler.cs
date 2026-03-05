// =========================================
// AUTHOR: Marek Karaba
// DATE:   19.11.2025
// =========================================

using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;

namespace TycoonBuilder.Infrastructure
{
	public class CDispatchersRewardHandler : BaseRewardHandler, IAldaFrameworkComponent
	{
		private readonly IEventBus _eventBus;
		private readonly CUser _user;

		public CDispatchersRewardHandler(IEventBus eventBus, CUser user
		)
		{
			_eventBus = eventBus;
			_user = user;
		}

		public async UniTask Claim(CDispatcherValuable dispatcherValuable, CancellationToken ct, CValueModifyParams modifyParams)
		{
			CShowDispatcherHiredMenuTask task = new (dispatcherValuable.Dispatcher, dispatcherValuable.ExpirationDurationIsSecs);
			await _eventBus.ProcessTaskAsync(task, ct);
			
			_user.OwnedValuables.ModifyValuableInternal(dispatcherValuable, modifyParams);
		}
	}
}