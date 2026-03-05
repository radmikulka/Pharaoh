// =========================================
// AUTHOR: Juraj Joscak
// DATE:   24.07.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using UnityEngine;
using Zenject;

namespace TycoonBuilder.Ui
{
	public class CHudHiderControl : ValidatedMonoBehaviour, IInitializable, IConstructable
	{
		[SerializeField, Self] private CHudHider _mainHider;
		
		private IScreenManager _screenManager;
		private IEventBus _eventBus;
		
		private CTopBarItemsUpperHolder _upperCurrenciesHolder;
		private CHudHider _currenciesHider;

		[Inject]
		private void Inject(IScreenManager screenManager, IEventBus eventBus, CTopBarItemsUpperHolder upperCurrenciesHolder)
		{
			_screenManager = screenManager;
			_eventBus = eventBus;
			_upperCurrenciesHolder = upperCurrenciesHolder;
		}
		
		public void Construct()
		{
			_currenciesHider = _upperCurrenciesHolder.GetComponent<CHudHider>();
		}

		public void Initialize()
		{
			_eventBus.AddTaskHandler<CHudShowTask>(ProcessShowTask);
			_eventBus.AddTaskHandler<CHudHideTask>(ProcessHideTask);
			_eventBus.AddTaskHandler<CGetHudCanvasGroup, CUiComponentCanvasGroup>(ProcessGetCanvasGroupTask);
			_screenManager.MenuOpenStart.Subscribe(OnMenuOpen);
			_screenManager.MenuCloseStart.Subscribe(OnMenuClose);
		}
		
		private void ProcessShowTask(CHudShowTask task)
		{
			_mainHider.Show(task.Locker, task.Instant, 0);
			
			if (task.IncludeCurrencies)
			{
				_currenciesHider.Show(task.Locker, task.Instant, 0);
			}
		}
		
		private void ProcessHideTask(CHudHideTask task)
		{
			_mainHider.Hide(task.Locker, task.Instant);
			
			if (task.IncludeCurrencies)
			{
				_currenciesHider.Hide(task.Locker, task.Instant);
			}
		}
		
		private CUiComponentCanvasGroup ProcessGetCanvasGroupTask(CGetHudCanvasGroup task)
		{
			return _mainHider.CanvasGroup;
		}

		private void OnMenuOpen(IScreen menu)
		{
			_mainHider.Hide(menu, false);
		}

		private void OnMenuClose(IScreen menu)
		{
			_mainHider.Show(menu, false, 0);
		}
	}
}