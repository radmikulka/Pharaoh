// =========================================
// AUTHOR: Radek Mikulka
// DATE:   29.07.2024
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData.Dto;
using ServerData.Hits;

namespace TycoonBuilder
{
	public class CDebugInfo
	{
		#if DEBUG_MODE || !UNITY_ENGINE
		
		public int Sps { get; private set; }
		public string ContextualPrice { get; private set; }
		public ECountryCode Country { get; private set; }
		
		private void Sync(CDebugInfoDto dto)
		{
			if(dto == null)
				return;
			
			ContextualPrice = dto.ContextualPrice;
			Country = dto.Country;
			Sps = dto.Sps;
		}

		#endif
		
		public void InitialSync(CDebugInfoDto dto)
		{
			#if DEBUG_MODE || !UNITY_ENGINE
			Sync(dto);
			#endif
		}
	}
	
}