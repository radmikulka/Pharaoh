// =========================================
// AUTHOR: Marek Karaba
// DATE:   21.07.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CPopUpNumberManager : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField, Self] private CPopUpNumberFactory _factory;
		
		private IEventBus _eventBus;

		private readonly List<CPopUpNumber> _pool = new();
		
		[Inject]
		private void Inject(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}
		
		public void Initialize()
		{
			_eventBus.AddAsyncTaskHandler<CRunCurrencyPopupNumberTask>(OnRunCurrencyPopupNumber);
		}

		private Task OnRunCurrencyPopupNumber(CRunCurrencyPopupNumberTask task, CancellationToken ct)
		{
			ShowNumber(task.Number, task.Source, ct).Forget();
			return Task.CompletedTask;
		}
		
		private async UniTask ShowNumber(int count, RectTransform source, CancellationToken ct)
		{
			CPopUpNumber popUpNumber = GetPopUpNumberFromPool();
			popUpNumber.gameObject.SetActive(true);
			popUpNumber.RunFixedPosition(count, source);

			await UniTask.WaitUntil(() => popUpNumber.IsAvailable, cancellationToken: ct);
			popUpNumber.gameObject.SetActive(false);
		}
		
		private CPopUpNumber GetPopUpNumberFromPool()
		{
			foreach (CPopUpNumber number in _pool.Where(number => number.IsAvailable))
			{
				return number;
			}

			CPopUpNumber newNumber = _factory.Create(transform);
			_pool.Add(newNumber);
			return newNumber;
		}
	}
}