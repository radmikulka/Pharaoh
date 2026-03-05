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
			_eventBus.Subscribe<CSpecialBuildingBoughtSignal>(OnSpecialBuildingBought);
			_eventBus.Subscribe<CServerHitsProcessedSignal>(OnHitsProcessed);
			_eventBus.Subscribe<CVehicleAddedSignal>(OnVehicleAdded);
		}

		private void OnVehicleAdded(CVehicleAddedSignal signal)
		{
			_cachedParams.Clear();
			_cachedParams.Add("Name", signal.VehicleId);
			_cachedParams.Add("Source", signal.ObtainSource);
			_analytics.SendData("VehicleChange", _cachedParams);
		}

		private void OnSpecialBuildingBought(CSpecialBuildingBoughtSignal signal)
		{
			_cachedParams.Clear();
			_cachedParams.Add("Name", signal.BuildingId);
			_analytics.SendData("BuildingChange", _cachedParams);
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
					LogAnalytics(modifiedData.ResourceModifications);
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
			_cachedParams.Add("SourceDetail", modifiedValuable.SourceDetail);
			_cachedParams.Add("Price", modifiedValuable.Price);
			_cachedParams.Add("Value", valuable.GetAnalyticsValue());
			_cachedParams.Add("ValueTotal", totalValue?.GetAnalyticsValue());
			_analytics.SendData("ValuableChange", _cachedParams);
		}

		private void LogAnalytics(CResourceModificationDto[] modifications)
		{
			foreach (CResourceModificationDto resource in modifications)
			{
				LogValuableChange(resource);
			}
		}

		private void LogValuableChange(CResourceModificationDto resource)
		{
			_cachedParams.Clear();
            _cachedParams.Add("Name", resource.Resource.Id);
            _cachedParams.Add("Source", resource.Source);
            _cachedParams.Add("SourceDetail", resource.SourceDetail);
            _cachedParams.Add("Price", resource.Price);
            _cachedParams.Add("Value", resource.Resource.Amount);
			_cachedParams.Add("ValueTotal", resource.OwnedValue.Amount);
			_analytics.SendData("ResourceChange", _cachedParams);
		}
	}
}