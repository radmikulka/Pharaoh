// =========================================
// AUTHOR: Radek Mikulka
// DATE:   22.02.2026
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CEventContractData
	{
		public readonly ELiveEvent EventId;
		public readonly bool IsInfinity;

		public CEventContractData(ELiveEvent eventId, bool isInfinity)
		{
			EventId = eventId;
			IsInfinity = isInfinity;
		}
	}
}
