// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.10.2023
// =========================================

using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;
using AldaEngine.Tcp;
using ServerData;
using ServerData.Dto;
using TycoonBuilder.Signal;
// ReSharper disable SuspiciousTypeConversion.Global

namespace TycoonBuilder
{
	public class CValuableAnalytics : IInitializable
	{
		private readonly IAnalytics _analytics;
		private readonly IEventBus _eventBus;
		private readonly IMapper _mapper;
		private readonly Dictionary<string, object> _cachedParams = new();

		public CValuableAnalytics(IAnalytics analytics, IEventBus eventBus, IMapper mapper)
		{
			_analytics = analytics;
			_eventBus = eventBus;
			_mapper = mapper;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CServerHitsProcessedSignal>(OnHitsProcessed);
		}

		private void OnHitsProcessed(CServerHitsProcessedSignal signal)
		{
			foreach (IHit hit in signal.Hits)
			{
				if (hit is not IIHaveModifiedData iIHaveModifiedData) 
					continue;
				
				CModifiedUserDataDto modifiedData = iIHaveModifiedData.GetModifiedData();
				if (modifiedData != null)
				{
					LogAnalytics(modifiedData.ValuableModifications);
				}
			}
		}

		private void LogAnalytics(CValuableModificationDto[] modifications)
		{
			foreach (CValuableModificationDto modifiedValut in modifications)
			{
				LogValuableChange(modifiedValut);
			}
		}

		private void LogValuableChange(CValuableModificationDto modifiedValuable)
		{
			IValuable valuable = _mapper.FromJson<IValuable>(modifiedValuable.Valuable);
			IValuable totalValue = _mapper.FromJson<IValuable>(modifiedValuable.OwnedValue);
			
			_cachedParams.Clear();
			_cachedParams.Add("Name", valuable.Id);
			_cachedParams.Add("Source", modifiedValuable.Source);
			_cachedParams.Add("Value", valuable.GetAnalyticsValue());
			_cachedParams.Add("ValueTotal", totalValue?.GetAnalyticsValue());
			_analytics.SendData("ValuableChange", _cachedParams);
		}
	}
}