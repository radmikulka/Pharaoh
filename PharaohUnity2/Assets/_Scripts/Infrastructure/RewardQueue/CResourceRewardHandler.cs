// =========================================
// AUTHOR: Marek Karaba
// DATE:   31.07.2025
// =========================================

using System.Threading;
using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData;
using UnityEngine;

namespace TycoonBuilder.Infrastructure
{
	public class CResourceRewardHandler : BaseRewardHandler
	{
		private readonly CUser _user;
		private readonly IEventBus _eventBus;
		
		public CResourceRewardHandler(CUser user, IEventBus eventBus)
		{
			_user = user;
			_eventBus = eventBus;
		}
		
		public async UniTask Claim(IValuable[] rewards, int index, CResourceValuable resource, CancellationToken ct, CValueModifyParams modifyParams)
		{
			IValuable reward = rewards[index];
			bool isLastReward = index == rewards.Length - 1;
			if (!isLastReward)
			{
				bool isNextRewardConsumable = AreSameType(reward, rewards[index + 1]);
				if (isNextRewardConsumable)
				{
					ClaimResourceReward(resource, ct, modifyParams).Forget();
					return;
				}
			}
			await ClaimResourceReward(resource, ct, modifyParams);
		}
		
		private async UniTask ClaimResourceReward(CResourceValuable reward, CancellationToken ct, CValueModifyParams modifyParams)
		{
			await SendGainParticles(reward, ct, modifyParams);
			_eventBus.Send(new CWarehouseResourceChangedSignal(reward.Resource));;
		}

		public void Remove(CResourceValuable price, CValueModifyParams modifyParams)
		{
			SendRemoveParticles(price, modifyParams);
		}
		
		private async UniTask SendGainParticles(CResourceValuable resource, CancellationToken ct, CValueModifyParams modifyParams)
		{
			CGainResourceParticleTask task = new (resource, modifyParams);
			await _eventBus.ProcessTaskAsync(task, ct);
		}
		
		private void SendRemoveParticles(CResourceValuable resource, CValueModifyParams modifyParams)
		{
			_user.OwnedValuables.ModifyValuableInternal(resource.Reverse(), modifyParams);
		}
	}
}