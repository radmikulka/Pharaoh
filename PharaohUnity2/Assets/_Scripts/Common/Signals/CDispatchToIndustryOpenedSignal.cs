// =========================================
// AUTHOR: Radek Mikulka
// DATE:   13.10.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CDispatchToIndustryOpenedSignal : IEventBusSignal
	{
		public readonly EIndustry Industry;
		
		public CDispatchToIndustryOpenedSignal(EIndustry industry)
		{
			Industry = industry;
		}
	}
}