// =========================================
// AUTHOR: Marek Karaba
// DATE:   18.08.2025
// =========================================

using System;
using System.Collections.Generic;
using AldaEngine.AldaFramework;
using ServerData;
using ServerData.Design;

namespace TycoonBuilder
{
	public class CClaimedRegionsProvider : IAldaFrameworkComponent
	{
		private readonly CIndustryRegionsProvider _industryRegionsProvider;
		private readonly CUser _user;

		public CClaimedRegionsProvider(CIndustryRegionsProvider industryRegionsProvider, CUser user)
		{
			_industryRegionsProvider = industryRegionsProvider;
			_user = user;
		}

		public ERegion[] GetValidRegions(EResource resource, EStaticContractId storyContract, ECity city)
		{
			if (storyContract != EStaticContractId.None)
			{
				return GetContractRegions(storyContract);
			}

			if (city != ECity.None)
			{
				return GetContractRegions(city);
			}
			
			if (resource != EResource.None)
			{
				ERegion[] resourceRegions = _industryRegionsProvider.GetAvailableRegions(resource);
				return resourceRegions;
			}
			return Array.Empty<ERegion>();
		}
		
		private ERegion[] GetContractRegions(EStaticContractId contractId)
		{
			CContract contract = _user.Contracts.GetStaticContractOrDefault(contractId);
			return contract == null ? Array.Empty<ERegion>() : contract.Regions;
		}

		private ERegion[] GetContractRegions(ECity cityId)
		{
			CContract contract = _user.Contracts.GetPassengerContractOrDefault(cityId);
			return contract == null ? Array.Empty<ERegion>() : contract.Regions;
		}
	}
}