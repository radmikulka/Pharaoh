// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.09.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CSetActiveIndustryDetailCameraSignal : IEventBusSignal
	{
		public readonly EIndustry IndustryId;
		public readonly bool State;

		public CSetActiveIndustryDetailCameraSignal(EIndustry industryId, bool state)
		{
			IndustryId = industryId;
			State = state;
		}
	}
}