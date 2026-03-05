// =========================================
// AUTHOR: Radek Mikulka
// DATE:   11.07.2025
// =========================================

using System.Collections.Generic;

namespace ServerData
{
	public class CBaseIndustryConfigs
	{
		private readonly Dictionary<EIndustry, CResourceIndustryConfig> _industries = new();
		private readonly Dictionary<EResource, CResourceIndustryConfig> _industriesByResource = new();
		private readonly Dictionary<ELiveEvent, CResourceIndustryConfig> _eventIndustries = new();

		private void AddIndustryConfig(CResourceIndustryConfig config)
		{
			_industries.Add(config.Id, config);
			_industriesByResource.Add(config.Resource, config);

			if (config.LiveEvent != ELiveEvent.None)
			{
				_eventIndustries.Add(config.LiveEvent, config);
			}
		}
		
		protected void AddIndustryConfig(CIndustryBuilder builder)
		{
			AddIndustryConfig(builder.Build());
		}
		
		protected CIndustryBuilder GetNewBuilder()
		{
			CIndustryBuilder builder = new();
			return builder;
		}
		
		public CResourceIndustryConfig GetIndustryOrDefault(EResource resource)
		{
			return _industriesByResource.GetValueOrDefault(resource);
		}
		
		public CResourceIndustryConfig GetConfig(EResource resource)
		{
			return _industriesByResource.GetValueOrDefault(resource);
		}

		public CResourceIndustryConfig GetEventIndustry(ELiveEvent liveEvent)
		{
			return _eventIndustries[liveEvent];
		}

		public CResourceIndustryConfig GetConfig(EIndustry id)
		{
			return _industries[id];
		}
	}
}