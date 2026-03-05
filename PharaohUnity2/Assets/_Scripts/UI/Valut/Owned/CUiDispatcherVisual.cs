// =========================================
// AUTHOR: Marek Karaba
// DATE:   14.01.2026
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using ServerData;
using TycoonBuilder.Configs;
using TycoonBuilder.Ui.DispatchMenu;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CUiDispatcherVisual : ValidatedMonoBehaviour, IInitializable
	{
		[SerializeField] private CUiComponentText _valueText;

		private CUiGlobalsConfig _globalsConfig;
		private IServerTime _serverTime;
		private IEventBus _eventBus;
		private CUser _user;

		private bool _expirableDispatcherExists;
		
		[Inject]
		private void Inject(
			CUiGlobalsConfig globalsConfig,
			IServerTime serverTime,
			IEventBus eventBus,
			CUser user)
		{
			_globalsConfig = globalsConfig;
			_serverTime = serverTime;
			_eventBus = eventBus;
			_user = user;
		}
		
		public void Initialize()
		{
			_eventBus.Subscribe<COwnedValuableChangedSignal>(OnOwnedValuableChanged);
			_eventBus.Subscribe<CVehicleStateChangedSignal>(OnVehicleStateChanged);
			_eventBus.Subscribe<CDispatchCompletedSignal>(OnDispatchCompleted);
			_eventBus.Subscribe<CVehicleSendSignal>(OnVehicleSend);
			
			UpdateValue();
		}

		private void Update()
		{
			if (!_expirableDispatcherExists)
				return;
			
			UpdateValue();
		}

		private void OnOwnedValuableChanged(COwnedValuableChangedSignal signal)
		{
			if (signal.Valuable.Id != EValuable.Dispatcher)
				return;
			
			UpdateValue();
		}

		private void OnVehicleStateChanged(CVehicleStateChangedSignal signal)
		{
			UpdateValue();
		}

		private void OnDispatchCompleted(CDispatchCompletedSignal signal)
		{
			UpdateValue();
		}
		
		private void OnVehicleSend(CVehicleSendSignal signal)
		{
			UpdateValue();
		}

		private void UpdateValue()
		{
			_expirableDispatcherExists = _user.Dispatchers.ExpirableDispatcherExists(_serverTime.GetTimestampInMs());
			
			int currentAmount = CurrentAmount();
			int maxCapacity = GetMaxCapacity();
			_valueText.SetValue($"{currentAmount} / {maxCapacity}");
			SetCorrectColor(currentAmount, maxCapacity);
		}

		private void SetCorrectColor(int currentAmount, int maxCapacity)
		{
			bool isMaxCapacity = currentAmount >= maxCapacity;
			Color color = isMaxCapacity ? _globalsConfig.NotEnoughCurrencyColor : _globalsConfig.EnoughCurrencyColor;
			_valueText.SetColor(color, false);
		}

		private int CurrentAmount()
		{
			int usedDispatchers = _user.Dispatchers.GetUsedDispatchersCount();
			return usedDispatchers;
		}
		
		private int GetMaxCapacity()
		{
			int dispatcherCapacity = _user.Dispatchers.GetActiveDispatchersCount(_serverTime.GetTimestampInMs());
			return dispatcherCapacity;
		}
	}
}