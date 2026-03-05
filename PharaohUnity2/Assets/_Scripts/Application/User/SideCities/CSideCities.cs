// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.11.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;

namespace TycoonBuilder
{
	public class CSideCities : CBaseUserComponent, IInitializable
	{
		private readonly Dictionary<ECity, CSideCity> _cities = new();

		private readonly IEventBus _eventBus;

		public CSideCities(IEventBus eventBus)
		{
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			_eventBus.Subscribe<CPassengerContractRewardsClaimedSignal>(OnPassengerContractRewardsClaimed);
		}

		private void OnPassengerContractRewardsClaimed(CPassengerContractRewardsClaimedSignal signal)
		{
			CSideCity city = GetOrCreateCity(signal.Contract.PassengerData.CityId);
			city.IncreaseCompletedContractsCount();
		}

		public void InitialSync(CSideCitiesDto dto)
		{
			foreach (CSideCityDto city in dto.Cities)
			{
				_cities.Add(city.City, new CSideCity(city.City, city.CompletedContractsCount));
			}
		}

		public int GetCompletedContractsForCity(ECity cityId)
		{
			CSideCity city = GetOrCreateCity(cityId);
			return city.CompletedContractsCount;
		}

		private CSideCity GetOrCreateCity(ECity cityId)
		{
			if (!_cities.TryGetValue(cityId, out var city))
			{
				city = new CSideCity(cityId, 0);
				_cities.Add(cityId, city);
			}
			return city;
		}
	}
}