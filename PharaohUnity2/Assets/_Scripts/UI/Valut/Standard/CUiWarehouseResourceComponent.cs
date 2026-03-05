// =========================================
// AUTHOR: Marek Karaba
// DATE:   25.09.2025
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;
using TycoonBuilder.Configs;
using TycoonBuilder.Ui;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CUiWarehouseResourceComponent : MonoBehaviour, IScreenCloseEnd, IAldaFrameworkComponent, IRequirementVisual
	{
		[SerializeField] private CUiComponentImage _iconImage;
		[SerializeField] private CUiComponentText _countText;

		private CUiGlobalsConfig _uiGlobalsConfig;
		private CResourceConfigs _resourceConfigs;
		private IBundleManager _bundleManager;
		private IServerTime _serverTime;
		private IEventBus _eventBus;
		private CUser _user;
		
		private Guid _valueChangeSubscriptionId;
		private EResource _activeResourceId;
		private int _currentCount;
		private int _requiredCount;
		private bool _checkIfEnoughInWarehouse;

		[Inject]
		private void Inject(
			CUiGlobalsConfig uiGlobalsConfig,
			CResourceConfigs resourceConfigs,
			IBundleManager bundleManager,
			IServerTime serverTime,
			IEventBus eventBus,
			CUser user)
		{
			_uiGlobalsConfig = uiGlobalsConfig;
			_resourceConfigs = resourceConfigs;
			_bundleManager = bundleManager;
			_serverTime = serverTime;
			_eventBus = eventBus;
			_user = user;
		}
		
		public void SetWarehouseResource(EResource resourceId, int amountNeeded = 0)
		{
			_checkIfEnoughInWarehouse = amountNeeded > 0;
			_requiredCount = amountNeeded;
			_activeResourceId = resourceId;
			BindToValueChange();
			
			SetCorrectValues();
		}

		private void SetCorrectValues()
		{
			SetCurrentCount();
			SetTextColor();
			SetIcon();
		}

		private void BindToValueChange()
		{
			TryUnsubscribeValueChange();
			_valueChangeSubscriptionId = _eventBus.Subscribe<CWarehouseResourceChangedSignal>(OnWarehouseResourceChanged);
		}
		
		private void OnWarehouseResourceChanged(CWarehouseResourceChangedSignal signal)
		{
			if (signal.NewValue.Id != _activeResourceId)
				return;

			SetCorrectValues();
		}

		private void SetTextColor()
		{
			Color color = _uiGlobalsConfig.EnoughCurrencyColor;
			if (_checkIfEnoughInWarehouse)
			{
				bool haveEnoughResource = HaveEnoughResource(_activeResourceId, _requiredCount);
				color = haveEnoughResource ? _uiGlobalsConfig.EnoughCurrencyColor : _uiGlobalsConfig.NotEnoughCurrencyColor;
			}
			_countText.SetColor(color, false);
		}

		private bool HaveEnoughResource(EResource resourceId, int requiredAmount)
		{
			if (resourceId == EResource.Passenger)
			{
				int availableAmount = GetAvailablePassengers();
				return availableAmount >= requiredAmount;
			}
			return _user.Warehouse.HaveResource(new SResource(resourceId, requiredAmount));
		}
		
		private int GetAvailablePassengers()
		{
			long currentTime = _serverTime.GetTimestampInMs();
			CObservableRecharger populationGenerator = _user.City.GetPassengersGenerator(currentTime);
			int currentAmount = populationGenerator.CurrentAmount;
			return currentAmount;
		}

		private void SetCurrentCount()
		{
			if (_activeResourceId == EResource.Passenger)
			{
				_currentCount = GetAvailablePassengers();
				_countText.SetValue(_currentCount, new CNumberFormatter());
				return;
			}

			_currentCount = GetCurrentCount();
			_countText.SetValue(_currentCount, new CNumberFormatter());
		}

		private int GetCurrentCount()
		{
			int currentCount;
			if (_activeResourceId == EResource.Passenger)
			{
				currentCount = GetAvailablePassengers();
			}
			else
			{
				currentCount = _user.Warehouse.GetResourceAmount(_activeResourceId);
			}
			return currentCount;
		}

		private void SetIcon()
		{
			CResourceResourceConfig config = _resourceConfigs.Resources.GetConfig(_activeResourceId);
			Sprite sprite = _bundleManager.LoadItem<Sprite>(config.Sprite);
			_iconImage.SetSprite(sprite);
		}

		public void OnScreenCloseEnd()
		{
			TryUnsubscribeValueChange();
		}
		
		private void TryUnsubscribeValueChange()
		{
			_eventBus.Unsubscribe(_valueChangeSubscriptionId);
		}

		public bool IsRequirementSatisfied()
		{
			if(!_checkIfEnoughInWarehouse)
				return true;
			
			if(_requiredCount <= 0)
				return true;
			
			return _currentCount >= _requiredCount;
		}
	}
}