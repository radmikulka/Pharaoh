// =========================================
// AUTHOR: Radek Mikulka
// DATE:   16.09.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;

namespace ServerData
{
	public class CCityConfigs
	{
		private readonly Dictionary<ECity, CCityConfig> _cityConfigs = new();
		private readonly List<CCityConfig> _citiesInOpeningOrder = new();
		
		public IReadOnlyList<CCityConfig> CitiesInOpeningOrder => _citiesInOpeningOrder;

		public CCityConfig GetCityConfig(ECity city)
		{
			return _cityConfigs[city];
		}
		
		protected void AddCity(CCityConfig config)
		{
			_cityConfigs.Add(config.CityId, config);
			if(config.LiveEvent != ELiveEvent.None)
				return;
			_citiesInOpeningOrder.Add(config);
		}
		
		protected CCityConfigBuilder GetBuilder(ECity cityId)
		{
			return new CCityConfigBuilder(cityId);
		}
	}
}