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
using Pharaoh;

namespace Pharaoh
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
			
		}
	}
}
