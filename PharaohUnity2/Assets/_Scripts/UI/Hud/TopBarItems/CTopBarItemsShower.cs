// =========================================
// AUTHOR: Juraj Joscak
// DATE:   25.09.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public enum ETopBarItem
	{
		None = EValuable.None,
		SoftCurrency = EValuable.SoftCurrency,
		HardCurrency = EValuable.HardCurrency,
		RealMoney = EValuable.RealMoney,
		Free = EValuable.Free,
		Advertisement = EValuable.Advertisement,
		Vehicle = EValuable.Vehicle,
		StaticOffer = EValuable.StaticOffer,
		Resource = EValuable.Resource,
		Fuel = EValuable.Fuel,
		Null = EValuable.Null,
		FreeNoHit = EValuable.FreeNoHit,
		Xp = EValuable.Xp,
		CityBlueprint = EValuable.CityBlueprint,
		CityPlan = EValuable.CityPlan,
		FuelPart = EValuable.FuelPart,
		DurabilityPart = EValuable.DurabilityPart,
		CapacityPart = EValuable.CapacityPart,
		AdvancedCapacityPart = EValuable.AdvancedCapacityPart,
		MachineOil = EValuable.MachineOil,
		Wrenche = EValuable.Wrenche,
		Dispatcher = EValuable.Dispatcher,
		Building = EValuable.Building,
		EventCoin = EValuable.EventCoin,
		EventPoint = EValuable.EventPoint,
		Frame = EValuable.Frame,
		
		// 1000+ Resources
		Passenger = 1000 + EResource.Passenger,
	}
	
	public class CTopBarItemsShower : ValidatedMonoBehaviour, IInitializable, IConstructable
	{
		[SerializeField, Self] private CUiTopBarItemsLayout _layout;
		
		private float ParticlesFadeOutDelay => 1f;
		
		private CUiTopBarItemFader[] _visuals;
		
		private IScreenManager _menuManager;
		private IEventBus _eventBus;
		private ICtsProvider _ctsProvider;
		
		private CTopBarItemsUpperHolder _upperHolder;

		private readonly ITopBarItem[] _baseCurrencies = 
		{
			new CTopBarItem(ETopBarItem.HardCurrency, true),
			new CTopBarItem(ETopBarItem.SoftCurrency, true),
			new CTopBarItem(ETopBarItem.Fuel, true),
		};

		private ITopBarItem[] _currentMenuTopBarItems;
		private readonly Dictionary<ETopBarItem, CCurrencyLock> _currencyLocks = new();
		
		[Inject]
		private void Inject(IScreenManager menuManager, CTopBarItemsUpperHolder currenciesUpperHolder, IEventBus eventBus, ICtsProvider ctsProvider)
		{
			_menuManager = menuManager;
			_upperHolder = currenciesUpperHolder;
			_eventBus = eventBus;
			_ctsProvider = ctsProvider;
		}

		public void Construct()
		{
			_visuals = GetComponentsInChildren<CUiTopBarItemFader>(true);
		}
		
		public void Initialize()
		{
			_currentMenuTopBarItems = _baseCurrencies;
			_menuManager.MenuOpenStart.Subscribe(OnMenuOpenStart);
			_menuManager.MenuCloseStart.Subscribe(OnMenuCloseStart);
			_eventBus.Subscribe<CCurrencyParticleStartedSignal>(OnCurrencyParticleStarted);
			_eventBus.Subscribe<CCurrencyParticleFinishedSignal>(OnCurrencyParticleFinished);
			
			transform.SetParent(_upperHolder.Transform, true);

			AddLocks(_currentMenuTopBarItems);
			foreach (CUiTopBarItemFader visual in _visuals)
			{
				visual.SetVisible(_currencyLocks.Any(item => item.Key == visual.Id));
			}
			RepositionLayout();
		}
		
		private void OnMenuOpenStart(IScreen menu)
		{
			RemoveLocks(_currentMenuTopBarItems);
			_currentMenuTopBarItems = (menu as IHaveTopBarItems)?.GetTopBarItems() ?? Array.Empty<ITopBarItem>();
			AddLocks(_currentMenuTopBarItems);
			Fade();
		}
		
		private void OnMenuCloseStart(IScreen menu)
		{
			RemoveLocks(_currentMenuTopBarItems);
			if (_menuManager.ActiveMenus.Count == 1)
			{
				_currentMenuTopBarItems = _baseCurrencies;
			}
			else
			{
				IScreen topMostMenu = _menuManager.GetTopMostMenuOrDefault();
				_currentMenuTopBarItems = (topMostMenu as IHaveTopBarItems)?.GetTopBarItems() ?? Array.Empty<CTopBarItem>();
			}
			AddLocks(_currentMenuTopBarItems);
			Fade();
		}
		
		private void Fade()
		{
			foreach (CUiTopBarItemFader visual in _visuals)
			{
				CCurrencyLock data = _currencyLocks.FirstOrDefault(item => item.Key == visual.Id && item.Value.LockCount > 0).Value;
				if (data != null)
				{
					visual.FadeIn(data.ShowButtonLocks > 0);
				}
				else
				{
					visual.FadeOut();
				}
			}

			RepositionLayout();
		}
		
		private void OnCurrencyParticleStarted(CCurrencyParticleStartedSignal signal)
		{
			AddLock((ETopBarItem)signal.CurrencyType, false);
			Fade();
		}
		
		private void OnCurrencyParticleFinished(CCurrencyParticleFinishedSignal signal)
		{
			CurrencyParticleFinishedAsync(signal.CurrencyType, _ctsProvider.Token).Forget();
		}

		private async UniTaskVoid CurrencyParticleFinishedAsync(EValuable currencyType, CancellationToken ct)
		{
			await UniTask.WaitForSeconds(ParticlesFadeOutDelay, cancellationToken: ct);
			RemoveLock((ETopBarItem)currencyType, false);
			Fade();
		}

		private void RepositionLayout()
		{
			_layout.Reposition
			(
			_currencyLocks
				.Where(item => item.Value.LockCount > 0)
				.Select(item => item.Key)
				.ToArray()
			);
		}

		private void AddLocks(ITopBarItem[] topBarItems)
		{
			foreach (ITopBarItem topBarItem in topBarItems)
			{
				AddLock(topBarItem.Id, topBarItem.ShowButton);
			}
		}
		
		private void AddLock(ETopBarItem valuableId, bool showButton)
		{
			if (_currencyLocks.TryGetValue(valuableId, out CCurrencyLock lockData))
			{
				lockData.LockCount++;
				lockData.ShowButtonLocks += showButton ? 1 : 0;
			}
			else
			{
				_currencyLocks[valuableId] = new CCurrencyLock(1, showButton ? 1 : 0);
			}
		}
		
		private void RemoveLocks(ITopBarItem[] topBarItems)
		{
			foreach (ITopBarItem topBarItem in topBarItems)
			{
				RemoveLock(topBarItem.Id, topBarItem.ShowButton);
			}
		}
		
		private void RemoveLock(ETopBarItem currency, bool showButton)
		{
			if (_currencyLocks.TryGetValue(currency, out CCurrencyLock lockData))
			{
				lockData.LockCount = Math.Max(0, lockData.LockCount - 1);
			}
		}
		
		private class CCurrencyLock
		{
			public int LockCount;
			public int ShowButtonLocks;
			
			public CCurrencyLock(int lockCount, int showButtonLockCount)
			{
				LockCount = lockCount;
				ShowButtonLocks = showButtonLockCount;
			}
		}
	}
}