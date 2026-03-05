// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.09.2023
// =========================================

using AldaEngine;
using AldaEngine.Tcp;
using ServerData;
using ServerData.Dto;
using ServerData.Hits;
using UnityEngine;
using TycoonBuilder;

namespace TycoonBuilder
{
	internal class CServerResponseDataSync
	{
		public void SyncHits(IHit[] hits, CUser user)
		{
			foreach (IHit hit in hits)
			{
				SyncHit(hit, user);
			}
		}

		private void SyncHit(IHit hit, CUser user)
		{
			if (hit is IIHaveModifiedData iIHaveModifiedData)
			{
				CModifiedUserDataDto modifiedData = iIHaveModifiedData.GetModifiedData();
				ProcessModifiedData(user, modifiedData);
			}
		}

		private void ProcessModifiedData(CUser user, CModifiedUserDataDto modifiedData)
		{
			if(modifiedData == null)
				return;

			SyncValuables(user, modifiedData);
			SyncResources(user, modifiedData);
			TryValidateRechargers(user, modifiedData);
			TrySyncEventCompetitions(user, modifiedData);
			TrySyncEventLeaderboardComplement(user, modifiedData);
			user.Offers.Sync(modifiedData.NewOffers);
			TrySyncProjects(user, modifiedData);
		}

		private void TrySyncProjects(CUser user, CModifiedUserDataDto modifiedData)
		{
			if (modifiedData.NewProjects == null) return;
			user.Projects.Sync(modifiedData.NewProjects);
		}

		private void TrySyncEventLeaderboardComplement(CUser user, CModifiedUserDataDto modifiedData)
		{
			user.LiveEvents.SyncLeaderboardComplements(modifiedData.LeaderboardComplements);
		}

		private void TrySyncEventCompetitions(CUser user, CModifiedUserDataDto modifiedData)
		{
			user.LiveEvents.SyncCompetitions(modifiedData.NewLiveEventLeaderboards);
		}

		private void TryValidateRechargers(CUser user, CModifiedUserDataDto modifiedData)
		{
			user.RechargerValidator.Sync(modifiedData);
		}

		private void SyncResources(CUser user, CModifiedUserDataDto modifiedData)
		{
			foreach (SResource resource in modifiedData.Resources)
			{
				user.Warehouse.Sync(resource);
			}
		}

		private void SyncValuables(CUser user, CModifiedUserDataDto modifiedData)
		{
			foreach (IOwnedValuableData valuable in modifiedData.Valuables)
			{
				user.OwnedValuables.Sync(valuable);
			}
		}
	}
}
