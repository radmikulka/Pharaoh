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
using Pharaoh.Ui;
using ServerData;
using Pharaoh;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
    public class CUiParticlesManager : MonoBehaviour, IInitializable
    {
        [SerializeField] private CUiParticlesPool _gainPool;
        [SerializeField] private CAudioClip _particleStartSound;
        [SerializeField] private CAudioClip _particleFinishSound;

        private CUiCurrencyParticles _uiCurrencies;
        private CUiParticleCounts _particleCounts;
        private IAudioManager _audioManager;
        private IEventBus _eventBus;
        private CUser _user;

        [Inject]
        private void Inject(
            CUiParticleCounts particleCounts, 
            CResourceConfigs resourceConfigs, 
            CUiCurrencyParticles uiCurrencies, 
            IBundleManager bundleManager, 
            IAudioManager audioManager, 
            IEventBus eventBus, 
            CUser user
            )
        {
            _particleCounts = particleCounts;
            _uiCurrencies = uiCurrencies;
            _audioManager = audioManager;
            _eventBus = eventBus;
            _user = user;
        }

        public void Initialize()
        {
            _eventBus.AddAsyncTaskHandler<CRunSheetParticleTask>(RunParticle);
            _eventBus.AddAsyncTaskHandler<CRunConsumableSheetParticleTask>(RunConsumableParticle);
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

            _audioManager.PlaySound2D(_particleStartSound);
                
            await RunParticle(uiCurrency.ParticleId, task.Start, currencyPoint, particlesCount, step =>
            {
                float progress = (float)step / particlesCount;
                int currencyAmount = CMath.CeilToInt(task.Currency.Value * progress);
                int diff = currencyAmount - previousAmount;
                previousAmount = currencyAmount;
                _user.AnimatedCurrencies.AddCurrency(CValuableFactory.Consumable(task.Currency.Id, diff));

                _audioManager.PlaySound2D(_particleFinishSound);

            }, ct);
            
            _user.AnimatedCurrencies.StopAnimating(task.Currency.Id, lockObject);
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