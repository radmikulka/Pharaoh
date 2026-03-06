// =========================================
// AUTHOR: Radek Mikulka
// DATE:   14.07.2025
// =========================================

using AldaEngine;
using ServerData;
using ServerData.Dto;
using ServerData.Hits;
using ServiceEngine;

namespace Pharaoh
{
	public class CAccount : CBaseUserComponent
	{
		public string PublicId { get; private set; }
		
		public void InitialSync(CAccountDto dtoAccount)
		{
			PublicId = dtoAccount.PublicId;
		}
	}
}