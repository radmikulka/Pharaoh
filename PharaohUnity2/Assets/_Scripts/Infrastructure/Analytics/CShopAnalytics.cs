// =========================================
// AUTHOR: Juraj Joscak
// DATE:   18.11.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Text;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using TycoonBuilder.Infrastructure;
using UnityEngine;
using UnityEngine.Purchasing;

namespace TycoonBuilder
{
	public class CShopAnalytics : IInitializable
	{
		private readonly IAnalytics _analytics;
		private readonly IEventBus _eventBus;
		private readonly CUser _user;
		private readonly CInAppPrices _inAppPrices;
		private readonly IPurchasing _purchasing;
		private readonly IPricesParser _pricesParser;
		
		private readonly Dictionary<string, object> _cachedParams = new();
		
		public CShopAnalytics(IAnalytics analytics, IEventBus eventBus, CUser user, CInAppPrices inAppPrices, IPurchasing purchasing, IPricesParser pricesParser)
		{
			_analytics = analytics;
			_eventBus = eventBus;
			_user = user;
			_inAppPrices = inAppPrices;
			_purchasing = purchasing;
			_pricesParser = pricesParser;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<COfferClaimedSignal>(OnOfferClaimed);
		}
		
		public void OfferShow(string offerId)
		{
			_cachedParams.Clear();
			
			COffer offerData = _user.Offers.GetOffer(offerId);
			_cachedParams.Add("AnalyticsId", offerData.GetAnalyticsId());
			_cachedParams.Add("Currency", GetOfferPriceCurrency(offerData.Price));
			_cachedParams.Add("Price", GetOfferPriceValue(offerData.Price));
			_cachedParams.Add("Content", GetOfferRewardsString(offerData));
			
			_analytics.SendData("OfferShow", _cachedParams);
		}
		
		private void OfferClaim(string offerId)
		{
			_cachedParams.Clear();
			
			COffer offerData = _user.Offers.GetOffer(offerId);
			
			_cachedParams.Add("AnalyticsId", offerData.GetAnalyticsId());
			_cachedParams.Add("Currency", GetOfferPriceCurrency(offerData.Price));
			_cachedParams.Add("Price", GetOfferPriceValue(offerData.Price));
			_cachedParams.Add("Content", GetOfferRewardsString(offerData));
			
			_analytics.SendData("OfferClaim", _cachedParams);
		}

		private string GetOfferRewardsString(COffer offer)
		{
			StringBuilder sb = new();
			for (int i = 0; i < offer.Rewards.Count; i++)
			{
				IValuable offerReward = offer.Rewards[i];
				if (offerReward is IOfferAnalyticsValueProvider analyticsValueProvider)
				{
					string analyticsValue = analyticsValueProvider.GetOfferRewardAnalyticsValue();
					sb.Append(analyticsValue);
				}
				else
				{
					sb.Append(offerReward.Id);
				}
				
				bool isLast = i == offer.Rewards.Count - 1;
				if (!isLast)
				{
					sb.Append(',');
				}
			}
			
			const int maxLength = 240;
			string result = sb.ToString(0, Math.Min(sb.Length, maxLength));
			return result;
		}
		
		private void OnOfferClaimed(COfferClaimedSignal signal)
		{
			OfferClaim(signal.OfferId);
		}

		private string GetOfferPriceCurrency(IValuable price)
		{
			switch (price)
			{
				case CConsumableValuable consumable:
					return consumable.Id.ToString();
				case CResourceValuable resource:
					return resource.Resource.Id.ToString();
				case CFreeValuable:
					return "Free";
				case CRealMoneyValuable realMoney:
					if (CPlatform.IsEditor)
						return CDebugConfig.Instance.CurrencyToDebug.ToString();
					
					string productId = _inAppPrices.GetStoreId(realMoney.Price);
					ProductMetadata metadata = _purchasing.GetProductMetadata(productId);
					return metadata.isoCurrencyCode;
				case CAdvertisementValuable:
					return "Ad";
				case CEventCoinValuable:
					return "Event Coin";
				default:
					throw new NotImplementedException($"Invalid price type: {price.GetType()}");
			}
		}

		private float GetOfferPriceValue(IValuable price)
		{
			switch (price)
			{
				case CConsumableValuable consumable:
					return consumable.Value;
				case CResourceValuable resource:
					return resource.Resource.Amount;
				case CFreeValuable:
					return 0f;
				case CRealMoneyValuable realMoney:
					string productId = _inAppPrices.GetStoreId(realMoney.Price);
					if (CPlatform.IsEditor)
						return _pricesParser.TryGetPriceOrLogError(productId, CDebugConfig.Instance.CurrencyToDebug);
					
					ProductMetadata metadata = _purchasing.GetProductMetadata(productId);
					return (float)metadata.localizedPrice;
				case CAdvertisementValuable:
					return 0f;
				case CEventCoinValuable eventCoin:
					return eventCoin.Amount;
				default:
					throw new NotImplementedException($"Invalid price type: {price.GetType()}");
			}
		}
	}
}