// =========================================
// AUTHOR: Juraj Joscak
// DATE:   22.07.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using ServiceEngine.Purchasing;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CRealMoneyValuableComponent : CUiValuableComponent<CRealMoneyValuable>, IInitializable
	{
		[SerializeField] private CUiComponentText _text;

		private IPurchasing _purchasing;
		private IPricesParser _pricesParser;
		private CInAppPrices _inAppPrices;
		private IEventBus _eventBus;
		private ITranslation _translation;
		
		private CRealMoneyValuable _value;
		
		[Inject]
		private void Inject(IPurchasing purchasing, IPricesParser pricesParser, CInAppPrices inAppPrices, IEventBus eventBus, ITranslation translation)
		{
			_purchasing = purchasing;
			_pricesParser = pricesParser;
			_inAppPrices = inAppPrices;
			_eventBus = eventBus;
			_translation = translation;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CPurchasingInitializedSignal>(OnPurchasingInitialized);
		}
		
		protected override void SetValue(CRealMoneyValuable value)
		{
			_value = value;
			string productId = _inAppPrices.GetStoreId(value.Price);
			
			if (CPlatform.IsEditor)
			{
				float price = _pricesParser.TryGetPriceOrLogError(productId, CDebugConfig.Instance.CurrencyToDebug);
				_text.SetValue($"{price} {CDebugConfig.Instance.CurrencyToDebug.ToString()}");
			}
			else
			{
				if (!_purchasing.IsInitialized)
				{
					_text.SetValue(_translation.GetText("ShopMenu.TapForMoreInfo"));
					return;
				}
				
				var metadata = _purchasing.GetProductMetadata(productId);
				_text.SetValue($"{metadata.localizedPrice} {metadata.isoCurrencyCode}");
			}
		}

		private void OnPurchasingInitialized(CPurchasingInitializedSignal signal)
		{
			SetValue(_value);
		}

		protected override bool IsValidValuable(IValuable valuable)
		{
			return valuable is CRealMoneyValuable;
		}
	}
}