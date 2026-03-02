// =========================================
// DATE:   02.03.2026
// =========================================

using AldaEngine;
using ServerData;

namespace Pharaoh
{
	public class CResearchPurchasedSignal : IEventBusSignal
	{
		public readonly EMissionId Mission;
		public readonly EResearchId ResearchId;

		public CResearchPurchasedSignal(EMissionId mission, EResearchId researchId)
		{
			Mission = mission;
			ResearchId = researchId;
		}
	}
}
