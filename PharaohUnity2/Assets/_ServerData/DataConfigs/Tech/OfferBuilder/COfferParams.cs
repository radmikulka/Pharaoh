// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

namespace ServerData
{
	public static class COfferParams
	{
		public static IOfferParam ExpirationTime(long timestampInMs)
		{
			return COfferParam.New(EOfferParam.ExpirationTime, timestampInMs);
		}

		public static IOfferParam AnalyticsId(string analyticsId)
		{
			return COfferParam.New(EOfferParam.AnalyticsId, analyticsId);
		}

		public static IOfferParam GroupId(string groupId)
		{
			return COfferParam.New(EOfferParam.GroupId, groupId);
		}

		public static IOfferParam CoolDown(long seconds)
		{
			return COfferParam.New(EOfferParam.CoolDownInSecs, seconds);
		}

		public static IOfferParam Sticker(EOfferStickerId id)
		{
			return COfferParam.New(EOfferParam.Sticker, id);
		}

		public static IOfferParam LocalBackground(EOfferLocalBackground localBackground)
		{
			return COfferParam.New(EOfferParam.LocalBackground, localBackground);
		}

		public static IOfferParam DiscountSticker(int value)
		{
			return COfferParam.New(EOfferParam.DiscountSticker, new SDiscountSticker(EDiscountStickerId.Red, value));
		}

		public static IOfferParam PiggyMilestone(int value)
		{
			return COfferParam.New(EOfferParam.PiggyMilestone, value);
		}
		
		public static IOfferParam PiggyPointsPerContract(int value)
		{
			return COfferParam.New(EOfferParam.PiggyPointsPerContract, value);
		}

		public static IOfferParam Size(EOfferSize size)
		{
			return COfferParam.New(EOfferParam.Size, size);
		}

		public static IOfferParam AnalyticsPostfix(string postfix)
		{
			return COfferParam.New(EOfferParam.AnalyticsPostfix, postfix);
		}

		public static IOfferParam FrontendOrderPriority(int index)
		{
			return COfferParam.New(EOfferParam.FrontendOrderPriority, index);
		}
		
		public static IOfferParam TrayIconPriority(int index)
		{
			return COfferParam.New(EOfferParam.TrayIconPriority, index);
		}

		public static IOfferParam MaxPurchasesCount(int count)
		{
			return COfferParam.New(EOfferParam.MaxPurchasesCount, count);
		}

		public static IOfferParam Placement(EOfferPlacement placement)
		{
			return COfferParam.New(EOfferParam.OfferPlacement, placement);
		}

		public static IOfferParam LiveEvent(ELiveEvent liveEventId)
		{
			return COfferParam.New(EOfferParam.LiveEvent, liveEventId);
		}

		public static IOfferParam TrayIcon(string url)
		{
			return COfferParam.New(EOfferParam.TrayIcon, url);
		}

		public static IOfferParam BackgroundColor(string hexColor)
		{
			return COfferParam.New(EOfferParam.BackgroundColor, hexColor);
		}

		public static IOfferParam BackgroundImage(string url)
		{
			return COfferParam.New(EOfferParam.BackgroundImage, url);
		}

		public static IOfferParam HeadlineLangKey(string langKey)
		{
			return COfferParam.New(EOfferParam.HeadlineLangKey, langKey);
		}
		
		public static IOfferParam SubHeadlineLangKey(string langKey)
		{
			return COfferParam.New(EOfferParam.SubHeadlineLangKey, langKey);
		}
		
		public static IOfferParam PopUpTitleLangKey(string langKey)
		{
			return COfferParam.New(EOfferParam.PopupTitleLangKey, langKey);
		}
		
		public static IOfferParam ShopTab(EOfferShopTab tab)
		{
			return COfferParam.New(EOfferParam.ShopTab, tab);
		}

		public static IOfferParam TitleText(string text)
		{
			return COfferParam.New(EOfferParam.HeadlineText, text);
		}

		public static IOfferParam PopUpInterval(long intervalInSecs)
		{
			return COfferParam.New(EOfferParam.PopupIntervalInSeconds, intervalInSecs);
		}

		public static IOfferParam OriginalAnalyticsId(string analyticsId)
		{
			return COfferParam.New(EOfferParam.OriginalAnalyticsId, analyticsId);
		}
		
		public static IOfferParam Frame(EOfferFrame frame)
		{
			return COfferParam.New(EOfferParam.Frame, frame);
		}
		
		public static IOfferParam SceneBackground(EOfferSceneBackground background)
		{
			return COfferParam.New(EOfferParam.VehicleBackground, background);
		}
		
		public static IOfferParam OfferType(EOfferType offerType)
		{
			return COfferParam.New(EOfferParam.OfferType, offerType);
		}
		
		public static IOfferParam ExpirationReminder(long reminderInMiliseconds)
		{
			return COfferParam.New(EOfferParam.ExpirationReminder, reminderInMiliseconds);
		}
	}
}