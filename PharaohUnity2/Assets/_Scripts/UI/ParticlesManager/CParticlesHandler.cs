// =========================================
// AUTHOR: Marek Karaba
// DATE:   21.07.2025
// =========================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;
using UnityEngine;

namespace TycoonBuilder.Ui
{
	public class CParticlesHandler : IInitializable
	{
		private readonly CParticlesSourceProvider _particlesSourceProvider;
		private readonly IEventBus _eventBus;
		private readonly CUser _user;

		public CParticlesHandler(CParticlesSourceProvider particlesSourceProvider, IEventBus eventBus, CUser user)
		{
			_particlesSourceProvider = particlesSourceProvider;
			_eventBus = eventBus;
			_user = user;
		}
		
		public void Initialize()
		{
			_eventBus.AddAsyncTaskHandler<CGainValuableParticleTask>(OnGainConsumable);
			_eventBus.AddAsyncTaskHandler<CGainResourceParticleTask>(OnGainResource);
		}

		private async Task OnGainConsumable(CGainValuableParticleTask task, CancellationToken ct)
		{
			List<UniTask> tasks = new();
			switch (task.Valuable)
			{
				case CConsumableValuable consumable:
					tasks.Add(SendConsumableGainParticles(consumable, ct, task.ModifyParams));
					break;
				case CXpValuable xp:
					tasks.Add(SendXpGainParticles(xp, ct));
					break;
				case CEventCoinValuable eventCoin:
					tasks.Add(SendEventCoinGainParticles(eventCoin, ct, task.ModifyParams));
					break;
				case CFrameValuable frameValuable:
					tasks.Add(SendFrameGainParticles(frameValuable, ct));
					break;
				case CEventPointValuable eventPoint:
					SendEventPointGainParticles(eventPoint, ct, task.ModifyParams).Forget();
					break;
			}
			await UniTask.WhenAll(tasks);
		}
		
		private async Task OnGainResource(CGainResourceParticleTask task, CancellationToken ct)
		{
			await SendResourceGainParticles(task.Resource, ct, task.ModifyParams);
		}

		private async UniTask SendConsumableGainParticles(CConsumableValuable consumable, CancellationToken ct, CValueModifyParams modifyParams)
		{
			modifyParams ??= new CValueModifyParams();
			
			RectTransform source = _particlesSourceProvider.GetRectTransform(consumable.Id);
			_eventBus.ProcessTaskAsync(new CRunCurrencyPopupNumberTask(source, consumable.Value), ct).Forget();
			
			CLockObject lockObject = new("ParticlesHandler.SendGainParticles");
			_user.AnimatedCurrencies.StartAnimating(consumable.Id, lockObject);
			_user.OwnedValuables.ModifyValuableInternal(consumable, modifyParams.Add(new CAnimatedChangeParam()));
			await _eventBus.ProcessTaskAsync(new CRunConsumableSheetParticleTask(source, consumable), ct);
			_user.AnimatedCurrencies.StopAnimating(consumable.Id, lockObject);
		}
		
		private async UniTask SendEventCoinGainParticles(CEventCoinValuable eventCoin, CancellationToken ct, CValueModifyParams modifyParams)
		{
			modifyParams ??= new CValueModifyParams();
			
			RectTransform source = _particlesSourceProvider.GetRectTransform(eventCoin.Id);
			_eventBus.ProcessTaskAsync(new CRunCurrencyPopupNumberTask(source, eventCoin.Amount), ct).Forget();
			
			CLockObject lockObject = new("ParticlesHandler.SendGainParticles");
			_user.AnimatedCurrencies.StartAnimating(eventCoin.Id, lockObject);
			_user.OwnedValuables.ModifyValuableInternal(eventCoin, modifyParams.Add(new CAnimatedChangeParam()));
			await _eventBus.ProcessTaskAsync(new CRunEventCoinSheetParticleTask(source, eventCoin), ct);
			_user.AnimatedCurrencies.StopAnimating(eventCoin.Id, lockObject);
		}
		
		private async UniTask SendEventPointGainParticles(CEventPointValuable eventPoint, CancellationToken ct, CValueModifyParams modifyParams)
		{
			modifyParams ??= new CValueModifyParams();
			
			RectTransform source = _particlesSourceProvider.GetRectTransform(eventPoint.Id);
			_eventBus.ProcessTaskAsync(new CRunCurrencyPopupNumberTask(source, eventPoint.Amount), ct).Forget();
			
			CLockObject lockObject = new("ParticlesHandler.SendGainParticles");
			_user.AnimatedCurrencies.StartAnimating(eventPoint.Id, lockObject);
			_user.OwnedValuables.ModifyValuableInternal(eventPoint, modifyParams.Add(new CAnimatedChangeParam()));
			await _eventBus.ProcessTaskAsync(new CRunEventPointSheetParticleTask(source, eventPoint), ct);
			_user.AnimatedCurrencies.StopAnimating(eventPoint.Id, lockObject);
		}
		
		private async UniTask SendResourceGainParticles(CResourceValuable resource, CancellationToken ct, CValueModifyParams modifyParams)
		{
			modifyParams ??= new CValueModifyParams();
			
			RectTransform source = _particlesSourceProvider.GetRectTransform(resource.Id);
			 _eventBus.ProcessTaskAsync(new CRunCurrencyPopupNumberTask(source, resource.Resource.Amount), ct).Forget();
			
			 CLockObject lockObject = new("ParticlesHandler.SendGainParticles");
			 _user.AnimatedCurrencies.StartAnimating(resource.Resource.Id, lockObject);
			Task particleTask = _eventBus.ProcessTaskAsync(new CRunResourceSheetParticleTask(source, resource.Resource), ct);
			_user.OwnedValuables.ModifyValuableInternal(resource, modifyParams.Add(new CAnimatedChangeParam()));
			 await particleTask;
			 _user.AnimatedCurrencies.StopAnimating(resource.Resource.Id, lockObject);
		}

		private async UniTask SendFrameGainParticles(CFrameValuable frameValuable, CancellationToken ct)
		{
			RectTransform source = _particlesSourceProvider.GetRectTransform(frameValuable.Id);
			await _eventBus.ProcessTaskAsync(new CRunFrameSheetParticleTask(source, frameValuable.Frame), ct);
		}

		private async UniTask SendXpGainParticles(CXpValuable xp, CancellationToken ct)
		{
			RectTransform source = _particlesSourceProvider.GetRectTransform(EValuable.Xp);
			_eventBus.ProcessTaskAsync(new CRunCurrencyPopupNumberTask(source, xp.Amount), ct).Forget();
			
			await _eventBus.ProcessTaskAsync(new CRunXpSheetParticleTask(source, xp.Amount), ct);
		}
	}
}