// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CModifiedUserDataDto
	{
		[JsonProperty] public CValuableModificationDto[] ValuableModifications { get; set; }
		[JsonProperty] public CResourceModificationDto[] ResourceModifications { get; set; }
		[JsonProperty] public CLiveEventLeaderboardDto[] NewLiveEventLeaderboards { get; set; }
		[JsonProperty] public CLeaderboardComplementDto[] LeaderboardComplements { get; set; }
		[JsonProperty] public CModifiedRechargersDto Rechargers { get; set; }
		[JsonProperty] public IOwnedValuableData[] Valuables { get;  set; }
		[JsonProperty] public CProjectsDto NewProjects { get; set; }
		[JsonProperty] public SResource[] Resources { get;  set; }
		[JsonProperty] public COffersDto NewOffers { get; set; }

		public CModifiedUserDataDto(
			CValuableModificationDto[] valuableModifications,
			CLeaderboardComplementDto[] leaderboardComplements,
			CModifiedRechargersDto rechargers,
			IOwnedValuableData[] valuables,
			SResource[] resources,
			CResourceModificationDto[] resourceModificationsDto,
			CLiveEventLeaderboardDto[] newLiveEventLeaderboards,
			COffersDto newOffers,
			CProjectsDto newProjects
			)
		{
			NewLiveEventLeaderboards = newLiveEventLeaderboards;
			ResourceModifications = resourceModificationsDto;
			ValuableModifications = valuableModifications;
			LeaderboardComplements = leaderboardComplements;
			NewProjects = newProjects;
			Rechargers = rechargers;
			NewOffers = newOffers;
			Resources = resources;
			Valuables = valuables;
		}

		[OnDeserialized]
		private void ReplaceNulls(StreamingContext context)
		{
			Rechargers ??= new CModifiedRechargersDto(
				null, 
				null,
				Array.Empty<CModifiedFactoryRechargerDto>(),
				Array.Empty<CModifiedVehicleRechargerDto>()
				);
			
			ResourceModifications ??= Array.Empty<CResourceModificationDto>();
			ValuableModifications ??= Array.Empty<CValuableModificationDto>();
			Valuables ??= Array.Empty<IOwnedValuableData>();
			Resources ??= Array.Empty<SResource>();
		}
	}
}