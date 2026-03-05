// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;
using TycoonBuilder;
using TycoonBuilder.Ui;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
    public class CUiParticlesManager : MonoBehaviour, IInitializable
    {
        [SerializeField] private CUiParticlesPool _gainPool;
        [SerializeField] private CAudioClip _particleStartSound;
        [SerializeField] private CAudioClip _particleFinishSound;

        private CUiCurrencyParticles _uiCurrencies;
        private IEventBus _eventBus;
        private CUser _user;
        private IAudioManager _audioManager;
        private CResourceConfigs _resourceConfigs;
        private IBundleManager _bundleManager;
        private CUiParticleCounts _particleCounts;

        [Inject]
        private void Inject(IEventBus eventBus, CUiCurrencyParticles uiCurrencies, CUser user, IAudioManager audioManager, CResourceConfigs resourceConfigs, IBundleManager bundleManager, CUiParticleCounts particleCounts)
        {
            _uiCurrencies = uiCurrencies;
            _eventBus = eventBus;
            _user = user;
            _audioManager = audioManager;
            _resourceConfigs = resourceConfigs;
            _bundleManager = bundleManager;
            _particleCounts = particleCounts;
        }

        public void Initialize()
        {
            _eventBus.AddAsyncTaskHandler<CRunSheetParticleTask>(RunParticle);
            _eventBus.AddAsyncTaskHandler<CRunConsumableSheetParticleTask>(RunConsumableParticle);
            _eventBus.AddAsyncTaskHandler<CRunEventCoinSheetParticleTask>(RunEventCoinParticle);
            _eventBus.AddAsyncTaskHandler<CRunResourceSheetParticleTask>(RunResourceParticle);
            _eventBus.AddAsyncTaskHandler<CRunFrameSheetParticleTask>(RunFrameParticle);
            _eventBus.AddAsyncTaskHandler<CRunXpSheetParticleTask>(RunXpParticle);
            _eventBus.AddAsyncTaskHandler<CRunEventPointSheetParticleTask>(RunEventPointParticle);
        }

        private async Task RunConsumableParticle(CRunConsumableSheetParticleTask task, CancellationToken ct)
        {
            CLockObject lockObject = new ("RunCurrencyParticle");
            _user.AnimatedCurrencies.StartAnimating(task.Currency.Id, lockObject);
            
            CUiCurrencyParticlePoint uiCurrency = _uiCurrencies.GetCurrency(task.Currency.Id);
            RectTransform currencyPoint = uiCurrency.RectTransform;
            
            int particlesCount = _particleCounts.GetCount(task.Currency.Id, task.Currency.Value);
            int previousAmount = 0;

            Guid guid = Guid.NewGuid();
            _eventBus.Send(new CCurrencyParticleStartedSignal(guid, task.Currency.Id, EResource.None));

            _audioManager.PlaySound2D(_particleStartSound);
                
            await RunParticle(uiCurrency.ParticleId, task.Start, currencyPoint, particlesCount, step =>
            {
                float progress = (float)step / particlesCount;
                int currencyAmount = CMath.CeilToInt(task.Currency.Value * progress);
                int diff = currencyAmount - previousAmount;
                previousAmount = currencyAmount;
                _user.AnimatedCurrencies.AddCurrency(CValuableFactory.Consumable(task.Currency.Id, diff));
                _eventBus.Send(new CCurrencyParticleStepFinishedSignal(guid, new CConsumableValuable(task.Currency.Id, diff)));

                _audioManager.PlaySound2D(_particleFinishSound);

            }, ct);
            
            _eventBus.Send(new CCurrencyParticleFinishedSignal(guid, task.Currency.Id, EResource.None));
            _user.AnimatedCurrencies.StopAnimating(task.Currency.Id, lockObject);
        }

        private async Task RunEventCoinParticle(CRunEventCoinSheetParticleTask task, CancellationToken ct)
        {
            CLockObject lockObject = new ("RunCurrencyParticle");
            _user.AnimatedCurrencies.StartAnimating(task.Currency.Id, lockObject);
            
            CUiCurrencyParticlePoint uiCurrency = _uiCurrencies.GetCurrency(task.Currency.Id);
            RectTransform currencyPoint = uiCurrency.RectTransform;
            
            int particlesCount = _particleCounts.GetCount(task.Currency.Id, task.Currency.Amount);
            int previousAmount = 0;

            Guid guid = Guid.NewGuid();
            _eventBus.Send(new CCurrencyParticleStartedSignal(guid, task.Currency.Id, EResource.None));

            _audioManager.PlaySound2D(_particleStartSound);

            CLiveEventResourceConfig liveEventCfg = _resourceConfigs.LiveEvents.GetConfig(task.Currency.LiveEvent);
            Sprite coinSprite = _bundleManager.LoadItem<Sprite>(liveEventCfg.EventCoinsIconSprite, EBundleCacheType.Persistent);
            
            await RunParticle(uiCurrency.ParticleId, task.Start, currencyPoint, particlesCount, step =>
            {
                float progress = (float)step / particlesCount;
                int currencyAmount = CMath.CeilToInt(task.Currency.Amount * progress);
                int diff = currencyAmount - previousAmount;
                previousAmount = currencyAmount;
                _user.AnimatedCurrencies.AddCurrency(CValuableFactory.Consumable(task.Currency.Id, diff));
                _eventBus.Send(new CCurrencyParticleStepFinishedSignal(guid, new CConsumableValuable(task.Currency.Id, diff)));

                _audioManager.PlaySound2D(_particleFinishSound);

            }, ct, coinSprite);
            
            _eventBus.Send(new CCurrencyParticleFinishedSignal(guid, task.Currency.Id, EResource.None));
            _user.AnimatedCurrencies.StopAnimating(task.Currency.Id, lockObject);
        }

        private async Task RunResourceParticle(CRunResourceSheetParticleTask task, CancellationToken ct)
        {
            CUiCurrencyParticlePoint uiCurrency = _uiCurrencies.GetCurrency(EValuable.Resource, task.Resource.Id);
            RectTransform currencyPoint = uiCurrency.RectTransform;
            
            int particlesCount = _particleCounts.GetCount(EValuable.Resource, task.Resource.Amount);
            int previousAmount = 0;

            Guid guid = Guid.NewGuid();
            _eventBus.Send(new CCurrencyParticleStartedSignal(guid, EValuable.Resource, task.Resource.Id));

            _audioManager.PlaySound2D(_particleStartSound);

            await RunParticle(uiCurrency.ParticleId, task.Start, currencyPoint, particlesCount, step =>
            {
                float progress = (float)step / particlesCount;
                int currencyAmount = CMath.CeilToInt(task.Resource.Amount * progress);
                int diff = currencyAmount - previousAmount;
                previousAmount = currencyAmount;
                _user.AnimatedCurrencies.AddResource(task.Resource.Id, diff);
                _eventBus.Send(new CCurrencyParticleStepFinishedSignal(guid, new CResourceValuable(task.Resource.Id, diff)));

                _audioManager.PlaySound2D(_particleFinishSound);

            }, ct, GetResourceSprite(task.Resource.Id));
            
            _eventBus.Send(new CCurrencyParticleFinishedSignal(guid, EValuable.Resource, task.Resource.Id));
        }

        private async Task RunFrameParticle(CRunFrameSheetParticleTask task, CancellationToken ct)
        {
            CUiCurrencyParticlePoint uiCurrency = _uiCurrencies.GetCurrency(EValuable.Frame);
            RectTransform currencyPoint = uiCurrency.RectTransform;
            
            int particlesCount = _particleCounts.GetCount(EValuable.Frame, 1);
            
            Guid guid = Guid.NewGuid();
            _eventBus.Send(new CCurrencyParticleStartedSignal(guid, EValuable.Frame, EResource.None));
            _audioManager.PlaySound2D(_particleStartSound);

            Sprite frameSprite = GetFrameSprite(task.FrameId);
            await RunParticle(uiCurrency.ParticleId, task.Start, currencyPoint, particlesCount, step =>
            {
                _eventBus.Send(new CCurrencyParticleStepFinishedSignal(guid, new CFrameValuable(task.FrameId)));
                _audioManager.PlaySound2D(_particleFinishSound);

            }, ct, frameSprite);
            
            _eventBus.Send(new CCurrencyParticleFinishedSignal(guid, EValuable.Frame, EResource.None));
        }

        private async Task RunXpParticle(CRunXpSheetParticleTask task, CancellationToken ct)
        {
            CUiCurrencyParticlePoint uiCurrency = _uiCurrencies.GetCurrency(EValuable.Xp);
            RectTransform currencyPoint = uiCurrency.RectTransform;
            
            int particlesCount = _particleCounts.GetCount(EValuable.Xp, task.Amount);
            int previousAmount = 0;

            Guid guid = Guid.NewGuid();
            _eventBus.Send(new CCurrencyParticleStartedSignal(guid, EValuable.Xp, EResource.None));
            _user.Progress.AddXp(task.Amount);

            _audioManager.PlaySound2D(_particleStartSound);

            await RunParticle(uiCurrency.ParticleId, task.Start, currencyPoint, particlesCount, step =>
            {
                float progress = (float)step / particlesCount;
                int currencyAmount = CMath.CeilToInt(task.Amount * progress);
                int diff = currencyAmount - previousAmount;
                previousAmount = currencyAmount;
                _eventBus.Send(new CCurrencyParticleStepFinishedSignal(guid, new CXpValuable(diff)));

                _audioManager.PlaySound2D(_particleFinishSound);

            }, ct);
            
            _eventBus.Send(new CCurrencyParticleFinishedSignal(guid, EValuable.Xp, EResource.None));
        }

        private async Task RunEventPointParticle(CRunEventPointSheetParticleTask task, CancellationToken ct)
        {
            RectTransform currencyPoint =_uiCurrencies.GetLiveEvent(task.Currency.LiveEvent);
            
            int particlesCount = _particleCounts.GetCount(task.Currency.Id, task.Currency.Amount);
            int previousAmount = 0;

            Guid guid = Guid.NewGuid();
            _eventBus.Send(new CCurrencyParticleStartedSignal(guid, task.Currency.Id, EResource.None));

            _audioManager.PlaySound2D(_particleStartSound);

            CLiveEventResourceConfig liveEventCfg = _resourceConfigs.LiveEvents.GetConfig(task.Currency.LiveEvent);
            Sprite pointSprite = _bundleManager.LoadItem<Sprite>(liveEventCfg.EventPointsIconSprite, EBundleCacheType.Persistent);
            
            await RunParticle(EUiParticleId.EventPoint, task.Start, currencyPoint, particlesCount, step =>
            {
                float progress = (float)step / particlesCount;
                int currencyAmount = CMath.CeilToInt(task.Currency.Amount * progress);
                int diff = currencyAmount - previousAmount;
                previousAmount = currencyAmount;
                _user.AnimatedCurrencies.AddCurrency(CValuableFactory.Consumable(task.Currency.Id, diff));
                _eventBus.Send(new CCurrencyParticleStepFinishedSignal(guid, new CEventPointValuable(task.Currency.LiveEvent, diff)));

                _audioManager.PlaySound2D(_particleFinishSound);

            }, ct, pointSprite); 
        }

        private Sprite GetResourceSprite(EResource id)
        {
            CBundleLink resourceSprite = _resourceConfigs.Resources.GetConfig(id).Sprite;
            return _bundleManager.LoadItem<Sprite>(resourceSprite, EBundleCacheType.Persistent);
        }

        private Sprite GetFrameSprite(EProfileFrame frameId)
        {
            CProfileFrameConfig config = _resourceConfigs.ProfileFrames.GetConfig(frameId);
            return _bundleManager.LoadItem<Sprite>(config.Sprite, EBundleCacheType.Persistent);
        }

        private async Task RunParticle(CRunSheetParticleTask task, CancellationToken ct)
        {
            await RunParticle(task.ParticleId, task.Start, task.End, task.Count, task.OnParticleStepCompleted, ct);
        }
        
        private async UniTask RunParticle(
            EUiParticleId particleId, 
            RectTransform start, 
            RectTransform end, 
            int count, 
            Action<int> onParticleStepCompleted,
            CancellationToken ct,
            Sprite overrideSprite = null
            )
        {
            CUiSheetParticleModule particle = GetModule(particleId);

            bool isRunning = true;

            if (overrideSprite != null)
            {
                particle.SetSprite(overrideSprite, false, true);
            }
            
            particle.Emit(count, 0.1f, start.position, end, data =>
            {
                onParticleStepCompleted.Invoke(data.Step);
            }, _ =>
            {
                isRunning = false;
            });
            
            await UniTask.WaitUntil(() => !isRunning, cancellationToken: ct);
            
            ReturnParticle(particleId, particle);
        }
        
        private void ReturnParticle(EUiParticleId particleId, CUiSheetParticleModule particle)
        {
            _gainPool.Return(particleId, particle);
        }

        private CUiSheetParticleModule GetModule(EUiParticleId id)
        {
            return _gainPool.Get(id);
        }
    }
}