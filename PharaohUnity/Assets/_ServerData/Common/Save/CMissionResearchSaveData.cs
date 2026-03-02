// =========================================
// DATE:   02.03.2026
// =========================================

using System;

namespace ServerData
{
	public class CMissionResearchSaveData
	{
		public int CurrentKP { get; set; }
		public long NextKpRegenTimestamp { get; set; }
		public int[] PurchasedResearchIds { get; set; } = Array.Empty<int>();
	}
}
