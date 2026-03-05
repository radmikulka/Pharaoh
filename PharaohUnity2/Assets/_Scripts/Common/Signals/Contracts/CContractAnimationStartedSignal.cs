// =========================================
// AUTHOR: Marek Karaba
// DATE:   11.11.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CContractAnimationStartedSignal : IEventBusSignal
	{
		public readonly IContract Contract;
		public readonly bool CanShowSkipButton;
		public readonly CAnimation Anim;

		public CContractAnimationStartedSignal(IContract contract, bool canShowSkipButton, CAnimation anim)
		{
			CanShowSkipButton = canShowSkipButton;
			Contract = contract;
			Anim = anim;
		}
	}
}