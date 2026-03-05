// =========================================
// AUTHOR: Marek Karaba
// DATE:   06.11.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;

namespace TycoonBuilder
{
	public class CRateUsAnalytics : IInitializable
	{
		private readonly IAnalytics _analytics;
		private readonly IEventBus _eventBus;
		
		private readonly Dictionary<string, object> _cachedParams = new();

		public CRateUsAnalytics(IAnalytics analytics, IEventBus eventBus)
		{
			_analytics = analytics;
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CRateUsCustomShowSignal>(OnCustomShow);
			_eventBus.Subscribe<CRateUsCustomRatingSignal>(OnCustomRating);
			_eventBus.Subscribe<CRateUsCustomConversionSignal>(OnCustomConversion);
			_eventBus.Subscribe<CRateUsNativeShowSignal>(OnNativeRateUsShow);
			_eventBus.Subscribe<CRateUsFeedbackSentSignal>(OnFeedbackSent);
		}

		private void OnCustomShow(CRateUsCustomShowSignal signal)
		{
			_cachedParams.Clear();
			_analytics.SendData("RateUsCustomShow");
		}

		private void OnCustomRating(CRateUsCustomRatingSignal signal)
		{
			_cachedParams.Clear();
			_cachedParams.Add("Rating", signal.StarsAmount);
			_analytics.SendData("RateUsCustomRating", _cachedParams);
		}

		private void OnCustomConversion(CRateUsCustomConversionSignal signal)
		{
			_cachedParams.Clear();
			_analytics.SendData("RateUsCustomConversion");
		}

		private void OnNativeRateUsShow(CRateUsNativeShowSignal signal)
		{
			_cachedParams.Clear();
			_analytics.SendData("RateUsNativeShow", _cachedParams);
		}

		private void OnFeedbackSent(CRateUsFeedbackSentSignal signal)
		{
			_cachedParams.Clear();
			_analytics.SendData("RateUsCustomFeedbackSend", _cachedParams);
		}
	}
}