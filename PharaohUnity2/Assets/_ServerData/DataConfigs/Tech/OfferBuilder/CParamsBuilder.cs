// =========================================
// AUTHOR: Radek Mikulka
// DATE:   15.07.2025
// =========================================

using System.Collections.Generic;
using System.Linq;
using AldaEngine;

namespace ServerData
{
	public class CParamsBuilder<T> where T : CParamsBuilder<T>
	{
		protected readonly Dictionary<EOfferParam, IOfferParam> Params = new();

		public IOfferParam[] AllParams => Params.Values.ToArray();

		public TParamType GetParamValueOrDefault<TParamType>(EOfferParam paramId)
		{
			if (!Params.TryGetValue(paramId, out var value))
				return default;

			return value.GetValueOrDefault<TParamType>();
		}

		public TParamType GetParamValue<TParamType>(EOfferParam paramId) where TParamType : class
		{
			return Params[paramId].GetValue<TParamType>();
		}

		public bool HaveParam(EOfferParam paramId)
		{
			return Params.ContainsKey(paramId);
		}

		public bool TryGetParam<TParamType>(EOfferParam paramId, out TParamType value)
		{
			if (!Params.TryGetValue(paramId, out var param))
			{
				value = default;
				return false;
			}

			value = param.GetValueOrDefault<TParamType>();
			return true;
		}

		public T SetFrontendOrderPriority(int priority)
		{
			IOfferParam param = COfferParams.FrontendOrderPriority(priority);
			return SetParam(param);
		}

		/// <summary>
		/// Podle toho client pozná do kterého event shopu má nabídku dát (eventů může běžet více najednou a každý má svůj shop)
		/// </summary>
		/// <param name="liveEventId"></param>
		/// <returns></returns>
		public T SetLiveEvent(ELiveEvent liveEventId)
		{
			IOfferParam param = COfferParams.LiveEvent(liveEventId);
			return SetParam(param);
		}

		public T SetTrayIconPriority(int priority)
		{
			IOfferParam param = COfferParams.TrayIconPriority(priority);
			return SetParam(param);
		}

		public T SetExpirationTime(long endTime)
		{
			IOfferParam param = COfferParams.ExpirationTime(endTime);
			return SetParam(param);
		}

		public T SetMaxPurchasesCount(int count)
		{
			IOfferParam param = COfferParams.MaxPurchasesCount(count);
			return SetParam(param);
		}

		public T SetGroupId(string groupId)
		{
			IOfferParam param = COfferParams.GroupId(groupId);
			return SetParam(param);
		}

		public T SetBackgroundColor(string hexColor)
		{
			IOfferParam param = COfferParams.BackgroundColor(hexColor);
			return SetParam(param);
		}

		public T SetPopupIntervalInSecs(long secs)
		{
			return SetPopupIntervalInSecs((int)secs);
		}

		public T SetPiggyMilestone(int value)
		{
			IOfferParam param = COfferParams.PiggyMilestone(value);
			return SetParam(param);
		}
		
		public T SetPiggyPointsPerContractTask(int value)
		{
			IOfferParam param = COfferParams.PiggyPointsPerContract(value);
			return SetParam(param);
		}

		public T SetPopupIntervalInSecs(int secs)
		{
			IOfferParam param = COfferParams.PopUpInterval(secs);
			return SetParam(param);
		}

		public T SetSticker(EOfferStickerId id)
		{
			IOfferParam param = COfferParams.Sticker(id);
			return SetParam(param);
		}

		public T SetCooldownInSecs(int seconds)
		{
			IOfferParam param = COfferParams.CoolDown(seconds);
			return SetParam(param);
		}

		public T SetCooldownInMin(int minutes)
		{
			IOfferParam param = COfferParams.CoolDown(minutes * CTimeConst.Minute.InSeconds);
			return SetParam(param);
		}

		public T SetLocalBackground(EOfferLocalBackground localBackground)
		{
			IOfferParam param = COfferParams.LocalBackground(localBackground);
			return SetParam(param);
		}

		public T SetDiscountSticker(int value)
		{
			IOfferParam param = COfferParams.DiscountSticker(value);
			return SetParam(param);
		}

		public T SetSize(EOfferSize size)
		{
			IOfferParam param = COfferParams.Size(size);
			return SetParam(param);
		}

		public T SetAnalyticsId(string analyticsId)
		{
			IOfferParam originalId = COfferParams.OriginalAnalyticsId(analyticsId);
			return SetParam(originalId);
		}

		public T SetFrame(EOfferFrame frame)
		{
			IOfferParam param = COfferParams.Frame(frame);
			return SetParam(param);
		}

		public T SetPlacement(EOfferPlacement placement)
		{
			IOfferParam param = COfferParams.Placement(placement);
			return SetParam(param);
		}

		public T SetAnalyticsPostfix(string postfix)
		{
			IOfferParam param = COfferParams.AnalyticsPostfix(postfix);
			return SetParam(param);
		}

		public T SetAnalyticsPostfix(int analyticsId)
		{
			return SetAnalyticsPostfix(analyticsId.ToString());
		}

		public T SetHeadlineLangKey(string langKey)
		{
			IOfferParam param = COfferParams.HeadlineLangKey(langKey);
			return SetParam(param);
		}
		
		public T SetSubHeadlineLangKey(string langKey)
		{
			IOfferParam param = COfferParams.SubHeadlineLangKey(langKey);
			return SetParam(param);
		}
		
		public T SetPopUpTitleLangKey(string langKey)
		{
			IOfferParam param = COfferParams.PopUpTitleLangKey(langKey);
			return SetParam(param);
		}

		public T SetParam(IOfferParam param)
		{
			Params.AddOrUpdate(param.Id, param);
			return (T)this;
		}
		
		public T SetShopTab(EOfferShopTab tab)
		{
			IOfferParam param = COfferParams.ShopTab(tab);
			return SetParam(param);
		}
		
		public T SetOfferType(EOfferType offerType)
		{
			IOfferParam param = COfferParams.OfferType(offerType);
			return SetParam(param);
		}
		
		public T SetSceneBackground(EOfferSceneBackground background)
        {
            IOfferParam param = COfferParams.SceneBackground(background);
            return SetParam(param);
        }

		public T SetExpirationReminder(long reminderInSeconds)
		{
			IOfferParam param = COfferParams.ExpirationReminder(reminderInSeconds * CTimeConst.Second.InMilliseconds);
			return SetParam(param);
		}

		public T SetParams(IReadOnlyList<IOfferParam> @params)
		{
			foreach (IOfferParam offerParam in @params)
			{
				Params.AddOrUpdate(offerParam.Id, offerParam);
			}

			return (T)this;
		}
	}
}