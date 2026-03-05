// =========================================
// AUTHOR: Marek Karaba
// DATE:   04.12.2025
// =========================================

using System.Threading;
using AldaEngine;
using ServerData;
using TycoonBuilder.Configs;
using TycoonBuilder.Ui.DispatchMenu;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CUiDispatcherTopBarItem : CUiTopBarItem
	{
		[SerializeField] private CUiComponentText _valueText;

		private CUiGlobalsConfig _globalsConfig;
		private ICtsProvider _ctsProvider;
		private IServerTime _serverTime;
		private IEventBus _eventBus;
		private CUser _user;
		
		[Inject]
		private void Inject(
			CUiGlobalsConfig globalsConfig,
			ICtsProvider ctsProvider,
			IServerTime serverTime,
			IEventBus eventBus,
			CUser user
			)
		{
			_globalsConfig = globalsConfig;
			_ctsProvider = ctsProvider;
			_serverTime = serverTime;
			_eventBus = eventBus;
			_user = user;
		}
		
		public override void Initialize()
		{
			base.Initialize();
			_button.AddClickListener(OnButtonClicked);
			
			_eventBus.Subscribe<COwnedValuableChangedSignal>(OnOwnedValuableChanged);
			_eventBus.Subscribe<CVehicleStateChangedSignal>(OnVehicleStateChanged);
			_eventBus.Subscribe<CDispatchCompletedSignal>(OnDispatchCompleted);
			_eventBus.Subscribe<CVehicleSendSignal>(OnVehicleSend);
			
			UpdateValue();
		}

		private void OnButtonClicked()
		{
			_eventBus.ProcessTaskAsync(new COpenShopTask(EShopTab.Dispatchers, EValuable.None), _ctsProvider.Token);
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