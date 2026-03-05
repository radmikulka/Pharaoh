// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public class COffer
	{
		private readonly Dictionary<EOfferParam, IOfferParam> _params = new();
		private readonly IValuable[] _rewards;
		private readonly EOfferTag[] _tags;

		public IOfferParam[] Params => _params.Values.ToArray();
		public IReadOnlyList<IValuable> Rewards => _rewards;
		public IReadOnlyList<EOfferTag> Tags => _tags;
		public readonly IValuable Price;
		public readonly string Guid;
		public bool IsSeen { get; private set; }

		public COffer(string guid, IValuable price, IValuable[] rewards, IOfferParam[] @params, EOfferTag[] tags, bool isSeen)
		{
			_rewards = rewards;
			_tags = tags;
			Price = price;
			Guid = guid;
			IsSeen = isSeen;

			foreach (var param in @params)
			{
				_params.Add(param.Id, param);
			}
		}

		public string GetAnalyticsId()
		{
			return GetParamValue<string>(EOfferParam.AnalyticsId);
		}

		public bool HavePiggyContractMilestone()
		{
			return _params.ContainsKey(EOfferParam.PiggyMilestone);
		}

		public void IncreaseCompletedPiggyContracts()
		{
			IncreaseIntParam(EOfferParam.CompletedPiggyContracts);
		}

		public void SetPiggyContractsSeen(int contractsSeen)
		{
			_params[EOfferParam.SeenCompletedPiggyContracts] = COfferParam.New(EOfferParam.SeenCompletedPiggyContracts, contractsSeen);
		}

		private void IncreaseIntParam(EOfferParam paramId)
		{
			int value = GetParamValueOrDefault<int>(paramId);
			_params[paramId] = COfferParam.New(paramId, value + 1);
		}

		public void Claim(long time)
		{
			int buyCount = 0;
			if (_params.TryGetValue(EOfferParam.PurchasesCount, out IOfferParam param) && param is COfferParam typedParam)
			{
				buyCount = typedParam.GetValue<int>();
			}
			
			_params[EOfferParam.PurchasesCount] = COfferParam.New(EOfferParam.PurchasesCount, buyCount + 1);
			_params[EOfferParam.LastPurchaseTime] = COfferParam.New(EOfferParam.LastPurchaseTime, time);
		}

		public T GetParamValueOrDefault<T>(EOfferParam paramType)
		{
			if (_params.TryGetValue(paramType, out IOfferParam param) && param is COfferParam typedParam)
				return typedParam.GetValueOrDefault<T>();
			return default;
		}
		
		public T GetParamValue<T>(EOfferParam paramType)
		{
			if (_params.TryGetValue(paramType, out IOfferParam param) && param is COfferParam typedParam)
				return typedParam.GetValue<T>();
			
			throw new KeyNotFoundException($"Parameter {paramType} not found in offer {Guid}.");
		}
		
		public bool IsExpired(long time)
		{
			if (Params.All(param => param.Id != EOfferParam.ExpirationTime))
				return false;

			long expirationTime = GetParamValue<long>(EOfferParam.ExpirationTime);
			return time > expirationTime;
		}
		
		public bool MaxPurchasesReached()
		{
			if (Params.All(param => param.Id != EOfferParam.MaxPurchasesCount))
				return false;
			
			int maxPurchasesCount = GetParamValue<int>(EOfferParam.MaxPurchasesCount);
			int buyCount = GetParamValueOrDefault<int>(EOfferParam.PurchasesCount);
			
			if (maxPurchasesCount <= 0)
				return false;
			
			return buyCount >= maxPurchasesCount;
		}
		
		public EOfferPlacement GetPlacement()
		{
			if (_params.TryGetValue(EOfferParam.OfferPlacement, out IOfferParam param) && param is COfferParam typedParam)
				return typedParam.GetValue<EOfferPlacement>();

			return EOfferPlacement.Shop;
		}
		
		public EOfferType GetOfferType()
		{
			if (_params.TryGetValue(EOfferParam.OfferType, out IOfferParam param) && param is COfferParam typedParam)
				return typedParam.GetValue<EOfferType>();

			return EOfferType.SimpleOffer;
		}

		public void MarkAsSeen()
		{
			IsSeen = true;
		}
		
		public int GetFrontendOrderPriority()
		{
			if (_params.TryGetValue(EOfferParam.FrontendOrderPriority, out IOfferParam param) && param is COfferParam typedParam)
				return typedParam.GetValue<int>();
			
			return 0;
		}
	}
}