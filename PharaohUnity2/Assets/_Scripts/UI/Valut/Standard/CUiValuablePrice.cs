// =========================================
// AUTHOR: Juraj Joscak
// DATE:   23.07.2025
// =========================================

using System;
using AldaEngine;
using RoboRyanTron.SearchableEnum;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CUiValuablePrice : CUiValuable, IRequirementVisual
	{
		[SerializeField] private bool _partialPaymentAllowed;
		
		private IPurchasing _purchasing;
		private IEventBus _eventBus;

		public IValuable Price => Valuable;

		[Inject]
		private void Inject(IPurchasing purchasing, IEventBus eventBus)
		{
			_purchasing = purchasing;
			_eventBus = eventBus;
		}

		protected override IValuable PreProcessInputValuable(IValuable valuable)
		{
			if (valuable is CConsumableValuable consumable)
			{
				return CalculateCorrectPriceFromPositiveValuable(consumable);
			}
			return base.PreProcessInputValuable(valuable);
		}

		private IValuable CalculateCorrectPriceFromPositiveValuable(CConsumableValuable consumable)
		{
			CConsumableValuable priceValuable = consumable.Reverse();
			CConsumableValuable modifiedPrice = (CConsumableValuable) ValuableRegionModifier.ModifyValuable(priceValuable, User.Progress.Region, _modificationSource);
			return modifiedPrice.Reverse();
		}

		public bool TryShowUnaffordableTooltip()
		{
			if(CanAfford())
				return false;

			switch (Valuable)
			{
				case CRealMoneyValuable:
					_eventBus.ProcessTask(new CShowTooltipTask(CPlatform.IsIosPlatform ? "Purchasing.Ios.NotInitialized" : "Purchasing.Android.NotInitialized", true));
					break;
				case CEventCoinValuable:
				case CConsumableValuable:
					_eventBus.ProcessTask(new CShowTooltipTask("Generic.NotEnoughCurrency", true));
					break;
				case CResourceValuable:
					_eventBus.ProcessTask(new CShowTooltipTask("Generic.NotEnoughResources", true));
					break;
			}

			return true;
		}

		public bool CanAfford()
		{
			if(Valuable == null)
				return true;
			
			switch (Valuable)
			{
				case CAdvertisementValuable:
				case CFreeValuable:
				case CFreeNoHitValuable:
				case CStaticOfferValuable:
					return true;
				case CRealMoneyValuable:
					return _purchasing.IsInitialized;
				case CConsumableValuable consumableValuable:
					return User.OwnedValuables.HaveValuable(_partialPaymentAllowed ? CValuableFactory.Consumable(consumableValuable.Id, 1) : consumableValuable);
				case CResourceValuable resourceValuable:
					return User.Warehouse.HaveResource(_partialPaymentAllowed ? new SResource(resourceValuable.Resource.Id, 1) : resourceValuable.Resource);
				case CEventCoinValuable eventCoinValuable:
					return User.LiveEvents.HaveEventCoin(eventCoinValuable);
				default:
					throw new ArgumentOutOfRangeException(nameof(Valuable));
			}
		}

		public bool IsRequirementSatisfied()
		{
			if (Valuable == null)
				return true;
			
			return CanAfford();
		}
	}
}