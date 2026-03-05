// =========================================
// AUTHOR: Marek Karaba
// DATE:   09.10.2025
// =========================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using Cysharp.Threading.Tasks;
using ServerData;
using UnityEngine;

namespace TycoonBuilder.Infrastructure
{
	public class CValuableRewardHandler : BaseRewardHandler
	{
		private readonly CValuableRegionModifier _valuableRegionModifier;
		private readonly IEventBus _eventBus;
		private readonly CUser _user;

		public CValuableRewardHandler(CValuableRegionModifier valuableRegionModifier, IEventBus eventBus, CUser user)
		{
			_valuableRegionModifier = valuableRegionModifier;
			_eventBus = eventBus;
			_user = user;
		}

		public async UniTask Claim(IValuable[] rewards, int index, CancellationToken ct, CValueModifyParams modifyParams)
		{
			IValuable reward = rewards[index];
			bool isLastReward = index == rewards.Length - 1;
			
			if (!isLastReward)
			{
				bool isValidType = IsValidType(reward, rewards[index + 1]);
				if (isValidType)
				{
					ClaimReward(reward, ct, modifyParams).Forget();
					return;
				}
			}
			
			await ClaimReward(reward, ct, modifyParams);
		}

		private async UniTask ClaimReward(IValuable reward, CancellationToken ct, CValueModifyParams modifyParams)
		{
			switch (reward)
			{
				case CConsumableValuable consumable:
					await ClaimConsumableReward(consumable, ct, modifyParams);
					break;
				case CXpValuable xp:
					await ClaimXpReward(xp, ct, modifyParams);
					break;
				case CEventPointValuable eventPointValuable:
					await ClaimEventPointsReward(modifyParams, eventPointValuable, ct);
					break;
				case CEventCoinValuable eventCoinValuable:
					CGainValuableParticleTask task = new (eventCoinValuable, modifyParams);
					await _eventBus.ProcessTaskAsync(task, ct);
					break;
				case CFrameValuable frameValuable:
					await ClaimFrameReward(modifyParams, frameValuable, ct);
					break;
				default:
					throw new System.Exception($"Unsupported reward type: {reward.GetType()}");
			}
		}

		public void Remove(CConsumableValuable price, EModificationSource source, CValueModifyParams modifyParams)
		{
			CConsumableValuable consumableToCharge = price.Reverse();
			IValuable priceToCharge = _valuableRegionModifier.ModifyValuable(consumableToCharge, _user.Progress.Region, source);
			_user.OwnedValuables.ModifyValuableInternal(priceToCharge, modifyParams);
		}

		private async UniTask ClaimEventPointsReward(CValueModifyParams modifyParams, CEventPointValuable eventPointValuable, CancellationToken ct)
		{
			CGainValuableParticleTask task = new (eventPointValuable, modifyParams);
			await _eventBus.ProcessTaskAsync(task, ct);
		}

		private async UniTask ClaimConsumableReward(CConsumableValuable reward, CancellationToken ct, CValueModifyParams modifyParams)
		{
			await SendGainParticles(reward, ct, modifyParams);
		}

		private async UniTask ClaimFrameReward(CValueModifyParams modifyParams, CFrameValuable frameValuable, CancellationToken ct)
		{
			CGainValuableParticleTask task = new (frameValuable, modifyParams);
			await _eventBus.ProcessTaskAsync(task, ct);
			_user.OwnedValuables.ModifyValuableInternal(frameValuable, modifyParams);
		}

		private async UniTask SendGainParticles(CConsumableValuable consumable, CancellationToken ct, CValueModifyParams modifyParams)
		{
			CGainValuableParticleTask task = new (consumable, modifyParams);
			await _eventBus.ProcessTaskAsync(task, ct);
		}

		private async UniTask ClaimXpReward(CXpValuable xp, CancellationToken ct, CValueModifyParams modifyParams)
		{
			await SendGainParticles(xp, ct, modifyParams);
		}

		private async UniTask SendGainParticles(CXpValuable xp, CancellationToken ct, CValueModifyParams modifyParams)
		{ 
			await _eventBus.ProcessTaskAsync(new CGainValuableParticleTask(xp, modifyParams), ct);
		}

		private bool IsValidType(IValuable a, IValuable b)
		{
			bool aIsValid = a is CConsumableValuable or CXpValuable or CEventPointValuable or CFrameValuable;
			bool bIsValid = b is CConsumableValuable or CXpValuable or CEventPointValuable or CFrameValuable;
			bool bothValid = aIsValid && bIsValid;
			return bothValid;
		}
	}
}