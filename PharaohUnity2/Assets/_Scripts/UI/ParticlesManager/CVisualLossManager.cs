// =========================================
// NAME: Marek Karaba
// DATE: 02.10.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CVisualLossManager : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField, Self] private CVisualLossFactory _factory;
		
		private CResourceConfigs _resourceConfigs;
		private IBundleManager _bundleManager;
		private ICtsProvider _ctsProvider;
		private IEventBus _eventBus;

		private readonly List<CVisualLoss> _pool = new();
		
		[Inject]
		private void Inject(
			CResourceConfigs resourceConfigs,
			IBundleManager bundleManager,
			ICtsProvider ctsProvider,
			IEventBus eventBus)
		{
			_resourceConfigs = resourceConfigs;
			_bundleManager = bundleManager;
			_ctsProvider = ctsProvider;
			_eventBus = eventBus;
		}
		
		public void Initialize()
		{
			_eventBus.AddTaskHandler<CRunVisualLossTask>(OnRunVisualLossTask);
			_eventBus.AddTaskHandler<CRunVisualLossCurrencyTask>(OnRunVisualLossCurrencyTask);
		}

		private void OnRunVisualLossCurrencyTask(CRunVisualLossCurrencyTask task)
		{
			CValuableResourceConfig config = _resourceConfigs.Valuables.GetConfig(task.ValuableId);
			Sprite icon = _bundleManager.LoadItem<Sprite>(config.Sprite);
			ShowVisual(task.Amount, task.Source, icon, _ctsProvider.Token).Forget();
		}

		private void OnRunVisualLossTask(CRunVisualLossTask task)
		{
			ShowVisual(task.Amount, task.Source, task.Icon, _ctsProvider.Token).Forget();
		}
		
		private async UniTask ShowVisual(int count, RectTransform source, Sprite sprite, CancellationToken ct)
		{
			CVisualLoss visualLoss = GetVisualFromPool();
			visualLoss.gameObject.SetActive(true);
			visualLoss.Run(count, source, sprite);

			await UniTask.WaitUntil(() => visualLoss.IsAvailable, cancellationToken: ct);
			visualLoss.gameObject.SetActive(false);
		}
		
		private CVisualLoss GetVisualFromPool()
		{
			foreach (CVisualLoss visualLoss in _pool.Where(number => number.IsAvailable))
			{
				return visualLoss;
			}

			CVisualLoss newVisualLoss = _factory.Create(transform);
			_pool.Add(newVisualLoss);
			return newVisualLoss;
		}
	}
}